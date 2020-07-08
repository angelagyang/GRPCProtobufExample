
using Grpc.Core;
using GRPCServer;
using ProtoBuf.Grpc.Client;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GRPCClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // Where can I configure a client certificate? 
            var channelCreds = new SslCredentials(GetRootCertificates());
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

        /**
        * Source: https://stackoverflow.com/questions/58125102/grpc-net-client-fails-to-connect-to-server-with-ssl
        **/
        public static string GetRootCertificates()
        {
            StringBuilder builder = new StringBuilder();
            X509Store store = new X509Store(StoreName.Root);
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 mCert in store.Certificates)
            {
                builder.AppendLine(
                    "# Issuer: " + mCert.Issuer.ToString() + "\n" +
                    "# Subject: " + mCert.Subject.ToString() + "\n" +
                    "# Label: " + mCert.FriendlyName.ToString() + "\n" +
                    "# Serial: " + mCert.SerialNumber.ToString() + "\n" +
                    "# SHA1 Fingerprint: " + mCert.GetCertHashString().ToString() + "\n" +
                    ExportToPEM(mCert) + "\n");
            }
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
