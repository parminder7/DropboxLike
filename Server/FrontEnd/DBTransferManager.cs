using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

using Server.Services;

namespace Server.FrontEnd
{       
    class DBTransferManager
    {
        private Dictionary<String, UPLOAD_INFO> pendingUploads;
        private TcpListener listenSocket;
        private IAzure azureLink;
        private ILocks globalLocks;
        private static DBTransferManager instance;
        private const Boolean VALIDATE_OLD_HASH = true;
        public static DBTransferManager getManager(DBServer srv)
        {
            if (instance == null)
            {
                if (srv == null)
                {
                    return null;
                }
                else
                {
                    instance = new DBTransferManager(srv);
                }
            }                
            return instance;
        }
        private DBTransferManager(DBServer srv)
        {
            this.azureLink = srv.getAzureLink();
            this.globalLocks = srv.getLocks();
            pendingUploads = new Dictionary<string, UPLOAD_INFO>();
            listenSocket = new TcpListener(IPAddress.Any, DBServer.SERVER_CONTROL_PORT + 1);
            listenSocket.Start();
            new Thread(new ThreadStart(listen)).Start();
            //control returns to DBServerWorker
        }
        public Boolean abortDownload(String filePath)
        {
            lock (pendingUploads)
            {
                foreach (String key in pendingUploads.Keys)
                {
                    UPLOAD_INFO u = pendingUploads[key];
                    if (u.path.Equals(filePath)) {
                        pendingUploads.Remove(key);
                        return true;
                    }
                }
            }
            return false;
        }
        public string getDownloadKey(UPLOAD_INFO f)
        {
            string key;
            lock(pendingUploads) 
            {
                while(true)
                {
                    string random1 = Path.GetRandomFileName().Substring(0, 8);
                    string random2 = Path.GetRandomFileName().Substring(0, 8);
                    key = random1 + random2;
                    try 
                    {
                        //ADD TIMEOUT NOT SUPPORTED
                        pendingUploads.Add(key, f);
                        break;
                    } catch (ArgumentException) {/* keep trying */}
                } 
                
                
            }
            String containerName, blobName, containerAzureName;
            String[] arr = f.path.Split(':');
            containerName = arr[0];
            blobName = arr[1];

            if (containerName.Equals(f.username)) 
            {
                containerAzureName = azureLink.findUserId(f.username).ToString();
            } 
            else 
            {
                int uid = azureLink.findUserId(f.username);
                IResourceManage r = new Resource(); 
                int rid = r.getContainerID(uid, containerName);
                if (rid == -1)
                {
                    pendingUploads.Remove(key);
                    throw new DBLikeExceptions.CloudContainerNotFoundException();
                }
                if (!r.canWrite(uid, rid))
                {
                    pendingUploads.Remove(key);
                    throw new DBLikeExceptions.UnauthorizedAccessException();
                }
                /* TODO: improve this */
                containerAzureName = new DBManager.ResourceM()
                    .getResourceById(new DBManager.DBAccess().getDBAccess(), rid)
                    .getContainerName();
            }
            if (VALIDATE_OLD_HASH)
            {
                try
                {
                    var blobMgr = new BlobStorageManager.BlobFileHandler();
                    String oldHash = blobMgr.getBlobMD5HashValue(containerAzureName, blobName);
                    if (oldHash != null && !oldHash.Equals(f.prevHash))
                    {
                        throw new DBLikeExceptions.HashConflictException();
                    }
                } catch(DBLikeExceptions.CloudBlobNotFoundException) {/* new file, no old hash */}
            }
            return key;
        }

        public void listen()
        {
            while (true)
            {
                //serverSocket.
                TcpClient s = listenSocket.AcceptTcpClient();
                Thread workerThread = new Thread(new ParameterizedThreadStart(doUpload));
                workerThread.Start(s);
            }
        }


        const byte KEYSIZE = 16;

        private void doUpload(object o)
        {
            TcpClient dataSocket = (TcpClient)o;
            byte[] keyBytes = new byte[KEYSIZE];
            int len, timeout = 5;
            do
            {
                
               len = dataSocket.GetStream().Read(keyBytes, 0, KEYSIZE);
            } while (len == 0 && (--timeout) > 0);

            if (len != KEYSIZE)
            {
                dataSocket.Close();
                return;
            }
            String keyStr = System.Text.Encoding.ASCII.GetString(keyBytes);
            UPLOAD_INFO f; //NONNULLABLE POS

            lock (pendingUploads)
            {
                if (pendingUploads.ContainsKey(keyStr))
                {
                    f = pendingUploads[keyStr];
                    pendingUploads.Remove(keyStr);
                }
                else 
                {
                    dataSocket.Close();
                    return;
                }
            }

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
                String computedHash;
                using (var md5 = System.Security.Cryptography.MD5.Create()) {
                    using (var stream = System.IO.File.OpenRead(tmpFileName)) {
                        byte[] hashBits = md5.ComputeHash(stream);
                        computedHash = Convert.ToBase64String(hashBits);
                    }
                }

                //f_info.Length
                if (f_info.Length == f.sz && f.curHash.Equals(computedHash))
                {
                    try
                    {
                        azureLink.upload(f, tmpFileName);
                    } catch (Exception e) 
                    {
                        if (e is DBLikeExceptions.CloudContainerNotFoundException ||
                            e is DBLikeExceptions.UnauthorizedAccessException)
                        {
                            Console.WriteLine("Unexpected error during uploading. Likely shared folder " +
                            "configuration changed during upload. \r\n{0}", e.Message);
                            
                        }
                        else if (e is Microsoft.WindowsAzure.Storage.StorageException)
                        {
                            Console.WriteLine("Azure storage exception {0}", e.Message);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid file upload: expected {0} bits hash {1}, got {2} bits hash {3}.",
                        f.sz,
                        f.curHash,
                        f_info.Length,
                        computedHash);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("I/O timeout reading file");
            }
            finally
            {
                Console.WriteLine("Download complete: release global file lock");
                this.globalLocks.releaseWriteLock(f.absPath);
                try
                {
                    //Console.WriteLine("Debug mode: not deleting temp file");
                    System.IO.File.Delete(tmpFileName);                                        
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Warning: unable to delete temp file {0}, cause {1}", 
                        tmpFileName, 
                        ex.Message);
                }
            }
        }

        internal void revokeKey(string key)
        {
            lock (pendingUploads)
            {
                pendingUploads.Remove(key);
            }
        }
    }
}
