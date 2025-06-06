using Grpc.Core.Interceptors;
using Grpc.Net.ClientFactory;
using Itmo.Dev.Platform.Grpc.Clients.Interceptors;
using Itmo.Dev.Platform.Grpc.Clients.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.GrpcCore;

namespace Itmo.Dev.Platform.Grpc.Clients.Builders;

public class ClientBuilder<TClient> : IPlatformGrpcClientBuilder
    where TClient : class
{
    private readonly IServiceCollection _collection;
    private readonly IHttpClientBuilder _clientBuilder;

    public ClientBuilder(IServiceCollection collection, string serviceName)
    {
        _collection = collection;

        _clientBuilder = collection.AddGrpcClient<TClient>(
            $"{serviceName}:{typeof(TClient).Name}",
            (sp, o) =>
            {
                var monitor = sp.GetRequiredService<IOptionsMonitor<PlatformGrpcClientOptions>>();
                var options = monitor.Get(serviceName);

                o.Address = options.Address;
            });

        _clientBuilder.AddInterceptor<PlatformHeaderClientInterceptor>(InterceptorScope.Client);
        _clientBuilder.AddInterceptor<ClientTracingInterceptor>(InterceptorScope.Client);
    }

    public IPlatformGrpcClientBuilder WithInterceptor<TInterceptor>()
        where TInterceptor : Interceptor
    {
        _collection.TryAddScoped<TInterceptor>();
        _clientBuilder.AddInterceptor<TInterceptor>(InterceptorScope.Client);

        return this;
    }
}
