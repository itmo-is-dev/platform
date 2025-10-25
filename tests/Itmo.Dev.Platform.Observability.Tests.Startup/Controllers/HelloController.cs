using Grpc.Core;

namespace Itmo.Dev.Platform.Observability.Tests.Startup.Controllers;

public class HelloController : HelloService.HelloServiceBase
{
    public override Task<HelloResponse> Hello(HelloRequest request, ServerCallContext context)
    {
        var response = new HelloResponse
        {
            Message = $"Hello, {request.Name}!",
        };

        return Task.FromResult(response);
    }
}
