using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace test.ClientManager
{
    class Delete
    {
        TcpClient socket;
        TcpClient downloadSocket;
        //private Thread workerThread;
        public bool delete(string container_id, string fileName)
        {
            // connection connection = new connection();
            socket = ClientWorker.connection.connect();


            NetworkStream networkStream = socket.GetStream();
            var streamReader = new System.IO.StreamReader(networkStream);
            var streamWriter = new System.IO.StreamWriter(networkStream);
            string delete;
            if (fileName == null)
            {
                delete = "DELETE" + " " + container_id + "\n";
            }
            else
            {
                delete = "DELETE" + " " + container_id + ":" + fileName + "\n";
            }
            
            streamWriter.WriteLine(delete);
            streamWriter.Flush();
            string response = streamReader.ReadLine();


            string[] upSplit = response.Split(' ');
            if (upSplit[0] == "TRUE")
            {   
                //The file is successfully deleted.
                return true;
                //Console.WriteLine("The file is deleted successfully");
                
            }
            else
            {
                //the file cannot be deleted.
                return false;
                //Console.WriteLine("The file cannot be deleted");
            }


            
        }
        public void Dispose()
        {
            downloadSocket.Close();
        }


    }
}

