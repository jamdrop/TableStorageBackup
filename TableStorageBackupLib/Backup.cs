using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace AntsCode.TableStorageBackupLib
{
    public class Backup
    {
        private delegate void StartDelegate(BackupDataSource source, BackupDataSource destination, OnErrorDelegate onError);
        private delegate void OnErrorDelegate(Exception e);
        public delegate void ProgressChangeDelegate(int progressPercentage);
        
        private BackupDataSource source;
        private ProgressChangeDelegate progressChange;

        public Backup()
        {
            this.State = new BackupState()
            {
                ProgressPercentage = 0,
                InProgress = false
            };
        }

        public void StartAsync(BackupDataSource source, BackupDataSource destination)
        {
            StartDelegate startDelegate = new StartDelegate(StartInternal);
            startDelegate.BeginInvoke(source, destination, OnError, null, null);
        }

        private void StartInternal(BackupDataSource source, BackupDataSource destination, OnErrorDelegate onError)
        {
            try
            {
                this.Start(source, destination, null);
            }
            catch (Exception e)
            {
                // Pass the exception to the calling thread
                onError(e);
            }
        }

        public void Start(BackupDataSource source, BackupDataSource destination, ProgressChangeDelegate progressChange)
        {
            this.source = source;

            if (this.State.InProgress)
            {
                throw new ApplicationException("A backup operation is already in progress.");
            }

            this.State.InProgress = true;
            this.State.ProgressPercentage = 0;
            this.progressChange = progressChange;

            // Transfer the data from the source to the destination
            source.TransferData(destination, ProgressChange);

            this.State.InProgress = false;
        }

        private void ProgressChange(int progressPercentage)
        {
            this.State.ProgressPercentage = progressPercentage;

            if (this.progressChange != null)
            {
                this.progressChange(progressPercentage);
            }
        }

        private void OnError(Exception e)
        {
            this.State.InProgress = false;
            this.State.Error = e.Message;
        }

        public BackupState State
        {
            get;
            private set;
        }

        public void CancelBackup()
        {
            if (this.source != null)
            {
                this.source.Cancelled = true;
                this.State.InProgress = false;
            }
        }
    }
}