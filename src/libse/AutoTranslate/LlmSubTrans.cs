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

                if (_cachedSubtitle == null || _sequentialCache == null)
                {
                    _isFailed = true;
                    return "Error: " + (Error ?? "Translation failed.");
                }

                // We ignore the 'text' passed by SE and rely on our pre-mapped sequential cache.
                // SE's AutoTranslate form iterates paragraph by paragraph in the same order
                // as our _originalSubtitle passed in the constructor.
                if (_currentBatchIndex < _sequentialCache.Count)
                {
                    var result = _sequentialCache[_currentBatchIndex];
                    _currentBatchIndex++;
                    return result;
                }

                return text;
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
            
            // Temporary files in temp folder, but project file will be moved to source folder
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var tempInput = Path.Combine(Path.GetTempPath(), $"se_llm_in_{uniqueId}.srt");
            var tempOutput = Path.Combine(Path.GetTempPath(), $"se_llm_out_{uniqueId}.srt");
            var tempProjectFile = Path.Combine(Path.GetTempPath(), $"se_llm_in_{uniqueId}.subtrans");
            var finalProjectFile = Path.Combine(subtitleFolder, $"{sourceBaseName}.subtrans");
            var logFile = Path.Combine(subtitleFolder, "llm_subtrans_log.txt");

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
                UseShellExecute = true, 
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
                        // Sync project file to subtitle folder
                        if (File.Exists(tempProjectFile))
                        {
                            try 
                            { 
                                if (File.Exists(finalProjectFile)) File.Delete(finalProjectFile);
                                File.Move(tempProjectFile, finalProjectFile); 
                            } catch { }
                        }

                        _cachedSubtitle = new Subtitle();
                        var linesFromFile = File.ReadAllLines(tempOutput, Encoding.UTF8).ToList();
                        srt.LoadSubtitle(_cachedSubtitle, linesFromFile, tempOutput);
                        
                        // Robust Time-Based Mapping
                        // We map each original paragraph to the closest result based on time
                        foreach (var original in _originalSubtitle.Paragraphs)
                        {
                            var match = GetBestTimeMatch(original, _cachedSubtitle.Paragraphs);
                            if (match != null)
                            {
                                _sequentialCache.Add(match.Text);
                            }
                            else
                            {
                                _sequentialCache.Add(original.Text); // Fallback to original
                            }
                        }

                        _lastSourceLanguage = sourceLanguageCode;
                        _lastTargetLanguage = targetLanguageCode;
                        _currentBatchIndex = 0;

                        // Cleanup
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

        private Paragraph GetBestTimeMatch(Paragraph target, List<Paragraph> candidates)
        {
            Paragraph bestMatch = null;
            double bestOverlap = -1;

            foreach (var p in candidates)
            {
                // Calculate overlap in milliseconds
                double start = Math.Max(target.StartTime.TotalMilliseconds, p.StartTime.TotalMilliseconds);
                double end = Math.Min(target.EndTime.TotalMilliseconds, p.EndTime.TotalMilliseconds);
                double overlap = end - start;

                if (overlap > bestOverlap)
                {
                    bestOverlap = overlap;
                    bestMatch = p;
                }
            }

            // If overlap is too small (e.g. less than 1ms), consider if the target is exactly between candidates
            if (bestOverlap <= 0)
            {
                // Just find the one with the closest start time
                double minDiff = double.MaxValue;
                foreach (var p in candidates)
                {
                    double diff = Math.Abs(target.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        bestMatch = p;
                    }
                }
            }

            return bestMatch;
        }
    }
}
