using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itmo.Dev.Platform.Grpc.Clients.Builders;

internal class ClientsBuilder : IPlatformGrpcClientsConfigurator
{
    private readonly IServiceCollection _collection;
    private readonly List<IPlatformGrpcClientServiceBuilder> _serviceBuilders;

    public ClientsBuilder(IServiceCollection collection)
    {
        _collection = collection;
        _serviceBuilders = new List<IPlatformGrpcClientServiceBuilder>();
    }

    public IPlatformGrpcClientsConfigurator AddHeaderProvider<TProvider>()
        where TProvider : class, IPlatformGrpcHeaderProvider
    {
        _collection.TryAddEnumerable(ServiceDescriptor.Scoped<IPlatformGrpcHeaderProvider, TProvider>());
        return this;
    }

    public IPlatformGrpcClientsConfigurator AddService(
        Func<IPlatformGrpcClientServiceNameConfigurator, IPlatformGrpcClientServiceBuilder> action)
    {
        var configurator = new ClientServiceNameConfigurator(_collection);
        var builder = action.Invoke(configurator);

        _serviceBuilders.Add(builder);

        return this;
    }

    public IPlatformGrpcClientsBuilder AddInterceptor<TInterceptor>() where TInterceptor : Interceptor
    {
        foreach (IPlatformGrpcClientServiceBuilder serviceBuilder in _serviceBuilders)
        {
            serviceBuilder.WithInterceptor<TInterceptor>();
        }

        return this;
    }
}