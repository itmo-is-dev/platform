using Grpc.Core.Interceptors;

namespace Itmo.Dev.Platform.Grpc.Clients;

public interface IPlatformGrpcClientBuilder
{
    IPlatformGrpcClientBuilder WithInterceptor<TInterceptor>() where TInterceptor : Interceptor;
}