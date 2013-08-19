using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceModel;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.ServiceModel.Description;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;

namespace AntsCode.TableStorageBackupLib
{
    [ServiceBehavior(
        AddressFilterMode = AddressFilterMode.Any, // AddressFilterMode = Any is for known issue with WCF & Windows Azure
        InstanceContextMode = InstanceContextMode.Single,
        IncludeExceptionDetailInFaults = true)]
    public class BackupService : IBackupService
    {
        private Backup backup;
        private string authKey;

        public BackupService(string authKey)
        {
            this.authKey = authKey;
            this.backup = new Backup();
        }

        public void StartEndpointToEndpointBackup(string authKey, string sourceConStr, string destConStr)
        {
            if (this.ValidateAuthKey(authKey))
            {
                var source = new TableStorageDataSource(BackupDirection.Source, sourceConStr);
                var dest = new TableStorageDataSource(BackupDirection.Destination, destConStr);

                this.backup.StartAsync(source, dest);
            }
        }

        public void StartEndpointToBlobBackup(string authKey, string sourceConStr, string destConStr, string containerName, string filename)
        {
            if (this.ValidateAuthKey(authKey))
            {
                var source = new TableStorageDataSource(BackupDirection.Source, sourceConStr);
                var dest = new FileDataSource(BackupDirection.Destination, destConStr, containerName, filename);

                this.backup.StartAsync(source, dest);
            }
        }

        public void StartBlobToEndpointBackup(string authKey, string sourceConStr, string destConStr, string containerName, string filename)
        {
            if (this.ValidateAuthKey(authKey))
            {
                var source = new FileDataSource(BackupDirection.Source, sourceConStr, containerName, filename);
                var dest = new TableStorageDataSource(BackupDirection.Destination, destConStr);

                this.backup.StartAsync(source, dest);
            }
        }

        public void StartBlobToBlobBackup(string authKey, string sourceConStr, string destConStr, string sourceContainerName, string sourceFilename, string destContainerName, string destFilename)
        {
            if (this.ValidateAuthKey(authKey))
            {
                CloudStorageAccount sourceAccount = CloudStorageAccount.Parse(sourceConStr);
                CloudStorageAccount destAccount = CloudStorageAccount.Parse(destConStr);

                CloudBlobClient sourceClient = sourceAccount.CreateCloudBlobClient();
                CloudBlobClient destClient = destAccount.CreateCloudBlobClient();

                CloudBlobContainer sourceContainer = sourceClient.GetContainerReference(sourceContainerName.ToLower());
                CloudBlobContainer destContainer = destClient.GetContainerReference(destContainerName.ToLower());

                CloudBlockBlob sourceBlob = sourceContainer.GetBlockBlobReference(sourceFilename);
                CloudBlockBlob destBlob = sourceContainer.GetBlockBlobReference(destFilename);

                // Copy the source blob to the destination
                destBlob.CopyFromBlob(sourceBlob);
            }
        }

        public void CancelBackup(string authKey)
        {
            if (this.ValidateAuthKey(authKey))
            {
                this.backup.CancelBackup();
            }
        }

        public BackupState GetState(string authKey)
        {
            if (this.ValidateAuthKey(authKey))
            {
                return this.backup.State;
            }
            else
            {
                return null;
            }
        }

        private bool ValidateAuthKey(string authKey)
        {
            if (authKey == this.authKey)
            {
                return true;
            }
            else
            {
                throw new ApplicationException("Invalid authentication key.");
            }
        }

        public static ServiceHost GetServiceHost(BindingType bindingType, string externalEndpoint, string authKey, StoreLocation storeLocation, StoreName storeName, string subjectName)
        {
            // Create an instance of the backup service
            BackupService backupService = new BackupService(authKey);

            // Add instance to service host
            ServiceHost serviceHost = new ServiceHost(backupService);

            // Create binding with transport security
            Binding binding;
            string protocol;

            switch (bindingType)
            {
                case BindingType.Https:
                    WebHttpBinding httpBinding = new WebHttpBinding(WebHttpSecurityMode.Transport);
                    httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                    binding = httpBinding;
                    protocol = "https";
                    break;

                default: //Tcp
                    NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.Transport);
                    tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
                    binding = tcpBinding;
                    protocol = "net.tcp";
                    break;
            }          

            // Add the service endpoint
            ServiceEndpoint ep = serviceHost.AddServiceEndpoint(
               typeof(IBackupService),
               binding,
               String.Format(protocol + "://{0}/BackupService", externalEndpoint));

            // Set the x.509 certificate
            serviceHost.Credentials.ServiceCertificate.SetCertificate(
                storeLocation,
                storeName,
                X509FindType.FindBySubjectName,
                subjectName);

            return serviceHost;
        }
    }
}