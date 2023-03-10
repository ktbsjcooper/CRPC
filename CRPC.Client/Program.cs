using CRPC.Common;
using Grpc.Core;
using Grpc.Core.Logging;
using Microsoft.Extensions.Configuration;
using ProtoBuf.Grpc.Client;
using static CRPC.Common.Contracts;

namespace CRPC.Client
{
    internal class Program
    {
        const int MAX_LENGTH = 128 * 1024 * 1024;

        static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var serverHostname = config.GetValue<string>("GrpcServer:Hostname");
            var serverPort = config.GetValue<int>("GrpcServer:Port");

            var channelOptions = new ChannelOption[] {
                new ChannelOption(ChannelOptions.MaxReceiveMessageLength, MAX_LENGTH),
                new ChannelOption(ChannelOptions.MaxSendMessageLength, MAX_LENGTH),
                new ChannelOption(ChannelOptions.MaxConcurrentStreams, int.MaxValue)
            };

            ChannelCredentials channelCredentials;
            var certPath = config.GetValue<string>("GrpcServer:CertificatePath");
            if (File.Exists(certPath))
            {
                channelCredentials = new SslCredentials(File.ReadAllText(certPath));
            }
            else
            {
                channelCredentials = new SslCredentials(SslDefault.Certificate);
                channelOptions = channelOptions.Append(new ChannelOption(ChannelOptions.SslTargetNameOverride, SslDefault.Hostname)).ToArray();
            }

            var channel = new Channel(serverHostname, serverPort, channelCredentials, channelOptions);
            var dataService = channel.CreateGrpcService<IDataService>();

            var parallelCount = config.GetValue<int>("DataService:ParallelCount");
            var dataSize = config.GetValue<int>("DataService:Size");
            while (true)
            {
                Console.WriteLine($"Waiting for {parallelCount} requests at size {dataSize}...");
                var senders = new System.Collections.Concurrent.ConcurrentDictionary<string, int>();

                var tasks = Enumerable.Range(0, parallelCount).AsParallel().Select(async t =>
                {
                    
                    var response = await dataService.GetCustomDataAsync(new CustomSizeDataRequest() { Size = dataSize });
                    if (response.Data?.Length != dataSize)
                    {
                        throw new ApplicationException($"Data size mismatch. Got {response.Data?.Length} Expected {dataSize}");
                    }

                    senders.AddOrUpdate(response.Sender ?? "UNKNOWN", 1, (k, v) => v + 1);
                });
                await Task.WhenAll(tasks);

                Console.WriteLine(string.Join(Environment.NewLine, senders.Select(x => $"[{x.Key}]: {x.Value} ")));
            }
        }
    }
}