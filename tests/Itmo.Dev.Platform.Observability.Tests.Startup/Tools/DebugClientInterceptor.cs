using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Itmo.Dev.Platform.Observability.Tests.Startup.Tools;

public sealed class DebugClientInterceptor : Interceptor
{
    private readonly ILogger<DebugClientInterceptor> _logger;

    public DebugClientInterceptor(ILogger<DebugClientInterceptor> logger)
    {
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        AsyncUnaryCall<TResponse> call = continuation(request, context);
        call.ResponseHeadersAsync.ContinueWith(async task =>
        {
            Metadata metadata = await task;

            _logger.LogInformation(
                "Response metadata: {Value}",
                string.Join(", ", metadata.Select(x => $"{x.Key}: {x.Value}")));
        });

        return call;
    }
}
