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
        private Dictionary<int, string> _indexToTranslation; // Map original index to translation from project file

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
            _indexToTranslation = new Dictionary<int, string>();
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
                    // Use project file index mapping if available (1-based index in project file)
                    int projectIndex = foundIndex + 1; // Convert to 1-based
                    if (_indexToTranslation != null && _indexToTranslation.TryGetValue(projectIndex, out var translation))
                    {
                        _currentBatchIndex = foundIndex + 1;
                        return translation;
                    }

                    // Fallback: Map original index to translation index
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

        private bool IsSubset(Subtitle originalSubtitle, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return false;

            try
            {
                var fullSubtitle = new Subtitle();
                fullSubtitle.LoadSubtitle(fileName, out _, null);

                // If counts are different, it's definitely a subset or different file
                if (originalSubtitle.Paragraphs.Count != fullSubtitle.Paragraphs.Count)
                    return true;

                // If any text or times differ, treat as subset
                for (int i = 0; i < originalSubtitle.Paragraphs.Count; i++)
                {
                    if (originalSubtitle.Paragraphs[i].Text != fullSubtitle.Paragraphs[i].Text ||
                        originalSubtitle.Paragraphs[i].StartTime.TotalMilliseconds != fullSubtitle.Paragraphs[i].StartTime.TotalMilliseconds)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // On any error, assume it's safer to treat as a subset/different
                return true;
            }

            return false;
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
            _indexToTranslation = new Dictionary<int, string>();

            var isSubset = IsSubset(_originalSubtitle, FileName);

            var subtitleFolder = !string.IsNullOrEmpty(FileName) ? Path.GetDirectoryName(FileName) : Path.GetTempPath();
            var sourceBaseName = !string.IsNullOrEmpty(FileName) ? Path.GetFileNameWithoutExtension(FileName) : "new_subtitle";

            // Use original file directly if FileName is available and NOT a subset, otherwise create temp input
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var tempInput = !isSubset && !string.IsNullOrEmpty(FileName) && File.Exists(FileName) ? FileName : Path.Combine(Path.GetTempPath(), $"se_llm_in_{uniqueId}.srt");
            var tempOutput = Path.Combine(Path.GetTempPath(), $"se_llm_out_{uniqueId}.srt");
            var tempProjectFile = Path.Combine(Path.GetTempPath(), $"se_llm_in_{uniqueId}.subtrans");
            var finalProjectFile = Path.Combine(subtitleFolder, $"{sourceBaseName}.subtrans");
            var logFile = Path.Combine(subtitleFolder, "llm_subtrans_log.txt");

            if (File.Exists(tempOutput)) File.Delete(tempOutput);

            var srt = new SubRip();

            // Write temp input only if original file not available
            if (!tempInput.Equals(FileName, StringComparison.OrdinalIgnoreCase))
            {
                File.WriteAllText(tempInput, srt.ToText(_originalSubtitle, string.Empty), new UTF8Encoding(false));
            }

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

            var namesFile = Configuration.Settings.Tools.LlmSubtransNamesFile;
            if (string.IsNullOrEmpty(namesFile) && !string.IsNullOrEmpty(subtitleFolder))
            {
                var defaultNames = Path.Combine(subtitleFolder, "names.txt");
                if (File.Exists(defaultNames)) namesFile = defaultNames;
            }

            var termFile = Configuration.Settings.Tools.LlmSubtransTerminologyFile;
            if (string.IsNullOrEmpty(termFile) && !string.IsNullOrEmpty(subtitleFolder))
            {
                var defaultTerm = Path.Combine(subtitleFolder, "term.txt");
                if (File.Exists(defaultTerm)) termFile = defaultTerm;
            }

            var workingDir = Path.GetDirectoryName(scriptPath);
            if (workingDir != null && workingDir.EndsWith("scripts", StringComparison.OrdinalIgnoreCase)) workingDir = Path.GetDirectoryName(workingDir);

            // Helper to escape paths for PowerShell (single quotes need to be doubled)
            string EscapeForPowerShell(string path) => path?.Replace("'", "''");

            // Use powershell with Tee-Object to show live output AND log to file
            // On error, keep window open so user can see the error message
            var projectArg = Configuration.Settings.Tools.LlmSubtransProject ? " --project" : "";
            var psCommand = $"& {{ try {{ & '{pythonPath}' '{EscapeForPowerShell(scriptPath)}' '{EscapeForPowerShell(tempInput)}'{projectArg}";
            psCommand += $" -l '{targetLanguageCode}'";
            psCommand += $" -o '{EscapeForPowerShell(tempOutput)}'";
            if (!string.IsNullOrEmpty(baseUrl)) psCommand += $" -s '{EscapeForPowerShell(baseUrl)}'";
            if (!string.IsNullOrEmpty(endpoint)) psCommand += $" -e '{endpoint}'";
            psCommand += $" -k '{Configuration.Settings.Tools.LlmSubtransApiKey}'";
            psCommand += $" -m '{Configuration.Settings.Tools.LlmSubtransModel}'";
            psCommand += $" --temperature {Configuration.Settings.Tools.LlmSubtransTemperature.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            psCommand += $" --ratelimit {Configuration.Settings.Tools.LlmSubtransRateLimit}";
            psCommand += $" --minbatchsize {Configuration.Settings.Tools.LlmSubtransMinBatchSize}";
            psCommand += $" --maxbatchsize {Configuration.Settings.Tools.LlmSubtransMaxBatchSize}";
            psCommand += $" --maxretries {Configuration.Settings.Tools.LlmSubtransMaxRetries}";
            psCommand += $" --backofftime {Configuration.Settings.Tools.LlmSubtransBackoffTime}";
            psCommand += $" --scenethreshold {Configuration.Settings.Tools.LlmSubtransSceneThreshold}";
            psCommand += $" --batchthreshold {Configuration.Settings.Tools.LlmSubtransBatchThreshold}";
            psCommand += $" --maxsummaries {Configuration.Settings.Tools.LlmSubtransMaxSummaries}";
            if (Configuration.Settings.Tools.LlmSubtransChat || (endpoint != null && endpoint.Contains("chat"))) psCommand += " --chat";
            if (Configuration.Settings.Tools.LlmSubtransPostProcess) psCommand += " --postprocess";
            if (Configuration.Settings.Tools.LlmSubtransSystemMessages) psCommand += " --systemmessages";
            if (Configuration.Settings.Tools.LlmSubtransAuto) psCommand += " --auto";
            if (Configuration.Settings.Tools.LlmSubtransIncludeOriginal) psCommand += " --includeoriginal";
            if (Configuration.Settings.Tools.LlmSubtransAddRtlMarkers) psCommand += " --addrtlmarkers";
            if (Configuration.Settings.Tools.LlmSubtransBuildTerminologyMap) psCommand += " --build-terminology-map";
            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.LlmSubtransInstructionFile)) psCommand += $" --instructionfile '{EscapeForPowerShell(Configuration.Settings.Tools.LlmSubtransInstructionFile)}'";
            if (!string.IsNullOrEmpty(namesFile)) psCommand += $" --names '{EscapeForPowerShell(namesFile)}'";
            if (!string.IsNullOrEmpty(termFile)) psCommand += $" --terminology-file '{EscapeForPowerShell(termFile)}'";

            psCommand += $" | Tee-Object -FilePath '{EscapeForPowerShell(logFile)}' }} catch {{ Write-Error $_; Read-Host 'Press Enter to close' }} if ($LASTEXITCODE -ne 0) {{ Read-Host 'Script failed - Press Enter to close' }} }}";
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

                        // First, try to load from project file for better sync accuracy
                        bool projectLoaded = false;
                        if (File.Exists(tempProjectFile))
                        {
                            projectLoaded = LoadTranslationsFromProjectFile(tempProjectFile);
                        }

                        // If project file failed or incomplete, fall back to SRT
                        if (!projectLoaded && File.Exists(tempOutput))
                        {
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

                            // Build sequential cache from SRT
                            _sequentialCache?.Clear();
                            if (_sequentialCache == null) _sequentialCache = new List<string>();
                            foreach (var p in _cachedSubtitle.Paragraphs)
                            {
                                _sequentialCache.Add(p.Text);
                            }
                        }

                        // Handle project file synchronization - move to source folder (only if NOT a subset to avoid overwriting full project file)
                        if (!isSubset && File.Exists(tempProjectFile))
                        {
                            try
                            {
                                if (File.Exists(finalProjectFile)) File.Delete(finalProjectFile);
                                File.Move(tempProjectFile, finalProjectFile);
                            }
                            catch { }
                        }

                        _lastSourceLanguage = sourceLanguageCode;
                        _lastTargetLanguage = targetLanguageCode;
                        _currentBatchIndex = 0;

                        // Cleanup temp files (but not the original input file)
                        try { if (File.Exists(tempInput) && !tempInput.Equals(FileName, StringComparison.OrdinalIgnoreCase)) File.Delete(tempInput); } catch { }
                        try { if (File.Exists(tempOutput)) File.Delete(tempOutput); } catch { }
                        try { if (isSubset && File.Exists(tempProjectFile)) File.Delete(tempProjectFile); } catch { }
                    }
                    else
                    {
                        // Read log file for detailed error message
                        if (File.Exists(logFile))
                        {
                            var logContent = File.ReadAllText(logFile);
                            Error = $"Python script failed (exit code {process.ExitCode}). Check log: {logFile}\n\n{logContent}";
                        }
                        else
                        {
                            Error = $"Python script failed (exit code {process.ExitCode}). Log file not created: {logFile}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        /// <summary>
        /// Loads translations from the llm-subtrans .subtrans project file.
        /// Uses the index field to map translations to original subtitle indices.
        /// Prioritizes 'originals' array which has accurate timing (start/end).
        /// </summary>
        private bool LoadTranslationsFromProjectFile(string projectFilePath)
        {
            try
            {
                var jsonContent = File.ReadAllText(projectFilePath, Encoding.UTF8);

                _cachedSubtitle = new Subtitle();
                _sequentialCache = new List<string>();
                _indexToTranslation = new Dictionary<int, string>();

                var paragraphDict = new Dictionary<int, Paragraph>();

                // Read scenes array
                var scenes = Json.ReadArray(jsonContent, "scenes");
                foreach (var sceneJson in scenes)
                {
                    // Read batches from scene
                    var batches = Json.ReadArray(sceneJson, "batches");
                    foreach (var batchJson in batches)
                    {
                        // FIRST: Read from 'originals' array - has accurate timing (start/end)
                        var originals = Json.ReadArray(batchJson, "originals");
                        foreach (var itemJson in originals)
                        {
                            var index = Json.ReadTag(itemJson, "index");
                            var translation = Json.ReadTag(itemJson, "translation");
                            var start = Json.ReadTag(itemJson, "start");
                            var end = Json.ReadTag(itemJson, "end");

                            if (int.TryParse(index, out var idx) && !string.IsNullOrWhiteSpace(translation))
                            {
                                double startMs = 0, endMs = 0;
                                if (double.TryParse(start, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out startMs) &&
                                    double.TryParse(end, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out endMs))
                                {
                                    var newPara = new Paragraph(
                                        HtmlUtil.RemoveHtmlTags(translation, true),
                                        startMs,
                                        endMs
                                    );
                                    paragraphDict[idx] = newPara;
                                    _indexToTranslation[idx] = HtmlUtil.RemoveHtmlTags(translation, true);
                                }
                            }
                        }

                        // SECOND: Fill gaps from 'translated' array (uses original timing - less accurate)
                        var translated = Json.ReadArray(batchJson, "translated");
                        foreach (var itemJson in translated)
                        {
                            var index = Json.ReadTag(itemJson, "index");
                            var content = Json.ReadTag(itemJson, "content");

                            if (int.TryParse(index, out var idx) && !string.IsNullOrWhiteSpace(content))
                            {
                                // Only use if we don't have it from originals array (which has better timing)
                                if (!_indexToTranslation.ContainsKey(idx))
                                {
                                    // Get timing from original subtitle (index is 1-based)
                                    var original = _originalSubtitle.GetParagraphOrDefault(idx - 1);
                                    if (original != null)
                                    {
                                        var newPara = new Paragraph(
                                            HtmlUtil.RemoveHtmlTags(content, true),
                                            original.StartTime.TotalMilliseconds,
                                            original.EndTime.TotalMilliseconds
                                        );
                                        paragraphDict[idx] = newPara;
                                        _indexToTranslation[idx] = HtmlUtil.RemoveHtmlTags(content, true);
                                    }
                                }
                            }
                        }
                    }
                }

                // Build subtitle from dictionary (preserves order)
                var sortedIndices = paragraphDict.Keys.OrderBy(k => k).ToList();
                foreach (var idx in sortedIndices)
                {
                    _cachedSubtitle.Paragraphs.Add(paragraphDict[idx]);
                    _sequentialCache.Add(paragraphDict[idx].Text);
                }

                _cachedSubtitle.Renumber();
                return _sequentialCache.Count > 0;
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "Failed to load project file: " + projectFilePath);
                return false;
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
