using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TableStorageBackup;
using System.Reflection;

namespace AntsCode.TableStorageBackup
{
    public partial class OptionsForm : Form
    {
        private BackupForm parentForm;
        private BackupSettings settings;

        public OptionsForm(BackupForm parentForm, BackupSettings settings)
        {
            InitializeComponent();

            this.parentForm = parentForm;
            this.settings = settings;

            this.ServerAddressTextBox.Text = settings.ServerUrl;
            this.AuthenticationKeyTextBox.Text = settings.AuthenticationKey;
            this.CertificateIdentityTextBox.Text = settings.CertificateIdentity;

            // Set version label
            this.VersionLabel.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Populate storage accounts
            bool https;

            foreach(BackupStorageAccount storageAccount in settings.StorageAccounts)
            {
                if (storageAccount.Protocol == "Https")
                {
                    https = true;
                }
                else
                {
                    https = false;
                }

                this.StorageAccountDataGridView.Rows.Add(storageAccount.Label, storageAccount.AccountName, storageAccount.AccountKey, https);
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            List<BackupStorageAccount> storageAccounts = new List<BackupStorageAccount>();

            // Update settings
            string protocol;
            foreach (DataGridViewRow row in this.StorageAccountDataGridView.Rows)
            {
                if (row.Cells["Label"].Value != null &&
                    row.Cells["AccountName"].Value != null &&
                    row.Cells["AccountKey"].Value != null)
                {
                    protocol = "Http";

                    if (row.Cells["Https"].Value != null && (bool)row.Cells["Https"].Value)
                    {
                        protocol = "Https";
                    }

                    storageAccounts.Add(new BackupStorageAccount()
                    {
                        Label = (string)row.Cells["Label"].Value,
                        AccountName = (string)row.Cells["AccountName"].Value,
                        AccountKey = (string)row.Cells["AccountKey"].Value,
                        Protocol = protocol
                    });
                }
            }

            // Update settings & save
            this.settings.ServerUrl = ServerAddressTextBox.Text;
            this.settings.AuthenticationKey = AuthenticationKeyTextBox.Text;

            if (CertificateIdentityTextBox.Text.Trim().Length > 0)
            {
                this.settings.CertificateIdentity = CertificateIdentityTextBox.Text;
            }
            else
            {
                this.settings.CertificateIdentity = null;
            }

            this.settings.StorageAccounts = storageAccounts.ToArray();
            this.settings.SaveSettings();

            // Update storage accounts in parent form
            this.parentForm.UpdateStorageAccounts();

            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://tablestoragebackup.codeplex.com/documentation");
        }
    }
}