using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using test.ui;

namespace client
{
   public class downloadAsync
    {
        TcpClient socket;
        TcpClient downloadSocket;
        private Thread workerThread;
        public void download(string container_id, string fileName, string localDestPath,string userName, string fileSize)
       {
           // connection connection = new connection();
           socket = ClientWorker.connection.connect();

           NetworkStream networkStream = socket.GetStream();
           var streamReader = new System.IO.StreamReader(networkStream);
           var streamWriter = new System.IO.StreamWriter(networkStream);

           string newdownload = "DOWNLOAD" + " " + container_id + ":" + fileName+"\n";

           streamWriter.WriteLine(newdownload);

           streamWriter.Flush();
           string uri = streamReader.ReadLine();
           //ContainerOperation containerOperation = new ContainerOperation();
           //containerOperation.Container(uri, localDestPath);

           //no idea as to we have to perform blob operation or container operation.
           String fName = fileName.Replace('/', '\\');
           String filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DBLite";

           if (uri == "ERROR")
           {
               new Notify().notifyChange(fileName, "Download Error: Internal Error");
               
           }
           else if (uri == "ERR_FILE_NOT_FOUND")
           {
               new Notify().notifyChange(fileName, "Download Error: File not found");
               
           }
           else if (uri == "ERR_FOLDER_NOT_FOUND")
           {
               
               new Notify().notifyChange(fileName, "Download Error: Folder not found");
               
           }
           else if (uri == "ERR_FILE_IS_LOCKED")
           {
               new Notify().notifyChange(fileName, "Download Error: File is locked");
 
           }

           else
           {
               if (fName.Contains("\\"))
               {
                   String[] files = fName.Split('\\');
                   fileName = files.Last();
                   int totalFolders = files.Length - 1;
                   string contName = "\\";
                   for (int i = 0; i < totalFolders; i++)
                   {
                       contName = contName + files[i];
                       bool isExists = System.IO.Directory.Exists(filePath + contName);

                       if (!isExists)
                       {
                           System.IO.Directory.CreateDirectory(filePath + contName);
                       }
                       String[] dirs = Directory.GetDirectories(filePath + contName);

                       filePath = filePath + contName;
                       contName = contName + "\\";

                   }
               }
               else
               {
                   filePath = localDestPath;
               }

               //downloadProgress dp = new downloadProgress();
               //dp.bg.RunWorkerAsync(uri + "+" + filePath + "\\" + fileName);
               BlobOperation blobOperation = new BlobOperation(uri, filePath + "\\" + fileName, container_id, fileName);
               workerThread = new Thread(new ThreadStart(blobOperation.UseBlobSAS));
               workerThread.IsBackground = true;

               workerThread.Start();
               //BlobOperation blobOperation = new BlobOperation(uri, filePath + "\\" + fileName, container_id, fileName);
               //blobOperation.UseBlobSAS();                        
           
           }  
           
           
        }
        
       public void Dispose() 
        {
            downloadSocket.Close();
        }
          
       }
    }

