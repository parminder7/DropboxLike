using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// testing
        /// 
        ///ABC
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Edited the name from LoginForm to ClientForm
            Application.Run(new LoginForm());
        }
    }
}
