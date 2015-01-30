using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.StartServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            //Services.User u = new Services.User();

            MainProgram.newMain(args);
            /*
            u.validateUser("parminder_jk@hotmail.com", "abc");
            Console.WriteLine("Creating blob");
            u.testUpload(123);
            */
            
            //THIS UPLOADS THE FILE TO CONTAINER
            //u.testUpload(9005);

            //u.createUser("parmindr@mss.icics.ubc.ca", "xyz");
            /*
            Console.WriteLine("TRUE:Exists    "+u.validateUser("Jenny1@ubc.com","qwer123"));
            Console.WriteLine("FALSE:No record found    " + u.validateUser("Jenny2@ubc.com", "qwer123"));
            Console.WriteLine("FALSE:Wrong creds    " + u.validateUser("Jenny1@ubc.com", "qwer1"));
            Console.WriteLine("create"+u.createUser("xz@ms.ca", "paas"));
            Console.WriteLine("Container name: " );
            String[] c = u.list("Jenny1@ubc.com", null);
            Console.WriteLine(c.Length);
            for (int i = 0; i >= c.Length; i++)
            {
                Console.WriteLine(c[i]);
            }

            u.upload("Jenny1@ubc.com", "9010:private9009", "E:/test2.txt");
            String[] cc = u.list("Jenny1@ubc.com", "9010:9010");
            Console.WriteLine(cc[0]);
            Console.WriteLine(cc[1]);
            
            Console.WriteLine(cc.Length);
            for (int i = 0; i >= cc.Length; i++)
            {
                Console.WriteLine(cc[i]);
            }*/
        }
    }
}