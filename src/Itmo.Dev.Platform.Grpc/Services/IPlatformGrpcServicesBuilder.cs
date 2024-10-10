using Grpc.Core.Interceptors;
using Itmo.Dev.Platform.Grpc.Services.Options;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Grpc.Services;

public interface IPlatformGrpcServicesBuilder
{
    IPlatformGrpcServicesBuilder AddInterceptor<TInterceptor>()
        where TInterceptor : Interceptor;

    IPlatformGrpcServicesBuilder AddHeaderHandler<THandler>()
        where THandler : class, IPlatformGrpcHeaderHandler;

    IPlatformGrpcServicesBuilder ConfigureOptions(Action<OptionsBuilder<PlatformGrpcServerOptions>> action);
}