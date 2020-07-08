using System;
using System.Security.Authentication;
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
        private static string _thumbprint = ""; // Thumbprint for Client Cert Verification
        static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel(o =>
                {
                    o.ListenAnyIP(5001, listenOptions =>
                    {
                        listenOptions.UseHttps(httpsOptions =>
                        {
                            httpsOptions.SslProtocols = SslProtocols.Tls12;
                            httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                            httpsOptions.ClientCertificateValidation = (certificate, chain, errors) =>
                            {
                                return certificate.Thumbprint.Equals(_thumbprint, StringComparison.OrdinalIgnoreCase);
                            };
                        });
                    });
                })
                .UseUrls("https://localhost:5001")
                .UseStartup<Program>();
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
