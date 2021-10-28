using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MicroservicesTemplate.DataService.SignalRServices
{
    public class DataAccessHub : Hub
    {
        private readonly ILogger<DataAccessHub> _logger;

        public DataAccessHub(ILogger<DataAccessHub> logger)
        {
            _logger = logger;
        }

        public async Task GetDataBlob(int numberOfDataRows)
        {
            _logger.LogInformation($"GetDataBlob request received: {numberOfDataRows}");
            List<DataReply> dataBlob = new();

            for (int i = 0; i < numberOfDataRows; i++)
            {
                DataReply dataRow = new()
                {
                    DataRow = "I am a long string, representing a lot of data"
                };
                dataBlob.Add(dataRow);
            }

            await Clients.Caller.SendAsync("GettingDataBlob",
                dataBlob);
        }

        public async Task GetDataByRow(int numberOfDataRows)
        {
            _logger.LogInformation($"GetDataByRow request received: {numberOfDataRows}");

            for (int i = 0; i < numberOfDataRows; i++)
            {
                DataReply dataRow = new()
                {
                    DataRow = "I am a long string, representing a lot of data"
                };

                await Clients.Caller.SendAsync("GettingDataRow",
                    dataRow);
            }
        }
    }
}