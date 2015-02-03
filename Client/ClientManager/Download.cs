using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace ClientWorker
{
    public class Download
    {
        TcpClient socket;
        TcpClient downloadSocket;

        public NetworkStream DocDownload(string container_id, string path)
        {
           // connection connection = new connection();
            socket = connection.connect();

            NetworkStream networkStream = socket.GetStream();

            var streamReader = new System.IO.StreamReader(networkStream);
            var streamWriter = new System.IO.StreamWriter(networkStream);

            string newdownload = "DOWNLOAD" + " " + container_id + " " + path + "\n";
            
            streamWriter.WriteLine(newdownload);          
            
            streamWriter.Flush();
            string uploadResponse = streamReader.ReadLine();
            string[] upSplit = uploadResponse.Split(' ');
            if (upSplit[0] == "OK")
            {
                //Display.toDisplay("You are ready to download the file");
                // opening a new tcp connection with the new port number.

                int newport = Int32.Parse(upSplit[1]);
                //connection.connect("localhost", newport);
                
                downloadSocket = new TcpClient(test.ClientManager.ClientConstants.SERVER_IP_ADDRESS, newport);
              
                NetworkStream downStream = downloadSocket.GetStream();
                //StreamReader sr = new StreamReader(downStream);
                
                //var uploadstreamReader = new System.IO.StreamReader(downStream);
              
                //string contents = sr.ReadToEnd();
                //downloadSocket.Close();
               // Display.toDisplay(contents);

                return downStream; 
            }
            return null;
           
        }
        public void Dispose() 
        {
            downloadSocket.Close();
        }



    }

}
