using System;
using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public partial class LlmSubtransSettings : Form
    {
        public LlmSubtransSettings()
        {
            InitializeComponent();

            var ts = Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools;
            textBoxPythonPath.Text = ts.LlmSubtransPythonPath;
            textBoxScriptPath.Text = ts.LlmSubtransScriptPath;
            textBoxUrl.Text = ts.LlmSubtransUrl;
            textBoxApiKey.Text = ts.LlmSubtransApiKey;
            textBoxModel.Text = ts.LlmSubtransModel;
            textBoxEndpoint.Text = ts.LlmSubtransEndpoint;
            numericUpDownTemperature.Value = (decimal)ts.LlmSubtransTemperature;
            numericUpDownRateLimit.Value = ts.LlmSubtransRateLimit;
            numericUpDownMinBatch.Value = ts.LlmSubtransMinBatchSize;
            numericUpDownMaxBatch.Value = ts.LlmSubtransMaxBatchSize;
            numericUpDownMaxRetries.Value = ts.LlmSubtransMaxRetries;
            numericUpDownBackoff.Value = ts.LlmSubtransBackoffTime;
            numericUpDownSceneThreshold.Value = ts.LlmSubtransSceneThreshold;
            numericUpDownBatchThreshold.Value = ts.LlmSubtransBatchThreshold;
            numericUpDownMaxSummaries.Value = ts.LlmSubtransMaxSummaries;
            textBoxInstructionFile.Text = ts.LlmSubtransInstructionFile;
            textBoxNamesFile.Text = ts.LlmSubtransNamesFile;
            textBoxTerminologyFile.Text = ts.LlmSubtransTerminologyFile;
            textBoxSubstitution.Text = ts.LlmSubtransSubstitution;
            checkBoxProject.Checked = ts.LlmSubtransProject;
            checkBoxBuildTerminologyMap.Checked = ts.LlmSubtransBuildTerminologyMap;
            checkBoxPostProcess.Checked = ts.LlmSubtransPostProcess;
            checkBoxChat.Checked = ts.LlmSubtransChat;
            checkBoxSystemMessages.Checked = ts.LlmSubtransSystemMessages;
            checkBoxAuto.Checked = ts.LlmSubtransAuto;
            checkBoxIncludeOriginal.Checked = ts.LlmSubtransIncludeOriginal;
            checkBoxAddRtlMarkers.Checked = ts.LlmSubtransAddRtlMarkers;

            if (string.IsNullOrEmpty(textBoxPythonPath.Text))
            {
                AutoDetectPython();
            }
        }

        private void AutoDetectPython()
        {
            string[] commonPaths = {
                @"C:\Python312\python.exe",
                @"C:\Python311\python.exe",
                @"C:\Python310\python.exe",
                @"C:\Program Files\Python312\python.exe",
                @"C:\Program Files\Python311\python.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Python\Python312\python.exe",
                @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Python\Python311\python.exe",
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                {
                    textBoxPythonPath.Text = path;
                    break;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var ts = Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools;
            ts.LlmSubtransPythonPath = textBoxPythonPath.Text;
            ts.LlmSubtransScriptPath = textBoxScriptPath.Text;
            ts.LlmSubtransUrl = textBoxUrl.Text;
            ts.LlmSubtransApiKey = textBoxApiKey.Text;
            ts.LlmSubtransModel = textBoxModel.Text;
            ts.LlmSubtransEndpoint = textBoxEndpoint.Text;
            ts.LlmSubtransTemperature = (double)numericUpDownTemperature.Value;
            ts.LlmSubtransRateLimit = (int)numericUpDownRateLimit.Value;
            ts.LlmSubtransMinBatchSize = (int)numericUpDownMinBatch.Value;
            ts.LlmSubtransMaxBatchSize = (int)numericUpDownMaxBatch.Value;
            ts.LlmSubtransMaxRetries = (int)numericUpDownMaxRetries.Value;
            ts.LlmSubtransBackoffTime = (int)numericUpDownBackoff.Value;
            ts.LlmSubtransSceneThreshold = (int)numericUpDownSceneThreshold.Value;
            ts.LlmSubtransBatchThreshold = (int)numericUpDownBatchThreshold.Value;
            ts.LlmSubtransMaxSummaries = (int)numericUpDownMaxSummaries.Value;
            ts.LlmSubtransInstructionFile = textBoxInstructionFile.Text;
            ts.LlmSubtransNamesFile = textBoxNamesFile.Text;
            ts.LlmSubtransTerminologyFile = textBoxTerminologyFile.Text;
            ts.LlmSubtransSubstitution = textBoxSubstitution.Text;
            ts.LlmSubtransProject = checkBoxProject.Checked;
            ts.LlmSubtransBuildTerminologyMap = checkBoxBuildTerminologyMap.Checked;
            ts.LlmSubtransPostProcess = checkBoxPostProcess.Checked;
            ts.LlmSubtransChat = checkBoxChat.Checked;
            ts.LlmSubtransSystemMessages = checkBoxSystemMessages.Checked;
            ts.LlmSubtransAuto = checkBoxAuto.Checked;
            ts.LlmSubtransIncludeOriginal = checkBoxIncludeOriginal.Checked;
            ts.LlmSubtransAddRtlMarkers = checkBoxAddRtlMarkers.Checked;

            Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Save();
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonBrowsePython_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxPythonPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void buttonBrowseScript_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Python scripts (*.py)|*.py|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxScriptPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void buttonBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.Description = "Select llm-subtrans clone folder";
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    string folder = folderBrowser.SelectedPath;
                    string script = Path.Combine(folder, "scripts", "llm-subtrans.py");
                    string python = Path.Combine(folder, "envsubtrans", "Scripts", "python.exe");

                    if (File.Exists(script)) textBoxScriptPath.Text = script;
                    if (File.Exists(python)) textBoxPythonPath.Text = python;
                }
            }
        }

        private void buttonBrowseInstruction_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxInstructionFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void buttonBrowseNames_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxNamesFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void buttonBrowseTerminology_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxTerminologyFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void buttonResetDefaults_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all LLM Subtrans settings to defaults?", "Reset Defaults", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var ts = new Nikse.SubtitleEdit.Core.Settings.ToolsSettings(); // Temporary instance to get hardcoded defaults
                textBoxUrl.Text = ts.LlmSubtransUrl;
                textBoxEndpoint.Text = ts.LlmSubtransEndpoint;
                textBoxModel.Text = ts.LlmSubtransModel;
                numericUpDownTemperature.Value = (decimal)ts.LlmSubtransTemperature;
                numericUpDownRateLimit.Value = ts.LlmSubtransRateLimit;
                numericUpDownMinBatch.Value = ts.LlmSubtransMinBatchSize;
                numericUpDownMaxBatch.Value = ts.LlmSubtransMaxBatchSize;
                numericUpDownMaxRetries.Value = ts.LlmSubtransMaxRetries;
                numericUpDownBackoff.Value = ts.LlmSubtransBackoffTime;
                numericUpDownSceneThreshold.Value = ts.LlmSubtransSceneThreshold;
                numericUpDownBatchThreshold.Value = ts.LlmSubtransBatchThreshold;
                numericUpDownMaxSummaries.Value = ts.LlmSubtransMaxSummaries;
                checkBoxProject.Checked = ts.LlmSubtransProject;
                checkBoxChat.Checked = ts.LlmSubtransChat;
                checkBoxPostProcess.Checked = ts.LlmSubtransPostProcess;
                checkBoxAuto.Checked = ts.LlmSubtransAuto;
                checkBoxSystemMessages.Checked = ts.LlmSubtransSystemMessages;
                checkBoxBuildTerminologyMap.Checked = ts.LlmSubtransBuildTerminologyMap;
                checkBoxIncludeOriginal.Checked = ts.LlmSubtransIncludeOriginal;
                checkBoxAddRtlMarkers.Checked = ts.LlmSubtransAddRtlMarkers;
                textBoxInstructionFile.Text = string.Empty;
                textBoxNamesFile.Text = string.Empty;
                textBoxTerminologyFile.Text = string.Empty;
                textBoxSubstitution.Text = string.Empty;
            }
        }
    }
}
