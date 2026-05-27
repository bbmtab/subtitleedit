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
        private HashSet<int> _usedTranslationIndices; // Track used translations to prevent repeats

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
            _usedTranslationIndices = new HashSet<int>();
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

                // Find the original paragraph index for the requested text
                var foundIndex = FindOriginalIndex(text, _currentBatchIndex);

                if (foundIndex >= 0)
                {
                    // Map original index to translation index
                    // The llm-subtrans script preserves line count in most cases
                    var translationIndex = MapOriginalToTranslation(foundIndex);

                    if (translationIndex >= 0 && translationIndex < _sequentialCache.Count)
                    {
                        // Prevent repeat: if this translation was already used, try next
                        if (_usedTranslationIndices.Contains(translationIndex))
                        {
                            // Find next unused translation
                            for (int i = translationIndex + 1; i < _sequentialCache.Count; i++)
                            {
                                if (!_usedTranslationIndices.Contains(i))
                                {
                                    translationIndex = i;
                                    break;
                                }
                            }

                            // If all subsequent are used, search from beginning
                            if (_usedTranslationIndices.Contains(translationIndex))
                            {
                                for (int i = 0; i < translationIndex; i++)
                                {
                                    if (!_usedTranslationIndices.Contains(i))
                                    {
                                        translationIndex = i;
                                        break;
                                    }
                                }
                            }
                        }

                        _usedTranslationIndices.Add(translationIndex);
                        _currentBatchIndex = foundIndex + 1;
                        return _sequentialCache[translationIndex];
                    }
                }

                // Fallback: use sequential cache with current batch index
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

        /// <summary>
        /// Maps original subtitle index to translation index.
        /// Handles cases where line counts differ due to merges/splits.
        /// </summary>
        private int MapOriginalToTranslation(int originalIndex)
        {
            if (_originalSubtitle == null || _cachedSubtitle == null)
                return originalIndex;

            var originalCount = _originalSubtitle.Paragraphs.Count;
            var translationCount = _cachedSubtitle.Paragraphs.Count;

            // Same count - direct mapping
            if (originalCount == translationCount)
                return originalIndex;

            // More translations than originals - likely splits, use ratio
            if (translationCount > originalCount)
            {
                return (int)Math.Round((double)originalIndex * translationCount / originalCount);
            }

            // Fewer translations than originals - likely merges, use ratio
            return Math.Min((int)Math.Round((double)originalIndex * translationCount / originalCount), translationCount - 1);
        }

        private int FindOriginalIndex(string text, int startIndex)
        {
            if (_originalSubtitle == null) return -1;

            var f = new Formatting();
            var normalizedText = f.SetTagsAndReturnTrimmed(text, "");

            // Calculate the "gap" from the previous lookup to detect potential overlaps
            // If we're being asked for text far ahead, there might be skipped lines
            var searchAheadThreshold = 10;

            // Search ahead first (most common case: sequential access)
            for (int i = startIndex; i < _originalSubtitle.Paragraphs.Count; i++)
            {
                if (f.SetTagsAndReturnTrimmed(_originalSubtitle.Paragraphs[i].Text, "") == normalizedText)
                    return i;
            }

            // Search from beginning if not found (wrap-around for repeated lookups)
            for (int i = 0; i < startIndex && i < startIndex - searchAheadThreshold; i++)
            {
                if (f.SetTagsAndReturnTrimmed(_originalSubtitle.Paragraphs[i].Text, "") == normalizedText)
                    return i;
            }

            // Fuzzy match: try matching without formatting tags on both sides
            var textWithoutTags = HtmlUtil.RemoveHtmlTags(text, true);
            for (int i = Math.Max(0, startIndex - 5); i < Math.Min(startIndex + 5, _originalSubtitle.Paragraphs.Count); i++)
            {
                var paraTextWithoutTags = HtmlUtil.RemoveHtmlTags(_originalSubtitle.Paragraphs[i].Text, true);
                if (textWithoutTags.Equals(paraTextWithoutTags, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
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
            args.Append($"'{scriptPath}' ");
            args.Append($"'{tempInput}' ");
            if (Configuration.Settings.Tools.LlmSubtransProject) args.Append("--project ");
            args.Append($"-l '{targetLanguageCode}' ");
            args.Append($"-o '{tempOutput}' ");
            if (!string.IsNullOrEmpty(baseUrl)) args.Append($"-s '{baseUrl}' ");
            if (!string.IsNullOrEmpty(endpoint)) args.Append($"-e '/{endpoint}' ");
            args.Append($"-k '{Configuration.Settings.Tools.LlmSubtransApiKey}' ");
            args.Append($"-m '{Configuration.Settings.Tools.LlmSubtransModel}' ");
            args.Append($"--temperature {Configuration.Settings.Tools.LlmSubtransTemperature.ToString(System.Globalization.CultureInfo.InvariantCulture)} ");
            args.Append($"--ratelimit {Configuration.Settings.Tools.LlmSubtransRateLimit} ");
            args.Append($"--minbatchsize {Configuration.Settings.Tools.LlmSubtransMinBatchSize} ");
            args.Append($"--maxbatchsize {Configuration.Settings.Tools.LlmSubtransMaxBatchSize} ");
            args.Append($"--maxretries {Configuration.Settings.Tools.LlmSubtransMaxRetries} ");
            args.Append($"--backofftime {Configuration.Settings.Tools.LlmSubtransBackoffTime} ");
            args.Append($"--scenethreshold {Configuration.Settings.Tools.LlmSubtransSceneThreshold} ");
            args.Append($"--batchthreshold {Configuration.Settings.Tools.LlmSubtransBatchThreshold} ");
            args.Append($"--maxsummaries {Configuration.Settings.Tools.LlmSubtransMaxSummaries} ");
            if (Configuration.Settings.Tools.LlmSubtransChat || (endpoint != null && endpoint.Contains("chat"))) args.Append("--chat ");
            if (Configuration.Settings.Tools.LlmSubtransPostProcess) args.Append("--postprocess ");
            if (Configuration.Settings.Tools.LlmSubtransSystemMessages) args.Append("--systemmessages ");
            if (Configuration.Settings.Tools.LlmSubtransAuto) args.Append("--auto ");
            if (Configuration.Settings.Tools.LlmSubtransIncludeOriginal) args.Append("--includeoriginal ");
            if (Configuration.Settings.Tools.LlmSubtransAddRtlMarkers) args.Append("--addrtlmarkers ");
            if (Configuration.Settings.Tools.LlmSubtransBuildTerminologyMap) args.Append("--build-terminology-map ");

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.LlmSubtransInstructionFile))
                args.Append($"--instructionfile '{Configuration.Settings.Tools.LlmSubtransInstructionFile}' ");

            var namesFile = Configuration.Settings.Tools.LlmSubtransNamesFile;
            if (string.IsNullOrEmpty(namesFile) && !string.IsNullOrEmpty(subtitleFolder))
            {
                var defaultNames = Path.Combine(subtitleFolder, "names.txt");
                if (File.Exists(defaultNames)) namesFile = defaultNames;
            }
            if (!string.IsNullOrEmpty(namesFile)) args.Append($"--names '{namesFile}' ");

            var termFile = Configuration.Settings.Tools.LlmSubtransTerminologyFile;
            if (string.IsNullOrEmpty(termFile) && !string.IsNullOrEmpty(subtitleFolder))
            {
                var defaultTerm = Path.Combine(subtitleFolder, "term.txt");
                if (File.Exists(defaultTerm)) termFile = defaultTerm;
            }
            if (!string.IsNullOrEmpty(termFile)) args.Append($"--terminology-file '{termFile}' ");

            var workingDir = Path.GetDirectoryName(scriptPath);
            if (workingDir != null && workingDir.EndsWith("scripts", StringComparison.OrdinalIgnoreCase)) workingDir = Path.GetDirectoryName(workingDir);

            // Use powershell with Tee-Object to show live output AND log to file
            var psCommand = $"& {{ & '{pythonPath}' {args} 2>&1 | Tee-Object -FilePath '{logFile}' }}";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
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
                        Error = "Failed to start PowerShell.";
                        return;
                    }

                    await Task.Run(() => process.WaitForExit(), cancellationToken);

                    if (process.ExitCode == 0 && File.Exists(tempOutput))
                    {
                        _cachedSubtitle = new Subtitle();
                        var linesFromFile = File.ReadAllLines(tempOutput, Encoding.UTF8).ToList();

                        // Basic cleanup of LLM-induced SRT noise
                        var cleanLines = new List<string>();
                        bool srtStarted = false;
                        foreach (var line in linesFromFile)
                        {
                            if (!srtStarted && !System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+$"))
                                continue;
                            srtStarted = true;
                            cleanLines.Add(line);
                        }

                        srt.LoadSubtitle(_cachedSubtitle, cleanLines, tempOutput);

                        // Handle project file synchronization
                        if (File.Exists(tempProjectFile))
                        {
                            try
                            {
                                if (File.Exists(finalProjectFile)) File.Delete(finalProjectFile);
                                File.Move(tempProjectFile, finalProjectFile);
                            }
                            catch { }
                        }

                        // Populate sequential cache as fallback
                        _sequentialCache.Clear();
                        foreach (var p in _cachedSubtitle.Paragraphs)
                        {
                            _sequentialCache.Add(p.Text);
                        }

                        _lastSourceLanguage = sourceLanguageCode;
                        _lastTargetLanguage = targetLanguageCode;
                        _currentBatchIndex = 0;

                        // Cleanup
                        try { if (File.Exists(tempInput)) File.Delete(tempInput); } catch { }
                        try { if (File.Exists(tempOutput)) File.Delete(tempOutput); } catch { }
                    }                    else
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

            // First pass: Calculate overlap for each candidate
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

            // If no positive overlap found, use closest start time
            if (bestOverlap <= 0)
            {
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

        /// <summary>
        /// Finds the best matching paragraph by considering both time overlap AND text length similarity.
        /// This handles cases where multiple subtitles have overlapping times (e.g., place name explanations).
        /// </summary>
        private Paragraph GetBestTimeMatchWithLengthCheck(Paragraph target, List<Paragraph> candidates, int originalIndex)
        {
            // Get candidates that have significant time overlap
            var overlappingCandidates = new List<(Paragraph paragraph, double overlap)>();
            foreach (var p in candidates)
            {
                double start = Math.Max(target.StartTime.TotalMilliseconds, p.StartTime.TotalMilliseconds);
                double end = Math.Min(target.EndTime.TotalMilliseconds, p.EndTime.TotalMilliseconds);
                double overlap = end - start;

                if (overlap > 100) // Only consider overlaps > 100ms
                {
                    overlappingCandidates.Add((p, overlap));
                }
            }

            if (overlappingCandidates.Count == 0)
            {
                // Fall back to closest start time
                double minDiff = double.MaxValue;
                Paragraph fallbackMatch = null;
                foreach (var p in candidates)
                {
                    double diff = Math.Abs(target.StartTime.TotalMilliseconds - p.StartTime.TotalMilliseconds);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        fallbackMatch = p;
                    }
                }
                return fallbackMatch;
            }

            if (overlappingCandidates.Count == 1)
            {
                return overlappingCandidates[0].paragraph;
            }

            // Multiple overlapping candidates - use text length to disambiguate
            // Similar to how MergeAndSplitHelper uses character proportions
            var targetLength = target.Text.Length;
            Paragraph lengthBestMatch = null;
            double bestScore = -1;

            foreach (var (p, overlap) in overlappingCandidates)
            {
                // Score based on overlap duration and text length similarity
                double lengthRatio = Math.Min(targetLength, p.Text.Length) / (double)Math.Max(targetLength, p.Text.Length);
                double score = overlap * lengthRatio;

                // Bonus for sequential access (same index as last lookup)
                if (candidates.IndexOf(p) == originalIndex)
                {
                    score *= 1.5;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    lengthBestMatch = p;
                }
            }

            return lengthBestMatch;
        }
    }
}
