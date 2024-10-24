using Grpc.Core.Interceptors;
using Itmo.Dev.Platform.Grpc.Clients.Options;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Grpc.Clients;

public interface IPlatformGrpcClientsConfigurator : IPlatformGrpcClientsBuilder
{
    IPlatformGrpcClientsConfigurator AddHeaderProvider<TProvider>()
        where TProvider : class, IPlatformGrpcHeaderProvider;

    IPlatformGrpcClientsConfigurator AddService(
        Func<IPlatformGrpcClientServiceNameConfigurator, IPlatformGrpcClientServiceBuilder> action);

    IPlatformGrpcClientsConfigurator ConfigureOptions(Action<OptionsBuilder<PlatformGrpcClientsOptions>> action);
}

public interface IPlatformGrpcClientsBuilder
{
    IPlatformGrpcClientsBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : Interceptor;
}