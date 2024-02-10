using Itmo.Dev.Platform.Grpc.Services.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Grpc.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformGrpcServices(
        this IServiceCollection collection,
        Func<IPlatformGrpcServicesBuilder, IPlatformGrpcServicesBuilder> action)
    {
        collection.AddGrpc(options =>
        {
            var builder = new PlatformGrpcServicesBuilder(collection, options);
            action.Invoke(builder);
        });

        collection.AddGrpcReflection();

        return collection;
    }
}