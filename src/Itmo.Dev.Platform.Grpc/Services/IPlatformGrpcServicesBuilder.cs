using Grpc.Core.Interceptors;
using Itmo.Dev.Platform.Grpc.Services.Options;
using Itmo.Dev.Platform.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Grpc.Services;

public interface IPlatformGrpcServicesBuilder
{
    IPlatformGrpcServicesBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : Interceptor;

    IPlatformGrpcServicesBuilder AddHeaderHandler<THandler>()
        where THandler : class, IPlatformGrpcHeaderHandler;

    IPlatformGrpcServicesBuilder ConfigureOptions(Action<OptionsBuilder<PlatformGrpcServerOptions>> action);

    [ProducesOptionRegistration<PlatformGrpcServerOptions>(SectionParameterName = nameof(sectionPath))]
    IPlatformGrpcServicesBuilder ConfigureOptions(string sectionPath)
    {
        return ConfigureOptions(builder => builder.BindConfiguration(sectionPath));
    }
}