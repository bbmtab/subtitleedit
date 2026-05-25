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
        public int MaxCharacters => 2000;

        private Subtitle _cachedSubtitle;
        private string _lastSourceLanguage;
        private string _lastTargetLanguage;
        private readonly Subtitle _originalSubtitle;
        public string FileName { get; set; }

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private int _currentBatchIndex = 0;
        private Dictionary<string, string> _translationMap;
        private List<string> _sequentialCache;

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
            _translationMap = null;
            _sequentialCache = null;
        }

        public List<TranslationPair> GetSupportedSourceLanguages()
        {
            return ChatGptTranslate.ListLanguages();
        }

        public List<TranslationPair> GetSupportedTargetLanguages()
        {
            return ChatGptTranslate.ListLanguages();
        }

        private string NormalizeForMatch(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        public async Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_cachedSubtitle == null || _lastSourceLanguage != sourceLanguageCode || _lastTargetLanguage != targetLanguageCode)
                {
                    await TranslateWholeSubtitle(sourceLanguageCode, targetLanguageCode, cancellationToken);
                }

                if (_cachedSubtitle == null || _translationMap == null)
                {
                    return "Error: " + (Error ?? "Translation failed.");
                }

                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var translatedLines = new List<string>();
                var logFile = Path.Combine(!string.IsNullOrEmpty(FileName) ? Path.GetDirectoryName(FileName) : Path.GetTempPath(), "llm_subtrans_log.txt");

                foreach (var line in lines)
                {
                    var searchLine = line.Trim();
                    if (string.IsNullOrEmpty(searchLine))
                    {
                        translatedLines.Add(string.Empty);
                        continue;
                    }

                    // 1. Try dictionary lookup (unformatted text match)
                    if (_translationMap.TryGetValue(searchLine, out string translated))
                    {
                        translatedLines.Add(translated);
                        continue;
                    }

                    // 2. Try normalized lookup
                    var normalized = NormalizeForMatch(searchLine);
                    if (_translationMap.TryGetValue(normalized, out translated))
                    {
                        translatedLines.Add(translated);
                        continue;
                    }

                    // 3. Fallback: Sequential access (the most reliable for bulk runs)
                    if (_currentBatchIndex < _sequentialCache.Count)
                    {
                        translatedLines.Add(_sequentialCache[_currentBatchIndex]);
                        try { File.AppendAllText(logFile, $"\nSYNC: Using index fallback for '{searchLine}' -> '{_sequentialCache[_currentBatchIndex]}'\n"); } catch { }
                        _currentBatchIndex++;
                    }
                    else
                    {
                        translatedLines.Add(searchLine);
                        try { File.AppendAllText(logFile, $"\nSYNC FAIL: Out of cache for '{searchLine}'\n"); } catch { }
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

            _translationMap = new Dictionary<string, string>();
            _sequentialCache = new List<string>();
            _currentBatchIndex = 0;

            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var tempInput = Path.Combine(Path.GetTempPath(), $"se_llm_in_{uniqueId}.srt");
            var tempOutput = Path.Combine(Path.GetTempPath(), $"se_llm_out_{uniqueId}.srt");
            var logFile = Path.Combine(!string.IsNullOrEmpty(FileName) ? Path.GetDirectoryName(FileName) : Path.GetTempPath(), "llm_subtrans_log.txt");

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

            var subtitleFolder = !string.IsNullOrEmpty(FileName) ? Path.GetDirectoryName(FileName) : string.Empty;
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
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDir
            };

            var log = new StringBuilder();
            log.AppendLine("--- LLM Subtrans Log ---");
            log.AppendLine("Time: " + DateTime.Now.ToString());
            log.AppendLine("Command: " + pythonPath + " " + args);

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.ErrorDataReceived += (s, e) => { if (e.Data != null) log.AppendLine("ERR: " + e.Data); };
                    process.OutputDataReceived += (s, e) => { if (e.Data != null) log.AppendLine("OUT: " + e.Data); };

                    if (!process.Start())
                    {
                        Error = "Failed to start Python.";
                        return;
                    }

                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    await Task.Run(() => process.WaitForExit(), cancellationToken);
                    log.AppendLine("Exit Code: " + process.ExitCode);
                    File.WriteAllText(logFile, log.ToString());

                    if (process.ExitCode == 0 && File.Exists(tempOutput))
                    {
                        _cachedSubtitle = new Subtitle();
                        srt.LoadSubtitle(_cachedSubtitle, null, tempOutput, true);
                        
                        var formatting = new Formatting();
                        for (int i = 0; i < _originalSubtitle.Paragraphs.Count; i++)
                        {
                            if (i < _cachedSubtitle.Paragraphs.Count)
                            {
                                var originalText = _originalSubtitle.Paragraphs[i].Text;
                                var translatedText = _cachedSubtitle.Paragraphs[i].Text;
                                
                                var unformattedOrig = formatting.SetTagsAndReturnTrimmed(originalText, sourceLanguageCode).Trim();
                                var unformattedTrans = formatting.SetTagsAndReturnTrimmed(translatedText, targetLanguageCode).Trim();
                                
                                if (!string.IsNullOrEmpty(unformattedOrig) && !_translationMap.ContainsKey(unformattedOrig))
                                    _translationMap.Add(unformattedOrig, unformattedTrans);
                                
                                var normalizedOrig = NormalizeForMatch(unformattedOrig);
                                if (!string.IsNullOrEmpty(normalizedOrig) && !_translationMap.ContainsKey(normalizedOrig))
                                    _translationMap.Add(normalizedOrig, unformattedTrans);

                                _sequentialCache.Add(unformattedTrans);
                            }
                        }

                        _lastSourceLanguage = sourceLanguageCode;
                        _lastTargetLanguage = targetLanguageCode;
                        _currentBatchIndex = 0;
                    }
                    else
                    {
                        Error = "Python script failed. See log: " + logFile;
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
