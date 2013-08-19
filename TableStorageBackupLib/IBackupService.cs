using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;

namespace AntsCode.TableStorageBackupLib
{
    [ServiceContract]
    public interface IBackupService
    {
        [OperationContract]
        void StartEndpointToEndpointBackup(string authKey, string sourceConStr, string destConStr);

        [OperationContract]
        void StartEndpointToBlobBackup(string authKey, string sourceConStr, string destConStr, string containerName, string filename);

        [OperationContract]
        void StartBlobToEndpointBackup(string authKey, string sourceConStr, string destConStr, string containerName, string filename);

        [OperationContract]
        void StartBlobToBlobBackup(string authKey, string sourceConStr, string destConStr, string sourceContainerName, string sourceFilename, string destContainerName, string destFilename);
        
        [OperationContract]
        void CancelBackup(string authKey);

        [OperationContract]
        BackupState GetState(string authKey);
    }
}