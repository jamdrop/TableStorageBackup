using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using System.Data.Services.Client;
using System.Globalization;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Threading;

namespace AntsCode.TableStorageBackupLib
{
    public class TableStorageDataSource : BackupDataSource
    {
        private const int requestRetries = 3;
        private CloudStorageAccount account;
        private CloudTableClient cloudTableClient;
        private bool isDevelopmentStorage;
        private string sourceAccountName;
        private string destAccountName;

        public TableStorageDataSource(BackupDirection direction, string connectionString) :
            base(direction)
        {
            try
            {
                this.account = CloudStorageAccount.Parse(connectionString);
                this.destAccountName = this.account.Credentials.AccountName;
                this.isDevelopmentStorage = this.destAccountName.Equals("devstoreaccount1", StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception)
            {
                // The connection string is not valid
                throw new FormatException("The " + this.DirectionLabel + " connection string is not valid.");
            }

            this.cloudTableClient = new CloudTableClient(account.TableEndpoint.ToString(), account.Credentials);
        }

        public override void TransferData(BackupDataSource destination, ProgressChange progressChange)
        {
            // Get all tables
            var tables = this.cloudTableClient.ListTables().ToArray();

            // The total number of backup operations is determined by the table count
            this.TotalOperations = tables.Length;

            string nextPartitionKey = null;
            string nextRowKey = null;
            int currentTable = 1;

            // Initialise detination writer
            destination.InitialiseWriting(tables);

            // Iterate through each table
            foreach (string table in tables)
            {
                if (this.Cancelled)
                {
                    // Exit the function if the user has cancelled
                    return;
                }
                
                // Retrieve all entities for this table in blocks
                do
                {
                    // Trigger delegate, passing in stream retrieved from table storage
                    this.BatchStream(destination.WriteData, GetTableData(table, ref nextPartitionKey, ref nextRowKey));
                }
                while (nextPartitionKey != null || nextRowKey != null);

                this.UpdateProgressPercentage(currentTable, progressChange);
                currentTable++;
            }

            // Finalise detination writer
            destination.FinaliseWriting();
        }

        public override void InitialiseWriting(string[] tableList)
        {
            // Delete all existing tables
            var tables = this.cloudTableClient.ListTables().ToList();

            foreach (string table in tables)
            {
                this.cloudTableClient.DeleteTable(table);
            }

            // Add tables from backup
            foreach (string table in tableList)
            {
                this.CreateTableWithRetries(table);
            }
        }

        private void CreateTableWithRetries(string table)
        {
            const int maxTries = 10;
            bool success = false;
            int retries = 0;

            while (!success && retries < maxTries)
            {
                try
                {
                    this.cloudTableClient.CreateTable(table);
                    success = true;
                }
                catch (StorageClientException)
                {
                    // The table has not finished deleting, so wait
                    Thread.Sleep(5000);
                    retries++;
                }
            }
        }

        public override void WriteData(string tableName, Stream data)
        {
            string tableEndpoint = this.account.TableEndpoint.ToString();

            // Build request uri
            string requestUri = Path.Combine(tableEndpoint, "$batch");

            // Create unique batch & boundary ids
            string batchId = "batch_" + Guid.NewGuid();
            string boundaryId = "changeset_" + Guid.NewGuid();

            // Initialise the batch
            HttpWebRequest request;
            StringBuilder sb;
            this.InitBatch(requestUri, batchId, boundaryId, out request, out sb);

            // Iterate through each entity in the stream
            using (XmlReader reader = XmlReader.Create(data, new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Fragment
            }))
            {
                int count = 1;
                string entityXml;
                
                while (true)
                {
                    if (reader.Name == "entry" && reader.NodeType == XmlNodeType.Element)
                    {
                        entityXml = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>\n" +
                            reader.ReadOuterXml();

                        // The category element must reference the destination storage account
                        entityXml = UpdateCategoryAccount(entityXml);

                        sb.AppendLine("--" + boundaryId);
                        sb.AppendLine("Content-Type: application/http");
                        sb.AppendLine("Content-Transfer-Encoding: binary");
                        sb.AppendLine();
                        sb.AppendLine("POST " + Path.Combine(tableEndpoint, tableName) + " HTTP/1.1");
                        sb.AppendLine("Content-ID: " + count);
                        sb.AppendLine("Content-Type: application/atom+xml;type=entry");
                        sb.AppendLine("Content-Length: " + Encoding.UTF8.GetBytes(entityXml).Length);
                        sb.AppendLine();
                        sb.AppendLine(entityXml);

                        if (this.isDevelopmentStorage)
                        {
                            // We are in development storage, so each entity must be sent individually
                            // due to a current limitation.
                            this.SendBatch(sb, request, boundaryId, batchId);

                            // Re-initialise the batch
                            this.InitBatch(requestUri, batchId, boundaryId, out request, out sb);
                        }
                        else
                        {
                            count++;
                        }
                    }
                    else
                    {
                        reader.Read();
                    }

                    if (reader.EOF)
                    {
                        break;
                    }   
                }
            }

            if(!this.isDevelopmentStorage)
            {
                // We are not in development storage, so send all entities as a batch
                this.SendBatch(sb, request, boundaryId, batchId);
            }
        }

        private void InitBatch(string requestUri, string batchId, string boundaryId, out HttpWebRequest request, out StringBuilder sb)
        {
            // Create HttpWebRequest
            request = (HttpWebRequest)HttpWebRequest.Create(requestUri);

            // Sign the request with storage credentials
            this.account.Credentials.SignRequestLite(request);

            // A POST is performed for batch transactions
            request.Method = "POST";

            // Add headers
            request.Accept = "application/atom+xml,application/xml";
            request.ContentType = "multipart/mixed; boundary=" + batchId;
            request.Headers.Add("x-ms-version", "2009-09-19");
            request.Headers.Add("DataServiceVersion", "1.0;NetFx");
            request.Headers.Add("MaxDataServiceVersion", "1.0;NetFx");
            request.Headers.Add("Accept-Charset", "UTF-8");

            // Add batch header
            sb = new StringBuilder();

            sb.AppendLine("--" + batchId);
            sb.AppendLine("Content-Type: multipart/mixed; boundary=" + boundaryId);
            sb.AppendLine();
        }

        private void SendBatch(StringBuilder sb, HttpWebRequest request, string boundaryId, string batchId)
        {
            // Write batch footer
            sb.AppendLine("--" + boundaryId + "--");
            sb.AppendLine("--" + batchId + "--");

            // Encode the request
            byte[] encodedRequest = Encoding.UTF8.GetBytes(sb.ToString());

            // Set the content length
            request.ContentLength = encodedRequest.Length;

            // Write the the request stream
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(encodedRequest, 0, encodedRequest.Length);
                requestStream.Close();
            }

            // Make the request
            HttpWebResponse response = this.GetResponseWithRetries(request);

            // Close the response
            response.Close();
        }

        private string UpdateCategoryAccount(string entityXml)
        {
            StringReader sr = new StringReader(entityXml);
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            string term;

            xw.Formatting = Formatting.Indented;

            using (XmlReader xr = XmlReader.Create(sr, new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Fragment
            }))
            {
                while (xr.Read())
                {
                    if (xr.Name == "category" && xr.NodeType == XmlNodeType.Element)
                    {
                        term = xr.GetAttribute("term");

                        // Get the source account
                        if (this.sourceAccountName == null)
                        {
                            // Build regex for finding existing account
                            Regex re = new Regex(@"(.*?)(?=\.)");
                            this.sourceAccountName = re.Match(term).Value;
                        }

                        // Update the account name
                        term = term.Replace(this.sourceAccountName, this.destAccountName);

                        // Write the updated category element
                        xw.WriteStartElement("category");
                        xw.WriteAttributeString("term", term);
                        xw.WriteAttributeString("scheme", xr.GetAttribute("scheme"));
                        xw.WriteEndElement();
                    }
                    else
                    {
                        // Copy the current node from the reader to the writer
                        this.CopyNode(xr, xw);
                    }
                }
            }

            return sw.ToString();
        }

        public override void FinaliseWriting()
        {
            // Not used
        }

        private Stream GetTableData(string tableName, ref string nextPartitionKey, ref string nextRowKey)
        {
            // Build request uri
            string requestUri = Path.Combine(this.account.TableEndpoint.ToString(), tableName);

            // Add any continuation token
            requestUri = AddContinuationToken(requestUri, nextPartitionKey, nextRowKey);

            // Create HttpWebRequest
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUri);

            // Sign the request with storage credentials
            this.account.Credentials.SignRequestLite(request);

            // Get the response
            HttpWebResponse response = GetResponseWithRetries(request);

            // Get any continuation token from the response
            this.GetContinuationToken(response, out nextPartitionKey, out nextRowKey);

            // Return a stream according to the response encoding
            Encoding responseEncoding = Encoding.GetEncoding(response.CharacterSet); 
            StreamReader sr = new StreamReader(response.GetResponseStream(), responseEncoding);

            return sr.BaseStream;
        }

        private HttpWebResponse GetResponseWithRetries(HttpWebRequest request)
        {
            HttpWebResponse response = null;
            Exception responseException;
            bool success = false;
            int tries = 0;

            // Get the response, with retries
            while (!success && tries < 3)
            {
                tries++;
                
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    success = true;
                }
                catch (WebException e)
                {
                    responseException = e;
                }
            }

            if (!success)
            {
                throw new ApplicationException("Table Service request failed after " + tries + " attempts.");
            }

            return response;
        }

        private string AddContinuationToken(string requestUri, string nextPartitionKey, string nextRowKey)
        {
            if (nextPartitionKey != null)
            {
                requestUri += "?NextPartitionKey=" + nextPartitionKey;

                if (nextRowKey != null)
                {
                    requestUri += "&NextRowKey=" + nextRowKey;
                }
            }

            return requestUri;
        }

        private void GetContinuationToken(HttpWebResponse response, out string nextPartitionKey, out string nextRowKey)
        {
            nextPartitionKey = response.Headers.Get("x-ms-continuation-NextPartitionKey");
            nextRowKey = response.Headers.Get("x-ms-continuation-NextRowKey");
        }
    }
}