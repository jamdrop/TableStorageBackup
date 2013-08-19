using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntsCode.TableStorageBackupLib
{
    public class BackupState
    {
        public int ProgressPercentage { get; set; }
        public bool InProgress { get; set; }
        public string Error { get; set; }
    }
}
