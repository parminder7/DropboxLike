using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ClientWorker
{
   public  class connection
    {
       private connection() { }
       
       private static TcpClient socket;
       public static TcpClient connect()
       {
          
           try
           {
               if (socket == null)
               {
                   //Not yet deployed on Azure VM, so hardcoding the Ip Address
                   socket = new TcpClient("128.189.75.94", 36000);
               }
               
           }
           catch
           {
               Console.WriteLine(
               "Failed to connect to correspong server ");
              
           }

           return socket;
       }
    }
}
