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
        private static wolfssl.WOLFSSL_ALERT_HISTORY myHistory = new wolfssl.WOLFSSL_ALERT_HISTORY();

        /// <summary>
        /// Verification callback
        /// </summary>
        /// <param name="preverify">1=Verify Okay, 0=Failure</param>
        /// <param name="x509_ctx">Certificate in WOLFSSL_X509_STORE_CTX format</param>
        private static int myVerify(int preverify, IntPtr x509_ctx)
        {
            int verify = preverify;
            int error = wolfssl.X509_STORE_CTX_get_error(x509_ctx);
            if (error == wolfcrypt.ASN_BEFORE_DATE_E)
            {
                Console.WriteLine("Overriding before date error");
                verify = 1; /* override error */
            }

            /* Can optionally override failures by returning non-zero value */
            return verify;
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
        public static void standard_log(int lvl, string msg)
        {
            /* try multi-byte and fall back to msg if invalid */
            string logMsg = wolfssl.MultiByteToWideChar(msg);
            if (logMsg.Length < msg.Length / 2)
            {
                /* not multi-byte. internal log() are already wide char */
                logMsg = msg;
            }
            Console.WriteLine(logMsg);
        }

        private static void show_alert_history_code(wolfssl.WOLFSSL_ALERT h, string m)
        {
            /* VS initializes .code and .level to zero; wolfSSL sets to -1 until there's a valid value. */
            if ((h.code > 0) || (h.level > 0))
            {
                Console.WriteLine(m + " code:  " + h.code.ToString());
            }
            if ((h.code > 0) || (h.level > 0))
            {
                Console.WriteLine(m + " level: " + h.level.ToString());
            }
        }

        private static void show_alert_history(IntPtr ssl)
        {
            int ret = 0;
            ret = wolfssl.get_alert_history(ssl, ref myHistory);
            if (ret == wolfssl.SUCCESS)
            {
                show_alert_history_code(myHistory.last_tx, "myHistory last_tx");
                show_alert_history_code(myHistory.last_rx, "myHistory last_rx");
            }
            else
            {
                Console.WriteLine("Failed: call to get_alert_history failed with error " + ret.ToString());
            }
        }

        public static void ConnectToServer()
        {
            StringBuilder caCert = new StringBuilder(Utility.LocalPath + "\\Cert\\ca-cert.pem");

            /* string conversion tests */
            /* WinCE:       using Unicode 16-bit
             * wolfSSL DLL: using multi-byte (8-bit) */
            string caCert1 = Path.GetFullPath("\\Cert\\ca-cert.pem");
            string uCaCert1 = wolfssl.WideCharToMultiByte(caCert1);
            string bCaCert1 = wolfssl.MultiByteToWideChar(uCaCert1);
            Console.WriteLine("Before: " + caCert1 + ", After: " + bCaCert1);
            /* odd length test */
            string caCert2 = Path.GetFullPath("\\Certs\\ca-cert.pem");
            string uCaCert2 = wolfssl.WideCharToMultiByte(caCert2);
            string bCaCert2 = wolfssl.MultiByteToWideChar(uCaCert2);
            Console.WriteLine("Before: " + caCert2 + ", After: " + bCaCert2);

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
            string ciphers = wolfssl.get_ciphers();
            Console.WriteLine("Ciphers: " + ciphers);

            // Create a new WolfSSL context
            ctx = wolfssl.CTX_new(wolfssl.useTLSv1_2_client());
            if (ctx == IntPtr.Zero) {
                Console.WriteLine("Error in creating ctx structure");
                return;
            }

            // Load trusted CA certificates
            int ret = wolfssl.CTX_load_verify_locations(ctx, caCert.ToString(), null);
            if (ret != wolfssl.SUCCESS)
            {
                Console.WriteLine("Error loading CA cert: ret = " + ret);
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
                show_alert_history(ssl);
                tcp.Close();
                clean(ssl, ctx);
                return;
            }
            Console.WriteLine("TLS Connected " + wolfssl.get_error(ssl));
            Console.WriteLine("TLS Connected: version is " + wolfssl.get_version(ssl));
            Console.WriteLine("TLS Cipher Suite is " + wolfssl.get_current_cipher(ssl));

            // Send example HTTP GET
            StringBuilder httpGetMsg = new StringBuilder("GET /index.html HTTP/1.0\r\n\r\n");
            Console.WriteLine("Write: " + httpGetMsg);
            if (wolfssl.write(ssl, httpGetMsg, httpGetMsg.Length) != httpGetMsg.Length)
            {
                Console.WriteLine("Error in write");
                tcp.Close();
                clean(ssl, ctx);
                return;
            }

            // read and print out the message then reply
            StringBuilder buff = new StringBuilder(1024);
            if (wolfssl.read(ssl, buff, 1023) < 0)
            {
                Console.WriteLine("Error in read");
                tcp.Close();
                clean(ssl, ctx);
                return;
            }
            Console.WriteLine("Read: " + buff);

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
