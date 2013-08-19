using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Security;
using AntsCode.TableStorageBackupLib;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using System.Threading;

namespace AntsCode.TableStorageBackup
{
    public class BackupClient
    {
        private const string tempContainerName = "TableStorageBackupTemp";

        private ChannelFactory<IBackupService> factory;
        private IBackupService service;
        private Backup backup;
        private bool cancelled;
        private string authKey;

        public event EventHandler<ErrorEventArgs> Error;
        public event EventHandler<OperationChangeEventArgs> OperationChange;
        public event EventHandler<ProgressEventArgs> Progress;

        public BackupClient(string endpoint, string authKey, string certificateIdentity)
        {
            // Create a Tcp binding with transport security via x.509 certificate
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);

            // No client credentials are required
            // (The credentials are in the storage account connection strings)
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;

            this.authKey = authKey;

            try
            {
                if (certificateIdentity != null)
                {
                    EndpointAddress address = new EndpointAddress(new Uri(endpoint), new DnsEndpointIdentity(certificateIdentity));
                    this.factory = new ChannelFactory<IBackupService>(binding, address);
                }
                else
                {
                    this.factory = new ChannelFactory<IBackupService>(binding, endpoint);
                }

                // Since certificates do not need to be from a certification authority, set the certificate validation to none
                this.factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;

                this.service = this.factory.CreateChannel();
                
                // Switch this line with the one above to run the backup service locally
                //this.service = new BackupService(authKey);
            }
            catch (Exception)
            {
                // The specified endpoint is invalid
            }


            this.backup = new Backup();
        }

        public void StartEndpointToEndpointBackup(string sourceConStr, string destConStr)
        {
            try
            {
                if (IsDevelopmentStorage(sourceConStr))
                {
                    this.DevEndpointToEndpointBackup(sourceConStr, destConStr);
                }
                else if (IsDevelopmentStorage(destConStr))
                {
                    this.EndpointToDevEndpointBackup(sourceConStr, destConStr);
                }
                else
                {
                    this.EndpointToEndpointBackup(sourceConStr, destConStr);
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public void StartEndpointToBlobBackup(string sourceConStr, string destConStr, string containerName, string filename)
        {
            try
            {
                if (IsDevelopmentStorage(sourceConStr))
                {
                    this.DevEndpointToBlobBackup(sourceConStr, destConStr, containerName, filename);
                }
                else if (IsDevelopmentStorage(destConStr))
                {
                    this.EndpointToDevBlobBackup(sourceConStr, destConStr, containerName, filename);
                }
                else
                {
                    this.EndpointToBlobBackup(sourceConStr, destConStr, containerName, filename, 1);   
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public void StartBlobToEndpointBackup(string sourceConStr, string destConStr, string containerName, string filename)
        {
            try
            {
                if (IsDevelopmentStorage(sourceConStr))
                {
                    this.DevBlobToEndpointBackup(sourceConStr, destConStr, containerName, filename);
                }
                else if (IsDevelopmentStorage(destConStr))
                {
                    this.BlobToDevEndpointBackup(sourceConStr, destConStr, containerName, filename);
                }
                else
                {
                    this.BlobToEndpointBackup(sourceConStr, destConStr, containerName, filename, 1, 1);
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public void StartBlobToBlobBackup(string sourceConStr, string destConStr, string sourceContainerName, string sourceFilename, string destContainerName, string destFilename)
        {
            try
            {
                if (IsDevelopmentStorage(sourceConStr) || IsDevelopmentStorage(destConStr))
                {
                    this.BlobToBlobBackup(sourceConStr, destConStr, sourceContainerName, sourceFilename, destContainerName, destFilename);
                }
                else
                {
                    this.ServerBlobToBlobBackup(sourceConStr, destConStr, sourceContainerName, sourceFilename, destContainerName, destFilename);
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public void StartEndpointToFileBackup(string sourceConStr, string filename)
        {
            try
            {
                if (IsDevelopmentStorage(sourceConStr))
                {
                    this.DevEndpointToFileBackup(sourceConStr, filename, 1);
                }
                else
                {
                    this.EndpointToFileBackup(sourceConStr, filename);
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public void StartBlobToFileBackup(string sourceConStr, string containerName, string blobFilename, string filename)
        {
            try
            {
                this.BlobToFileBackup(sourceConStr, containerName, blobFilename, filename);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public void StartFileToEndpointBackup(string filename, string destConStr)
        {
            try
            {
                if (IsDevelopmentStorage(destConStr))
                {
                    this.FileToDevEndpointBackup(filename, destConStr, 1, 1);
                }
                else
                {
                    this.FileToEndpointBackup(filename, destConStr);
                }
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public void StartFileToBlobBackup(string filename, string destConStr, string containerName, string blobFilename)
        {
            try
            {
                this.FileToBlobBackup(filename, destConStr, containerName, blobFilename);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public void StartFileToFileBackup(string sourceFilename, string destFilename)
        {
            try
            {
                this.FileToFileBackup(sourceFilename, destFilename);
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        public void CancelBackup()
        {
            try
            {
                if (this.backup.State.InProgress)
                {
                    this.backup.CancelBackup();
                }
                else
                {
                    this.service.CancelBackup(this.authKey);
                }

                this.cancelled = true;
            }
            catch (Exception e)
            {
                OnError(e.Message);
            }
        }

        private void EndpointToEndpointBackup(string sourceConStr, string destConStr)
        {
            OnOperationChange(1, 1, "Backing Up");

            this.CheckServiceAvailability();

            this.service.StartEndpointToEndpointBackup(this.authKey, sourceConStr, destConStr);

            // Wait for completion
            this.WaitForServiceCompletion();
        }

        private void DevEndpointToEndpointBackup(string sourceConStr, string destConStr)
        {
            OnOperationChange(1, 3, "Backing Up Development Storage");

            // Perform local backup to file
            string tempFilename = this.GetTempFilename();
            string tempPath = Path.Combine(Path.GetTempPath(), tempFilename);
            DevEndpointToFileBackup(sourceConStr, tempPath, 3);

            OnOperationChange(2, 3, "Uploading Backup File");

            // Upload the file to server
            UploadBlob(destConStr, tempContainerName, tempFilename, tempPath, true, true);

            OnOperationChange(3, 3, "Restoring to Table Endpoint");

            // Start blob to endpoint backup
            this.StartBlobToEndpointBackup(destConStr, destConStr, tempContainerName, tempFilename);

            // Wait for completion
            this.WaitForServiceCompletion();
        }

        private void EndpointToDevEndpointBackup(string sourceConStr, string destConStr)
        {
            // Start endpoint to temp blob backup
            string tempFilename = this.GetTempFilename();
            string tempPath = Path.Combine(Path.GetTempPath(), tempFilename);
            this.EndpointToBlobBackup(sourceConStr, sourceConStr, tempContainerName, tempFilename, 3);

            // Wait for completion
            this.WaitForServiceCompletion();

            OnOperationChange(2, 3, "Downloading Backup File");

            // Download the blob to file
            DownloadBlob(sourceConStr, tempContainerName, tempFilename, tempPath, true, true);

            // Perform file to local endpoint backup
            FileToDevEndpointBackup(tempPath, sourceConStr, 3, 3);

            // Delete the temp file
            File.Delete(tempPath);
        }

        private void DevEndpointToFileBackup(string sourceConStr, string filename, int operationCount)
        {
            OnOperationChange(1, operationCount, "Backing up development storage");

            // Perform local backup
            string tempFilename = Path.GetFileNameWithoutExtension(filename) + ".tsbak";
            var source = new TableStorageDataSource(BackupDirection.Source, sourceConStr);
            var dest = new FileDataSource(BackupDirection.Destination, sourceConStr, tempContainerName, tempFilename);
            this.backup.Start(source, dest, OnProgress);

            // Download the blob to file
            DownloadBlob(sourceConStr, tempContainerName, tempFilename, filename, true, false);
        }

        private void EndpointToFileBackup(string sourceConStr, string filename)
        {
            // Start endpoint to temp blob backup
            string tempFilename = Path.GetFileNameWithoutExtension(filename) + ".tsbak";

            EndpointToBlobBackup(sourceConStr, sourceConStr, tempContainerName, tempFilename, 2);

            // Wait for completion
            this.WaitForServiceCompletion();

            OnOperationChange(2, 2, "Downloading Backup File");

            // Download the blob to file
            DownloadBlob(sourceConStr, tempContainerName, tempFilename, filename, true, true);
        }

        private void EndpointToBlobBackup(string sourceConStr, string destConStr, string containerName, string filename, int operationCount)
        {
            OnOperationChange(1, operationCount, "Backing Up Table Endpoint");

            this.CheckServiceAvailability();

            this.service.StartEndpointToBlobBackup(this.authKey, sourceConStr, destConStr, containerName, filename);

            // Wait for completion
            this.WaitForServiceCompletion();
        }

        private void DevEndpointToBlobBackup(string sourceConStr, string destConStr, string containerName, string filename)
        {
            // Perform dev endpoint to file backup
            string tempFilename = this.GetTempFilename();
            string tempPath = Path.Combine(Path.GetTempPath(), tempFilename);

            DevEndpointToFileBackup(sourceConStr, tempPath, 2);

            OnOperationChange(2, 2, "Uploading Backup File");

            // Upload the blob
            UploadBlob(destConStr, containerName, filename, tempPath, true, true);
        }

        private void EndpointToDevBlobBackup(string sourceConStr, string destConStr, string containerName, string filename)
        {
            // Perform endpoint to file backup
            string tempFilename = this.GetTempFilename();
            string tempPath = Path.Combine(Path.GetTempPath(), tempFilename);

            EndpointToFileBackup(sourceConStr, tempPath);

            // Upload to local blob
            UploadBlob(destConStr, containerName, filename, tempPath, true, false);
        }

        private void BlobToEndpointBackup(string sourceConStr, string destConStr, string containerName, string filename, int currentOperation, int operationCount)
        {
            OnOperationChange(currentOperation, operationCount, "Restoring to Table Endpoint");

            this.CheckServiceAvailability();

            this.service.StartBlobToEndpointBackup(this.authKey, sourceConStr, destConStr, containerName, filename);

            // Wait for completion
            this.WaitForServiceCompletion();
        }

        private void DevBlobToEndpointBackup(string sourceConStr, string destConStr, string containerName, string filename)
        {
            OnOperationChange(1, 2, "Uploading Backup File");
            
            // Download the blob to file
            string tempFilename = this.GetTempFilename();
            string tempPath = Path.Combine(Path.GetTempPath(), tempFilename);

            DownloadBlob(sourceConStr, containerName, filename, tempPath, false, false);

            // Upload blob
            UploadBlob(destConStr, tempContainerName, tempFilename, tempPath, true, true);

            OnOperationChange(2, 2, "Restoring to Table Endpoint");

            // Start blob to endpoint backup
            this.StartBlobToEndpointBackup(destConStr, destConStr, tempContainerName, tempFilename);

            // Wait for completion
            this.WaitForServiceCompletion();

            // Delete the temp blob
            DeleteBlob(destConStr, tempContainerName, tempFilename);
        }

        private void BlobToDevEndpointBackup(string sourceConStr, string destConStr, string containerName, string filename)
        {
            OnOperationChange(1, 2, "Downloading Backup File");

            // Download the blob
            string tempFilename = this.GetTempFilename();
            string tempPath = Path.Combine(Path.GetTempPath(), tempFilename);
            DownloadBlob(sourceConStr, containerName, filename, tempPath, false, true);

            // Perform file to dev endpoint backup
            FileToDevEndpointBackup(tempPath, destConStr, 2, 2);

            // Delete the temp file
            File.Delete(tempPath);
        }

        private void ServerBlobToBlobBackup(string sourceConStr, string destConStr, string sourceContainerName, string sourceFilename, string destContainerName, string destFilename)
        {
            OnOperationChange(1, 1, "Copying Backup File");

            this.CheckServiceAvailability();

            this.service.StartBlobToBlobBackup(this.authKey, sourceConStr, destConStr, sourceContainerName, sourceFilename, destContainerName, destFilename);

            // Wait for completion
            this.WaitForServiceCompletion();
        }

        private void BlobToBlobBackup(string sourceConStr, string destConStr, string sourceContainerName, string sourceFilename, string destContainerName, string destFilename)
        {
            OnOperationChange(1, 1, "Copying Backup File");

            // Download the blob to file
            string tempFilename = this.GetTempFilename();
            string tempPath = Path.Combine(Path.GetTempPath(), tempFilename);

            DownloadBlob(sourceConStr, sourceContainerName, sourceFilename, tempPath, false, true);

            // Upload the file
            UploadBlob(destConStr, destContainerName, destFilename, tempPath, true, true);
        }

        private void BlobToFileBackup(string sourceConStr, string containerName, string blobFilename, string filename)
        {
            OnOperationChange(1, 1, "Downloading Backup File");
            
            DownloadBlob(sourceConStr, containerName, blobFilename, filename, false, true);
        }

        private void FileToDevEndpointBackup(string filename, string sourceConStr, int operationNo, int operationCount)
        {
            OnOperationChange(operationNo, operationCount, "Restoring to Development Storage");

            // Upload file to blob
            string tempFilename = this.GetTempFilename();
            UploadBlob(sourceConStr, tempContainerName, tempFilename, filename, false, false);

            // Perform local backup
            var source = new FileDataSource(BackupDirection.Source, sourceConStr, tempContainerName, tempFilename);
            var dest = new TableStorageDataSource(BackupDirection.Destination, sourceConStr);

            this.backup.Start(source, dest, OnProgress);
        }

        private void FileToEndpointBackup(string filename, string destConStr)
        {
            OnOperationChange(1, 2, "Uploading Backup File");

            // Upload file to blob
            string tempFilename = this.GetTempFilename();
            UploadBlob(destConStr, tempContainerName, tempFilename, filename, false, true);

            // Perform blob to endpoint backup
            this.BlobToEndpointBackup(destConStr, destConStr, tempContainerName, tempFilename, 2, 2);

            // Wait for completion
            this.WaitForServiceCompletion();

            // Delete the temp blob
            DeleteBlob(destConStr, tempContainerName, tempFilename);
        }

        private void FileToBlobBackup(string filename, string destConStr, string containerName, string blobFilename)
        {
            OnOperationChange(1, 1, "Uploading Backup File");

            UploadBlob(destConStr, containerName, blobFilename, filename, false, true);
        }

        private void FileToFileBackup(string sourceFilename, string destFilename)
        {
            OnOperationChange(1, 1, "Copying Backup File");

            File.Copy(sourceFilename, destFilename);
        }

        private void WaitForServiceCompletion()
        {
            BackupState state;

            do
            {
                state = this.service.GetState(this.authKey);
                OnProgress(state.ProgressPercentage);
                Thread.Sleep(1000);
            }
            while (state.InProgress);

            if (state.Error != null)
            {
                OnError(state.Error);
            }
            else
            {
                OnProgress(100);
            }
        }

        private string GetTempFilename()
        {
            return Guid.NewGuid().ToString("N") + ".tsbak";
        }

        private void DownloadBlob(string connectionString, string containerName, string blobFilename, string filename, bool deleteAfterDownload, bool updateProgress)
        {
            if (!this.cancelled)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName.ToLower());
                CloudBlockBlob blob = container.GetBlockBlobReference(blobFilename);
                int progressPercentage;
               
                // Open a stream for writing
                using (BlobStream inStream = blob.OpenRead())
                using (FileStream outStream = new FileStream(filename, FileMode.Create))
                {
                    // Get the attributes for this blob
                    blob.FetchAttributes();

                    long totalSize = blob.Properties.Length;
                    int bufferSize = 4096;
                    long totalRead = 0;
                    byte[] buffer = new byte[bufferSize];
                    while (true)
                    {
                        int read = inStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            break;
                        }
                        outStream.Write(buffer, 0, read);

                        totalRead += read;

                        if (updateProgress)
                        {
                            progressPercentage = (int)(((double)totalRead / (double)totalSize) * 100);
                            OnProgress(progressPercentage);
                        }
                    }
                };

                if (deleteAfterDownload)
                {
                    blob.Delete();
                }
            }
        }

        private void UploadBlob(string connectionString, string containerName, string blobFilename, string filename, bool deleteAfterUpload, bool updateProgress)
        {
            if (!this.cancelled)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName.ToLower());
                int progressPercentage;

                // Create the container if it doesn't exist
                container.CreateIfNotExist();

                CloudBlockBlob blob = container.GetBlockBlobReference(blobFilename);

                // Open a str3am for writing
                using (FileStream inStream = new FileStream(filename, FileMode.Open))
                using (BlobStream outStream = blob.OpenWrite())
                {
                    long totalSize = inStream.Length;
                    long totalRead = 0;
                    int bufferSize = 4096;
                    byte[] buffer = new byte[bufferSize];
                    while (true)
                    {
                        int read = inStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            break;
                        }
                        outStream.Write(buffer, 0, read);

                        totalRead += read;

                        if (updateProgress)
                        {
                            progressPercentage = (int)(((double)totalRead / (double)totalSize) * 100);
                            OnProgress(progressPercentage);
                        }
                    }
                };

                if (deleteAfterUpload)
                {
                    File.Delete(filename);
                }
            }
        }

        private void DeleteBlob(string connectionString, string containerName, string blobFilename)
        {
            if (!this.cancelled)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName.ToLower());
                CloudBlockBlob blob = container.GetBlockBlobReference(blobFilename);

                blob.Delete();
            }
        }

        private bool IsDevelopmentStorage(string connectionString)
        {
            bool isDevelopmentStorage = false;

            if (connectionString.IndexOf("UseDevelopmentStorage=true") > -1)
            {
                isDevelopmentStorage = true;
            }

            return isDevelopmentStorage;
        }

        private void OnError(string message)
        {
            if (Error != null)
            {
                Error(this, new ErrorEventArgs()
                {
                    Message = message
                });
            }
        }

        private void OnOperationChange(int operationNo, int operationCount, string operation)
        {
            OperationChange(this, new OperationChangeEventArgs()
            {
                OperationNo = operationNo,
                OperationCount = operationCount,
                Operation = operation
            });
        }

        private void OnProgress(int progressPercentage)
        {
            Progress(this, new ProgressEventArgs()
            {
                ProgressPercentage = progressPercentage
            });
        }

        private void CheckServiceAvailability()
        {
            if (this.service == null)
            {
                throw new ApplicationException("Could not connect to Backup Service, check your connection settings.");
            }
        }

        public class ErrorEventArgs : EventArgs
        {
            public string Message { get; set; }
        }

        public class OperationChangeEventArgs : EventArgs
        {
            public string Operation { get; set; }
            public int OperationNo { get; set; }
            public int OperationCount { get; set; }
        }

        public class ProgressEventArgs : EventArgs
        {
            public int ProgressPercentage { get; set; }
        }
    }
}