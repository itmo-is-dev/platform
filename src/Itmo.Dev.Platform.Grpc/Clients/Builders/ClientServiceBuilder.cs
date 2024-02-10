using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Grpc.Clients.Builders;

internal class ClientServiceNameConfigurator : IPlatformGrpcClientServiceNameConfigurator
{
    private readonly IServiceCollection _collection;

    public ClientServiceNameConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IPlatformGrpcClientServiceOptionsConfigurator Called(string name)
        => new ClientServiceOptionsConfigurator(_collection, name);
}

internal class ClientServiceOptionsConfigurator : IPlatformGrpcClientServiceOptionsConfigurator
{
    private readonly IServiceCollection _collection;
    private readonly string _name;

    public ClientServiceOptionsConfigurator(IServiceCollection collection, string name)
    {
        _collection = collection;
        _name = name;
    }

    public IPlatformGrpcClientConfigurator WithConfiguration(IConfiguration configuration)
    {
        _collection
            .AddOptions<PlatformGrpcClientOptions>(_name)
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return new ClientServiceBuilder(_collection, _name);
    }
}

internal class ClientServiceBuilder : IPlatformGrpcClientConfigurator
{
    private readonly IServiceCollection _collection;
    private readonly string _serviceName;
    private readonly List<IPlatformGrpcClientBuilder> _clientBuilders;

    public ClientServiceBuilder(IServiceCollection collection, string serviceName)
    {
        _collection = collection;
        _serviceName = serviceName;
        _clientBuilders = new List<IPlatformGrpcClientBuilder>();
    }

    public IPlatformGrpcClientConfigurator WithClient<TClient>(
        Func<IPlatformGrpcClientBuilder, IPlatformGrpcClientBuilder>? action = null)
        where TClient : class
    {
        var builder = new ClientBuilder<TClient>(_collection, _serviceName);
        action?.Invoke(builder);

        _clientBuilders.Add(builder);

        return this;
    }

    public IPlatformGrpcClientServiceBuilder WithInterceptor<TInterceptor>() where TInterceptor : Interceptor
    {
        foreach (IPlatformGrpcClientBuilder builder in _clientBuilders)
        {
            builder.WithInterceptor<TInterceptor>();
        }

        return this;
    }
}