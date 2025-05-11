using Grpc.Core.Interceptors;
using Itmo.Dev.Platform.Grpc.Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Grpc.Clients;

public interface IPlatformGrpcClientServiceNameConfigurator
{
    IPlatformGrpcClientServiceOptionsConfigurator Called(string name);
}

public interface IPlatformGrpcClientServiceOptionsConfigurator
{
    IPlatformGrpcClientConfigurator WithConfiguration(Action<OptionsBuilder<PlatformGrpcClientOptions>> action);

    IPlatformGrpcClientConfigurator WithConfiguration(string sectionPath)
    {
        return WithConfiguration(builder => builder.BindConfiguration(sectionPath));
    }
}

public interface IPlatformGrpcClientConfigurator : IPlatformGrpcClientServiceBuilder
{
    IPlatformGrpcClientConfigurator WithClient<TClient>(
        Func<IPlatformGrpcClientBuilder, IPlatformGrpcClientBuilder>? action = null)
        where TClient : class;
}

public interface IPlatformGrpcClientServiceBuilder
{
    IPlatformGrpcClientServiceBuilder WithInterceptor<TInterceptor>()
        where TInterceptor : Interceptor;
}