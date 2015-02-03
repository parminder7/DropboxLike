using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Services;

namespace Server.FrontEnd
{
    class Backend : IAzure, IResourceManage
    {
        private File fileMgr;
        private User userMgr;
        private Resource contMgr;

        public Backend()
        {
            fileMgr = new File();
            userMgr = new User();
            contMgr = new Resource();
        }
        public string[] list(string username, string path)
        {
            return fileMgr.list(username, path);
        }

        public void upload(UPLOAD_INFO f, string localpath)
        {
            fileMgr.upload(f, localpath);
        }

        public void download(string username, string path, System.IO.Stream targetStream)
        {
            fileMgr.download(username, path, targetStream);
        }

        public string downloadWithSAS(string username, string path)
        {
            return fileMgr.downloadWithSAS(username, path);
        }

        public System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, Model.BlobFileModel>> getUserCloudFileSystem(string emailID)
        {
            return fileMgr.getUserCloudFileSystem(emailID);
        }

        public void deleteFileFromContainer(String username, String path)
        {
            fileMgr.deleteFileFromContainer(username, path);
        }

        public void renameFile(String username, String path, String newname)
        {
            fileMgr.renameFile(username, path, newname);
        }

        public bool validateUser(string username, string password)
        {
            return userMgr.validateUser(username, password);
        }

        public bool createUser(string fullname, string username, string password)
        {
            return userMgr.createUser(fullname, username, password);
        }

        public int findUserId(string username)
        {
            return userMgr.findUserId(username);
        }

        int IResourceManage.getContainerID(int userid, string givenname)
        {
            return contMgr.getContainerID(userid, givenname);
        }

        int IResourceManage.createSharedContainer(int userid, string givenname)
        {
            return contMgr.createSharedContainer(userid, givenname);
        }

        bool IResourceManage.deleteSharedContainer(int containerID)
        {
            return contMgr.deleteSharedContainer(containerID);
        }

        bool IResourceManage.grantRights(int otheruserID, int containerID, bool writeAccess)
        {
            return contMgr.grantRights(otheruserID, containerID, writeAccess);
        }

        void IResourceManage.removeRights(int otheruserID, int containerID)
        {
            contMgr.removeRights(otheruserID, containerID);
        }

        bool IResourceManage.isOwner(int userID, int containerID)
        {
            throw new NotImplementedException();
        }

        bool IResourceManage.canRead(int userID, int containerID)
        {
            throw new NotImplementedException();
        }

        bool IResourceManage.canWrite(int userID, int containerID)
        {
            throw new NotImplementedException();
        }
    }
    class DBServer
    {
        public const int SERVER_CONTROL_PORT = 5001;
        private const long MEGABYTE = 1024 * 1024;
        public const long MAX_FILE_SIZE = 50 * MEGABYTE;
        private TcpListener serverSocket;
        private IAzure azureLink;
        private ILocks lockDb;

        public ILocks getLocks()
        {
            return lockDb;
        }
        public DBServer()
        {
            //Console.WriteLine("Remember: spaces in filenames not allowed atm");
            IPAddress localIP = LocalIPAddress();
            if (localIP == null)
            {
                Console.WriteLine("Server does not have internet connectivity, cannot continue.");
                Environment.Exit(-1);
            }
            azureLink = new Backend();

            int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            ILocks distLockDb = DBLocks.DBLocks.initLocks(localIP.ToString(), pid);
            lockDb = new LockTable(distLockDb);
            try
            {
                serverSocket = new TcpListener(localIP, SERVER_CONTROL_PORT);
                serverSocket.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating server socket: {0}", e.Message);                
                Environment.Exit(-1);
            }
        }

        public IAzure getAzureLink()
        {
            return azureLink;
        }

        public void start() 
        {
            //serverSocket.Start();
            
            Console.WriteLine("Server listening on {0}:{1}",
                ((System.Net.IPEndPoint)(serverSocket.LocalEndpoint)).Address,
                ((System.Net.IPEndPoint)(serverSocket.LocalEndpoint)).Port);

            new Thread(new ParameterizedThreadStart(DBServerWorker.runConsole)).Start(this);
            Console.WriteLine("DEBUG CONSOLE ENABLED");

            while (true)
            {
                //serverSocket.
                TcpClient s = serverSocket.AcceptTcpClient();
                DBServerWorker newSession = new DBServerWorker(s, this);
                Thread workerThread = new Thread(new ThreadStart(newSession.run));
                workerThread.Start();
            }
        }

        //Using code from stackoverflow.com/questions/6803073/get-local-ip-address-c-sharp
        private static IPAddress LocalIPAddress()
        {
            if (! System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }
            System.Net.IPHostEntry host;
            //string localIP = "";
            //Console.WriteLine(System.Net.Dns.);
            host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                //Console.Write(ip);
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                    //Console.Write(" valid");
                    //localIP = ip.ToString();
                    //break;
                    
                }
                //Console.WriteLine();
            }
            return null;
        }
        ~DBServer()
        {
            lockDb.Dispose();
        }
    }
}
