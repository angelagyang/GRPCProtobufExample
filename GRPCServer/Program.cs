using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Server;

namespace GRPCServer
{
    class Program
    {
        // Using sample test credentials from https://github.com/grpc/grpc/tree/master/src/core/tsi/test_creds
        private static string _sslThumbprint = ""; // Your own SSL certificate (for localhost) thumbprint 
        private static string _clientThumbprint = "BC0C1B5DAC867DB1B5502CA60539569C75F342C4"; // Thumbprint from sample cert 

        static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel(o =>
                {
                    var sslCertificate = LoadSSLCertificate(); 
                    o.ListenAnyIP(5001, listenOptions =>
                    {
                        listenOptions.UseHttps(sslCertificate, httpsOptions =>
                        {
                            httpsOptions.SslProtocols = SslProtocols.Tls12;
                            httpsOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                            httpsOptions.ClientCertificateValidation = (certificate, chain, errors) =>
                            {
                                return certificate.Thumbprint.Equals(_clientThumbprint, StringComparison.OrdinalIgnoreCase);
                            };
                        });
                    });
                })
                .UseUrls("https://localhost:5001")
                .UseStartup<Program>();
        }

        private static X509Certificate2 LoadSSLCertificate()
        {
            var certStore = new X509Store(StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);
            var cert = certStore.Certificates.Find(X509FindType.FindByThumbprint, _sslThumbprint, true)[0];
            certStore.Close();

            return cert;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddCodeFirstGrpc(config =>
            {
                config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseRouting(); 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
            });
        }
    }
}
