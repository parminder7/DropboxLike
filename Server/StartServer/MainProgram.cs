using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server.FrontEnd;

namespace Server.StartServer
{
    class MainProgram
    {
        public static void newMain(string[] args)
        {
            new DBServer().start();
        }
    }
}
