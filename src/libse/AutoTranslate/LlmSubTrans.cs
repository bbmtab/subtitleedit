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

        public LlmSubTrans()
        {
        }

        public LlmSubTrans(Subtitle originalSubtitle)
        {
            _originalSubtitle = originalSubtitle;
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
                return "Error: Translation failed or not started.";
            }

            // Try to find the matching paragraph by text
            // Note: SE often sends multiple paragraphs joined by newline
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
            var tempOutput = Path.Combine(Path.GetTempPath(), "llm_subtrans_in." + targetLanguageCode + ".srt");

            if (File.Exists(tempOutput)) File.Delete(tempOutput);

            var srt = new SubRip();
            File.WriteAllText(tempInput, srt.ToText(_originalSubtitle, string.Empty), Encoding.UTF8);

            var pythonPath = Configuration.Settings.Tools.LlmSubtransPythonPath;
            var scriptPath = Configuration.Settings.Tools.LlmSubtransScriptPath;

            if (!File.Exists(pythonPath))
            {
                Error = "Python not found at " + pythonPath;
                return;
            }

            if (!File.Exists(scriptPath))
            {
                Error = "Script not found at " + scriptPath;
                return;
            }

            var args = new StringBuilder();
            args.Append($"\"{scriptPath}\" ");
            args.Append($"\"{tempInput}\" ");
            args.Append("--project ");
            args.Append($"-l \"{targetLanguageCode}\" ");
            args.Append($"-s \"{Configuration.Settings.Tools.LlmSubtransUrl}\" ");
            args.Append($"-k \"{Configuration.Settings.Tools.LlmSubtransApiKey}\" ");
            args.Append($"-m \"{Configuration.Settings.Tools.LlmSubtransModel}\" ");
            args.Append($"--temperature \"{Configuration.Settings.Tools.LlmSubtransTemperature.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" ");
            args.Append($"--ratelimit \"{Configuration.Settings.Tools.LlmSubtransRateLimit}\" ");
            args.Append($"--minbatchsize \"{Configuration.Settings.Tools.LlmSubtransMinBatchSize}\" ");
            args.Append($"--maxbatchsize \"{Configuration.Settings.Tools.LlmSubtransMaxBatchSize}\" ");
            args.Append($"--maxretries \"{Configuration.Settings.Tools.LlmSubtransMaxRetries}\" ");
            args.Append($"--backofftime \"{Configuration.Settings.Tools.LlmSubtransBackoffTime}\" ");
            args.Append($"--scenethreshold \"{Configuration.Settings.Tools.LlmSubtransSceneThreshold}\" ");
            args.Append("--chat ");
            args.Append("--postprocess ");

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
                using (var process = Process.Start(processStartInfo))
                {
                    await Task.Run(() => process.WaitForExit(), cancellationToken);
                }

                if (File.Exists(tempOutput))
                {
                    _cachedSubtitle = new Subtitle();
                    srt.LoadSubtitle(_cachedSubtitle, null, tempOutput);
                    _lastSourceLanguage = sourceLanguageCode;
                    _lastTargetLanguage = targetLanguageCode;
                }
                else
                {
                    Error = "Output file not generated by script.";
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
    }
}
