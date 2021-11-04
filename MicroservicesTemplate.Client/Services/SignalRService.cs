using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MicroservicesTemplate.DataService;
using Microsoft.AspNetCore.SignalR.Client;

namespace MicroservicesTemplate.Client
{
    public class SignalRService : IAsyncDisposable
    {
        private static readonly HubConnection _connection;
        private static bool _receivingDataBlob;
        private static int _receivingDataRowCount;
        private static readonly object _receivingLock = new();

        public static event Action<List<DataReply>> DataBlobReceived;

        public static event Action<DataReply> DataRowReceived;

        static SignalRService()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/hubs/dataaccess")
                .Build();
            _connection.On<List<DataReply>>("GettingDataBlob",
                (dataBlob) => DataBlobReceived.Invoke(dataBlob));
            _connection.On<DataReply>("GettingDataRow",
                (dataRow) => DataRowReceived.Invoke(dataRow));
            DataBlobReceived += OnDataBlobReceived;
            DataRowReceived += OnDataRowReceived;
        }

        public SignalRService()
        {
            ConnectAsync().GetAwaiter().GetResult();
        }

        private static void OnDataBlobReceived(List<DataReply> dataBlob)
        {
            if (dataBlob.Count >= Program.NumberOfDataRowsBenchmark)
            {
                lock (_receivingLock)
                {
                    _receivingDataBlob = false;
                }
            }
        }

        private static void OnDataRowReceived(DataReply dataRow)
        {
            _receivingDataRowCount++;

            if (_receivingDataRowCount >= Program.NumberOfDataRowsBenchmark)
            {
                lock (_receivingLock)
                {
                    _receivingDataBlob = false;
                }
            }
        }

        public async Task ConnectAsync()
        {
            await _connection.StartAsync();
        }

        public async Task GetDataBlob()
        {
            //await _connection.SendAsync("GetDataBlob", Program.NUMBER_OF_DATA_ROWS);
            await _connection.InvokeAsync("GetDataBlob", Program.NumberOfDataRows);
            Console.WriteLine("SignalR Client -- Done requesting GetDataBlob");
        }

        public async Task GetDataByRow()
        {
            await _connection.InvokeAsync("GetDataByRow", Program.NumberOfDataRows);
            Console.WriteLine("SignalR Client -- Done requesting GetDataByRow");
        }

        [Benchmark]
        public async Task GetDataBlobBenchmark()
        {
            _receivingDataBlob = true;
            await _connection.InvokeAsync("GetDataBlob", Program.NumberOfDataRowsBenchmark);

            bool receivingBlob = false;

            lock (_receivingLock)
            {
                receivingBlob = _receivingDataBlob;
            }

            while (receivingBlob)
            {
                lock (_receivingLock)
                {
                    receivingBlob = _receivingDataBlob;
                }

                await Task.Delay(50);
            }
        }

        [Benchmark]
        public async Task GetDataByRowBenchmark()
        {
            _receivingDataBlob = true;
            _receivingDataRowCount = 0;
            await _connection.InvokeAsync("GetDataByRow", Program.NumberOfDataRowsBenchmark);

            bool receivingBlob = false;

            lock (_receivingLock)
            {
                receivingBlob = _receivingDataBlob;
            }

            while (receivingBlob)
            {
                lock (_receivingLock)
                {
                    receivingBlob = _receivingDataBlob;
                }

                await Task.Delay(50);
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await _connection.DisposeAsync();
            DataBlobReceived -= OnDataBlobReceived;
        }
    }
}
