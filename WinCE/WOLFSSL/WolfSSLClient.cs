using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Net;
using wolfSSL.CSharp;
using System.IO;
using NETtime.WinCE.Globals;



namespace NETtime.WinCE
{
    public static class WOLFSSLWrapper
    {
        private static int myVerify(int preverify, IntPtr x509_ctx)
        {
            /* Use the provided verification */
            /* Can optionally override failures by returning non-zero value */
            return preverify;
        }

        private static void clean(IntPtr ssl, IntPtr ctx)
        {
            wolfssl.free(ssl);
            wolfssl.CTX_free(ctx);
            wolfssl.Cleanup();
        }

        /// <summary>
        /// Example of a logging function
        /// </summary>
        /// <param name="lvl">level of log</param>
        /// <param name="msg">message to log</param>
        public static void standard_log(int lvl, StringBuilder msg)
        {
            Console.WriteLine(wolfssl.UnicodeToAscii(msg));
        }

        public static void ConnectToServer()
        {
            StringBuilder caCert = new StringBuilder(Utility.LocalPath + "\\Cert\\ca-cert.pem");
            IntPtr ssl = IntPtr.Zero;
            IntPtr ctx = IntPtr.Zero;

            // Tested successfully using:
            // ./examples/client/client -h stratus-clock-n2a.cloud.paychex.com -p 443 -A ca-cert.pem -x -g
            string host = "stratus-clock-n2a.cloud.paychex.com";
            int port = 443;

            Console.WriteLine("Enabling Debug");
            wolfssl.Debugging_ON();

            // example of function used for setting logging
            Console.WriteLine("Setting Logging");
            wolfssl.SetLogging(standard_log);

            // Initialize WolfSSL
            Console.WriteLine("Start Init");
            if (wolfssl.Init() == wolfssl.SUCCESS) {
                Console.WriteLine("Successfully initialized wolfssl");
            }
            else {
                Console.WriteLine("ERROR: Failed to initialize wolfssl");
                return;
            }

            // show list of available TLS ciphers
            StringBuilder ciphers = new StringBuilder(new String(' ', 4096));
            wolfssl.get_ciphers(ciphers, 4096);
            Console.WriteLine("Ciphers: " + wolfssl.UnicodeToAscii(ciphers));

            // Create a new WolfSSL context
            ctx = wolfssl.CTX_new(wolfssl.useTLSv1_2_client());
            if (ctx == IntPtr.Zero) {
                Console.WriteLine("Error in creating ctx structure");
                return;
            }

            // Load trusted CA certificates
            if (wolfssl.CTX_load_verify_locations(ctx, caCert.ToString(), null) != wolfssl.SUCCESS)
            {
                Console.WriteLine("Error loading CA cert");
                clean(ssl, ctx);
                return;
            }

            // Set peer certificate verification options
            if (wolfssl.CTX_set_verify(ctx, wolfssl.SSL_VERIFY_PEER, myVerify) != wolfssl.SUCCESS)
            {
                Console.WriteLine("Error setting verify callback!");
                clean(ssl, ctx);
                return;
            }

            // TCP Connect
            Socket tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = GetEndPoint(host, port);
            Console.WriteLine("TCP Connecting to: " + host + ":" + port);
            try
            {
                tcp.Connect(endPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("tcp.Connect() error " + e.ToString());
                clean(IntPtr.Zero, ctx);
                return;
            }
            Console.WriteLine("TCP Connected");
            ssl = wolfssl.new_ssl(ctx);
            wolfssl.set_fd(ssl, tcp);

            // TLS Connect
            if (wolfssl.connect(ssl) != wolfssl.SUCCESS)
            {
                /* get and print out the error */
                Console.WriteLine("TLS Connect failed: " + wolfssl.get_error(ssl));
                tcp.Close();
                clean(ssl, ctx);
                return;
            }
            Console.WriteLine("TLS Connected: version is " + wolfssl.get_version(ssl));
            Console.WriteLine("TLS Cipher Suite is " + wolfssl.get_current_cipher(ssl));

            // Send example HTTP GET
            StringBuilder httpGetMsg = new StringBuilder("GET /index.html HTTP/1.0\r\n\r\n");
            if (wolfssl.write(ssl, httpGetMsg, httpGetMsg.Length) != httpGetMsg.Length)
            {
                Console.WriteLine("Error in write");
                tcp.Close();
                clean(ssl, ctx);
                return;
            }

            // read and print out the message then reply
            StringBuilder buff = new StringBuilder(1024);
            if (wolfssl.read(ssl, buff, buff.Length-1) < 0)
            {
                Console.WriteLine("Error in read");
                tcp.Close();
                clean(ssl, ctx);
                return;
            }
            Console.WriteLine(buff);

            // Send TLS shutdown to close connection gracefully
            wolfssl.shutdown(ssl);

            // Cleanups
            tcp.Close();
            clean(ssl, ctx);
        }

        static IPEndPoint GetEndPoint(string hostname, int port)
        {

            IPHostEntry hostEntry = Dns.GetHostEntry(hostname);
            IPAddress ipAddress = hostEntry.AddressList[0]; // Get the first IP address
            return new IPEndPoint(ipAddress, port);
        }

    }
}
