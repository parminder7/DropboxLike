using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using test.ClientManager;
using System.Net;
using System.Windows.Forms;

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
                   //socket = new TcpClient("137.135.57.195", 5001);
                   socket = new TcpClient(ClientConstants.SERVER_IP_ADDRESS, 5001);
               }
               
           }
           catch
           {
               Console.WriteLine("Failed to connect to corresponding server ");
              
           }

           return socket;
       }

        /*private static TcpClient conn;
        public static TcpClient connect()
        {
            if (socket != null && socket.Connected)
            {
                return socket;
            }
            IPAddress[] ips;

            ips = Dns.GetHostAddresses("server.distributedsystem.net");

            for (int i = 0; i < ips.Length; i++)
            {
                if (pingHost(ips[i]))
                {
                    //MessageBox.Show("Connected to " + ips[i]);
                    Console.WriteLine("Connected to " + ips[i]);
                    return socket;
                }
            }
            Console.WriteLine("Server is not available");
            return null;
        }

        public static Boolean pingHost(IPAddress VMip) 
        {
             try
             {
                 socket = new TcpClient(VMip.ToString(), 5001);
                 ClientConstants.SERVER_IP_ADDRESS = VMip.ToString();
                 return true;
             }
             catch (Exception)
             {
                 return false;
             }
        }*/
    }
}
