using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;


namespace ClientWorker
{
  class NewUser
    {
        /**
               * The client collects the information from the user using the GUI.
               * The client first starts connecting to the server listening on the port number:3600
              **/
      TcpClient socket;
      public  Boolean newRegister(string user,string password)
      {
          //connection connection = new connection();
          socket = connection.connect();
           NetworkStream networkStream = socket.GetStream();
           var streamReader = new System.IO.StreamReader(networkStream);
           var streamWriter = new System.IO.StreamWriter(networkStream); 

           string newCredential ="NEWUSER"+" "+user + " " + password + "\n";
           try
           {
               streamWriter.WriteLine(newCredential);
           }
           catch 
           {
               Console.WriteLine("Unable to write to the server");
           }

           
           streamWriter.Flush();
          
           
               string newUserResponse = streamReader.ReadLine();
               string[] nwSplit= newUserResponse.Split(' ');
               if (nwSplit[0] == "ERR_USER_EXISTS")
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
