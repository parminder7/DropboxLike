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
        class MESSAGES
        {
            public const String ERR = "ERROR";
            public const String OK = "OK";
            public const string E_EXISTS = "ERR_FOLDERNAME_EXISTS";
            public const string E_NOUSER = "ERR_NO_USER";
            public const string E_BADFOLDERNAME = "ERR_INVALID_FOLDERNAME";
            public const string E_NOFILE = "ERR_FILE_NOT_FOUND";
            public const string E_NOFOLDER = "ERR_FOLDER_NOT_FOUND";
            public const string E_BADFILESIZE = "ERR_INVALID_FILESIZE";
            public const string E_NOACCESS = "ERR_NO_ACCESS";
            public const string E_LOCK = "ERR_FILE_IS_LOCKED";
            public const string E_CONFLICT = "ERR_HASH_MISMATCH";
            public const string E_NOTEMPTY = "ERR_FOLDER_NOT_EMPTY";
        }
        
        private const bool STRICT = false;
        const char path_sep = ':';
        
        private DBServer srv;
        private TcpClient s;
        //private NetworkStream netio;
        private String loggedInUsername;
        private int loggedInUserid;
        private Boolean abortConnection;
        //TODO: delegate void runCommand(String line);

        //private HashSet<System.Reflection.M> mySet;

        private static DBServerWorker consoleWorker = null;
        private DBServerWorker() { }
        public DBServerWorker(TcpClient s, DBServer srv)
        {
            // TODO: Complete member initialization
            this.loggedInUsername = null;
            this.s = s;
            this.srv = srv;
           // this.netio = s.GetStream();
        }

        public static void runConsole(object o)
        {
            if (consoleWorker != null)
                return;

            if (!(o is DBServer))
                return;

            consoleWorker = new DBServerWorker();
            consoleWorker.srv = (DBServer)o;

            while (true)
            {
                String line = Console.ReadLine();
                if (String.IsNullOrEmpty(line))
                    continue;
                //Console.WriteLine(line); <-- echo
                if (line.ToLower().Equals("quit") || line.ToLower().Equals("exit"))
                {
                    //throw new Exception("BUG"); /* ALL TESTS LOOK OK */
                    Environment.Exit(0);
                }
                String hResult = consoleWorker.processCommand(line);
                Console.WriteLine("reply {0}", hResult);
            }
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
            try
            {
                if (line.StartsWith("NEWUSER "))
                    return doNewUser(line.Substring(8));
                else if (line.StartsWith("LOGIN "))
                    return doLogin(line.Substring(6));
                else if (line.StartsWith("LS"))
                    return doList(line.Substring(2));
                else if (line.StartsWith("EDIT "))
                    return doEdit(line.Substring(5));
                else if (line.StartsWith("UPLOAD "))
                    return doUpload(line.Substring(7));
                else if (line.StartsWith("DOWNLOAD "))
                    return doDownload(line.Substring(9));
                else if (line.StartsWith("CREATE "))
                    return doCreate(line.Substring(7));
                else if (line.StartsWith("DELETE "))
                    return doDelete(line.Substring(7));
                else if (line.StartsWith("DOWNLOADCOMPLETE "))
                    return doCompleteDownload(line.Substring(17));
                else
                    return MESSAGES.ERR;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return "ERR_INTERNAL_ERROR " + e.Message;
            }
        }

        private string doCompleteDownload(string line)
        {
            if (!isLoggedIn())
                return MESSAGES.ERR;
            string[] tokens = line.Split(path_sep);
            if (tokens.Length != 2)
            {
                return MESSAGES.ERR;
            }
            downloadCompleteImpl(tokens[0], tokens[1]);
            return MESSAGES.OK;
        }

        private void downloadCompleteImpl(string containerGivenName, string fileName)
        {
            
            string lockstr = makeAbsPath(this.loggedInUsername, containerGivenName + path_sep + fileName);
            Console.WriteLine("Release read lock for {0}", lockstr);
            srv.getLocks().releaseReadLock(lockstr);
        }

        private string doDelete(string line)
        {
            if (!isLoggedIn())
                return MESSAGES.ERR;
            int idx = line.IndexOf(path_sep);
            //String filePath = line.Substring(idx + 1);
            if (idx == -1)
            {
                return doDeleteContainer(line);
            }
            if (idx != line.LastIndexOf(path_sep))
            {
                return MESSAGES.ERR;
            }
            else
            {
                string lockstr = makeAbsPath(this.loggedInUsername, line);
                if (srv.getLocks().acquireWriteLock(lockstr))
                {
                    try
                    {
                        srv.getAzureLink().deleteFileFromContainer(this.loggedInUsername, line);
                        return MESSAGES.OK;
                        //return srv.getAzureLink().downloadWithSAS(this.loggedInUserid, line);
                    }
                    catch (DBLikeExceptions.CloudContainerNotFoundException)
                    {
                        return MESSAGES.E_NOFOLDER;
                    }
                    catch (DBLikeExceptions.CloudBlobNotFoundException)
                    {
                        return MESSAGES.E_NOFILE;
                    }
                    catch (DBLikeExceptions.UnauthorizedAccessException)
                    {
                        return MESSAGES.E_NOACCESS;
                    }
                    finally
                    {
                        srv.getLocks().releaseWriteLock(lockstr);
                    }
                }
                else
                {
                    return MESSAGES.E_LOCK;
                }
            }
        }

        private string doDeleteContainer(string line)
        {
            string givenContainerName = line;
            if (line.Equals(this.loggedInUsername))
            {
                return MESSAGES.ERR;
            }
            Services.IResourceManage r = new Services.Resource();
            //int uid = srv.getAzureLink().findUserId(this.loggedInUserid);
            int rid = r.getContainerID(loggedInUserid, givenContainerName);
            if (rid == -1)
            {
                return MESSAGES.E_NOFOLDER;
            }
            if (!r.isOwner(loggedInUserid, rid))
            {
                return MESSAGES.E_NOACCESS;
            }
            try
            {
                r.deleteSharedContainer(rid);
            }
            catch (ArgumentException e)
            {
                if (e.Message.Equals("container not empty"))
                {
                    return MESSAGES.E_NOTEMPTY;
                }
                else
                {
                    throw;
                }

            }
            return MESSAGES.OK;
        }

        private string doNewUser(string line)
        {
            if (isLoggedIn())
            {
                return MESSAGES.ERR;
            }
            String[] tokens = splitline(line, 3);
            if (tokens.Length < 3)
            {
                return MESSAGES.ERR;
            }

            String username = tokens[0];
            String password = tokens[1];
            String fullname = tokens[2];
            for (int i = 3; i < tokens.Length; i++)
                fullname += (" " + tokens[i]);


                if (!Validator.isValidEmailAddress(username))
                {
                    abortConnection = STRICT;
                    return "ERR_INVALID_USERID";
                }
            Boolean isValidUser = srv.getAzureLink().createUser(fullname, username, password);
            if (isValidUser)
            {
                this.loggedInUsername = username;
                this.loggedInUserid = srv.getAzureLink().findUserId(username);
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
            if (isLoggedIn())
            {
                return MESSAGES.ERR;
            }
            String[] tokens = splitline(line, 2);
            if (tokens.Length != 2)
            {
                return MESSAGES.ERR;
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
                this.loggedInUsername = username;
                this.loggedInUserid = srv.getAzureLink().findUserId(username);
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
                return MESSAGES.ERR;

            var cloudInfo = srv.getAzureLink().getUserCloudFileSystem(this.loggedInUsername);

            StringBuilder replyStr = new StringBuilder();
            replyStr.Append("<?xml version='1.0' encoding='UTF-8'?>\r\n");
            replyStr.Append("<cloudinfo>\r\n");
            Services.IResourceManage mg = new Services.Resource();
            /* this is INCREDIBLY FUGLY CODE */


            //int uid = srv.getAzureLink().findUserId(this.loggedInUserid);
            foreach (String containerName in cloudInfo.Keys)
            {
                
                replyStr.Append("\t<container name='");
                replyStr.Append(containerName);
                replyStr.Append("' access='");
                int containerID = mg.getContainerID(loggedInUserid, containerName);                

                if (mg.isOwner(loggedInUserid, containerID)) 
                {
                    replyStr.Append("owner");
                } 
                else if (mg.canWrite(loggedInUserid, containerID)) 
                {
                    replyStr.Append("readwrite");
                } 
                else 
                {
                    replyStr.Append("readonly");
                }
                replyStr.Append("'>\r\n");
                
                var containerFiles = cloudInfo[containerName];
                foreach (String fileName in containerFiles.Keys) 
                {
                    var file = containerFiles[fileName];
                    replyStr.Append("\t\t<file name='" + fileName + "' ");
                    replyStr.Append(" size='" + file.getSize() + "' ");
                    replyStr.Append(" timestamp='" + file.getLastmodifiedTime() + "' ");
                    replyStr.Append(" md5='" + file.getMD5HashValue() + "'");
                    replyStr.Append(" deleted='" + file.isDeleted() + "'");                    
                    replyStr.Append(" />\r\n");
                    //replyStr.Append("\t\t</file>\r\n");
                }
                if (mg.isOwner(loggedInUserid, containerID))
                {
                    string sharedUsersXML = makeSharedUsersXML(containerID);
                    replyStr.Append(sharedUsersXML);
                }
                replyStr.Append("\t</container>\r\n");
            }
            replyStr.Append("</cloudinfo>");
            replyStr.Replace('\'', '\"');
            return replyStr.ToString();
        }
        /* This should be moved into Resource.cs but the file is checked out right now */
        private static string makeAbsPath(string username, string path)
        {
            string[] tokens = path.Split(path_sep);
            string containerGivenName = tokens[0];
            string relPath = tokens[1];
            using (var conn = new DBManager.DBAccess().getDBAccess())
            {
                using (var stmt = conn.CreateCommand())
                {
                    stmt.CommandText = "SELECT ContainerName FROM containers WHERE OWNER IN (SELECT UID FROM USERS WHERE EMAIL = @USERID) AND GivenName = @SHFNAME UNION SELECT ContainerName From containers WHERE RID IN (SELECT RID FROM CLIST WHERE UID IN (SELECT UID FROM USERS WHERE EMAIL = @USERID)) AND GivenName = @SHFNAME";
                    stmt.Parameters.AddWithValue("@USERID", username);
                    stmt.Parameters.AddWithValue("@SHFNAME", containerGivenName);
                    return (string)stmt.ExecuteScalar() + path_sep + relPath;
                }
            }
        }
        private static string makeSharedUsersXML(int containerID)
        {
            StringBuilder result = new StringBuilder();
            using (var cmd = new DBManager.DBAccess().getDBAccess().CreateCommand()) 
            { 
                cmd.CommandText = "Select U.EMAIL, U.FULLNAME, C.READWRITE from CLIST C, USERS U WHERE C.UID = U.UID AND RID=" + containerID;
                var Reader = cmd.ExecuteReader();

                using (Reader)
                {
                    while (Reader.Read())
                    {
                        result.Append("\t\t<user email='");
                        result.Append(Reader.GetString(0));
                        result.Append("' name='");
                        if (Reader.IsDBNull(1))
                        {
                            result.Append(Reader.GetString(0));
                        }
                        else
                        {
                            result.Append(Reader.GetString(1));
                        }
                        result.Append("' access='");
                        if (Reader.GetInt32(2) == 1)
                        {
                            result.Append("readwrite");
                        }
                        else
                        {
                            result.Append("readonly");
                        }
                        result.Append("' />\r\n");
                    }               
                }
            }
            result.Replace('\'', '\"');
            return result.ToString();
        }

        private bool isLoggedIn()
        {
            return (this.loggedInUsername != null);
        }

        //DOWNLOAD sharedfoldername:path
        private string doDownload(string line)
        {
            if (!isLoggedIn())
                return MESSAGES.ERR;
            int idx = line.IndexOf(path_sep);
            //String filePath = line.Substring(idx + 1);
            if (idx == -1 || idx != line.LastIndexOf(path_sep))
            {
                return MESSAGES.ERR;
            }
            else
            {
                try
                {
                    string uri = srv.getAzureLink().downloadWithSAS(this.loggedInUsername, line);
                    //String[] arr = line.Split('/');
                    string lockstr = makeAbsPath(this.loggedInUsername, line);
                    Console.WriteLine("## acquire read lock ##");
                    if (srv.getLocks().acquireReadLock(lockstr)) 
                    {
                        return uri;
                    } 
                    else 
                    {
                        return MESSAGES.E_LOCK;
                    }
                }
                catch (DBLikeExceptions.CloudContainerNotFoundException)
                {
                    return MESSAGES.E_NOFOLDER;
                }
                catch (DBLikeExceptions.CloudBlobNotFoundException)
                {
                    return MESSAGES.E_NOFILE;
                }
            }
            /*
            String[] tokens = splitline(line);
            if (tokens.Length != 2)
                return MESSAGES.ERR;
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
                return MESSAGES.ERR;
            }
             * */
        }
        private string doEdit(string line)
        {
            const String sep = " SHAREWITH ";
            if (!isLoggedIn())
                return MESSAGES.ERR;
            //int uid = srv.getAzureLink().findUserId(this.loggedInUserid);
            int idx = line.LastIndexOf(sep);
            if (idx == -1)
            {
                return MESSAGES.ERR;
            }
            String sharedFolderName = line.Substring(0, idx);
            
            String sharedUsersStr = line.Substring(idx + sep.Length);
            String[] sharedUsers = sharedUsersStr.Split(',');
            if (sharedUsers.Length == 0)
            {
                Console.WriteLine("#doEdit(): zeroing CLIST");
                //return MESSAGES.ERR;
            }
            int[] userids = new int[sharedUsers.Length];
            Boolean[] write = new Boolean[sharedUsers.Length];
            for (int i = 0; i < sharedUsers.Length; i++)
            {
                String shareInfo = sharedUsers[i];
                int sep_idx = shareInfo.IndexOf(' ');
                if (sep_idx == -1 || sep_idx != shareInfo.LastIndexOf(' '))
                {
                    return MESSAGES.ERR;
                }
                String sharedUser = shareInfo.Substring(0, sep_idx);
                int userid = srv.getAzureLink().findUserId(sharedUser);
                if (userid == -1)
                {
                    return MESSAGES.E_NOUSER + " " + sharedUser;
                }
                if (userid == loggedInUserid)
                {
                    return MESSAGES.ERR;
                }
                userids[i] = userid;

                String acl_str = shareInfo.Substring(sep_idx + 1);
                if (acl_str.Equals("R"))
                {
                    write[i] = false;
                }
                else if (acl_str.Equals("RW"))
                {
                    write[i] = true;
                }
                else
                {
                    return MESSAGES.ERR;
                }
            }
            Services.IResourceManage m = (Backend)(srv.getAzureLink());
            int sharedContainerId = m.getContainerID(loggedInUserid, sharedFolderName);
            if (sharedContainerId == -1)
            {
                return MESSAGES.E_NOFOLDER;
            }
            if (!m.isOwner(this.loggedInUserid, sharedContainerId))
            {
                return MESSAGES.E_NOACCESS;
            }
            deleteAllRights(sharedContainerId);
            for (int i = 0; i < userids.Length; i++)
            {
                m.grantRights(userids[i], sharedContainerId, write[i]);
            }
            return MESSAGES.OK;

        }

        private static void deleteAllRights(int sharedContainerId)
        {
            using (var cmd = new DBManager.DBAccess().getDBAccess().CreateCommand())
            {
                cmd.CommandText = "DELETE FROM CLIST WHERE RID=" + sharedContainerId;
                cmd.ExecuteNonQuery();
            }
            
        }
        //UPLOAD container:path SIZE HASH TIME[ULONG]NEWLINE
        private static string fetchLong(string input, out long output, char sep) 
        {
            int idx = input.LastIndexOf(sep);
            if (idx == -1 || idx == input.Length - 1)
            {
                throw new ArgumentException("no token");
            }
            output = long.Parse(input.Substring(idx + 1));
            return input.Substring(0, idx);
        }
        private static string fetchString(string input, out string output, char sep)
        {
            int idx = input.LastIndexOf(sep);
            if (idx == -1 || idx == input.Length - 1)
            {
                throw new ArgumentException("no token");
            }
            output = input.Substring(idx + 1);
            return input.Substring(0, idx);
        }
        private const int MD5LENGTH = 32;
        private string doUpload(string line)
        {
            if (!isLoggedIn())
                return MESSAGES.ERR;
            long filesize, timestamp;
            string md5, oldmd5;
            try 
            {
                line = fetchLong(line, out timestamp, ' ');
                line = fetchString(line, out md5, ' ');
                line = fetchLong(line, out filesize, ' ');
            }
            catch (Exception)
            {
                return MESSAGES.ERR;
            }
            try
            {
                line = fetchString(line, out oldmd5, ' ');
            }
            catch (ArgumentException) 
            {
                Console.WriteLine("no old hash");
                oldmd5 = null;
            }
            
            String[] tokens = line.Split(path_sep);
            if (tokens.Length != 2)
                return MESSAGES.ERR;
            String containerName = tokens[0];
            String path = tokens[1];
            //String lengthStr = tokens[2];
            // no longer need to reject files of zero size
            if (filesize > DBServer.MAX_FILE_SIZE)
            {
                return MESSAGES.E_BADFILESIZE;
            }
            /*
            if (md5.Length != MD5LENGTH)
            {
                Console.WriteLine("md5length fail, this is nyi");
                //return MESSAGES.ERR;
            }*/
            if (timestamp < 0) {
                return "ERR_TIMESTAMP_TOO_OLD";
            }
            DateTimeOffset fileStamp = new DateTimeOffset(new DateTime(timestamp, DateTimeKind.Utc));
            //new DateTimeOffset()
            /*
            if (fileStamp.CompareTo(new DateTimeOffset().AddMinutes(5)) > 0) 
            {
                return "ERR_TIMESTAMP_IN_FUTURE";
            }*/

            Services.UPLOAD_INFO f = new Services.UPLOAD_INFO();
            f.username = this.loggedInUsername;
             
            //f.path = containerName + ":" + path;
            f.path = line;
            f.sz = filesize;
            f.utcTime = fileStamp;
            f.curHash = md5;
            f.prevHash = oldmd5;
            f.absPath = makeAbsPath(f.username, f.path);
            try
            {
                DBTransferManager mgr = DBTransferManager.getManager(srv);
                string key = mgr.getDownloadKey(f);
                if (srv.getLocks().acquireWriteLock(f.absPath))
                {
                    Console.WriteLine("Acquired global write lock");
                    return MESSAGES.OK + " " + key;
                }
                else
                {
                    mgr.revokeKey(key);
                    Console.WriteLine("Global write lock was denied");
                    return MESSAGES.E_LOCK;
                }
            }
            catch (Exception e)
            {
                //details not settled yet
                if (e is DBLikeExceptions.UnauthorizedAccessException)
                {
                    return MESSAGES.E_NOACCESS;
                }
                if (e is DBLikeExceptions.CloudContainerNotFoundException)
                {
                    return MESSAGES.E_NOFOLDER;
                }
                if (e is DBLikeExceptions.HashConflictException)
                {
                    return MESSAGES.E_CONFLICT;
                }
                Console.WriteLine(e);
                return MESSAGES.ERR;
            }
        }

        private string doCreate(string line)
        {
            const String sep = " SHAREWITH ";
            if (!isLoggedIn())
                return MESSAGES.ERR;
            //int uid = srv.getAzureLink().findUserId(this.loggedInUserid);
            int idx = line.LastIndexOf(sep);
            if (idx == -1)
            {
                return MESSAGES.ERR;
            }
            String sharedFolderName = line.Substring(0, idx);
            /* This should be done via a regex check... */
            if (sharedFolderName.IndexOf('@') != -1 || 
                sharedFolderName.IndexOf('/') != -1 ||
                sharedFolderName.IndexOf('\\') != -1)
            {
                return MESSAGES.E_BADFOLDERNAME;
            }
                        
            try
            {
                new System.IO.FileInfo(sharedFolderName);
            }
            catch (Exception)
            {
                return "ERR_INVALID_FOLDERNAME";
            }
            String sharedUsersStr = line.Substring(idx + sep.Length);
            String[] sharedUsers = sharedUsersStr.Split(',');
            if (sharedUsers.Length == 0)
            {
                return MESSAGES.ERR;
            }
            int[] userids = new int[sharedUsers.Length];
            Boolean[] write = new Boolean[sharedUsers.Length];
            Services.Resource m = new Services.Resource();

            for (int i = 0; i < sharedUsers.Length; i++)
            {
                String shareInfo = sharedUsers[i];
                int sep_idx = shareInfo.IndexOf(' ');
                if (sep_idx == -1 || sep_idx != shareInfo.LastIndexOf(' '))
                {
                    return MESSAGES.ERR;
                }
                String sharedUser = shareInfo.Substring(0, sep_idx);
                int userid = srv.getAzureLink().findUserId(sharedUser);
                if (userid == -1)
                {
                    return MESSAGES.E_NOUSER + " " + sharedUser;
                }
                if (userid == this.loggedInUserid)
                {
                    return MESSAGES.ERR;
                }
                // this is not perfectly MT-safe, alas...
                if (m.getContainerID(userid, sharedFolderName) != -1)
                {
                    return MESSAGES.E_EXISTS;
                }
                userids[i] = userid;

                String acl_str = shareInfo.Substring(sep_idx + 1);
                if (acl_str.Equals("R"))
                {
                    write[i] = false;
                }
                else if (acl_str.Equals("RW"))
                {
                    write[i] = true;
                }
                else
                {
                    return MESSAGES.ERR;
                }
            }
            
            
            try
            {
                int sharedContainerId = m.createSharedContainer(loggedInUserid, sharedFolderName);
                if (sharedContainerId == -1)
                {
                    return MESSAGES.E_EXISTS;
                }
                for (int i = 0; i < userids.Length; i++)
                {
                    m.grantRights(userids[i], sharedContainerId, write[i]);
                }
                return MESSAGES.OK;
            }
            catch (DBLikeExceptions.CloudContainerAlreadyExistException)
            {
                return MESSAGES.E_EXISTS;
            }

        }

        private static string joinStrings(String[] arr) 
        {
            if (arr == null)
            {
                return "";
            }
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
                NetworkStream netio = s.GetStream();
                //var ssl = new System.Net.Security.SslStream(netio);
                //ssl.
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
                    if (hResult.Equals(MESSAGES.ERR))
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
