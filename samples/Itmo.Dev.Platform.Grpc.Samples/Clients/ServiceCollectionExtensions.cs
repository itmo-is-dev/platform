using Itmo.Dev.Platform.Grpc.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Grpc.Samples.Clients;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection collection)
    {
        return collection.AddPlatformGrpcClients(clients => clients
            .AddService(service => service
                .Called("sample")
                .WithConfiguration(x => x.BindConfiguration("Sample:Grpc"))
                .WithClient<SampleService.SampleServiceClient>(c => c.WithInterceptor<SampleClientInterceptor>())
                .WithInterceptor<SampleServiceInterceptor>())
            .AddHeaderProvider<HeaderProvider>()
            .AddInterceptor<SampleGlobalInterceptor>());
    }
}