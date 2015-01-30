using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

namespace ClientWorker
{
    public class SearchFile
    {


        TcpClient socket;
        string pathFile;
        ArrayList allpaths;
        public ArrayList Listing(string container_id, string root)
        {
            allpaths = new ArrayList();
            socket = connection.connect();
            NetworkStream networkStream = socket.GetStream();
            var streamReader = new System.IO.StreamReader(networkStream);
            var streamWriter = new System.IO.StreamWriter(networkStream);

            string listing = "LS" + " " + container_id + " " + root + "\n";

            streamWriter.WriteLine(listing);
            //Console.WriteLine("Unable to write to the server");
            streamWriter.Flush();

            string newUserResponse = streamReader.ReadLine();
            while (newUserResponse != null)
            {
                string[] nwSplit = newUserResponse.Split(' ');
                if (nwSplit[0] == "ERR_NO_ACCESS")
                {
                    ArrayList nouse = new ArrayList();
                    nouse.Add("Error finding the path");
                    return nouse;
                }
                else
                {

                    pathFile = root;
                    string pathstring = null;
                    for (int i = 0; i <= nwSplit.Length; i++)
                    {
                        pathstring = pathstring + "/" + nwSplit[i];
                    }
                    pathFile = pathFile + pathstring;

                    allpaths.Add(pathFile);

                }
                newUserResponse = streamReader.ReadLine();
            }
            return allpaths;


        }
    }
}
