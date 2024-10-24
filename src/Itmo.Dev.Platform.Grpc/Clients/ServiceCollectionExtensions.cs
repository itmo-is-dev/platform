using Itmo.Dev.Platform.Grpc.Clients.Builders;
using Itmo.Dev.Platform.Grpc.Clients.Interceptors;
using Itmo.Dev.Platform.Grpc.Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.GrpcCore;

namespace Itmo.Dev.Platform.Grpc.Clients;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformGrpcClients(
        this IServiceCollection collection,
        Func<IPlatformGrpcClientsConfigurator, IPlatformGrpcClientsBuilder> action)
    {
        collection.AddScoped<PlatformHeaderClientInterceptor>();
        collection.AddScoped<ClientTracingInterceptor>();

        collection.AddSingleton<ClientTracingInterceptorOptions>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PlatformGrpcClientsOptions>>();

            return new ClientTracingInterceptorOptions
            {
                RecordException = options.Value.RecordExceptions,
                RecordMessageEvents = options.Value.RecordMessageEvents,
            };
        });

        var builder = new ClientsBuilder(collection);
        action.Invoke(builder);

        return collection;
    }
}