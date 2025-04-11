using System;
using NETtime.WinCE.Globals;


namespace NETtime.WinCE
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnHanldedExceptionHandler);
                Utility.RemoveReadOnlyFromDirectory(Utility.LocalPath);
                Application.Init(Array.IndexOf<string>(args, "/debug") > -1);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main Exception: " + ex.ToString());
            }
            System.Threading.Thread.Sleep(2000);
        }

        static void UnHanldedExceptionHandler(object obj, UnhandledExceptionEventArgs args)
        {
            try
            {
                Exception ex = (args.ExceptionObject as Exception);
                if (ex != null)
                {
                    Console.WriteLine("UnHanldedExceptionHandler: " + ex.ToString());
                    System.Threading.Thread.Sleep(2000);
                }
            }
            catch
            {
            }
        }
    }
}