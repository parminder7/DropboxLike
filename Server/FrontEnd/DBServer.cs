using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Services;

namespace Server.FrontEnd
{
    class DBServer
    {
        public const int SERVER_CONTROL_PORT = 36000;
        public const int MAX_FILE_SIZE = 1024 * 1024;
        private TcpListener serverSocket;
        private IAzure azureLink;
        public DBServer()
        {
            Console.WriteLine("Remember: spaces in filenames not allowed atm");
            IPAddress localIP = LocalIPAddress();
            if (localIP == null)
            {
                Console.WriteLine("Server does not have internet connectivity, cannot continue.");
                Environment.Exit(-1);
            }
            azureLink = new Services.User();
            
            try
            {
                serverSocket = new TcpListener(localIP, SERVER_CONTROL_PORT);
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
            serverSocket.Start();
            Console.WriteLine("Server listening on {0}:{1}",
                ((System.Net.IPEndPoint)(serverSocket.LocalEndpoint)).Address,
                ((System.Net.IPEndPoint)(serverSocket.LocalEndpoint)).Port);
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
    }
}
