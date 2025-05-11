using Grpc.Core.Interceptors;
using Itmo.Dev.Platform.Grpc.Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Grpc.Clients;

public interface IPlatformGrpcClientsConfigurator : IPlatformGrpcClientsBuilder
{
    IPlatformGrpcClientsConfigurator AddHeaderProvider<TProvider>()
        where TProvider : class, IPlatformGrpcHeaderProvider;

    IPlatformGrpcClientsConfigurator AddService(
        Func<IPlatformGrpcClientServiceNameConfigurator, IPlatformGrpcClientServiceBuilder> action);

    IPlatformGrpcClientsConfigurator ConfigureOptions(Action<OptionsBuilder<PlatformGrpcClientsOptions>> action);

    IPlatformGrpcClientsConfigurator ConfigureOptions(string sectionPath)
    {
        return ConfigureOptions(builder => builder.BindConfiguration(sectionPath));
    }
}

public interface IPlatformGrpcClientsBuilder
{
    IPlatformGrpcClientsBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : Interceptor;
}