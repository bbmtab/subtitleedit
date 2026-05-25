using Nikse.SubtitleEdit.Core.Settings;
using System;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.UI.Forms.Translate
{
    public partial class LlmSubtransSettings : Form
    {
        public LlmSubtransSettings()
        {
            InitializeComponent();

            var ts = Configuration.Settings.Tools;
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
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var ts = Configuration.Settings.Tools;
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
    }
}
