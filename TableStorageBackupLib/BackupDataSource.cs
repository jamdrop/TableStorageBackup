using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace AntsCode.TableStorageBackupLib
{
    public abstract class BackupDataSource
    {
        public BackupDataSource(BackupDirection direction)
        {
            this.DirectionLabel = direction.ToString().ToLower();
            this.TotalOperations = 1;
        }

        /// <summary>
        /// Splits a stream of entites into matches according to Table Service rules.
        /// </summary>
        /// <param name="onBlockRead">The delegate to trigger per batch.</param>
        /// <param name="inStream">The input stream of entities.</param>
        protected void BatchStream(BlockTransfer onBlockRead, Stream inStream)
        {
            using (XmlTextReader reader = new XmlTextReader(inStream))
            {
                MemoryStream outStream = new MemoryStream();
                string batchTableName = null;
                string batchPartitionKey = null;
                string tableName;
                string partitionKey;
                Stream entityStream;
                int entityCount = 0;

                while (reader.Read())
                {
                    if (this.Cancelled)
                    {
                        // Exit the function if the user has cancelled
                        return;
                    }

                    if (reader.Name == "entry" && reader.NodeType == XmlNodeType.Element)
                    {
                        // Get the entity
                        entityStream = this.GetEntity(reader, out tableName, out partitionKey);
                        entityCount++;

                        if (batchTableName == null)
                        {
                            // First entity, so set the first batch table name & partition key
                            batchTableName = tableName;
                            batchPartitionKey = partitionKey;
                        }

                        // Can we add this entity to the current batch?
                        if (entityCount > 100 || // A batch can have a max of 100 transactions
                           (tableName != batchTableName) || // The batch must be within the same table
                           (partitionKey != batchPartitionKey) || // The batch must be within the same partition
                           ((entityStream.Length + outStream.Length) / 1024 > 3800)) // The max payload is 4mb, we make it less to allow for HTTP headers etc
                        {
                            // No, so send current batch to delegate
                            outStream.Position = 0;
                            onBlockRead(batchTableName, outStream);

                            // Reset batch
                            outStream = new MemoryStream();
                            entityCount = 1;
                            batchTableName = tableName;
                            batchPartitionKey = partitionKey;
                        }

                        // Add the entity to the current batch
                        StreamUtil.CopyStream(entityStream, outStream);
                    }
                }

                // Send any remaining entities to delegate
                outStream.Position = 0;
                onBlockRead(batchTableName, outStream);

                // Close the stream
                outStream.Close();
            }
        }

        private Stream GetEntity(XmlTextReader reader, out string tableName, out string partitionKey)
        {
            tableName = null;
            partitionKey = null;
            MemoryStream outStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(outStream, null);
            writer.Formatting = Formatting.Indented;

            do
            {
                // Copy the current node from the reader to the writer
                this.CopyNode(reader, writer);

                switch (reader.Name)
                {
                    case "link":
                        tableName = reader.GetAttribute("title");
                        partitionKey = this.GetPartitionKeyFromHref(reader.GetAttribute("href"));
                        break;

                    case "entry":
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            writer.Flush();
                            outStream.Position = 0;
                            return outStream;
                        }
                        else
                        {
                            // The d namespace prefix needs to be registered against the entry
                            // since the feed element is removed
                            if (string.IsNullOrEmpty(reader.GetAttribute("xmlns:d")))
                            {
                                writer.WriteAttributeString("xmlns:d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
                            }

                            if (string.IsNullOrEmpty(reader.GetAttribute("xmlns")))
                            {
                                writer.WriteAttributeString("xmlns", "http://www.w3.org/2005/Atom");
                            }
                        }
                        break;
                }
            }
            while (reader.Read());

            return outStream;
        }

        private string GetPartitionKeyFromHref(string href)
        {
            int start = href.IndexOf("PartitionKey='") + 14;
            int end = href.IndexOf("',RowKey=");

            return href.Substring(start, end - start);
        }

        protected void CopyNode(XmlReader reader, XmlWriter writer)
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    writer.WriteStartElement(reader.Name);
                    writer.WriteAttributes(reader, false);

                    if (reader.IsEmptyElement)
                    {
                        writer.WriteEndElement();
                    }
                    break;
                case XmlNodeType.EndElement:
                    writer.WriteEndElement();
                    break;
                case XmlNodeType.Text:
                    writer.WriteValue(reader.Value);
                    break;
            }
        }

        protected void UpdateProgressPercentage(long currentOperation, ProgressChange progressChange)
        {
            // Increment the current operation count
            int progressPercentage = (int)(((double)currentOperation / (double)this.TotalOperations) * 100);

            progressChange(progressPercentage);
        }

        public delegate void ProgressChange(int progressPercentage);
        public delegate void BlockTransfer(string tableName, Stream data);
        public abstract void TransferData(BackupDataSource destination, ProgressChange progressChange);
        public abstract void InitialiseWriting(string[] tableList);
        public abstract void WriteData(string tableName, Stream data);
        public abstract void FinaliseWriting();
        public string DirectionLabel { get; private set; }
        public bool Cancelled { get; set; }
        protected long TotalOperations { get; set; }
    }
}