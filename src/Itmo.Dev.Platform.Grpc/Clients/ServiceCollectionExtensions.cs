using Itmo.Dev.Platform.Grpc.Clients.Builders;
using Itmo.Dev.Platform.Grpc.Clients.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Grpc.Clients;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformGrpcClients(
        this IServiceCollection collection,
        Func<IPlatformGrpcClientsConfigurator, IPlatformGrpcClientsBuilder> action)
    {
        collection.AddScoped<PlatformHeaderClientInterceptor>();

        var builder = new ClientsBuilder(collection);
        action.Invoke(builder);

        return collection;
    }
}