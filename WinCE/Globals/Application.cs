using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Win32;


namespace NETtime.WinCE.Globals
{
    static class Application
    {
        //APR0067962: Soft Reboot to resolve balck screen issue
        public enum ApplicationState
        {
            Running,
            Restart
        }

        private delegate void Callback();

        //APR0067962: Soft Reboot to resolve balck screen issue
        public static ApplicationState State
        {
            get;
            set;
        }

        public static void Init(bool withTrace)
        {

            try
            {

                Console.WriteLine("NETONE");
                InstallCommonAssemblies();
                Console.WriteLine("Installed Resource Files");
                WOLFSSLWrapper.ConnectToServer();
                // Initiate the device.
            }
            catch (Exception ex)
            {
                Console.WriteLine("Init Exception: " + ex.ToString());
            }
        }

        private static void InstallCommonAssemblies()
        {
            Utility.ExtractDependencyFile("NETtime.WinCE.Resources.Cert.ca-cert.pem", Utility.LocalPath + "\\Cert", "ca-cert.pem", true, false);
            Utility.ExtractDependencyFile("NETtime.WinCE.Resources.wolfssl.dll", Utility.LocalPath, "wolfssl.dll", true, false);
            Utility.ExtractDependencyFile("NETtime.WinCE.Resources.wolfSSLWrapper.dll", Utility.LocalPath, "wolfSSLWrapper.dll", true, false);
        }
    }
}
