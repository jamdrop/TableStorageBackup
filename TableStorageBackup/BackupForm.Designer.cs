namespace TableStorageBackup
{
    partial class BackupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackupForm));
            this.SourceGroupBox = new System.Windows.Forms.GroupBox();
            this.SourceBlobBrowseButton = new System.Windows.Forms.Button();
            this.SourceBlobFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SourceTableEndpoint = new System.Windows.Forms.RadioButton();
            this.SourceBlobFile = new System.Windows.Forms.RadioButton();
            this.SourceBrowseButton = new System.Windows.Forms.Button();
            this.SourceLocalFilename = new System.Windows.Forms.TextBox();
            this.SourceStorageAccount = new System.Windows.Forms.ComboBox();
            this.SourceLocalFile = new System.Windows.Forms.RadioButton();
            this.StatusGroupBox = new System.Windows.Forms.GroupBox();
            this.CurrentOperationLabel = new System.Windows.Forms.Label();
            this.StatusProgressBar = new System.Windows.Forms.ProgressBar();
            this.OptionsButton = new System.Windows.Forms.Button();
            this.StartButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SourceOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.DestinationFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DestBlobBrowseButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.DestBlobFilename = new System.Windows.Forms.TextBox();
            this.DestTableEndpoint = new System.Windows.Forms.RadioButton();
            this.DestBlobFile = new System.Windows.Forms.RadioButton();
            this.DestLocalFileBrowse = new System.Windows.Forms.Button();
            this.DestLocalFilename = new System.Windows.Forms.TextBox();
            this.DestStorageAccount = new System.Windows.Forms.ComboBox();
            this.DestLocalFile = new System.Windows.Forms.RadioButton();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.SourceGroupBox.SuspendLayout();
            this.StatusGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // SourceGroupBox
            // 
            this.SourceGroupBox.Controls.Add(this.SourceBlobBrowseButton);
            this.SourceGroupBox.Controls.Add(this.SourceBlobFilename);
            this.SourceGroupBox.Controls.Add(this.label1);
            this.SourceGroupBox.Controls.Add(this.SourceTableEndpoint);
            this.SourceGroupBox.Controls.Add(this.SourceBlobFile);
            this.SourceGroupBox.Controls.Add(this.SourceBrowseButton);
            this.SourceGroupBox.Controls.Add(this.SourceLocalFilename);
            this.SourceGroupBox.Controls.Add(this.SourceStorageAccount);
            this.SourceGroupBox.Controls.Add(this.SourceLocalFile);
            this.SourceGroupBox.Location = new System.Drawing.Point(12, 12);
            this.SourceGroupBox.Name = "SourceGroupBox";
            this.SourceGroupBox.Size = new System.Drawing.Size(467, 125);
            this.SourceGroupBox.TabIndex = 0;
            this.SourceGroupBox.TabStop = false;
            this.SourceGroupBox.Text = "Source";
            // 
            // SourceBlobBrowseButton
            // 
            this.SourceBlobBrowseButton.Enabled = false;
            this.SourceBlobBrowseButton.Location = new System.Drawing.Point(385, 67);
            this.SourceBlobBrowseButton.Name = "SourceBlobBrowseButton";
            this.SourceBlobBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.SourceBlobBrowseButton.TabIndex = 12;
            this.SourceBlobBrowseButton.Text = "Browse...";
            this.SourceBlobBrowseButton.UseVisualStyleBackColor = true;
            this.SourceBlobBrowseButton.Click += new System.EventHandler(this.SourceBlobBrowseButton_Click);
            // 
            // SourceBlobFilename
            // 
            this.SourceBlobFilename.Location = new System.Drawing.Point(120, 69);
            this.SourceBlobFilename.Name = "SourceBlobFilename";
            this.SourceBlobFilename.Size = new System.Drawing.Size(259, 20);
            this.SourceBlobFilename.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Storage account:";
            // 
            // SourceTableEndpoint
            // 
            this.SourceTableEndpoint.AutoSize = true;
            this.SourceTableEndpoint.Checked = true;
            this.SourceTableEndpoint.Location = new System.Drawing.Point(10, 45);
            this.SourceTableEndpoint.Name = "SourceTableEndpoint";
            this.SourceTableEndpoint.Size = new System.Drawing.Size(96, 17);
            this.SourceTableEndpoint.TabIndex = 9;
            this.SourceTableEndpoint.TabStop = true;
            this.SourceTableEndpoint.Text = "Table endpoint";
            this.SourceTableEndpoint.UseVisualStyleBackColor = true;
            this.SourceTableEndpoint.CheckedChanged += new System.EventHandler(this.SetStartState);
            // 
            // SourceBlobFile
            // 
            this.SourceBlobFile.AutoSize = true;
            this.SourceBlobFile.Location = new System.Drawing.Point(10, 70);
            this.SourceBlobFile.Name = "SourceBlobFile";
            this.SourceBlobFile.Size = new System.Drawing.Size(104, 17);
            this.SourceBlobFile.TabIndex = 6;
            this.SourceBlobFile.Text = "Blob backup file:";
            this.SourceBlobFile.UseVisualStyleBackColor = true;
            this.SourceBlobFile.CheckedChanged += new System.EventHandler(this.SetStartState);
            // 
            // SourceBrowseButton
            // 
            this.SourceBrowseButton.Location = new System.Drawing.Point(385, 93);
            this.SourceBrowseButton.Name = "SourceBrowseButton";
            this.SourceBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.SourceBrowseButton.TabIndex = 4;
            this.SourceBrowseButton.Text = "Browse...";
            this.SourceBrowseButton.UseVisualStyleBackColor = true;
            this.SourceBrowseButton.Click += new System.EventHandler(this.SourceBrowseButton_Click);
            // 
            // SourceLocalFilename
            // 
            this.SourceLocalFilename.Location = new System.Drawing.Point(120, 95);
            this.SourceLocalFilename.Name = "SourceLocalFilename";
            this.SourceLocalFilename.Size = new System.Drawing.Size(259, 20);
            this.SourceLocalFilename.TabIndex = 3;
            this.SourceLocalFilename.TextChanged += new System.EventHandler(this.SetStartState);
            // 
            // SourceStorageAccount
            // 
            this.SourceStorageAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SourceStorageAccount.FormattingEnabled = true;
            this.SourceStorageAccount.Location = new System.Drawing.Point(102, 18);
            this.SourceStorageAccount.Name = "SourceStorageAccount";
            this.SourceStorageAccount.Size = new System.Drawing.Size(358, 21);
            this.SourceStorageAccount.TabIndex = 2;
            this.SourceStorageAccount.SelectedIndexChanged += new System.EventHandler(this.SourceStorageAccount_SelectedIndexChanged);
            // 
            // SourceLocalFile
            // 
            this.SourceLocalFile.AutoSize = true;
            this.SourceLocalFile.Location = new System.Drawing.Point(10, 96);
            this.SourceLocalFile.Name = "SourceLocalFile";
            this.SourceLocalFile.Size = new System.Drawing.Size(109, 17);
            this.SourceLocalFile.TabIndex = 1;
            this.SourceLocalFile.Text = "Local backup file:";
            this.SourceLocalFile.UseVisualStyleBackColor = true;
            this.SourceLocalFile.CheckedChanged += new System.EventHandler(this.SetStartState);
            // 
            // StatusGroupBox
            // 
            this.StatusGroupBox.Controls.Add(this.CurrentOperationLabel);
            this.StatusGroupBox.Controls.Add(this.StatusProgressBar);
            this.StatusGroupBox.Location = new System.Drawing.Point(12, 274);
            this.StatusGroupBox.Name = "StatusGroupBox";
            this.StatusGroupBox.Size = new System.Drawing.Size(467, 68);
            this.StatusGroupBox.TabIndex = 2;
            this.StatusGroupBox.TabStop = false;
            this.StatusGroupBox.Text = "Status";
            // 
            // CurrentOperationLabel
            // 
            this.CurrentOperationLabel.AutoSize = true;
            this.CurrentOperationLabel.Location = new System.Drawing.Point(7, 19);
            this.CurrentOperationLabel.Name = "CurrentOperationLabel";
            this.CurrentOperationLabel.Size = new System.Drawing.Size(24, 13);
            this.CurrentOperationLabel.TabIndex = 1;
            this.CurrentOperationLabel.Text = "Idle";
            // 
            // StatusProgressBar
            // 
            this.StatusProgressBar.Location = new System.Drawing.Point(6, 35);
            this.StatusProgressBar.MarqueeAnimationSpeed = 0;
            this.StatusProgressBar.Name = "StatusProgressBar";
            this.StatusProgressBar.Size = new System.Drawing.Size(455, 23);
            this.StatusProgressBar.TabIndex = 0;
            // 
            // OptionsButton
            // 
            this.OptionsButton.Location = new System.Drawing.Point(396, 348);
            this.OptionsButton.Name = "OptionsButton";
            this.OptionsButton.Size = new System.Drawing.Size(83, 23);
            this.OptionsButton.TabIndex = 3;
            this.OptionsButton.Text = "Options...";
            this.OptionsButton.UseVisualStyleBackColor = true;
            this.OptionsButton.Click += new System.EventHandler(this.OptionsButton_Click);
            // 
            // StartButton
            // 
            this.StartButton.Enabled = false;
            this.StartButton.Location = new System.Drawing.Point(234, 348);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(75, 23);
            this.StartButton.TabIndex = 5;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Enabled = false;
            this.CancelButton.Location = new System.Drawing.Point(315, 348);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 6;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // SourceOpenFileDialog
            // 
            this.SourceOpenFileDialog.Filter = "Table Storage Backup Files|*.tsbak";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DestBlobBrowseButton);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.DestBlobFilename);
            this.groupBox1.Controls.Add(this.DestTableEndpoint);
            this.groupBox1.Controls.Add(this.DestBlobFile);
            this.groupBox1.Controls.Add(this.DestLocalFileBrowse);
            this.groupBox1.Controls.Add(this.DestLocalFilename);
            this.groupBox1.Controls.Add(this.DestStorageAccount);
            this.groupBox1.Controls.Add(this.DestLocalFile);
            this.groupBox1.Location = new System.Drawing.Point(12, 143);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(467, 125);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Destination";
            // 
            // DestBlobBrowseButton
            // 
            this.DestBlobBrowseButton.Enabled = false;
            this.DestBlobBrowseButton.Location = new System.Drawing.Point(385, 67);
            this.DestBlobBrowseButton.Name = "DestBlobBrowseButton";
            this.DestBlobBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.DestBlobBrowseButton.TabIndex = 14;
            this.DestBlobBrowseButton.Text = "Browse...";
            this.DestBlobBrowseButton.UseVisualStyleBackColor = true;
            this.DestBlobBrowseButton.Click += new System.EventHandler(this.DestBlobBrowseButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Storage account:";
            // 
            // DestBlobFilename
            // 
            this.DestBlobFilename.Location = new System.Drawing.Point(120, 69);
            this.DestBlobFilename.Name = "DestBlobFilename";
            this.DestBlobFilename.Size = new System.Drawing.Size(259, 20);
            this.DestBlobFilename.TabIndex = 13;
            // 
            // DestTableEndpoint
            // 
            this.DestTableEndpoint.AutoSize = true;
            this.DestTableEndpoint.Checked = true;
            this.DestTableEndpoint.Location = new System.Drawing.Point(10, 45);
            this.DestTableEndpoint.Name = "DestTableEndpoint";
            this.DestTableEndpoint.Size = new System.Drawing.Size(96, 17);
            this.DestTableEndpoint.TabIndex = 9;
            this.DestTableEndpoint.TabStop = true;
            this.DestTableEndpoint.Text = "Table endpoint";
            this.DestTableEndpoint.UseVisualStyleBackColor = true;
            this.DestTableEndpoint.CheckedChanged += new System.EventHandler(this.SetStartState);
            // 
            // DestBlobFile
            // 
            this.DestBlobFile.AutoSize = true;
            this.DestBlobFile.Location = new System.Drawing.Point(10, 70);
            this.DestBlobFile.Name = "DestBlobFile";
            this.DestBlobFile.Size = new System.Drawing.Size(104, 17);
            this.DestBlobFile.TabIndex = 6;
            this.DestBlobFile.Text = "Blob backup file:";
            this.DestBlobFile.UseVisualStyleBackColor = true;
            this.DestBlobFile.CheckedChanged += new System.EventHandler(this.SetStartState);
            // 
            // DestLocalFileBrowse
            // 
            this.DestLocalFileBrowse.Location = new System.Drawing.Point(385, 93);
            this.DestLocalFileBrowse.Name = "DestLocalFileBrowse";
            this.DestLocalFileBrowse.Size = new System.Drawing.Size(75, 23);
            this.DestLocalFileBrowse.TabIndex = 4;
            this.DestLocalFileBrowse.Text = "Browse...";
            this.DestLocalFileBrowse.UseVisualStyleBackColor = true;
            this.DestLocalFileBrowse.Click += new System.EventHandler(this.DestLocalFileBrowse_Click);
            // 
            // DestLocalFilename
            // 
            this.DestLocalFilename.Location = new System.Drawing.Point(120, 95);
            this.DestLocalFilename.Name = "DestLocalFilename";
            this.DestLocalFilename.Size = new System.Drawing.Size(259, 20);
            this.DestLocalFilename.TabIndex = 3;
            this.DestLocalFilename.TextChanged += new System.EventHandler(this.SetStartState);
            // 
            // DestStorageAccount
            // 
            this.DestStorageAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DestStorageAccount.FormattingEnabled = true;
            this.DestStorageAccount.Location = new System.Drawing.Point(102, 18);
            this.DestStorageAccount.Name = "DestStorageAccount";
            this.DestStorageAccount.Size = new System.Drawing.Size(358, 21);
            this.DestStorageAccount.TabIndex = 2;
            this.DestStorageAccount.SelectedIndexChanged += new System.EventHandler(this.DestStorageAccount_SelectedIndexChanged);
            // 
            // DestLocalFile
            // 
            this.DestLocalFile.AutoSize = true;
            this.DestLocalFile.Location = new System.Drawing.Point(10, 96);
            this.DestLocalFile.Name = "DestLocalFile";
            this.DestLocalFile.Size = new System.Drawing.Size(109, 17);
            this.DestLocalFile.TabIndex = 1;
            this.DestLocalFile.Text = "Local backup file:";
            this.DestLocalFile.UseVisualStyleBackColor = true;
            this.DestLocalFile.CheckedChanged += new System.EventHandler(this.SetStartState);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(9, 351);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(117, 13);
            this.linkLabel1.TabIndex = 8;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "antscode.blogspot.com";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // BackupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(491, 383);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.OptionsButton);
            this.Controls.Add(this.StatusGroupBox);
            this.Controls.Add(this.SourceGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "BackupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Table Storage Backup & Restore";
            this.SourceGroupBox.ResumeLayout(false);
            this.SourceGroupBox.PerformLayout();
            this.StatusGroupBox.ResumeLayout(false);
            this.StatusGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox SourceGroupBox;
        private System.Windows.Forms.GroupBox StatusGroupBox;
        private System.Windows.Forms.ProgressBar StatusProgressBar;
        private System.Windows.Forms.Button OptionsButton;
        private System.Windows.Forms.Button SourceBrowseButton;
        private System.Windows.Forms.TextBox SourceLocalFilename;
        private System.Windows.Forms.ComboBox SourceStorageAccount;
        private System.Windows.Forms.RadioButton SourceLocalFile;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.OpenFileDialog SourceOpenFileDialog;
        private System.Windows.Forms.FolderBrowserDialog DestinationFolderBrowserDialog;
        private System.Windows.Forms.RadioButton SourceBlobFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton SourceTableEndpoint;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton DestTableEndpoint;
        private System.Windows.Forms.RadioButton DestBlobFile;
        private System.Windows.Forms.Button DestLocalFileBrowse;
        private System.Windows.Forms.TextBox DestLocalFilename;
        private System.Windows.Forms.ComboBox DestStorageAccount;
        private System.Windows.Forms.RadioButton DestLocalFile;
        private System.Windows.Forms.Label CurrentOperationLabel;
        private System.Windows.Forms.Button SourceBlobBrowseButton;
        private System.Windows.Forms.TextBox SourceBlobFilename;
        private System.Windows.Forms.Button DestBlobBrowseButton;
        private System.Windows.Forms.TextBox DestBlobFilename;
        private System.Windows.Forms.LinkLabel linkLabel1;
    }
}

