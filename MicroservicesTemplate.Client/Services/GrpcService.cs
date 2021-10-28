using System;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Grpc.Core;
using Grpc.Net.Client;
using MicroservicesTemplate.DataService;

namespace MicroservicesTemplate.Client
{
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
                NumberOfDataRows = Program.NUMBER_OF_DATA_ROWS_BENCHMARK
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
                NumberOfDataRows = Program.NUMBER_OF_DATA_ROWS_BENCHMARK
            });

            await foreach (DataReply item in dataReplyStream.ResponseStream.ReadAllAsync())
            {
            }
        }
    }
}