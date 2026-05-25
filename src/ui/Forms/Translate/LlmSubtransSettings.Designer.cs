namespace Nikse.SubtitleEdit.Forms.Translate
{
    partial class LlmSubtransSettings
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.labelPythonPath = new System.Windows.Forms.Label();
            this.textBoxPythonPath = new System.Windows.Forms.TextBox();
            this.buttonBrowsePython = new System.Windows.Forms.Button();
            this.labelScriptPath = new System.Windows.Forms.Label();
            this.textBoxScriptPath = new System.Windows.Forms.TextBox();
            this.buttonBrowseScript = new System.Windows.Forms.Button();
            this.buttonBrowseFolder = new System.Windows.Forms.Button();
            this.labelUrl = new System.Windows.Forms.Label();
            this.textBoxUrl = new System.Windows.Forms.TextBox();
            this.labelApiKey = new System.Windows.Forms.Label();
            this.textBoxApiKey = new System.Windows.Forms.TextBox();
            this.labelModel = new System.Windows.Forms.Label();
            this.textBoxModel = new System.Windows.Forms.TextBox();
            this.labelEndpoint = new System.Windows.Forms.Label();
            this.textBoxEndpoint = new System.Windows.Forms.TextBox();
            this.labelTemperature = new System.Windows.Forms.Label();
            this.numericUpDownTemperature = new System.Windows.Forms.NumericUpDown();
            this.labelRateLimit = new System.Windows.Forms.Label();
            this.numericUpDownRateLimit = new System.Windows.Forms.NumericUpDown();
            this.labelMinBatch = new System.Windows.Forms.Label();
            this.numericUpDownMinBatch = new System.Windows.Forms.NumericUpDown();
            this.labelMaxBatch = new System.Windows.Forms.Label();
            this.numericUpDownMaxBatch = new System.Windows.Forms.NumericUpDown();
            this.labelMaxRetries = new System.Windows.Forms.Label();
            this.numericUpDownMaxRetries = new System.Windows.Forms.NumericUpDown();
            this.labelBackoff = new System.Windows.Forms.Label();
            this.numericUpDownBackoff = new System.Windows.Forms.NumericUpDown();
            this.labelSceneThreshold = new System.Windows.Forms.Label();
            this.numericUpDownSceneThreshold = new System.Windows.Forms.NumericUpDown();
            this.labelBatchThreshold = new System.Windows.Forms.Label();
            this.numericUpDownBatchThreshold = new System.Windows.Forms.NumericUpDown();
            this.labelMaxSummaries = new System.Windows.Forms.Label();
            this.numericUpDownMaxSummaries = new System.Windows.Forms.NumericUpDown();
            this.labelInstructionFile = new System.Windows.Forms.Label();
            this.textBoxInstructionFile = new System.Windows.Forms.TextBox();
            this.buttonBrowseInstruction = new System.Windows.Forms.Button();
            this.labelNamesFile = new System.Windows.Forms.Label();
            this.textBoxNamesFile = new System.Windows.Forms.TextBox();
            this.buttonBrowseNames = new System.Windows.Forms.Button();
            this.labelTerminologyFile = new System.Windows.Forms.Label();
            this.textBoxTerminologyFile = new System.Windows.Forms.TextBox();
            this.buttonBrowseTerminology = new System.Windows.Forms.Button();
            this.labelSubstitution = new System.Windows.Forms.Label();
            this.textBoxSubstitution = new System.Windows.Forms.TextBox();
            this.checkBoxProject = new System.Windows.Forms.CheckBox();
            this.checkBoxBuildTerminologyMap = new System.Windows.Forms.CheckBox();
            this.checkBoxPostProcess = new System.Windows.Forms.CheckBox();
            this.checkBoxChat = new System.Windows.Forms.CheckBox();
            this.checkBoxSystemMessages = new System.Windows.Forms.CheckBox();
            this.checkBoxAuto = new System.Windows.Forms.CheckBox();
            this.checkBoxIncludeOriginal = new System.Windows.Forms.CheckBox();
            this.checkBoxAddRtlMarkers = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPagePaths = new System.Windows.Forms.TabPage();
            this.tabPageServer = new System.Windows.Forms.TabPage();
            this.tabPageAdvanced = new System.Windows.Forms.TabPage();
            this.tabPageFiles = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTemperature)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRateLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinBatch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxBatch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxRetries)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBackoff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSceneThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBatchThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSummaries)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPagePaths.SuspendLayout();
            this.tabPageServer.SuspendLayout();
            this.tabPageAdvanced.SuspendLayout();
            this.tabPageFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelPythonPath
            // 
            this.labelPythonPath.AutoSize = true;
            this.labelPythonPath.Location = new System.Drawing.Point(6, 13);
            this.labelPythonPath.Name = "labelPythonPath";
            this.labelPythonPath.Size = new System.Drawing.Size(68, 13);
            this.labelPythonPath.TabIndex = 0;
            this.labelPythonPath.Text = "Python Path:";
            // 
            // textBoxPythonPath
            // 
            this.textBoxPythonPath.Location = new System.Drawing.Point(9, 29);
            this.textBoxPythonPath.Name = "textBoxPythonPath";
            this.textBoxPythonPath.Size = new System.Drawing.Size(350, 20);
            this.textBoxPythonPath.TabIndex = 1;
            // 
            // buttonBrowsePython
            // 
            this.buttonBrowsePython.Location = new System.Drawing.Point(365, 27);
            this.buttonBrowsePython.Name = "buttonBrowsePython";
            this.buttonBrowsePython.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowsePython.TabIndex = 2;
            this.buttonBrowsePython.Text = "Browse...";
            this.buttonBrowsePython.UseVisualStyleBackColor = true;
            this.buttonBrowsePython.Click += new System.EventHandler(this.buttonBrowsePython_Click);
            // 
            // labelScriptPath
            // 
            this.labelScriptPath.AutoSize = true;
            this.labelScriptPath.Location = new System.Drawing.Point(6, 62);
            this.labelScriptPath.Name = "labelScriptPath";
            this.labelScriptPath.Size = new System.Drawing.Size(62, 13);
            this.labelScriptPath.TabIndex = 3;
            this.labelScriptPath.Text = "Script Path:";
            // 
            // textBoxScriptPath
            // 
            this.textBoxScriptPath.Location = new System.Drawing.Point(9, 78);
            this.textBoxScriptPath.Name = "textBoxScriptPath";
            this.textBoxScriptPath.Size = new System.Drawing.Size(350, 20);
            this.textBoxScriptPath.TabIndex = 4;
            // 
            // buttonBrowseScript
            // 
            this.buttonBrowseScript.Location = new System.Drawing.Point(365, 76);
            this.buttonBrowseScript.Name = "buttonBrowseScript";
            this.buttonBrowseScript.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseScript.TabIndex = 5;
            this.buttonBrowseScript.Text = "Browse...";
            this.buttonBrowseScript.UseVisualStyleBackColor = true;
            this.buttonBrowseScript.Click += new System.EventHandler(this.buttonBrowseScript_Click);
            // 
            // buttonBrowseFolder
            // 
            this.buttonBrowseFolder.Location = new System.Drawing.Point(9, 110);
            this.buttonBrowseFolder.Name = "buttonBrowseFolder";
            this.buttonBrowseFolder.Size = new System.Drawing.Size(150, 23);
            this.buttonBrowseFolder.TabIndex = 6;
            this.buttonBrowseFolder.Text = "Browse llm-subtrans folder...";
            this.buttonBrowseFolder.UseVisualStyleBackColor = true;
            this.buttonBrowseFolder.Click += new System.EventHandler(this.buttonBrowseFolder_Click);
            // 
            // labelUrl
            // 
            this.labelUrl.AutoSize = true;
            this.labelUrl.Location = new System.Drawing.Point(6, 13);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(66, 13);
            this.labelUrl.TabIndex = 0;
            this.labelUrl.Text = "Server URL:";
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.Location = new System.Drawing.Point(9, 29);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(431, 20);
            this.textBoxUrl.TabIndex = 1;
            // 
            // labelApiKey
            // 
            this.labelApiKey.AutoSize = true;
            this.labelApiKey.Location = new System.Drawing.Point(6, 62);
            this.labelApiKey.Name = "labelApiKey";
            this.labelApiKey.Size = new System.Drawing.Size(48, 13);
            this.labelApiKey.TabIndex = 2;
            this.labelApiKey.Text = "API Key:";
            // 
            // textBoxApiKey
            // 
            this.textBoxApiKey.Location = new System.Drawing.Point(9, 78);
            this.textBoxApiKey.Name = "textBoxApiKey";
            this.textBoxApiKey.Size = new System.Drawing.Size(431, 20);
            this.textBoxApiKey.TabIndex = 3;
            // 
            // labelModel
            // 
            this.labelModel.AutoSize = true;
            this.labelModel.Location = new System.Drawing.Point(6, 111);
            this.labelModel.Name = "labelModel";
            this.labelModel.Size = new System.Drawing.Size(39, 13);
            this.labelModel.TabIndex = 4;
            this.labelModel.Text = "Model:";
            // 
            // textBoxModel
            // 
            this.textBoxModel.Location = new System.Drawing.Point(9, 127);
            this.textBoxModel.Name = "textBoxModel";
            this.textBoxModel.Size = new System.Drawing.Size(431, 20);
            this.textBoxModel.TabIndex = 5;
            // 
            // labelEndpoint
            // 
            this.labelEndpoint.AutoSize = true;
            this.labelEndpoint.Location = new System.Drawing.Point(6, 160);
            this.labelEndpoint.Name = "labelEndpoint";
            this.labelEndpoint.Size = new System.Drawing.Size(52, 13);
            this.labelEndpoint.TabIndex = 6;
            this.labelEndpoint.Text = "Endpoint:";
            // 
            // textBoxEndpoint
            // 
            this.textBoxEndpoint.Location = new System.Drawing.Point(9, 176);
            this.textBoxEndpoint.Name = "textBoxEndpoint";
            this.textBoxEndpoint.Size = new System.Drawing.Size(431, 20);
            this.textBoxEndpoint.TabIndex = 7;
            // 
            // labelTemperature
            // 
            this.labelTemperature.AutoSize = true;
            this.labelTemperature.Location = new System.Drawing.Point(6, 13);
            this.labelTemperature.Name = "labelTemperature";
            this.labelTemperature.Size = new System.Drawing.Size(70, 13);
            this.labelTemperature.TabIndex = 0;
            this.labelTemperature.Text = "Temperature:";
            // 
            // numericUpDownTemperature
            // 
            this.numericUpDownTemperature.DecimalPlaces = 2;
            this.numericUpDownTemperature.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numericUpDownTemperature.Location = new System.Drawing.Point(9, 29);
            this.numericUpDownTemperature.Maximum = new decimal(new int[] { 2, 0, 0, 0 });
            this.numericUpDownTemperature.Name = "numericUpDownTemperature";
            this.numericUpDownTemperature.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownTemperature.TabIndex = 1;
            // 
            // labelRateLimit
            // 
            this.labelRateLimit.AutoSize = true;
            this.labelRateLimit.Location = new System.Drawing.Point(100, 13);
            this.labelRateLimit.Name = "labelRateLimit";
            this.labelRateLimit.Size = new System.Drawing.Size(57, 13);
            this.labelRateLimit.TabIndex = 2;
            this.labelRateLimit.Text = "Rate Limit:";
            // 
            // numericUpDownRateLimit
            // 
            this.numericUpDownRateLimit.Location = new System.Drawing.Point(103, 29);
            this.numericUpDownRateLimit.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownRateLimit.Name = "numericUpDownRateLimit";
            this.numericUpDownRateLimit.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownRateLimit.TabIndex = 3;
            // 
            // labelMinBatch
            // 
            this.labelMinBatch.AutoSize = true;
            this.labelMinBatch.Location = new System.Drawing.Point(6, 62);
            this.labelMinBatch.Name = "labelMinBatch";
            this.labelMinBatch.Size = new System.Drawing.Size(58, 13);
            this.labelMinBatch.TabIndex = 4;
            this.labelMinBatch.Text = "Min Batch:";
            // 
            // numericUpDownMinBatch
            // 
            this.numericUpDownMinBatch.Location = new System.Drawing.Point(9, 78);
            this.numericUpDownMinBatch.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownMinBatch.Name = "numericUpDownMinBatch";
            this.numericUpDownMinBatch.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownMinBatch.TabIndex = 5;
            // 
            // labelMaxBatch
            // 
            this.labelMaxBatch.AutoSize = true;
            this.labelMaxBatch.Location = new System.Drawing.Point(100, 62);
            this.labelMaxBatch.Name = "labelMaxBatch";
            this.labelMaxBatch.Size = new System.Drawing.Size(61, 13);
            this.labelMaxBatch.TabIndex = 6;
            this.labelMaxBatch.Text = "Max Batch:";
            // 
            // numericUpDownMaxBatch
            // 
            this.numericUpDownMaxBatch.Location = new System.Drawing.Point(103, 78);
            this.numericUpDownMaxBatch.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownMaxBatch.Name = "numericUpDownMaxBatch";
            this.numericUpDownMaxBatch.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownMaxBatch.TabIndex = 7;
            // 
            // labelMaxRetries
            // 
            this.labelMaxRetries.AutoSize = true;
            this.labelMaxRetries.Location = new System.Drawing.Point(6, 111);
            this.labelMaxRetries.Name = "labelMaxRetries";
            this.labelMaxRetries.Size = new System.Drawing.Size(66, 13);
            this.labelMaxRetries.TabIndex = 8;
            this.labelMaxRetries.Text = "Max Retries:";
            // 
            // numericUpDownMaxRetries
            // 
            this.numericUpDownMaxRetries.Location = new System.Drawing.Point(9, 127);
            this.numericUpDownMaxRetries.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numericUpDownMaxRetries.Name = "numericUpDownMaxRetries";
            this.numericUpDownMaxRetries.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownMaxRetries.TabIndex = 9;
            // 
            // labelBackoff
            // 
            this.labelBackoff.AutoSize = true;
            this.labelBackoff.Location = new System.Drawing.Point(100, 111);
            this.labelBackoff.Name = "labelBackoff";
            this.labelBackoff.Size = new System.Drawing.Size(47, 13);
            this.labelBackoff.TabIndex = 10;
            this.labelBackoff.Text = "Backoff:";
            // 
            // numericUpDownBackoff
            // 
            this.numericUpDownBackoff.Location = new System.Drawing.Point(103, 127);
            this.numericUpDownBackoff.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownBackoff.Name = "numericUpDownBackoff";
            this.numericUpDownBackoff.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownBackoff.TabIndex = 11;
            // 
            // labelSceneThreshold
            // 
            this.labelSceneThreshold.AutoSize = true;
            this.labelSceneThreshold.Location = new System.Drawing.Point(200, 13);
            this.labelSceneThreshold.Name = "labelSceneThreshold";
            this.labelSceneThreshold.Size = new System.Drawing.Size(91, 13);
            this.labelSceneThreshold.TabIndex = 12;
            this.labelSceneThreshold.Text = "Scene Threshold:";
            // 
            // numericUpDownSceneThreshold
            // 
            this.numericUpDownSceneThreshold.Location = new System.Drawing.Point(203, 29);
            this.numericUpDownSceneThreshold.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownSceneThreshold.Name = "numericUpDownSceneThreshold";
            this.numericUpDownSceneThreshold.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownSceneThreshold.TabIndex = 13;
            // 
            // labelBatchThreshold
            // 
            this.labelBatchThreshold.AutoSize = true;
            this.labelBatchThreshold.Location = new System.Drawing.Point(200, 62);
            this.labelBatchThreshold.Name = "labelBatchThreshold";
            this.labelBatchThreshold.Size = new System.Drawing.Size(88, 13);
            this.labelBatchThreshold.TabIndex = 14;
            this.labelBatchThreshold.Text = "Batch Threshold:";
            // 
            // numericUpDownBatchThreshold
            // 
            this.numericUpDownBatchThreshold.Location = new System.Drawing.Point(203, 78);
            this.numericUpDownBatchThreshold.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericUpDownBatchThreshold.Name = "numericUpDownBatchThreshold";
            this.numericUpDownBatchThreshold.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownBatchThreshold.TabIndex = 15;
            // 
            // labelMaxSummaries
            // 
            this.labelMaxSummaries.AutoSize = true;
            this.labelMaxSummaries.Location = new System.Drawing.Point(200, 111);
            this.labelMaxSummaries.Name = "labelMaxSummaries";
            this.labelMaxSummaries.Size = new System.Drawing.Size(85, 13);
            this.labelMaxSummaries.TabIndex = 16;
            this.labelMaxSummaries.Text = "Max Summaries:";
            // 
            // numericUpDownMaxSummaries
            // 
            this.numericUpDownMaxSummaries.Location = new System.Drawing.Point(203, 127);
            this.numericUpDownMaxSummaries.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numericUpDownMaxSummaries.Name = "numericUpDownMaxSummaries";
            this.numericUpDownMaxSummaries.Size = new System.Drawing.Size(60, 20);
            this.numericUpDownMaxSummaries.TabIndex = 17;
            // 
            // labelInstructionFile
            // 
            this.labelInstructionFile.AutoSize = true;
            this.labelInstructionFile.Location = new System.Drawing.Point(6, 13);
            this.labelInstructionFile.Name = "labelInstructionFile";
            this.labelInstructionFile.Size = new System.Drawing.Size(79, 13);
            this.labelInstructionFile.TabIndex = 0;
            this.labelInstructionFile.Text = "Instruction File:";
            // 
            // textBoxInstructionFile
            // 
            this.textBoxInstructionFile.Location = new System.Drawing.Point(9, 29);
            this.textBoxInstructionFile.Name = "textBoxInstructionFile";
            this.textBoxInstructionFile.Size = new System.Drawing.Size(350, 20);
            this.textBoxInstructionFile.TabIndex = 1;
            // 
            // buttonBrowseInstruction
            // 
            this.buttonBrowseInstruction.Location = new System.Drawing.Point(365, 27);
            this.buttonBrowseInstruction.Name = "buttonBrowseInstruction";
            this.buttonBrowseInstruction.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseInstruction.TabIndex = 2;
            this.buttonBrowseInstruction.Text = "Browse...";
            this.buttonBrowseInstruction.UseVisualStyleBackColor = true;
            this.buttonBrowseInstruction.Click += new System.EventHandler(this.buttonBrowseInstruction_Click);
            // 
            // labelNamesFile
            // 
            this.labelNamesFile.AutoSize = true;
            this.labelNamesFile.Location = new System.Drawing.Point(6, 62);
            this.labelNamesFile.Name = "labelNamesFile";
            this.labelNamesFile.Size = new System.Drawing.Size(61, 13);
            this.labelNamesFile.TabIndex = 3;
            this.labelNamesFile.Text = "Names File:";
            // 
            // textBoxNamesFile
            // 
            this.textBoxNamesFile.Location = new System.Drawing.Point(9, 78);
            this.textBoxNamesFile.Name = "textBoxNamesFile";
            this.textBoxNamesFile.Size = new System.Drawing.Size(350, 20);
            this.textBoxNamesFile.TabIndex = 4;
            // 
            // buttonBrowseNames
            // 
            this.buttonBrowseNames.Location = new System.Drawing.Point(365, 76);
            this.buttonBrowseNames.Name = "buttonBrowseNames";
            this.buttonBrowseNames.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseNames.TabIndex = 5;
            this.buttonBrowseNames.Text = "Browse...";
            this.buttonBrowseNames.UseVisualStyleBackColor = true;
            this.buttonBrowseNames.Click += new System.EventHandler(this.buttonBrowseNames_Click);
            // 
            // labelTerminologyFile
            // 
            this.labelTerminologyFile.AutoSize = true;
            this.labelTerminologyFile.Location = new System.Drawing.Point(6, 111);
            this.labelTerminologyFile.Name = "labelTerminologyFile";
            this.labelTerminologyFile.Size = new System.Drawing.Size(86, 13);
            this.labelTerminologyFile.TabIndex = 6;
            this.labelTerminologyFile.Text = "Terminology File:";
            // 
            // textBoxTerminologyFile
            // 
            this.textBoxTerminologyFile.Location = new System.Drawing.Point(9, 127);
            this.textBoxTerminologyFile.Name = "textBoxTerminologyFile";
            this.textBoxTerminologyFile.Size = new System.Drawing.Size(350, 20);
            this.textBoxTerminologyFile.TabIndex = 7;
            // 
            // buttonBrowseTerminology
            // 
            this.buttonBrowseTerminology.Location = new System.Drawing.Point(365, 125);
            this.buttonBrowseTerminology.Name = "buttonBrowseTerminology";
            this.buttonBrowseTerminology.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseTerminology.TabIndex = 8;
            this.buttonBrowseTerminology.Text = "Browse...";
            this.buttonBrowseTerminology.UseVisualStyleBackColor = true;
            this.buttonBrowseTerminology.Click += new System.EventHandler(this.buttonBrowseTerminology_Click);
            // 
            // labelSubstitution
            // 
            this.labelSubstitution.AutoSize = true;
            this.labelSubstitution.Location = new System.Drawing.Point(6, 160);
            this.labelSubstitution.Name = "labelSubstitution";
            this.labelSubstitution.Size = new System.Drawing.Size(65, 13);
            this.labelSubstitution.TabIndex = 9;
            this.labelSubstitution.Text = "Substitution:";
            // 
            // textBoxSubstitution
            // 
            this.textBoxSubstitution.Location = new System.Drawing.Point(9, 176);
            this.textBoxSubstitution.Name = "textBoxSubstitution";
            this.textBoxSubstitution.Size = new System.Drawing.Size(431, 20);
            this.textBoxSubstitution.TabIndex = 10;
            // 
            // checkBoxProject
            // 
            this.checkBoxProject.AutoSize = true;
            this.checkBoxProject.Location = new System.Drawing.Point(9, 210);
            this.checkBoxProject.Name = "checkBoxProject";
            this.checkBoxProject.Size = new System.Drawing.Size(59, 17);
            this.checkBoxProject.TabIndex = 18;
            this.checkBoxProject.Text = "Project";
            this.checkBoxProject.UseVisualStyleBackColor = true;
            // 
            // checkBoxBuildTerminologyMap
            // 
            this.checkBoxBuildTerminologyMap.AutoSize = true;
            this.checkBoxBuildTerminologyMap.Location = new System.Drawing.Point(103, 210);
            this.checkBoxBuildTerminologyMap.Name = "checkBoxBuildTerminologyMap";
            this.checkBoxBuildTerminologyMap.Size = new System.Drawing.Size(131, 17);
            this.checkBoxBuildTerminologyMap.TabIndex = 19;
            this.checkBoxBuildTerminologyMap.Text = "Build Terminology Map";
            this.checkBoxBuildTerminologyMap.UseVisualStyleBackColor = true;
            // 
            // checkBoxPostProcess
            // 
            this.checkBoxPostProcess.AutoSize = true;
            this.checkBoxPostProcess.Location = new System.Drawing.Point(240, 210);
            this.checkBoxPostProcess.Name = "checkBoxPostProcess";
            this.checkBoxPostProcess.Size = new System.Drawing.Size(89, 17);
            this.checkBoxPostProcess.TabIndex = 20;
            this.checkBoxPostProcess.Text = "Post Process";
            this.checkBoxPostProcess.UseVisualStyleBackColor = true;
            // 
            // checkBoxChat
            // 
            this.checkBoxChat.AutoSize = true;
            this.checkBoxChat.Location = new System.Drawing.Point(340, 210);
            this.checkBoxChat.Name = "checkBoxChat";
            this.checkBoxChat.Size = new System.Drawing.Size(48, 17);
            this.checkBoxChat.TabIndex = 21;
            this.checkBoxChat.Text = "Chat";
            this.checkBoxChat.UseVisualStyleBackColor = true;
            // 
            // checkBoxSystemMessages
            // 
            this.checkBoxSystemMessages.AutoSize = true;
            this.checkBoxSystemMessages.Location = new System.Drawing.Point(9, 233);
            this.checkBoxSystemMessages.Name = "checkBoxSystemMessages";
            this.checkBoxSystemMessages.Size = new System.Drawing.Size(111, 17);
            this.checkBoxSystemMessages.TabIndex = 22;
            this.checkBoxSystemMessages.Text = "System Messages";
            this.checkBoxSystemMessages.UseVisualStyleBackColor = true;
            // 
            // checkBoxAuto
            // 
            this.checkBoxAuto.AutoSize = true;
            this.checkBoxAuto.Location = new System.Drawing.Point(130, 233);
            this.checkBoxAuto.Name = "checkBoxAuto";
            this.checkBoxAuto.Size = new System.Drawing.Size(48, 17);
            this.checkBoxAuto.TabIndex = 23;
            this.checkBoxAuto.Text = "Auto";
            this.checkBoxAuto.UseVisualStyleBackColor = true;
            // 
            // checkBoxIncludeOriginal
            // 
            this.checkBoxIncludeOriginal.AutoSize = true;
            this.checkBoxIncludeOriginal.Location = new System.Drawing.Point(190, 233);
            this.checkBoxIncludeOriginal.Name = "checkBoxIncludeOriginal";
            this.checkBoxIncludeOriginal.Size = new System.Drawing.Size(100, 17);
            this.checkBoxIncludeOriginal.TabIndex = 24;
            this.checkBoxIncludeOriginal.Text = "Include Original";
            this.checkBoxIncludeOriginal.UseVisualStyleBackColor = true;
            // 
            // checkBoxAddRtlMarkers
            // 
            this.checkBoxAddRtlMarkers.AutoSize = true;
            this.checkBoxAddRtlMarkers.Location = new System.Drawing.Point(300, 233);
            this.checkBoxAddRtlMarkers.Name = "checkBoxAddRtlMarkers";
            this.checkBoxAddRtlMarkers.Size = new System.Drawing.Size(103, 17);
            this.checkBoxAddRtlMarkers.TabIndex = 25;
            this.checkBoxAddRtlMarkers.Text = "Add RTL Markers";
            this.checkBoxAddRtlMarkers.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(300, 310);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 26;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(381, 310);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 27;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPagePaths);
            this.tabControl1.Controls.Add(this.tabPageServer);
            this.tabControl1.Controls.Add(this.tabPageFiles);
            this.tabControl1.Controls.Add(this.tabPageAdvanced);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(460, 290);
            this.tabControl1.TabIndex = 28;
            // 
            // tabPagePaths
            // 
            this.tabPagePaths.Controls.Add(this.labelPythonPath);
            this.tabPagePaths.Controls.Add(this.textBoxPythonPath);
            this.tabPagePaths.Controls.Add(this.buttonBrowsePython);
            this.tabPagePaths.Controls.Add(this.labelScriptPath);
            this.tabPagePaths.Controls.Add(this.textBoxScriptPath);
            this.tabPagePaths.Controls.Add(this.buttonBrowseScript);
            this.tabPagePaths.Controls.Add(this.buttonBrowseFolder);
            this.tabPagePaths.Location = new System.Drawing.Point(4, 22);
            this.tabPagePaths.Name = "tabPagePaths";
            this.tabPagePaths.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePaths.Size = new System.Drawing.Size(452, 264);
            this.tabPagePaths.TabIndex = 0;
            this.tabPagePaths.Text = "Paths";
            this.tabPagePaths.UseVisualStyleBackColor = true;
            // 
            // tabPageServer
            // 
            this.tabPageServer.Controls.Add(this.labelUrl);
            this.tabPageServer.Controls.Add(this.textBoxUrl);
            this.tabPageServer.Controls.Add(this.labelApiKey);
            this.tabPageServer.Controls.Add(this.textBoxApiKey);
            this.tabPageServer.Controls.Add(this.labelModel);
            this.tabPageServer.Controls.Add(this.textBoxModel);
            this.tabPageServer.Controls.Add(this.labelEndpoint);
            this.tabPageServer.Controls.Add(this.textBoxEndpoint);
            this.tabPageServer.Location = new System.Drawing.Point(4, 22);
            this.tabPageServer.Name = "tabPageServer";
            this.tabPageServer.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageServer.Size = new System.Drawing.Size(452, 264);
            this.tabPageServer.TabIndex = 1;
            this.tabPageServer.Text = "Server";
            this.tabPageServer.UseVisualStyleBackColor = true;
            // 
            // tabPageAdvanced
            // 
            this.tabPageAdvanced.Controls.Add(this.labelTemperature);
            this.tabPageAdvanced.Controls.Add(this.numericUpDownTemperature);
            this.tabPageAdvanced.Controls.Add(this.labelRateLimit);
            this.tabPageAdvanced.Controls.Add(this.numericUpDownRateLimit);
            this.tabPageAdvanced.Controls.Add(this.labelMinBatch);
            this.tabPageAdvanced.Controls.Add(this.numericUpDownMinBatch);
            this.tabPageAdvanced.Controls.Add(this.labelMaxBatch);
            this.tabPageAdvanced.Controls.Add(this.numericUpDownMaxBatch);
            this.tabPageAdvanced.Controls.Add(this.labelMaxRetries);
            this.tabPageAdvanced.Controls.Add(this.numericUpDownMaxRetries);
            this.tabPageAdvanced.Controls.Add(this.labelBackoff);
            this.tabPageAdvanced.Controls.Add(this.numericUpDownBackoff);
            this.tabPageAdvanced.Controls.Add(this.labelSceneThreshold);
            this.tabPageAdvanced.Controls.Add(this.numericUpDownSceneThreshold);
            this.tabPageAdvanced.Controls.Add(this.labelBatchThreshold);
            this.tabPageAdvanced.Controls.Add(this.numericUpDownBatchThreshold);
            this.tabPageAdvanced.Controls.Add(this.labelMaxSummaries);
            this.tabPageAdvanced.Controls.Add(this.numericUpDownMaxSummaries);
            this.tabPageAdvanced.Controls.Add(this.checkBoxProject);
            this.tabPageAdvanced.Controls.Add(this.checkBoxBuildTerminologyMap);
            this.tabPageAdvanced.Controls.Add(this.checkBoxPostProcess);
            this.tabPageAdvanced.Controls.Add(this.checkBoxChat);
            this.tabPageAdvanced.Controls.Add(this.checkBoxSystemMessages);
            this.tabPageAdvanced.Controls.Add(this.checkBoxAuto);
            this.tabPageAdvanced.Controls.Add(this.checkBoxIncludeOriginal);
            this.tabPageAdvanced.Controls.Add(this.checkBoxAddRtlMarkers);
            this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdvanced.Name = "tabPageAdvanced";
            this.tabPageAdvanced.Size = new System.Drawing.Size(452, 264);
            this.tabPageAdvanced.TabIndex = 2;
            this.tabPageAdvanced.Text = "Advanced";
            this.tabPageAdvanced.UseVisualStyleBackColor = true;
            // 
            // tabPageFiles
            // 
            this.tabPageFiles.Controls.Add(this.labelInstructionFile);
            this.tabPageFiles.Controls.Add(this.textBoxInstructionFile);
            this.tabPageFiles.Controls.Add(this.buttonBrowseInstruction);
            this.tabPageFiles.Controls.Add(this.labelNamesFile);
            this.tabPageFiles.Controls.Add(this.textBoxNamesFile);
            this.tabPageFiles.Controls.Add(this.buttonBrowseNames);
            this.tabPageFiles.Controls.Add(this.labelTerminologyFile);
            this.tabPageFiles.Controls.Add(this.textBoxTerminologyFile);
            this.tabPageFiles.Controls.Add(this.buttonBrowseTerminology);
            this.tabPageFiles.Controls.Add(this.labelSubstitution);
            this.tabPageFiles.Controls.Add(this.textBoxSubstitution);
            this.tabPageFiles.Location = new System.Drawing.Point(4, 22);
            this.tabPageFiles.Name = "tabPageFiles";
            this.tabPageFiles.Size = new System.Drawing.Size(452, 264);
            this.tabPageFiles.TabIndex = 3;
            this.tabPageFiles.Text = "Files";
            this.tabPageFiles.UseVisualStyleBackColor = true;
            // 
            // LlmSubtransSettings
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(484, 345);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LlmSubtransSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LLM Subtrans Settings";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTemperature)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRateLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinBatch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxBatch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxRetries)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBackoff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSceneThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBatchThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSummaries)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPagePaths.ResumeLayout(false);
            this.tabPagePaths.PerformLayout();
            this.tabPageServer.ResumeLayout(false);
            this.tabPageServer.PerformLayout();
            this.tabPageAdvanced.ResumeLayout(false);
            this.tabPageAdvanced.PerformLayout();
            this.tabPageFiles.ResumeLayout(false);
            this.tabPageFiles.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label labelPythonPath;
        private System.Windows.Forms.TextBox textBoxPythonPath;
        private System.Windows.Forms.Button buttonBrowsePython;
        private System.Windows.Forms.Label labelScriptPath;
        private System.Windows.Forms.TextBox textBoxScriptPath;
        private System.Windows.Forms.Button buttonBrowseScript;
        private System.Windows.Forms.Label labelUrl;
        private System.Windows.Forms.TextBox textBoxUrl;
        private System.Windows.Forms.Label labelApiKey;
        private System.Windows.Forms.TextBox textBoxApiKey;
        private System.Windows.Forms.Label labelModel;
        private System.Windows.Forms.TextBox textBoxModel;
        private System.Windows.Forms.Label labelEndpoint;
        private System.Windows.Forms.TextBox textBoxEndpoint;
        private System.Windows.Forms.Label labelTemperature;
        private System.Windows.Forms.NumericUpDown numericUpDownTemperature;
        private System.Windows.Forms.Label labelRateLimit;
        private System.Windows.Forms.NumericUpDown numericUpDownRateLimit;
        private System.Windows.Forms.Label labelMinBatch;
        private System.Windows.Forms.NumericUpDown numericUpDownMinBatch;
        private System.Windows.Forms.Label labelMaxBatch;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxBatch;
        private System.Windows.Forms.Label labelMaxRetries;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxRetries;
        private System.Windows.Forms.Label labelBackoff;
        private System.Windows.Forms.NumericUpDown numericUpDownBackoff;
        private System.Windows.Forms.Label labelSceneThreshold;
        private System.Windows.Forms.NumericUpDown numericUpDownSceneThreshold;
        private System.Windows.Forms.Label labelBatchThreshold;
        private System.Windows.Forms.NumericUpDown numericUpDownBatchThreshold;
        private System.Windows.Forms.Label labelMaxSummaries;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxSummaries;
        private System.Windows.Forms.Label labelInstructionFile;
        private System.Windows.Forms.TextBox textBoxInstructionFile;
        private System.Windows.Forms.Button buttonBrowseInstruction;
        private System.Windows.Forms.Label labelNamesFile;
        private System.Windows.Forms.TextBox textBoxNamesFile;
        private System.Windows.Forms.Button buttonBrowseNames;
        private System.Windows.Forms.Label labelTerminologyFile;
        private System.Windows.Forms.TextBox textBoxTerminologyFile;
        private System.Windows.Forms.Button buttonBrowseTerminology;
        private System.Windows.Forms.Label labelSubstitution;
        private System.Windows.Forms.TextBox textBoxSubstitution;
        private System.Windows.Forms.CheckBox checkBoxProject;
        private System.Windows.Forms.CheckBox checkBoxBuildTerminologyMap;
        private System.Windows.Forms.CheckBox checkBoxPostProcess;
        private System.Windows.Forms.CheckBox checkBoxChat;
        private System.Windows.Forms.CheckBox checkBoxSystemMessages;
        private System.Windows.Forms.CheckBox checkBoxAuto;
        private System.Windows.Forms.CheckBox checkBoxIncludeOriginal;
        private System.Windows.Forms.CheckBox checkBoxAddRtlMarkers;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPagePaths;
        private System.Windows.Forms.TabPage tabPageServer;
        private System.Windows.Forms.TabPage tabPageAdvanced;
        private System.Windows.Forms.TabPage tabPageFiles;
        private System.Windows.Forms.Button buttonBrowseFolder;
    }
}
