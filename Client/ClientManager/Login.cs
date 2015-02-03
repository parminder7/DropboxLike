using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using test.Util;

namespace ClientWorker
{
   public class Login
   {
       TcpClient socket;
       public  Boolean userLogin(string user, string password)
       {

         // connection connection=new connection();
          socket = connection.connect();

           NetworkStream networkStream = socket.GetStream();
           var streamReader = new System.IO.StreamReader(networkStream);
           var streamWriter = new System.IO.StreamWriter(networkStream);
           MD5Encryption MD5Encryption = new MD5Encryption();
           string hashPassword = MD5Encryption.GetHashedPassword(user, password);
           string newCredential = "LOGIN" + " " + user + " " + hashPassword + "\n";
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
           string[] nwSplit = newUserResponse.Split(' ');
           if (nwSplit[0] == "ERR_INVALID_LOGIN")
           {
               return false;
           }
           else //(nwSplit[0] == "LOGIN_OK")
           {
               return true;
           }

           
       }
    }
}
