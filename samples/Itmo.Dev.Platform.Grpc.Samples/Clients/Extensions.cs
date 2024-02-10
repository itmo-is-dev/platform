using Itmo.Dev.Platform.Grpc.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Grpc.Samples.Clients;

public static class Extensions
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection collection, IConfiguration configuration)
    {
        return collection.AddPlatformGrpcClients(clients => clients
            .AddService(service => service
                .Called("sample")
                .WithConfiguration(configuration.GetSection("Sample:Grpc"))
                .WithClient<SampleService.SampleServiceClient>())
            .AddHeaderProvider<HeaderProvider>());
    }
}