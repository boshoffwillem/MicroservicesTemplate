using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Grpc.Core;
using Grpc.Net.Client;
using MicroservicesTemplate.DataService;

namespace MicroservicesTemplate.Client
{
    class Program
    {
        public const int NUMBER_OF_DATA_ROWS = 1_000_000;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using GrpcService grpcService = new();

            UserPrompt();
            string userCommand = Console.ReadLine();

            while (userCommand != "exit")
            {
                if (userCommand == "1")
                {
                    await grpcService.GrpcGetDataBlob();
                }
                else if (userCommand == "2")
                {
                    await grpcService.GrpcGetDataStream();
                }
                else if (userCommand == "3")
                {
                    Summary summary = BenchmarkRunner.Run<GrpcService>();
                }
                else
                {
                    Console.WriteLine("Invalid choice!");
                }

                UserPrompt();
                userCommand = Console.ReadLine();
            }
        }

        private static void UserPrompt()
        {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1 -- Grpc GetDataBlob");
            Console.WriteLine("2 -- Grpc GetData");
            Console.WriteLine("3 -- Grpc Benchmark");
        }

    }

    public class GrpcService : IDisposable
    {
        private readonly GrpcChannel _grpcChannel;
        private readonly DataAccess.DataAccessClient _grpcDataAccessClient;

        public GrpcService()
        {
            HttpClientHandler httpHandler = new();
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            _grpcChannel = GrpcChannel.ForAddress("https://localhost:5001",
                new GrpcChannelOptions { HttpHandler = httpHandler });
            _grpcDataAccessClient = new DataAccess.DataAccessClient(_grpcChannel);
        }

        public void Dispose()
        {
            _grpcChannel.Dispose();
        }

        public async Task GrpcGetDataBlob()
        {
            var response = await _grpcDataAccessClient.GetDataBlobAsync(new DataRequest
            {
                NumberOfDataRows = Program.NUMBER_OF_DATA_ROWS
            });

            Console.WriteLine($"Server response with {response.DataRows.Count} rows");

            for (int i = 0; i < response.DataRows.Count; i++)
            {
                Console.WriteLine(response.DataRows[i].DataRow);
            }
        }

        [Benchmark]
        public async Task GrpcGetDataBlobBenchmark()
        {
            var response = await _grpcDataAccessClient.GetDataBlobAsync(new DataRequest
            {
                NumberOfDataRows = Program.NUMBER_OF_DATA_ROWS
            });
        }

        public async Task GrpcGetDataStream()
        {
            using AsyncServerStreamingCall<DataReply> dataReplyStream = _grpcDataAccessClient.GetDataStream(new DataRequest
            {
                NumberOfDataRows = Program.NUMBER_OF_DATA_ROWS
            });

            await foreach (DataReply item in dataReplyStream.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(item.DataRow);
            }
        }

        [Benchmark]
        public async Task GrpcGetDataStreamBenchmark()
        {
            using AsyncServerStreamingCall<DataReply> dataReplyStream = _grpcDataAccessClient.GetDataStream(new DataRequest
            {
                NumberOfDataRows = Program.NUMBER_OF_DATA_ROWS
            });

            await foreach (DataReply item in dataReplyStream.ResponseStream.ReadAllAsync())
            {
            }
        }
    }
}
