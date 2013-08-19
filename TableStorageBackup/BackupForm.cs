using System;
using System.Windows.Forms;
using AntsCode.TableStorageBackup;
using System.IO;
using AntsCode.TableStorageBackupLib;
using System.ServiceModel;
using System.Threading;
using System.Drawing;

namespace TableStorageBackup
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public partial class BackupForm : Form
    {
        private const string developmentStorageLabel = "Development Storage";

        private BackupSettings settings;
        private delegate void StartEvent();
        private delegate void EndEvent();
        private delegate void OperationChangeEvent(int operationNo, int operationCount, string operation);
        private delegate void ProgressEvent(int progressPercentage);
        private delegate void BackupCompleteEvent();
        private BackupClient backupClient;
        private StartEvent startEvent;
        private bool cancelled;
        private bool error;

        public BackupForm()
        {
            InitializeComponent();

            // Load current settings
            this.settings = new BackupSettings();
            settings.LoadSettings();

            // Populate storage accounts
            UpdateStorageAccounts();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.CurrentOperationLabel.Text = "Cancelling...";
            backupClient.CancelBackup();
            this.cancelled = true;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            // Initialise backup client
            backupClient = new BackupClient(this.settings.ServerUrl, this.settings.AuthenticationKey, this.settings.CertificateIdentity);
            backupClient.Error += (s, er) => { ShowError(er.Message); };
            backupClient.OperationChange += (s, op) => { this.Invoke(new OperationChangeEvent(UpdateCurrentOperation), new object[] { op.OperationNo, op.OperationCount, op.Operation}); };
            backupClient.Progress += (s, pr) => { this.Invoke(new ProgressEvent(UpdateProgress), new object[] { pr.ProgressPercentage}); };

            // Toggle Start & Cancel buttons
            this.StartButton.Enabled = false;
            this.CancelButton.Enabled = true;

            // Reset progress bar
            this.StatusProgressBar.Value = 0;

            // Start backup operating async
            this.startEvent.BeginInvoke(OnBackupComplete, null);

            // Reset the cancelled & error flags
            this.cancelled = false;
            this.error = false;
        }

        public void OnBackupComplete(IAsyncResult result)
        {
            this.Invoke(new BackupCompleteEvent(EndBackup));
        }

        public void EndBackup()
        {
            string status;

            if (this.error)
            {
                status = "Operation failed.";
            }
            else if (this.cancelled)
            {
                status = "Operation cancelled.";
            }
            else
            {
                status = "Operation complete.";
            }

            this.CurrentOperationLabel.Text = status;
            this.StartButton.Enabled = true;
            this.CancelButton.Enabled = false;
        }

        public void UpdateCurrentOperation(int operationNo, int operationCount, string operation)
        {
            this.CurrentOperationLabel.Text = operationNo + " of " + operationCount + ": " + operation + "...";
        }

        public void UpdateProgress(int progressPercentage)
        {
            this.StatusProgressBar.Value = progressPercentage;
        }

        public void UpdateStorageAccounts()
        {
            this.SetStorageAccounts(this.SourceStorageAccount);
            this.SetStorageAccounts(this.DestStorageAccount);
        }

        private void SetStorageAccounts(ComboBox comboBox)
        {
            comboBox.Items.Clear();

            foreach (BackupStorageAccount storageAccount in this.settings.StorageAccounts)
            {
                comboBox.Items.Add(storageAccount.Label);
            }

            // Add development storage
            comboBox.Items.Add(developmentStorageLabel);
        }

        private void SetStartState(object sender, EventArgs e)
        {
            // Assume disabled by default
            bool startState = false;
            string sourceAccount = (string)SourceStorageAccount.SelectedItem;
            string destAccount = (string)DestStorageAccount.SelectedItem;
            string sourceBlobFilename = SourceBlobFilename.Text;
            string destBlobFilename = DestBlobFilename.Text;
            string sourceLocalFilename = SourceLocalFilename.Text;
            string destLocalFilename = DestLocalFilename.Text;

            if (SourceTableEndpoint.Checked && DestTableEndpoint.Checked)
            {
                // Endpoint to endpoint backup
                if (IsValidStorageAccount(sourceAccount) && IsValidStorageAccount(destAccount))
                {
                    if (sourceAccount != destAccount)
                    {
                        startState = true;
                        startEvent = delegate()
                        {
                            if (this.ConfirmRestore(destAccount))
                            {
                                backupClient.StartEndpointToEndpointBackup(
                                    settings.GetConnectionString(sourceAccount),
                                    settings.GetConnectionString(destAccount));
                            }
                        };
                    }
                }
            }
            else if (SourceTableEndpoint.Checked && DestBlobFile.Checked)
            {
                // Endpoint to blob backup
                if (IsValidStorageAccount(sourceAccount) && IsValidBlob(destBlobFilename) &&
                    IsValidStorageAccount(destAccount))
                {
                    startState = true;
                    startEvent = delegate()
                    {
                        backupClient.StartEndpointToBlobBackup(
                            settings.GetConnectionString(sourceAccount),
                            settings.GetConnectionString(destAccount),
                            GetContainerName(destBlobFilename),
                            GetBlobName(destBlobFilename));
                    };
                }
            }
            else if (SourceTableEndpoint.Checked && DestLocalFile.Checked)
            {
                // Endpoint to file backup
                if (IsValidStorageAccount(sourceAccount) && IsValidBackupPath(destLocalFilename))
                {
                    startState = true;
                    startEvent = delegate()
                    {
                        backupClient.StartEndpointToFileBackup(
                            settings.GetConnectionString(sourceAccount),
                            destLocalFilename);
                    };
                }
            }
            else if (SourceBlobFile.Checked && DestTableEndpoint.Checked)
            {
                // Blob to endpoint backup
                if (IsValidStorageAccount(sourceAccount) && IsValidBlob(sourceBlobFilename) && IsValidStorageAccount(destAccount))
                {
                    startState = true;
                    startEvent = delegate()
                    {
                        if (this.ConfirmRestore(destAccount))
                        {
                            backupClient.StartBlobToEndpointBackup(
                                settings.GetConnectionString(sourceAccount),
                                settings.GetConnectionString(destAccount),
                                GetContainerName(sourceBlobFilename),
                                GetBlobName(sourceBlobFilename));
                        }
                    };
                }
            }
            else if (SourceBlobFile.Checked && DestBlobFile.Checked)
            {
                // Blob to blob backup
                if (IsValidStorageAccount(sourceAccount) && IsValidBlob(sourceBlobFilename) &&
                    IsValidStorageAccount(destAccount) && IsValidBlob(destBlobFilename))
                {
                    string sourceContainer = GetContainerName(sourceBlobFilename);
                    string destContainer = GetContainerName(destBlobFilename);
                    string sourceFilename = GetBlobName(sourceBlobFilename);
                    string destFilename = GetBlobName(destBlobFilename);

                    if ((sourceContainer != destContainer) && (sourceFilename != destFilename))
                    {
                        startState = true;
                        startEvent = delegate()
                        {
                            backupClient.StartBlobToBlobBackup(
                                settings.GetConnectionString(sourceAccount),
                                settings.GetConnectionString(destAccount),
                                sourceContainer,
                                sourceFilename,
                                destContainer,
                                destFilename);
                        };
                    }
                }
            }
            else if (SourceBlobFile.Checked && DestLocalFile.Checked)
            {
                // Blob to file backup
                if (IsValidStorageAccount(sourceAccount) && IsValidBlob(sourceBlobFilename) && 
                    IsValidBackupPath(destLocalFilename))
                {
                    startState = true;
                    startEvent = delegate()
                    {
                        backupClient.StartBlobToFileBackup(
                            settings.GetConnectionString(sourceAccount),
                            GetContainerName(sourceBlobFilename),
                            GetBlobName(sourceBlobFilename),
                            destLocalFilename);
                    };
                }
            }
            else if (SourceLocalFile.Checked && DestTableEndpoint.Checked)
            {
                // File to endpoint backup
                if (IsValidBackupFile(sourceLocalFilename) && IsValidStorageAccount(destAccount))
                {
                    startState = true;
                    startEvent = delegate()
                    {
                        if (this.ConfirmRestore(destAccount))
                        {
                            backupClient.StartFileToEndpointBackup(
                                sourceLocalFilename,
                                settings.GetConnectionString(destAccount));
                        }
                    };
                }
            }
            else if (SourceLocalFile.Checked && DestBlobFile.Checked)
            {
                // File to blob backup
                if (IsValidBackupFile(sourceLocalFilename) && IsValidBlob(destBlobFilename) &&
                    IsValidStorageAccount(destAccount))
                {
                    startState = true;
                    startEvent = delegate()
                    {
                        backupClient.StartFileToBlobBackup(
                            sourceLocalFilename,
                            settings.GetConnectionString((string)destAccount),
                            GetContainerName(destBlobFilename),
                            GetBlobName(destBlobFilename));
                    };
                }
            }
            else if (SourceLocalFile.Checked && DestLocalFile.Checked)
            {
                // File to file backup
                if (IsValidBackupFile(sourceLocalFilename) && IsValidBackupFile(destLocalFilename))
                {
                    if (sourceLocalFilename != destLocalFilename)
                    {
                        startState = true;
                        startEvent = delegate()
                        {
                            backupClient.StartFileToFileBackup(
                                sourceLocalFilename,
                                destLocalFilename);
                        };
                    }
                }
            }

            StartButton.Enabled = startState;

            if (StartButton.Enabled)
            {
                StartButton.Focus();
            }
        }

        private bool ConfirmRestore(string accountName)
        {
            bool confirmed = false;

            if (MessageBox.Show("You are about to overwrite the storage account \"" + accountName + "\". This cannot be undone!\n\nDo you want to continue?", "Confirm Restore", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                confirmed = true;
            }

            return confirmed;
        }

        private void CopyStream(Stream input, Stream output)
        {
            int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    return;
                }
                output.Write(buffer, 0, read);
            }
        }

        private bool IsValidStorageAccount(object item)
        {
            bool validStorageAccount = false;

            if (!string.IsNullOrEmpty((string)item))
            {
                validStorageAccount = true;
            }

            return validStorageAccount;
        }

        private bool IsValidBlob(string path)
        {
            bool validBlob = false;

            if(path.Split(@"\".ToCharArray()).Length == 2)
            {
                validBlob = true;
            }

            return validBlob;
        }

        private bool IsValidBackupFile(string filePath)
        {
            bool validFile = false;

            if (File.Exists(filePath))
            {
                validFile = true;
            }

            return validFile;
        }

        private bool IsValidBackupPath(string filePath)
        {
            bool validFile = false;

            try
            {
                string directory = Path.GetDirectoryName(filePath);

                if (Directory.Exists(directory))
                {
                    validFile = true;
                }
            }
            catch (Exception)
            { }

            return validFile;
        }

        private string GetContainerName(string path)
        {
            return path.Split(@"\".ToCharArray())[0];
        }

        private string GetBlobName(string path)
        {
            return path.Split(@"\".ToCharArray())[1];
        }

        private void OptionsButton_Click(object sender, EventArgs e)
        {
            OptionsForm optionsForm = new OptionsForm(this, this.settings);

            optionsForm.Show();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SourceBrowseButton_Click(object sender, EventArgs e)
        {
            this.SourceOpenFileDialog.ShowDialog();
            this.SourceLocalFilename.Text = this.SourceOpenFileDialog.FileName;
            this.SourceLocalFile.Checked = true;
        }

        private void SourceBlobBrowseButton_Click(object sender, EventArgs e)
        {
            string conStr = this.settings.GetConnectionString((string)SourceStorageAccount.SelectedItem);

            BlobBrowserForm containerForm = new BlobBrowserForm(conStr);
            containerForm.ShowDialog();

            this.SourceBlobFilename.Text = containerForm.SelectedBlob;
            this.SourceBlobFile.Checked = true;
        }

        private void DestLocalFileBrowse_Click(object sender, EventArgs e)
        {
            this.DestinationFolderBrowserDialog.ShowDialog();
            this.DestLocalFilename.Text = GetDefaultFileName(this.DestinationFolderBrowserDialog.SelectedPath);
            this.DestLocalFile.Checked = true;
        }

        private void DestBlobBrowseButton_Click(object sender, EventArgs e)
        {
            string conStr = this.settings.GetConnectionString((string)DestStorageAccount.SelectedItem);

            ContainerBrowserForm containerForm = new ContainerBrowserForm(conStr);
            containerForm.ShowDialog();

            if (containerForm.SelectedContainer != null)
            {
                this.DestBlobFilename.Text = GetDefaultFileName(containerForm.SelectedContainer);
                this.DestBlobFile.Checked = true;
            }
        }

        private string GetDefaultFileName(string basePath)
        {
            string defaultFileName = DateTime.Now.ToString("yyyy.MM.dd") + ".tsbak";
            return Path.Combine(basePath, defaultFileName);
        }

        private void ShowError(string message)
        {
            this.error = true;
            MessageBox.Show("The server returned the following error:\n\n" + message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowContainerDefault(object sender, EventArgs e)
        {
            this.ShowLabel(sender, "Container");
        }

        private void ShowFilenameDefault(object sender, EventArgs e)
        {
            this.ShowLabel(sender, "Filename");
        }

        private void HideContainerDefault(object sender, EventArgs e)
        {
            this.HideLabel(sender, "Container");
        }

        private void HideFilenameDefault(object sender, EventArgs e)
        {
            this.HideLabel(sender, "Filename");
        }

        private void ShowLabel(object sender, string label)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Trim().Length == 0)
            {
                textBox.Text = label;
                textBox.ForeColor = Color.Gray;
            }
        }

        private void HideLabel(object sender, string label)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text == label)
            {
                textBox.Text = "";
                textBox.ForeColor = Color.Black;
            }
        }

        private void SourceStorageAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SourceBlobBrowseButton.Enabled = IsValidStorageAccount(this.SourceStorageAccount.SelectedItem);
            this.SetStartState(sender, e);
        }

        private void DestStorageAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.DestBlobBrowseButton.Enabled = IsValidStorageAccount(this.DestStorageAccount.SelectedItem);
            this.SetStartState(sender, e);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://antscode.blogspot.com");
        }
    }
}