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
            if (_cachedSubtitle == null || _lastSourceLanguage != sourceLanguageCode || _lastTargetLanguage != targetLanguageCode)
            {
                await TranslateWholeSubtitle(sourceLanguageCode, targetLanguageCode, cancellationToken);
            }

            if (_cachedSubtitle == null)
            {
                return "Error: " + (Error ?? "Translation failed or not started.");
            }

            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var translatedLines = new List<string>();

            foreach (var line in lines)
            {
                var p = FindParagraphByText(_originalSubtitle, line.Trim());
                if (p != null)
                {
                    var index = _originalSubtitle.Paragraphs.IndexOf(p);
                    if (index >= 0 && index < _cachedSubtitle.Paragraphs.Count)
                    {
                        translatedLines.Add(_cachedSubtitle.Paragraphs[index].Text);
                    }
                    else
                    {
                        translatedLines.Add("[Missing Index]");
                    }
                }
                else
                {
                    translatedLines.Add("[Line not found: " + line + "]");
                }
            }

            return string.Join(Environment.NewLine, translatedLines);
        }

        private Paragraph FindParagraphByText(Subtitle subtitle, string text)
        {
            if (subtitle == null) return null;
            return subtitle.Paragraphs.FirstOrDefault(p => p.Text.Trim() == text);
        }

        private async Task TranslateWholeSubtitle(string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            if (_originalSubtitle == null)
            {
                Error = "Original subtitle not available for batch translation.";
                return;
            }

            var tempInput = Path.Combine(Path.GetTempPath(), "llm_subtrans_in.srt");
            var tempOutput = Path.Combine(Path.GetTempPath(), "llm_subtrans_out.srt");

            if (File.Exists(tempOutput)) File.Delete(tempOutput);

            var srt = new SubRip();
            File.WriteAllText(tempInput, srt.ToText(_originalSubtitle, string.Empty), new UTF8Encoding(false));

            var pythonPath = Configuration.Settings.Tools.LlmSubtransPythonPath;
            var scriptPath = Configuration.Settings.Tools.LlmSubtransScriptPath;

            if (string.IsNullOrEmpty(pythonPath))
            {
                pythonPath = "python.exe"; // Fallback to PATH
            }

            if (!File.Exists(scriptPath))
            {
                Error = "Script not found at " + scriptPath + ". Please check settings.";
                return;
            }

            var subtitleFolder = string.Empty;
            if (!string.IsNullOrEmpty(FileName))
            {
                subtitleFolder = Path.GetDirectoryName(FileName);
            }

            var args = new StringBuilder();
            args.Append($"\"{scriptPath}\" ");
            args.Append($"\"{tempInput}\" ");
            if (Configuration.Settings.Tools.LlmSubtransProject) args.Append("--project ");
            args.Append($"-l \"{targetLanguageCode}\" ");
            args.Append($"-o \"{tempOutput}\" ");
            args.Append($"-s \"{Configuration.Settings.Tools.LlmSubtransUrl}\" ");
            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.LlmSubtransEndpoint)) args.Append($"-e \"{Configuration.Settings.Tools.LlmSubtransEndpoint}\" ");
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
            if (Configuration.Settings.Tools.LlmSubtransChat) args.Append("--chat ");
            if (Configuration.Settings.Tools.LlmSubtransPostProcess) args.Append("--postprocess ");
            if (Configuration.Settings.Tools.LlmSubtransSystemMessages) args.Append("--systemmessages ");
            if (Configuration.Settings.Tools.LlmSubtransAuto) args.Append("--auto ");
            if (Configuration.Settings.Tools.LlmSubtransIncludeOriginal) args.Append("--includeoriginal ");
            if (Configuration.Settings.Tools.LlmSubtransAddRtlMarkers) args.Append("--addrtlmarkers ");
            if (Configuration.Settings.Tools.LlmSubtransBuildTerminologyMap) args.Append("--build-terminology-map ");

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.LlmSubtransInstructionFile))
                args.Append($"--instructionfile \"{Configuration.Settings.Tools.LlmSubtransInstructionFile}\" ");

            // Default names.txt in subtitle folder
            var namesFile = Configuration.Settings.Tools.LlmSubtransNamesFile;
            if (string.IsNullOrEmpty(namesFile) && !string.IsNullOrEmpty(subtitleFolder))
            {
                var defaultNames = Path.Combine(subtitleFolder, "names.txt");
                if (File.Exists(defaultNames)) namesFile = defaultNames;
            }
            if (!string.IsNullOrEmpty(namesFile)) args.Append($"--names \"{namesFile}\" ");

            // Default term.txt in subtitle folder
            var termFile = Configuration.Settings.Tools.LlmSubtransTerminologyFile;
            if (string.IsNullOrEmpty(termFile) && !string.IsNullOrEmpty(subtitleFolder))
            {
                var defaultTerm = Path.Combine(subtitleFolder, "term.txt");
                if (File.Exists(defaultTerm)) termFile = defaultTerm;
            }
            if (!string.IsNullOrEmpty(termFile)) args.Append($"--terminology-file \"{termFile}\" ");

            if (!string.IsNullOrEmpty(Configuration.Settings.Tools.LlmSubtransSubstitution))
            {
                foreach (var sub in Configuration.Settings.Tools.LlmSubtransSubstitution.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(sub))
                        args.Append($"--substitution \"{sub.Trim()}\" ");
                }
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = args.ToString(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath)
            };

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    var stderr = new StringBuilder();
                    var stdout = new StringBuilder();
                    process.ErrorDataReceived += (s, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
                    process.OutputDataReceived += (s, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };

                    if (!process.Start())
                    {
                        Error = "Failed to start process: " + pythonPath;
                        return;
                    }

                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    await Task.Run(() => process.WaitForExit(), cancellationToken);

                    if (process.ExitCode != 0 || !File.Exists(tempOutput))
                    {
                        var msg = new StringBuilder();
                        if (process.ExitCode != 0) msg.AppendLine($"Python exited with code {process.ExitCode}");
                        if (!File.Exists(tempOutput)) msg.AppendLine("Output file not generated.");
                        
                        if (stderr.Length > 0) msg.AppendLine("Error Log:\n" + stderr.ToString());
                        if (stdout.Length > 0) msg.AppendLine("Output Log:\n" + stdout.ToString());
                        
                        msg.AppendLine("\nCommand run:");
                        msg.AppendLine($"{pythonPath} {args}");

                        Error = msg.ToString();
                        return;
                    }
                }

                if (File.Exists(tempOutput))
                {
                    _cachedSubtitle = new Subtitle();
                    srt.LoadSubtitle(_cachedSubtitle, null, tempOutput);
                    _lastSourceLanguage = sourceLanguageCode;
                    _lastTargetLanguage = targetLanguageCode;
                }
            }
            catch (Exception ex)
            {
                Error = "Exception: " + ex.Message + "\n" + ex.StackTrace;
            }
        }
    }
}
