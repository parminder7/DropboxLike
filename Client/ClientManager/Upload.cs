using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using test.ui;

namespace ClientWorker
{
    public class Upload
    {
        TcpClient socket;
        public Boolean DocUplaod(string container_id, string path, string file_size, String fileName, String hash, string ticks, string oldHash)
        {
            //connection connection = new connection();
            socket = connection.connect();

            NetworkStream networkStream = socket.GetStream();
            var streamReader = new System.IO.StreamReader(networkStream);
            var streamWriter = new System.IO.StreamWriter(networkStream);
            string newupload;
            if (!file_size.Equals("0"))
            {
                if (oldHash == null && !fileName.Equals("shared2/tset.txt"))
                {
                    newupload = "UPLOAD" + " " + container_id + ":" + fileName + " " + file_size + " " + hash + " " + ticks + "\n";
                }
                else
                {
                    if (oldHash != null && hash != null && !oldHash.Equals(hash))
                    {
                        newupload = "UPLOAD" + " " + container_id + ":" + fileName + " " + oldHash + " " + file_size + " " + hash + " " + ticks + "\n";
                    }
                    else
                    {
                        return false;
                    }
                    
                }
            }
            else
            {
                return false;
            }
            
            

            streamWriter.WriteLine(newupload);

            streamWriter.Flush();

            string uploadResponse = streamReader.ReadLine();
            string[] upSplit = uploadResponse.Split(' ');
            if (upSplit[0] == "OK")
            {
                TcpClient upsocket;

                string key = upSplit[1];

                upsocket = new TcpClient(test.ClientManager.ClientConstants.SERVER_IP_ADDRESS, 5002);

                NetworkStream Stream = upsocket.GetStream();
               // Stream.Write(Convert.FromBase64String(key), 0, key.Length);
                var uploadstreamReader = new System.IO.StreamReader(Stream);
                var uploadstreamWriter = new System.IO.StreamWriter(Stream);
                uploadstreamWriter.Write(key);
                uploadstreamWriter.Flush();
               
                String fName = fileName.Replace('/', '\\');
                if (fName.Contains("\\"))
                {
                    fName = fName.Split('\\').Last();
                }
                FileStream f = new FileStream(path + "\\" +fName, FileMode.Open);
                f.CopyTo(Stream);
                //uploadstreamWriter.Write(readContents);
                //uploadstreamWriter.Flush();
                Stream.Flush();
                upsocket.Close();
                f.Close();
                return true;
            }
            else if (upSplit[0] == "ERR_NO_ACCESS")
            {
               // new Notify().notifyChange(fileName, "Access required to make changes");
                return false;
            }
            else if (upSplit[0] == "ERR_HASH_MISMATCH")
            {
               // new Notify().notifyChange(fileName, "Conflict. Please resolve conflicts");
                return false;
            }
            else if (upSplit[0] == "ERR_FILE_IS_LOCKED")
            {
                //try again later
                new Notify().notifyChange(fileName, "File Locked, Try after 5 mins");
                return false;
            }

            else 
            {                
                return false;
            }



        }
    }
}
