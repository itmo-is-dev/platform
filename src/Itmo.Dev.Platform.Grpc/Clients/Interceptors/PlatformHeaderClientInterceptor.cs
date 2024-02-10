using Grpc.Core;
using Grpc.Core.Interceptors;
using Itmo.Dev.Platform.Grpc.Extensions;

namespace Itmo.Dev.Platform.Grpc.Clients.Interceptors;

internal class PlatformHeaderClientInterceptor : Interceptor
{
    private readonly IEnumerable<IPlatformGrpcHeaderProvider> _headerProviders;

    public PlatformHeaderClientInterceptor(IEnumerable<IPlatformGrpcHeaderProvider> headerProviders)
    {
        _headerProviders = headerProviders;
    }

    private IEnumerable<Metadata.Entry> Entries => _headerProviders.SelectMany(x => x.GetHeaders());

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var options = context.Options.WithAppendedHeaders(Entries);
        context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);

        return continuation.Invoke(request, context);
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var options = context.Options.WithAppendedHeaders(Entries);
        context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);

        return continuation.Invoke(request, context);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var options = context.Options.WithAppendedHeaders(Entries);
        context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);

        return continuation.Invoke(request, context);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var options = context.Options.WithAppendedHeaders(Entries);
        context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);

        return continuation.Invoke(context);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        var options = context.Options.WithAppendedHeaders(Entries);
        context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);

        return continuation.Invoke(context);
    }
}