using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Itmo.Dev.Platform.Grpc.Clients.Builders;

internal class ClientsBuilder : IPlatformGrpcClientsBuilder
{
    private readonly IServiceCollection _collection;

    public ClientsBuilder(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IPlatformGrpcClientsBuilder AddHeaderProvider<TProvider>()
        where TProvider : class, IPlatformGrpcHeaderProvider
    {
        _collection.TryAddEnumerable(ServiceDescriptor.Scoped<IPlatformGrpcHeaderProvider, TProvider>());
        return this;
    }

    public IPlatformGrpcClientsBuilder AddService(
        Func<IPlatformGrpcClientServiceNameConfigurator, IPlatformGrpcClientServiceBuilder> action)
    {
        var configurator = new ClientServiceNameConfigurator(_collection);
        action.Invoke(configurator);

        return this;
    }
}