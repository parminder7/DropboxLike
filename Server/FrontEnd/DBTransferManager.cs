using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

using Server.Services;

namespace Server.FrontEnd
{
    struct FILE_INFO
    {
        public String username;
        public String path;
        public int sz;
    }
    
    enum TRANSFER_TYPE
    {
        UPLOAD,
        DOWNLOAD
    };
       
    class DBTransferManager
    {
        private TcpListener listenSocket;
        private TcpClient dataSocket;
        private IAzure azureLink;
        private IPAddress src;
        private TRANSFER_TYPE mode;
        private FILE_INFO f;
        
        public DBTransferManager(EndPoint src, IAzure azureLink, TRANSFER_TYPE mode, FILE_INFO f)
        {
            this.azureLink = azureLink;
            this.src = ((IPEndPoint)src).Address;
            this.mode = mode;
            this.f = f;
            listenSocket = new TcpListener(IPAddress.Any, 0);
            listenSocket.Start();
            new Thread(new ThreadStart(start)).Start();
            //control returns to DBServerWorker
        }

        public void start()
        {
            
            //listenSocket.Start();
            //CURRENTLY NOT WORKING
            listenSocket.Server.ReceiveTimeout = 30 * 1000;

            Console.WriteLine(listenSocket.LocalEndpoint);
            Thread killThread = new Thread(new ThreadStart(die));
            killThread.IsBackground = true;
            killThread.Start();

            try
            {
                //protection against attacks
                while (true)
                {
                    dataSocket = listenSocket.AcceptTcpClient();
                    IPEndPoint ep = (IPEndPoint)dataSocket.Client.RemoteEndPoint;
                    if (ep.Address.Equals(src))
                    {
                        //a bit ugly but ok
                        new Thread(new ThreadStart(run)).Start();
                        //listenSocket.Stop();
                        killThread.Abort();
                        break;
                    }
                    else
                    {
                        dataSocket.Close();
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ErrorCode);
            }
            finally
            {
                listenSocket.Stop();
            }
        }

        public int getPort()
        {
            //EndPoint ep = s.LocalEnd
;
            return ((IPEndPoint)(listenSocket.LocalEndpoint)).Port;
        }

        public void run()
        {
            if (mode == TRANSFER_TYPE.DOWNLOAD)
                doDownload();
            else if (mode == TRANSFER_TYPE.UPLOAD)
                doUpload();
            else
                throw new NotSupportedException();
        }

        private void doDownload()
        {
            azureLink.download(f.username, f.path, dataSocket.GetStream());
            dataSocket.Close();
            /*
            NetworkStream ns = dataSocket.GetStream();
            byte[] b  = (byte[]) o;
            ns.Write(b, 0, b.Length);*/
        }

        private void doUpload()
        {
            
            
            String tmpFileName = Path.GetTempFileName();
            try 
            {
                //int totalBytes = 0;
                //byte[] buffer = new byte [1024 * 1024];
                Console.WriteLine("Saving to file {0}", tmpFileName);

                //StreamReader reader = new StreamReader(dataSocket.GetStream());
                FileStream writer = new FileStream(tmpFileName, FileMode.OpenOrCreate);
                //int nBytes = dataSocket.GetStream().Read(buffer, 0, f.sz);
                    //reader.ReadBlock(buffer, 0, Math.Min(buffer.Length, f.sz));
                dataSocket.GetStream().CopyTo(writer);
                /*
                while (nBytes > 0) 
                {
                    writer.Write(buffer, 0, nBytes);
                    totalBytes += nBytes;
                    nBytes = dataSocket.GetStream().Read(buffer, 0, Math.Min(buffer.Length, f.sz));
                }*/
                writer.Close();
                //reader.Close();
                dataSocket.Close();
                FileInfo f_info = new FileInfo(tmpFileName);
                //f_info.Length
                if (f_info.Length == f.sz)
                {
                    azureLink.upload(f.username, f.path, tmpFileName);

                }
                else
                {
                    Console.WriteLine("Invalid file upload: expected {0}, got {1}.",
                        f.sz,
                        f_info.Length);                
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("I/O timeout reading file");
            }
            finally 
            {
                try
                {
                    Console.WriteLine("Debug mode: not deleting temp file");
                    //File.Delete(tmpFileName);
                }
                catch (IOException ex) 
                {
                    Console.WriteLine("Warning: unable to delete temp file {0}, cause {1}", tmpFileName, ex.Message);
                }
            }
        }

        public void die()
        {
            const int SEC = 1000;
            const int TIMEOUT = 30 * SEC;

            Thread.Sleep(TIMEOUT);
            listenSocket.Stop();
        }
    }
}
