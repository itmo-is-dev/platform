using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;

namespace Itmo.Dev.Platform.Grpc.Clients;

public interface IPlatformGrpcClientServiceNameConfigurator
{
    IPlatformGrpcClientServiceOptionsConfigurator Called(string name);
}

public interface IPlatformGrpcClientServiceOptionsConfigurator
{
    IPlatformGrpcClientConfigurator WithConfiguration(IConfiguration configuration);
}

public interface IPlatformGrpcClientConfigurator : IPlatformGrpcClientServiceBuilder
{
    IPlatformGrpcClientConfigurator WithClient<TClient>(
        Func<IPlatformGrpcClientBuilder, IPlatformGrpcClientBuilder>? action = null)
        where TClient : class;
}

public interface IPlatformGrpcClientServiceBuilder
{
    IPlatformGrpcClientServiceBuilder WithInterceptor<TInterceptor>() where TInterceptor : Interceptor;
}