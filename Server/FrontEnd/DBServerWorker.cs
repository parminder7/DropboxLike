using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Server.FrontEnd
{
    class DBServerWorker
    {
        
        
        private const bool STRICT = false;
        private const String ERR = "ERROR";
        private DBServer srv;
        private TcpClient s;
        private NetworkStream netio;
        private String loggedInUserid;
        private Boolean abortConnection;
        //TODO: delegate void runCommand(String line);

        //private HashSet<System.Reflection.M> mySet;

        public DBServerWorker(TcpClient s, DBServer srv)
        {
            // TODO: Complete member initialization
            this.loggedInUserid = null;
            this.s = s;
            this.srv = srv;
            this.netio = s.GetStream();
        }

        private static char[] space = { ' ' };

        private static String[] splitline(String line, int maxTokens = 0)
        {
            if (maxTokens == 0)
            {
                return line.Split(space);
            }
            else
            {
                return line.Split(space, maxTokens);
            }
        }

        private String processCommand(String line)
        {
            if (line.StartsWith("NEWUSER "))
                return doNewUser(line.Substring(8));
            else if (line.StartsWith("LOGIN "))
                return doLogin(line.Substring(6));
            else if (line.StartsWith("LS "))
                return doList(line.Substring(3));
            else if (line.StartsWith("INFO "))
                return doInfo(line.Substring(5));
            else if (line.StartsWith("UPLOAD "))
                return doUpload(line.Substring(7));
            else if (line.StartsWith("DOWNLOAD "))
                return doDownload(line.Substring(9));
            else if (line.StartsWith("CREATE "))
                return doCreate(line.Substring(7));
            else
                return ERR;
        }

        private string doNewUser(string line)
        {            

            String[] tokens = splitline(line, 2);
            if (tokens.Length != 2)
            {
                return ERR;
            }

            String username = tokens[0];
            String password = tokens[1];

            if (!Validator.isValidEmailAddress(username))
            {
                abortConnection = STRICT;
                return "ERR_INVALID_USERID";
            }
            Boolean isValidUser = srv.getAzureLink().createUser(username, password);
            if (isValidUser)
            {
                this.loggedInUserid = username;
                return "NEWUSER_OK " + username;                
            }
            else
            {
                abortConnection = STRICT;
                return "ERR_USER_EXISTS";
            }
        }

        private string doLogin(string line)
        {
            String[] tokens = splitline(line, 2);
            if (tokens.Length != 2)
            {
                return ERR;
            }

            String username = tokens[0];
            String password = tokens[1];

            if (!Validator.isValidEmailAddress(username))
            {
                abortConnection = STRICT;
                return "ERR_INVALID_USERID";
            }
            Boolean isValidUser = srv.getAzureLink().validateUser(username, password);
            if (isValidUser)
            {
                this.loggedInUserid = username;
                return "LOGIN_OK " + username;                
            }
            else
            {
                abortConnection = STRICT;
                return "ERR_INVALID_LOGIN";
            }
        }

        private string doList(string line)
        {
            if (!isLoggedIn())
                return ERR;
            
            if (line.Length == 0)
            {
                String[] containers = srv.getAzureLink().list(this.loggedInUserid, "");               
                return joinStrings(containers);
            }

            String user, returnStr;
            String[] entries;

            String[] tokens = splitline(line);            
            
            switch (tokens.Length)
            {                
                case 1:
                    user = tokens[0];
                    entries = srv.getAzureLink().list(this.loggedInUserid, user + ":");
                    returnStr = joinStrings(entries);
                    break;

                case 2:
                    user = tokens[0];
                    String path = tokens[1];
                    entries = srv.getAzureLink().list(this.loggedInUserid, user + ":" + path);
                    returnStr = joinStrings(entries);
                    break;

                default:
                    returnStr = ERR;
                    break;
            }

            return returnStr;            
        }

        private bool isLoggedIn()
        {
            return (this.loggedInUserid != null);
        }

        private string doDownload(string line)
        {
            if (!isLoggedIn())
                return ERR;
            
            String[] tokens = splitline(line);
            if (tokens.Length != 2)
                return ERR;
            String username = tokens[0];
            String path = tokens[1];
            FILE_INFO f = new FILE_INFO();
            f.username = this.loggedInUserid;
            f.path = username + ":" + path;
            f.sz = -1;
            try
            {
                DBTransferManager mgr = new DBTransferManager(
                    s.Client.RemoteEndPoint,
                    srv.getAzureLink(),
                    TRANSFER_TYPE.DOWNLOAD,
                    f);
                int port = mgr.getPort();
                return "OK " + port;
            }
            catch (Exception e)
            {
                //details not settled yet
                if (e is DBLikeExceptions.UnauthorizedAccessException)
                    return "ERR_NO_ACCESS";
                return ERR;
            }
        }

        private string doUpload(string line)
        {
            if (!isLoggedIn())
                return ERR;

            String[] tokens = splitline(line);
            if (tokens.Length != 3)
                return ERR;
            String username = tokens[0];
            String path = tokens[1];
            String lengthStr = tokens[2];
            int sz = -1;
            try
            {

                sz = Int32.Parse(lengthStr);
            }
            catch (FormatException)
            {
                return ERR;
            }
            catch(OverflowException)
            {
                return "ERR_FILE_TOO_BIG";
            }
            if (sz > DBServer.MAX_FILE_SIZE)
            {
                return "ERR_FILE_TOO_BIG";
            }
                
            FILE_INFO f = new FILE_INFO();
            f.username = this.loggedInUserid;
            f.path = username + ":" + path;
            f.sz = sz;
            try
            {
                DBTransferManager mgr = new DBTransferManager(
                    s.Client.RemoteEndPoint,
                    srv.getAzureLink(),
                    TRANSFER_TYPE.UPLOAD,
                    f);
                int port = mgr.getPort();
                return "OK " + port;
            }
            catch (Exception e)
            {
                //details not settled yet
                if (e is DBLikeExceptions.UnauthorizedAccessException)
                {
                    return "ERR_NO_ACCESS";
                }
                Console.WriteLine(e);
                return ERR;
            }
        }

        private string doInfo(string line)
        {
            if (!isLoggedIn())
                return ERR;

            throw new NotImplementedException();
        }

        private string doCreate(string line)
        {
            if (!isLoggedIn())
                return ERR;

            throw new NotImplementedException();
        }

        private static string joinStrings(String[] arr) 
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in arr)
            {
                sb.AppendLine(str);
            }
            return sb.ToString();
        }


        public void run()
        {
            try
            {
                System.IO.StreamReader socket_in = new System.IO.StreamReader(netio);
                System.IO.StreamWriter socket_out = new System.IO.StreamWriter(netio);
                IPAddress ipv4 = ((IPEndPoint)(s.Client.RemoteEndPoint)).Address;
                int port = ((System.Net.IPEndPoint)(s.Client.RemoteEndPoint)).Port;
                while (s.Connected)
                {
                    String line = socket_in.ReadLine();
                    if (String.IsNullOrEmpty(line))
                        continue;
                    Console.WriteLine(line);
                    String hResult = processCommand(line);
                    Console.WriteLine("reply {0}", hResult);
                    socket_out.WriteLine(hResult);
                    socket_out.Flush();
                    if (hResult.Equals(ERR))
                    {
                        abortConnection = true;
                    }
                    if (abortConnection)
                    {
                        break;
                    }
                }
                Console.WriteLine("Client {0}:{1} disconnected",
                    ipv4, port);
            }
            catch (System.IO.IOException e) 
            {
                Console.WriteLine("Worker thread: IO Exception {0}", e.Message);
                SocketException sockerr = e.GetBaseException() as SocketException;
                if (sockerr != null)
                {
                    Console.WriteLine("Worker thread : Socket error code {0}", sockerr.SocketErrorCode);
                }
            }
            finally
            {
                s.Close();
            }
        } 
    }
}
