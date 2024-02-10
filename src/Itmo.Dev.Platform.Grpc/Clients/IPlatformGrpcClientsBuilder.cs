using Grpc.Core.Interceptors;

namespace Itmo.Dev.Platform.Grpc.Clients;

public interface IPlatformGrpcClientsConfigurator : IPlatformGrpcClientsBuilder
{
    IPlatformGrpcClientsConfigurator AddHeaderProvider<TProvider>() where TProvider : class, IPlatformGrpcHeaderProvider;

    IPlatformGrpcClientsConfigurator AddService(
        Func<IPlatformGrpcClientServiceNameConfigurator, IPlatformGrpcClientServiceBuilder> action);
}

public interface IPlatformGrpcClientsBuilder
{
    IPlatformGrpcClientsBuilder AddInterceptor<TInterceptor>() where TInterceptor : Interceptor;
}