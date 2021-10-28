using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using MicroservicesTemplate.DataService;
using Microsoft.AspNetCore.SignalR.Client;

namespace MicroservicesTemplate.Client
{
    class Program
    {
        private static bool _busyWithSignalRBenchmark;

        public const int NUMBER_OF_DATA_ROWS = 10;
        public const int NUMBER_OF_DATA_ROWS_BENCHMARK = 1_000_000;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // gRPC
            using GrpcService grpcService = new();

            // SignalR
            await using SignalRService signalRService = new();
            await signalRService.ConnectAsync();
            signalRService.DataBlobReceived += OnDataBlobReceived;
            signalRService.DataRowReceived += OnDataRowReceived;

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
                else if (userCommand == "4")
                {
                    await signalRService.GetDataBlob();
                }
                else if (userCommand == "5")
                {
                    await signalRService.GetDataByRow();
                }
                else if (userCommand == "6")
                {
                    _busyWithSignalRBenchmark = true;
                    Summary summary = BenchmarkRunner.Run<SignalRService>();
                    _busyWithSignalRBenchmark = false;
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
            Console.WriteLine("4 -- SignalR GetDataBlob");
            Console.WriteLine("5 -- SignalR GetDataByRow");
            Console.WriteLine("6 -- SignalR Benchmark");
        }

        private static void OnDataBlobReceived(List<DataReply> dataBlob)
        {
            if (_busyWithSignalRBenchmark) return;

            foreach (DataReply dataRow in dataBlob)
            {
                Console.WriteLine(dataRow.DataRow);
            }
        }

        private static void OnDataRowReceived(DataReply dataRow)
        {
            if (_busyWithSignalRBenchmark) return;

            Console.WriteLine(dataRow.DataRow);
        }
    }
}
