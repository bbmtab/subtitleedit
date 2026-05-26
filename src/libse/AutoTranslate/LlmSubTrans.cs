using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core.Translate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Core.AutoTranslate
{
    public class LlmSubTrans : IAutoTranslator
    {
        public static string StaticName { get; set; } = "LLM Subtrans";
        public override string ToString() => StaticName;
        public string Name => StaticName;
        public string Url => "https://github.com/machinewrapped/llm-subtrans";
        public string Error { get; set; }
        public int MaxCharacters => 5000;

        private Subtitle _cachedSubtitle;
        private string _lastSourceLanguage;
        private string _lastTargetLanguage;
        private readonly Subtitle _originalSubtitle;
        public string FileName { get; set; }

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private int _currentBatchIndex = 0;
        private List<string> _sequentialCache;
        private bool _isFailed = false;

        public LlmSubTrans()
        {
        }

        public LlmSubTrans(Subtitle originalSubtitle)
        {
            _originalSubtitle = originalSubtitle;
        }

        public LlmSubTrans(Subtitle originalSubtitle, string fileName)
        {
            _originalSubtitle = originalSubtitle;
            FileName = fileName;
        }

        public void Initialize()
        {
            _cachedSubtitle = null;
            _currentBatchIndex = 0;
            _sequentialCache = null;
            _isFailed = false;
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return ChatGptTranslate.ListLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return ChatGptTranslate.ListLanguages();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_isFailed)
                {
                    return text;
                }

                if (_cachedSubtitle == null || _lastSourceLanguage != sourceLanguageCode || _lastTargetLanguage != targetLanguageCode)
                {
                    await TranslateWholeSubtitle(sourceLanguageCode, targetLanguageCode, cancellationToken);
                }

                if (_cachedSubtitle == null)
                {
                    _isFailed = true;
                    return "Error: " + (Error ?? "Translation failed.");
                }

                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var translatedLines = new List<string>();

                foreach (var line in lines)
                {
                    // Sequential access is the only reliable way to map LLM results back to SE
                    if (_currentBatchIndex < _sequentialCache.Count)
                    {
                        translatedLines.Add(_sequentialCache[_currentBatchIndex]);
                        _currentBatchIndex++;
                    }
                    else
                    {
                        translatedLines.Add(line);
                    }
                }

                return string.Join(Environment.NewLine, translatedLines);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task TranslateWholeSubtitle(string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            if (_originalSubtitle == null)
            {
                Error = "Subtitle data not available.";
                return;
            }

            _sequentialCache = new List<string>();
            _currentBatchIndex = 0;

            var subtitleFolder = !string.IsNullOrEmpty(FileName) ? Path.GetDirectoryName(FileName) : Path.GetTempPath();
            var sourceBaseName = !string.IsNullOrEmpty(FileName) ? Path.GetFileNameWithoutExtension(FileName) : "new_subtitle";
            
            // Stable names to allow resuming
            var tempInput = Path.Combine(subtitleFolder, $"{sourceBaseName}.llm-tmp.srt");
            var tempOutput = Path.Combine(subtitleFolder, $"{sourceBaseName}.llm-out.srt");
            var finalProjectFile = Path.Combine(subtitleFolder, $"{sourceBaseName}.subtrans");
            var tempProjectFile = Path.Combine(subtitleFolder, $"{sourceBaseName}.llm-tmp.subtrans");

            if (File.Exists(tempOutput)) File.Delete(tempOutput);

            var srt = new SubRip();
            File.WriteAllText(tempInput, srt.ToText(_originalSubtitle, string.Empty), new UTF8Encoding(false));

            var pythonPath = Configuration.Settings.Tools.LlmSubtransPythonPath;
            var scriptPath = Configuration.Settings.Tools.LlmSubtransScriptPath;

            if (string.IsNullOrEmpty(pythonPath)) pythonPath = "python.exe";
            if (!File.Exists(scriptPath))
            {
                Error = "Script not found. Check settings.";
                return;
            }

            var baseUrl = Configuration.Settings.Tools.LlmSubtransUrl?.TrimEnd('/');
            var endpoint = Configuration.Settings.Tools.LlmSubtransEndpoint?.TrimStart('/');
            
            var args = new StringBuilder();
            args.Append($"\"{scriptPath}\" ");
            args.Append($"\"{tempInput}\" ");
            if (Configuration.Settings.Tools.LlmSubtransProject) args.Append("--project ");
            args.Append($"-l \"{targetLanguageCode}\" ");
            args.Append($"-o \"{tempOutput}\" ");
            if (!string.IsNullOrEmpty(baseUrl)) args.Append($"-s \"{baseUrl}\" ");
            if (!string.IsNullOrEmpty(endpoint)) args.Append($"-e \"/{endpoint}\" ");
            args.Append($"-k \"{Configuration.Settings.Tools.LlmSubtransApiKey}\" ");
            args.Append($"-m \"{Configuration.Settings.Tools.LlmSubtransModel}\" ");
            args.Append($"--temperature \"{Configuration.Settings.Tools.LlmSubtransTemperature.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" ");
            args.Append($"--ratelimit \"{Configuration.Settings.Tools.LlmSubtransRateLimit}\" ");
            args.Append($"--minbatchsize \"{Configuration.Settings.Tools.LlmSubtransMinBatchSize}\" ");
            args.Append($"--maxbatchsize \"{Configuration.Settings.Tools.LlmSubtransMaxBatchSize}\" ");
            args.Append($"--maxretries \"{Configuration.Settings.Tools.LlmSubtransMaxRetries}\" ");
            args.Append($"--backofftime \"{Configuration.Settings.Tools.LlmSubtransBackoffTime}\" ");
            args.Append($"--scenethreshold \"{Configuration.Settings.Tools.LlmSubtransSceneThreshold}\" ");
            args.Append($"--batchthreshold \"{Configuration.Settings.Tools.LlmSubtransBatchThreshold}\" ");
            args.Append($"--maxsummaries \"{Configuration.Settings.Tools.LlmSubtransMaxSummaries}\" ");
            if (Configuration.Settings.Tools.LlmSubtransChat || (endpoint != null && endpoint.Contains("chat"))) args.Append("--chat ");
            if (Configuration.Settings.Tools.LlmSubtransPostProcess) args.Append("--postprocess ");
            if (Configuration.Settings.Tools.LlmSubtransSystemMessages) args.Append("--systemmessages ");
            if (Configuration.Settings.Tools.LlmSubtransAuto) args.Append("--auto ");
            if (Configuration.Settings.Tools.LlmSubtransIncludeOriginal) args.Append("--includeoriginal ");
            if (Configuration.Settings.Tools.LlmSubtransAddRtlMarkers) args.Append("--addrtlmarkers ");
            if (Configuration.Settings.Tools.LlmSubtransBuildTerminologyMap) args.Append("--build-terminology-map ");

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.LlmSubtransInstructionFile))
                args.Append($"--instructionfile \"{Configuration.Settings.Tools.LlmSubtransInstructionFile}\" ");

            var namesFile = Configuration.Settings.Tools.LlmSubtransNamesFile;
            if (string.IsNullOrEmpty(namesFile) && !string.IsNullOrEmpty(subtitleFolder))
            {
                var defaultNames = Path.Combine(subtitleFolder, "names.txt");
                if (File.Exists(defaultNames)) namesFile = defaultNames;
            }
            if (!string.IsNullOrEmpty(namesFile)) args.Append($"--names \"{namesFile}\" ");

            var termFile = Configuration.Settings.Tools.LlmSubtransTerminologyFile;
            if (string.IsNullOrEmpty(termFile) && !string.IsNullOrEmpty(subtitleFolder))
            {
                var defaultTerm = Path.Combine(subtitleFolder, "term.txt");
                if (File.Exists(defaultTerm)) termFile = defaultTerm;
            }
            if (!string.IsNullOrEmpty(termFile)) args.Append($"--terminology-file \"{termFile}\" ");

            var workingDir = Path.GetDirectoryName(scriptPath);
            if (workingDir != null && workingDir.EndsWith("scripts", StringComparison.OrdinalIgnoreCase)) workingDir = Path.GetDirectoryName(workingDir);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = args.ToString(),
                UseShellExecute = true, // Open in a real terminal window
                CreateNoWindow = false,
                WorkingDirectory = workingDir
            };

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    if (!process.Start())
                    {
                        Error = "Failed to start Python.";
                        return;
                    }

                    await Task.Run(() => process.WaitForExit(), cancellationToken);

                    if (process.ExitCode == 0 && File.Exists(tempOutput))
                    {
                        // Handle project file renaming (llm-subtrans creates it based on input filename)
                        if (File.Exists(tempProjectFile))
                        {
                            if (File.Exists(finalProjectFile)) File.Delete(finalProjectFile);
                            File.Move(tempProjectFile, finalProjectFile);
                        }

                        _cachedSubtitle = new Subtitle();
                        var linesFromFile = File.ReadAllLines(tempOutput, Encoding.UTF8).ToList();
                        srt.LoadSubtitle(_cachedSubtitle, linesFromFile, tempOutput);
                        
                        // Populate sequential cache, ensuring it matches original count exactly
                        for (int i = 0; i < _originalSubtitle.Paragraphs.Count; i++)
                        {
                            if (i < _cachedSubtitle.Paragraphs.Count)
                            {
                                _sequentialCache.Add(_cachedSubtitle.Paragraphs[i].Text);
                            }
                            else
                            {
                                // If LLM returned fewer lines, fallback to original or empty
                                _sequentialCache.Add(_originalSubtitle.Paragraphs[i].Text);
                            }
                        }

                        _lastSourceLanguage = sourceLanguageCode;
                        _lastTargetLanguage = targetLanguageCode;
                        _currentBatchIndex = 0;

                        // Cleanup temp files
                        try { if (File.Exists(tempInput)) File.Delete(tempInput); } catch { }
                        try { if (File.Exists(tempOutput)) File.Delete(tempOutput); } catch { }
                    }
                    else
                    {
                        Error = "Python script failed or was interrupted.";
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
    }
}
