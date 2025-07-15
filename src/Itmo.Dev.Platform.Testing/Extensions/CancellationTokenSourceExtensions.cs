using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace System.Threading;

public static class CancellationTokenSourceExtensions
{
    public static void CancelAfterDebug(this CancellationTokenSource cts, TimeSpan delay)
    {
        cts.CancelAfter(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : delay);
    }
}
