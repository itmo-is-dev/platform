using Sentry.Extensibility;

namespace Itmo.Dev.Platform.Observability.Sentry.ExceptionFilters;

internal class CancelledExceptionFilter : IExceptionFilter
{
    public bool Filter(Exception ex)
    {
        return ex is not TaskCanceledException and not OperationCanceledException;
    }
}