using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntsCode.TableStorageBackup
{
    public class BackupStorageAccount
    {
        public string Label { get; set; }
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string Protocol { get; set; }
    }
}
