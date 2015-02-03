using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace test.ClientManager
{
    class ShareFolder
    {
        public Boolean createSharedFolder(String ownerEmail, String folderName, String userList)
        {
           TcpClient socket = ClientWorker.connection.connect();
           string newFolder;
           NetworkStream networkStream = socket.GetStream();
           var streamReader = new System.IO.StreamReader(networkStream);
           var streamWriter = new System.IO.StreamWriter(networkStream);
           if (SharedFolderForm.isCreate == true)
           {
               newFolder = "CREATE" + " " + folderName + " SHAREWITH " + userList + "\n";
           }
           else
           {
               newFolder = "EDIT" + " " + folderName + " SHAREWITH " + userList + "\n";
           }
           
           try
           {
               streamWriter.WriteLine(newFolder);
           }
           catch
           {
               Console.WriteLine("Unable to create the folder");
           }


           streamWriter.Flush();

           string newUserResponse = streamReader.ReadLine();
           string[] nwSplit = newUserResponse.Split(' ');
           if (nwSplit[0] == "ERR_NO_EXISTS" || nwSplit[0] == "ERROR" || nwSplit[0] == "ERR_FOLDERNAME_EXISTS")
           {
               return false;
           }
           else
           {
               return true;
           }
        }
    }
}
