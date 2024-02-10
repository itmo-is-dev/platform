using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Itmo.Dev.Platform.Grpc.Services.Interceptors;

internal class PlatformHeaderServerInterceptor : Interceptor
{
    private readonly IEnumerable<IPlatformGrpcHeaderHandler> _handlers;

    public PlatformHeaderServerInterceptor(IEnumerable<IPlatformGrpcHeaderHandler> handlers)
    {
        _handlers = handlers;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var headers = new MetadataCollection(context.RequestHeaders);

        foreach (IPlatformGrpcHeaderHandler handler in _handlers)
        {
            await handler.HandleHeadersAsync(headers, context.CancellationToken);
        }

        return await continuation.Invoke(request, context);
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        var headers = new MetadataCollection(context.RequestHeaders);

        foreach (IPlatformGrpcHeaderHandler handler in _handlers)
        {
            await handler.HandleHeadersAsync(headers, context.CancellationToken);
        }

        return await continuation.Invoke(requestStream, context);
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        var headers = new MetadataCollection(context.RequestHeaders);

        foreach (IPlatformGrpcHeaderHandler handler in _handlers)
        {
            await handler.HandleHeadersAsync(headers, context.CancellationToken);
        }

        await continuation.Invoke(request, responseStream, context);
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        var headers = new MetadataCollection(context.RequestHeaders);

        foreach (IPlatformGrpcHeaderHandler handler in _handlers)
        {
            await handler.HandleHeadersAsync(headers, context.CancellationToken);
        }

        await continuation.Invoke(requestStream, responseStream, context);
    }
}