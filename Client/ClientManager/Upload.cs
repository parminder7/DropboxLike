using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace ClientWorker
{
    public class Upload
    {
        TcpClient socket;
        public Boolean DocUplaod(string container_id, string path, string file_size, String fileName)
        {
            //connection connection = new connection();
            socket = connection.connect();

            NetworkStream networkStream = socket.GetStream();
            var streamReader = new System.IO.StreamReader(networkStream);
            var streamWriter = new System.IO.StreamWriter(networkStream);

            string newupload = "UPLOAD" + " " + container_id + " " + fileName + " " + file_size + "\n";

            streamWriter.WriteLine(newupload);

            streamWriter.Flush();


            string uploadResponse = streamReader.ReadLine();
            string[] upSplit = uploadResponse.Split(' ');
            if (upSplit[0] == "OK")
            {
                //Display.toDisplay("You are ready to upload the file");
                // opening a new tcp connection.
                int newport = Int32.Parse(upSplit[1]);
                //connection.connect("localhost", newport);

                TcpClient upsocket;

                upsocket = new TcpClient("128.189.75.94", newport);


                NetworkStream Stream = upsocket.GetStream();
                var uploadstreamReader = new System.IO.StreamReader(Stream);
                var uploadstreamWriter = new System.IO.StreamWriter(Stream);

                FileStream f = new FileStream(path, FileMode.Open);
                f.CopyTo(Stream);
                //uploadstreamWriter.Write(readContents);
                //uploadstreamWriter.Flush();
                Stream.Flush();
                upsocket.Close();
                return true;
            }
            else if (upSplit[0] == "ERR_NO_ACCESS")
            {
                // Display.toDisplay("You are not allowed to UPLOAD:Access Denied");
                return false;
            }

            else //if (upSplit[0]=="ERROR")
            {
                //  Display.toDisplay("Server has gone in undesirable state.");
                return true;
            }



        }
    }
}
