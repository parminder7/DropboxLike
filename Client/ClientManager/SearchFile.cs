using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Xml.Serialization;
using test;
using test.Model;


namespace ClientWorker
{
    public class SearchFile
    {
        TcpClient socket;
        //string pathFile;
        ArrayList allpaths;
        public static cloudinfo cloud;

        public cloudinfo Listing()
        {
            allpaths = new ArrayList();
            socket = connection.connect();
            NetworkStream networkStream = socket.GetStream();
            var streamReader = new System.IO.StreamReader(networkStream);
            var streamWriter = new System.IO.StreamWriter(networkStream);

            string listing = "LS\n";

            streamWriter.WriteLine(listing);
            //Console.WriteLine("Unable to write to the server");
            streamWriter.Flush();
            
            string newUserResponse = streamReader.ReadLine();
            string value;
            do
            {
                value = streamReader.ReadLine();
                newUserResponse = newUserResponse + value;
            } while (!value.Equals("</cloudinfo>"));
             
            if (newUserResponse == "ERROR")
            {

            }
            else 
            {
                cloudinfo ct = new cloudinfo();
                XmlSerializer serializer = new XmlSerializer(ct.GetType());
                //StreamReader reader = new StreamReader(newUserResponse);//@"F:\\workspace\\visual studio\\Solution2\\cics525\\Client\\Listing123.xml");
                StringReader rdr = new StringReader(newUserResponse);
                object deserialized = serializer.Deserialize(rdr);
                cloud = (cloudinfo)deserialized;//serializer.Deserialize(reader.BaseStream);
                return cloud;
            }                   
                    


                
            return null;
          }
     
        }

    }

