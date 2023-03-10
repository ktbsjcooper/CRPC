using CRPC.Common;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using ProtoBuf.Grpc.Server;

namespace CRPC.Server
{
    internal class Program
    {
        const int MAX_LENGTH = 128 * 1024 * 1024;

        static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var channelOptions = new ChannelOption[] {
                new ChannelOption(ChannelOptions.SoReuseport, 0),
                new ChannelOption(ChannelOptions.MaxReceiveMessageLength, MAX_LENGTH),
                new ChannelOption(ChannelOptions.MaxSendMessageLength, MAX_LENGTH),
                new ChannelOption(ChannelOptions.MaxConcurrentStreams, int.MaxValue)
            };

            ServerCredentials serverCredentials;
            var certPath = config.GetValue<string>("GrpcServer:CertificatePath");
            var certKeyPath = config.GetValue<string>("GrpcServer:CertificateKeyPath");
            if (File.Exists(certPath) && File.Exists(certKeyPath))
            {
                var certs = new List<KeyCertificatePair>
                {
                    new KeyCertificatePair(File.ReadAllText(certPath), File.ReadAllText(certKeyPath))
                };
                serverCredentials = new SslServerCredentials(certs);
            }
            else
            {
                var certs = new List<KeyCertificatePair>
                {
                    new KeyCertificatePair(SslDefault.Certificate, SslDefault.Key)
                };
                serverCredentials = new SslServerCredentials(certs);
                channelOptions = channelOptions.Append(new ChannelOption(ChannelOptions.SslTargetNameOverride, SslDefault.Hostname)).ToArray();
            }

            var server = new Grpc.Core.Server(channelOptions)
            {
                Ports = { { "0.0.0.0", config.GetValue<int>("GrpcServer:Port"), serverCredentials } },
            };

            var dataService = new DataService();
            server.Services.AddCodeFirst(dataService);

            server.Start();

            await server.ShutdownTask;
        }
    }
}