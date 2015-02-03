using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using test.ui;
using System.Net.Sockets;

namespace client
{
    public class BlobOperation
    {
        private string sas, localPath;
        private StreamWriter streamWriter;
        private StreamReader streamReader;
        TcpClient socket;
        string container_id;
        string fileName;
        public BlobOperation(string sas, string localPath, string container_id, string fileName)
        {
            this.sas = sas;
            this.localPath = localPath;
            this.container_id = container_id;
            this.fileName = fileName;
            //this.streamWriter = streamWriter;
        }
        public void UseBlobSAS()
        {
            try
            {
                CloudBlockBlob blob = new CloudBlockBlob(new Uri(sas));
                // MemoryStream msRead = new MemoryStream();
                using (var filestream = System.IO.File.OpenWrite(localPath))
                {
                    blob.DownloadToStream(filestream);
                    filestream.Flush();
                    socket = ClientWorker.connection.connect();
                    streamWriter = new System.IO.StreamWriter(socket.GetStream());
                    string newdownload = "DOWNLOADCOMPLETE" + " " + container_id + ":" + fileName + "\n";
                    streamWriter.WriteLine(newdownload);
                    streamWriter.Flush();
                    streamReader = new System.IO.StreamReader(socket.GetStream());
                    
                    string value = streamReader.ReadLine();
                    if (value == "OK")
                    {
                        new Notify().notifyChange(localPath, "Download complete");
                    }
                }
               
               

            }
            catch(StorageException a)
            {
                new Notify().notifyChange(localPath, "Error: File cannot be downloaded"); ;
            }
        }
    }
}
