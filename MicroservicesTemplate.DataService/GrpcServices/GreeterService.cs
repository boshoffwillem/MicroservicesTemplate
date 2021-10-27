using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace MicroservicesTemplate.DataService
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request,
                                                  ServerCallContext context)
        {
            _logger.LogInformation($"SayHello request received: {request.Name}");
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task SayHelloStream(HelloRequest request,
                                            IServerStreamWriter<HelloReply> responseStream,
                                            ServerCallContext context)
        {
            _logger.LogInformation($"SayHelloStream request received: {request.Name}");

            for (int i = 0; i < 10; i++)
            {
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = "Hello " + request.Name + $" {i}"
                });
            }
        }
    }
}
