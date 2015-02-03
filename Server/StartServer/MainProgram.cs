using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Server.FrontEnd;

namespace Server.StartServer
{
    class MainProgram
    {
        public static void newMain(string[] args)
        {            
            var srv = new DBServer();
            try
            {
                srv.start();
            }
            finally
            {
                srv.getLocks().Dispose();
            }
        }        
    }
}
