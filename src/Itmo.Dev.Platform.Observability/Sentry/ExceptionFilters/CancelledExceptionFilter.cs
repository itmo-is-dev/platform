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
        var isRelevant = ex is not TaskCanceledException and not OperationCanceledException;

        if (isRelevant is false)
        {
            _logger.LogTrace(ex, "Skipping writing exception to sentry");
        }

        return isRelevant;
    }
}