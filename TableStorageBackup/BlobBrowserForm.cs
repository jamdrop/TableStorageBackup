using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;

namespace AntsCode.TableStorageBackup
{
    public partial class BlobBrowserForm : Form
    {
        private string connectionString;
        private CloudBlobClient blobClient;
        private delegate void GetContainersDelegate();
        private delegate void SetContainersDelegate(IEnumerable<CloudBlobContainer> containers);
        private delegate void GetBlobsDelegate(string containerName);
        private delegate void SetBlobsDelegate(IEnumerable<IListBlobItem> blobs);

        public BlobBrowserForm(string connectionString)
        {
            InitializeComponent();

            this.connectionString = connectionString;
        }

        private void BlobBrowserForm_Load(object sender, EventArgs e)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.connectionString);
                this.blobClient = storageAccount.CreateCloudBlobClient();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to storage account:\n\n" + ex.Message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            this.ContainerList.Items.Add("Loading containers...");

            GetContainersDelegate getContainersDelegate = GetContainers;
            getContainersDelegate.BeginInvoke(null, null);
        }

        private void GetContainers()
        {
            IEnumerable<CloudBlobContainer> containers = null;

            try
            {
                containers = this.blobClient.ListContainers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not get containers:\n\n" + ex.Message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }

            this.Invoke(new SetContainersDelegate(SetContainers), containers);
        }

        private void SetContainers(IEnumerable<CloudBlobContainer> containers)
        {
            this.ContainerList.Items.Clear();

            foreach (var container in containers)
            {
                this.ContainerList.Items.Add(container.Name);
            }
        }

        private void ContainerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the blobs for this container
            GetBlobsDelegate getBlobsDelegate = GetBlobs;

            this.BlobList.Items.Clear();
            this.BlobList.Items.Add("Loading blobs...");

            getBlobsDelegate.BeginInvoke(this.ContainerList.FocusedItem.Text, null, null);
        }

        private void GetBlobs(string containerName)
        {
            CloudBlobContainer container = this.blobClient.GetContainerReference(containerName);
            var blobs = container.ListBlobs();

            this.Invoke(new SetBlobsDelegate(SetBlobs), blobs);
        }

        private void SetBlobs(IEnumerable<IListBlobItem> blobs)
        {
            this.BlobList.Items.Clear();

            foreach (var blob in blobs)
            {
                if (Path.GetExtension(blob.Uri.AbsolutePath) == ".tsbak")
                {
                    this.BlobList.Items.Add(Path.GetFileName(blob.Uri.AbsolutePath));
                }
            }
        }

        public string SelectedBlob
        {
            get
            {
                if (this.ContainerList.SelectedItems.Count == 1 && this.BlobList.SelectedItems.Count == 1)
                {
                    return Path.Combine(this.ContainerList.SelectedItems[0].Text, this.BlobList.SelectedItems[0].Text);
                }
                else
                {
                    return null;
                }
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}