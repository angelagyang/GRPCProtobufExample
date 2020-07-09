
using Grpc.Core;
using GRPCServer;
using ProtoBuf.Grpc.Client;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GRPCClient
{
    class Program
    {
        private static string _sslThumbprint = ""; // Your own SSL certificate thumbprint 

        static void Main(string[] args)
        {
            // Using sample test credentials from https://github.com/grpc/grpc/tree/master/src/core/tsi/test_creds
            string rootDir = $"{Directory.GetCurrentDirectory()}/../.."; 
            var keyCertPair = new KeyCertificatePair(File.ReadAllText($"{rootDir}/samplecert.pem.txt"), File.ReadAllText($"{rootDir}/samplecert.key.txt")); 
            var channelCreds = new SslCredentials(GetRootCertificates(), keyCertPair);
            
            var channel = new Channel("localhost", 5001, channelCreds);

            var greeter = channel.CreateGrpcService<IGreeterService>();
            try
            {
                Console.WriteLine(greeter.Ping()); 
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine(); 
        }

        private static X509Certificate2 LoadSSLCertificate()
        {
            var certStore = new X509Store(StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);
            var cert = certStore.Certificates.Find(X509FindType.FindByThumbprint, _sslThumbprint, true)[0];
            certStore.Close();

            return cert;
        }

        /**
        * Source: https://stackoverflow.com/questions/58125102/grpc-net-client-fails-to-connect-to-server-with-ssl
        **/
        public static string GetRootCertificates()
        {
            StringBuilder builder = new StringBuilder();
            var cert = LoadSSLCertificate(); 
            builder.AppendLine(
                    "# Issuer: " + cert.Issuer.ToString() + "\n" +
                    "# Subject: " + cert.Subject.ToString() + "\n" +
                    "# Label: " + cert.FriendlyName.ToString() + "\n" +
                    "# Serial: " + cert.SerialNumber.ToString() + "\n" +
                    "# SHA1 Fingerprint: " + cert.GetCertHashString().ToString() + "\n" +
                    ExportToPEM(cert) + "\n");
            return builder.ToString();
        }

        public static string ExportToPEM(X509Certificate cert)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");

            return builder.ToString();
        }
    }
}
