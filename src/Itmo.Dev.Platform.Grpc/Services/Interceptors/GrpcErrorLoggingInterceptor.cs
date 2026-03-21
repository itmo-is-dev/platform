using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Itmo.Dev.Platform.Grpc.Services.Interceptors;

internal sealed class GrpcErrorLoggingInterceptor(ILogger<GrpcErrorLoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await base.UnaryServerHandler(request, context, continuation);
        }
        catch (Exception exception) when (ShouldIgnoreException(exception) is false)
        {
            logger.LogError(
                exception,
                "Error while executing gRPC method = '{Method}'",
                context.Method);

            throw;
        }
    }

    private static bool ShouldIgnoreException(Exception exception)
    {
        return exception switch
        {
            RpcException e => ShouldIgnoreRpcException(e),
            OperationCanceledException => true,
            _ => false,
        };
    }

    private static bool ShouldIgnoreRpcException(RpcException exception)
    {
        return exception.StatusCode is
            StatusCode.Aborted
            or StatusCode.Cancelled
            or StatusCode.DeadlineExceeded
            or StatusCode.ResourceExhausted
            or StatusCode.Unavailable
            or StatusCode.Unknown;
    }
}
