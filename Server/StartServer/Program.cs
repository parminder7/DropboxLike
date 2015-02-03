using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace Server.StartServer
{
    class Program
    {
        public unsafe delegate void MyCallback(void* param);
        [DllImport("kernel32.DLL")]
        public static extern unsafe int RegisterApplicationRecoveryCallback(MyCallback callback, void* param, int dwPingInterval, int dwFlags);
        [DllImport("kernel32.DLL")]
        public static extern unsafe int RegisterApplicationRestart([MarshalAs(UnmanagedType.LPWStr)]string pwzCommandline, uint dwFlags);
        public unsafe static void appExit(void* param)
        {
            Console.WriteLine("UNSAFE HANDLER RUNNING");
            Environment.Exit(0);
        }
        public static void Main(string[] args)
        {
            const int SECONDS = 1000;
            const int RESTART_NO_CRASH = 1;
            const int RESTART_NO_HANG = 2;
            const int RESTART_NO_PATCH = 4;
            const int RESTART_NO_REBOOT = 8;
            unsafe
            {
                int result;
                result = RegisterApplicationRecoveryCallback(new MyCallback(appExit), null, 60 * SECONDS, 0);
                if (result != 0)
                {
                    Console.WriteLine("SEVERE: could not install fail handler.");
                }
                result = RegisterApplicationRestart("--restart", RESTART_NO_REBOOT);
                if (result != 0)
                {
                    Console.WriteLine("WARNING: could not install restart handler");
                }
            }

            GraceFullCtrlC();
            MainProgram.newMain(args);
 
        }
        /* Copied from http://www.codeproject.com/Articles/16164/Managed-Application-Shutdown 
         * Unfortunately, closing the window doesn't trip this method either... */
        static void GraceFullCtrlC()
        {
            Console.CancelKeyPress += delegate(object sender,
                                    ConsoleCancelEventArgs e)
            {
                if (e.SpecialKey == ConsoleSpecialKey.ControlBreak)
                {
                    e.Cancel = true;
                    Console.WriteLine("Ctrl-Break catched and" +
                      " translated into an cooperative shutdown");
                    // Environment.Exit(1) would NOT do 
                    // a cooperative shutdown. No finalizers are called!
                    var t = new Thread(delegate()
                    {
                        Console.WriteLine("Asynchronous shutdown started");
                        Environment.Exit(1);
                    });

                    t.Start();
                    t.Join();
                }
                if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                {
                    e.Cancel = true; // tell the CLR to keep running
                    Console.WriteLine("Ctrl-C catched and " +
                      "translated into cooperative shutdown");
                    // If we want to call exit triggered from
                    // out event handler we have to spin
                    // up another thread. If somebody of the
                    // CLR team reads this. Please fix!
                    new Thread(delegate()
                    {
                        Console.WriteLine("Asynchronous shutdown started");
                        Environment.Exit(2);
                    }).Start();
                }
            };

            Console.WriteLine("Ctrl-C / Ctrl-Break handler installed.");
        }
    }
}