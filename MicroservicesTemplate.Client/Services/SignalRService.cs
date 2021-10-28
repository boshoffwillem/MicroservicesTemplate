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
        private readonly HubConnection _connection;

        public event Action<List<DataReply>> DataBlobReceived;

        public event Action<DataReply> DataRowReceived;

        public SignalRService()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/hubs/dataaccess")
                .Build();
            _connection.On<List<DataReply>>("GettingDataBlob",
                (dataBlob) => DataBlobReceived.Invoke(dataBlob));
            _connection.On<DataReply>("GettingDataRow",
                (dataRow) => DataRowReceived.Invoke(dataRow));
        }

        public async Task ConnectAsync()
        {
            await _connection.StartAsync();
        }

        public async Task GetDataBlob()
        {
            //await _connection.SendAsync("GetDataBlob", Program.NUMBER_OF_DATA_ROWS);
            await _connection.InvokeAsync("GetDataBlob", Program.NUMBER_OF_DATA_ROWS);
            Console.WriteLine("SignalR Client -- Done requesting GetDataBlob");
        }

        public async Task GetDataByRow()
        {
            await _connection.InvokeAsync("GetDataByRow", Program.NUMBER_OF_DATA_ROWS);
            Console.WriteLine("SignalR Client -- Done requesting GetDataByRow");
        }
        
        [Benchmark]
        public async Task GetDataBlobBenchmark()
        {
            await _connection.InvokeAsync("GetDataBlob", Program.NUMBER_OF_DATA_ROWS_BENCHMARK);
            Console.WriteLine("SignalR Client -- Done requesting GetDataBlob");
        }

        [Benchmark]
        public async Task GetDataByRowBenchmark()
        {
            await _connection.InvokeAsync("GetDataByRow", Program.NUMBER_OF_DATA_ROWS_BENCHMARK);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}