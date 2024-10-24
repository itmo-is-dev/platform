using Itmo.Dev.Platform.Grpc.Services.Builders;
using Itmo.Dev.Platform.Grpc.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.GrpcCore;

namespace Itmo.Dev.Platform.Grpc.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformGrpcServices(
        this IServiceCollection collection,
        Func<IPlatformGrpcServicesBuilder, IPlatformGrpcServicesBuilder> action)
    {
        var builder = new PlatformGrpcServicesBuilder(collection);
        action.Invoke(builder);

        collection.AddGrpc(options =>
        {
            builder.ConfigureOptions(options);
            options.Interceptors.Add<ServerTracingInterceptor>();
        });

        collection.AddSingleton<ServerTracingInterceptorOptions>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PlatformGrpcServerOptions>>();

            return new ServerTracingInterceptorOptions
            {
                RecordMessageEvents = options.Value.RecordMessageEvents,
                RecordException = options.Value.RecordExceptions,
            };
        });

        collection.AddGrpcReflection();

        return collection;
    }
}