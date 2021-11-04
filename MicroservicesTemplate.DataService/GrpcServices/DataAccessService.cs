using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace MicroservicesTemplate.DataService
{
    public class DataAccessService : DataAccess.DataAccessBase
    {
        private readonly ILogger<DataAccessService> _logger;

        public DataAccessService(ILogger<DataAccessService> logger)
        {
            _logger = logger;
        }

        public override Task<DataReplyBlob> GetDataBlob(DataRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"GetDataBlob request received: {request.NumberOfDataRows}");
            DataReplyBlob dataBlob = new();

            for (int i = 0; i < request.NumberOfDataRows; i++)
            {
                DataReply dataRow = new()
                {
                    DataRow = "I am a long string, representing a lot of data"
                };
                dataBlob.DataRows.Add(dataRow);
            }

            return Task.FromResult(dataBlob);
        }

        public override async Task GetDataStream(DataRequest request, IServerStreamWriter<DataReply> responseStream, ServerCallContext context)
        {
            _logger.LogInformation($"GetDataStream request received: {request.NumberOfDataRows}");

            for (int i = 0; i < request.NumberOfDataRows; i++)
            {
                await responseStream.WriteAsync(new DataReply
                {
                    DataRow = "I am a long string, representing a lot of data"
                });
            }
        }
    }
}
