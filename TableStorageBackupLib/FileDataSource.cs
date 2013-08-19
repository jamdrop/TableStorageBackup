using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace AntsCode.TableStorageBackupLib
{
    public class FileDataSource : BackupDataSource
    {
        private string filename;
        private string xmlFilename;
        private CloudBlobContainer container;
        private XmlTextWriter writer;
        private StreamReader inStream;
        private BlockTransfer blockTransfer;
        private ProgressChange progressChange;

        public FileDataSource(BackupDirection direction, string connectionString, string containerName, string filename) :
            base(direction)
        {
            this.filename = filename;

            // Ensure the file has a tsbak extension
            if (Path.GetExtension(filename) != ".tsbak")
            {
                Path.ChangeExtension(filename, ".tsbak");
            }

            this.xmlFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".xml");

            // Get blob container
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            this.container = blobClient.GetContainerReference(containerName.ToLower()); // Container names must be lowercase

            // Ensure the container exists
            this.container.CreateIfNotExist();

            // Ensure the container is private
            this.container.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Off });
        }

        public override void TransferData(BackupDataSource destination, ProgressChange progressChange)
        {
            // Get the backup file from blob
            CloudBlockBlob zipBlob = this.container.GetBlockBlobReference(this.filename);
            BlobStream zipStream = zipBlob.OpenRead();

            // Create a temp file for xml file
            CloudBlockBlob xmlBlob = container.GetBlockBlobReference(this.xmlFilename);
            BlobStream xmlStream = xmlBlob.OpenWrite();

            // Unzip the backup file
            this.UnzipFile(zipStream, xmlStream);

            // Set delegate to class variable
            this.blockTransfer = destination.WriteData;
            this.progressChange = progressChange;

            // Open a stream to the xml file
            using (this.inStream = new StreamReader(xmlBlob.OpenRead()))
            {
                // The total number of backup operations is determined by the strem size
                this.TotalOperations = (int)this.inStream.BaseStream.Length;

                // Initialise detination writer
                destination.InitialiseWriting(this.GetTables(this.inStream));

                // Split stream into batches & trigger delegate
                this.BatchStream(this.BlockTranferWithProgressUpdate, this.inStream.BaseStream);

                if (this.Cancelled)
                {
                    // Exit the function if the user has cancelled
                    return;
                }

                // Finalise detination writer
                destination.FinaliseWriting();
            }

            // Delete the xml file
            xmlBlob.Delete();
        }

        private string[] GetTables(StreamReader streamReader)
        {
            XmlTextReader reader = new XmlTextReader(streamReader);
            List<string> tables = new List<string>();

            while (reader.Read())
            {
                if(reader.Name == "table" && reader.NodeType == XmlNodeType.Element)
                {
                    // Move the reader to the text node
                    reader.Read();

                    // Get the value
                    tables.Add(reader.Value);
                }
                else if (reader.Name == "tables" && reader.NodeType == XmlNodeType.EndElement)
                {
                    // End of table list
                    break;
                }
            }

            // Reset the stream position to beginning
            streamReader.BaseStream.Position = 0;

            return tables.ToArray();
        }

        private void BlockTranferWithProgressUpdate(string tableName, Stream data)
        {
            // Call delegate
            this.blockTransfer(tableName, data);

            // Update progress based on our position in the stream
            this.UpdateProgressPercentage(this.inStream.BaseStream.Position, this.progressChange);
        }

        public override void InitialiseWriting(string[] tableList)
        {
            // Create a temp file for xml file
            CloudBlockBlob xmlBlob = container.GetBlockBlobReference(this.xmlFilename);
            BlobStream xmlStream = xmlBlob.OpenWrite();

            // Initialise XmlTextWriter
            writer = new XmlTextWriter(xmlStream, null);
            writer.Formatting = Formatting.Indented;

            // Write xml header
            writer.WriteStartElement("backup");
                writer.WriteElementString("starttime", XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc));

                // Write tables
                writer.WriteStartElement("tables");

                foreach (string table in tableList)
                {
                    writer.WriteElementString("table", table);
                }

                writer.WriteEndElement();

                // Write start tag for entities
                writer.WriteStartElement("entities");

            // Flush output to file
            writer.Flush();
        }

        public override void WriteData(string tableName, Stream data)
        {
            using(XmlReader reader = XmlReader.Create(data, new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Fragment
            }))
            {
                while (reader.Read())
                {
                    // Copy the current node from the reader to the writer
                    this.CopyNode(reader, writer);
                }
            }

            // Flush output to file
            writer.Flush();
        }

        public override void FinaliseWriting()
        {
            // Write xml footer
                writer.WriteEndElement(); // entities element
                writer.WriteElementString("endtime", XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc));
            writer.WriteEndElement(); // backup element

            writer.Close();

            // Open the xml blob for reading
            CloudBlockBlob xmlBlob = container.GetBlockBlobReference(this.xmlFilename);
            BlobStream xmlStream = xmlBlob.OpenRead();

            // Create backup blob for writing
            CloudBlockBlob zipBlob = this.container.GetBlockBlobReference(this.filename);
            BlobStream zipStream = zipBlob.OpenWrite();

            // Zip the file
            this.ZipFile(this.xmlFilename, xmlStream, zipStream);

            // Delete the xml blob
            xmlBlob.Delete();
        }

        public Stream GetBackupFile()
        {
            CloudBlockBlob backupBlob = this.container.GetBlockBlobReference(this.filename);
            return backupBlob.OpenRead();
        }

        private void ZipFile(string filename, Stream inputStream, Stream outputStream)
        {
            using (ZipOutputStream s = new ZipOutputStream(outputStream))
            {
                // Highest compression level
                s.SetLevel(9);

                byte[] buffer = new byte[4096];

                ZipEntry entry = new ZipEntry(filename);
                entry.DateTime = DateTime.Now;
                s.PutNextEntry(entry);

                using (inputStream)
                {
                    int sourceBytes;
                    do
                    {
                        sourceBytes = inputStream.Read(buffer, 0, buffer.Length);
                        s.Write(buffer, 0, sourceBytes);
                    } while (sourceBytes > 0);
                }

                s.Finish();
                s.Close();
            }
        }

        private void UnzipFile(Stream inputStream, Stream outputStream)
        {
            using (ZipInputStream s = new ZipInputStream(inputStream))
            {
                ZipEntry entry = s.GetNextEntry();
                string fileName = Path.GetFileName(entry.Name);

                using (outputStream)
                {
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            outputStream.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}