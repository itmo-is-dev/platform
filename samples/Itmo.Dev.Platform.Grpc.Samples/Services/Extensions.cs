using Itmo.Dev.Platform.Grpc.Samples.Clients;
using Itmo.Dev.Platform.Grpc.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Grpc.Samples.Services;

public static class Extensions
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection collection)
    {
        return collection.AddPlatformGrpcServices(services => services
            .AddInterceptor<SampleServiceInterceptor>()
            .AddHeaderHandler<HeaderHandler>());
    }
}