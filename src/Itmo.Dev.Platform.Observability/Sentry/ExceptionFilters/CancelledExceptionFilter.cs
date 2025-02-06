using Grpc.Core;
using Sentry.Extensibility;

namespace Itmo.Dev.Platform.Observability.Sentry.ExceptionFilters;

internal class CancelledExceptionFilter : IExceptionFilter
{
    private readonly ILogger _logger;

    public CancelledExceptionFilter(ILogger logger)
    {
        _logger = logger;
    }

    public bool Filter(Exception ex)
    {
        var isFilteredOut = ex is
            TaskCanceledException
            or OperationCanceledException
            or RpcException { StatusCode: StatusCode.Cancelled };

        if (isFilteredOut)
        {
            _logger.LogTrace(ex, "Skipping writing exception to sentry");
        }

        return isFilteredOut;
    }
}
