using Grpc.Core.Interceptors;

namespace Itmo.Dev.Platform.Grpc.Services;

public interface IPlatformGrpcServicesBuilder
{
    IPlatformGrpcServicesBuilder AddInterceptor<TInterceptor>() where TInterceptor : Interceptor;

    IPlatformGrpcServicesBuilder AddHeaderHandler<THandler>() where THandler : class, IPlatformGrpcHeaderHandler;
}