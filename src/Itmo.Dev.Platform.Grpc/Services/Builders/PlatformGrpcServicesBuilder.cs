using Grpc.AspNetCore.Server;
using Grpc.Core.Interceptors;
using Itmo.Dev.Platform.Grpc.Services.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Grpc.Services.Builders;

internal class PlatformGrpcServicesBuilder : IPlatformGrpcServicesBuilder
{
    private readonly IServiceCollection _collection;
    private readonly List<Type> _interceptorTypes;

    public PlatformGrpcServicesBuilder(IServiceCollection collection)
    {
        _collection = collection;
        _interceptorTypes = [];
    }

    public void ConfigureOptions(GrpcServiceOptions options)
    {
        foreach (Type interceptorType in _interceptorTypes)
        {
            options.Interceptors.Add(interceptorType);
        }
    }

    public IPlatformGrpcServicesBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : Interceptor
    {
        _collection.TryAddScoped<TInterceptor>();
        _interceptorTypes.Add(typeof(TInterceptor));

        return this;
    }

    public IPlatformGrpcServicesBuilder AddHeaderHandler<THandler>()
        where THandler : class, IPlatformGrpcHeaderHandler
    {
        _collection.TryAddEnumerable(ServiceDescriptor.Scoped<IPlatformGrpcHeaderHandler, THandler>());
        return this;
    }

    public IPlatformGrpcServicesBuilder ConfigureOptions(Action<OptionsBuilder<PlatformGrpcServerOptions>> action)
    {
        OptionsBuilder<PlatformGrpcServerOptions> builder = _collection.AddOptions<PlatformGrpcServerOptions>();
        action.Invoke(builder);

        return this;
    }
}