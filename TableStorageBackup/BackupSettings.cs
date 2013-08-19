using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace AntsCode.TableStorageBackup
{
    public class BackupSettings
    {
        private const string prefsFilename = "Prefs.txt";
        private const string key = "sdj54745hgf73hgr325huv59";
        private IsolatedStorageFile isoStore;

        public BackupSettings()
        {
            this.StorageAccounts = new BackupStorageAccount[0];
        }

        public void LoadSettings()
        {
            // Look for settings in isolated storage
            this.isoStore = IsolatedStorageFile.GetUserStoreForAssembly();

            //this.isoStore.DeleteFile("Prefs.txt");

            string[] files = this.isoStore.GetFileNames(prefsFilename);

            if (files.Length > 0)
            {
                using (StreamReader reader = new StreamReader(new IsolatedStorageFileStream(prefsFilename, FileMode.Open, this.isoStore)))
                {
                    // Decrypt the stream
                    string decrypted = Encryption.Decrypt(reader.ReadToEnd(), key, false);
                    StringReader sr = new StringReader(decrypted);

                    // Deserialize the settings
                    XmlSerializer serializer = new XmlSerializer(this.GetType());
                    BackupSettings settings = (BackupSettings)serializer.Deserialize(sr);

                    // Set properties
                    this.ServerUrl = settings.ServerUrl;
                    this.AuthenticationKey = settings.AuthenticationKey;
                    this.CertificateIdentity = settings.CertificateIdentity;
                    this.StorageAccounts = settings.StorageAccounts;
                }
            }
        }

        public void SaveSettings()
        {
            StringWriter sw = new StringWriter();

            // Serialise the current instance to a string
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            serializer.Serialize(sw, this);

            // Encrypt it
            string encrypted = Encryption.Encrypt(sw.ToString(), key, false);

            using (StreamWriter writer = new StreamWriter(new IsolatedStorageFileStream(prefsFilename, FileMode.Create, this.isoStore)))
            {
                writer.Write(encrypted);
                writer.Close();
            }
        }

        public string GetConnectionString(string label)
        {
            if (label == "Development Storage")
            {
                return "UseDevelopmentStorage=true";
                //return "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://ipv4.fiddler";
            }
            else
            {
                var storageAccount = this.StorageAccounts.Where(s => s.Label == label).First();

                return
                    "DefaultEndpointsProtocol=" + storageAccount.Protocol.ToLower() + ";" +
                    "AccountName=" + storageAccount.AccountName + ";" +
                    "AccountKey=" + storageAccount.AccountKey;
            }
        }

        public string ServerUrl
        {
            get;
            set;
        }

        public string AuthenticationKey
        {
            get;
            set;
        }

        public string CertificateIdentity
        {
            get;
            set;
        }

        public BackupStorageAccount[] StorageAccounts
        {
            get;
            set;
        }
    }
}
