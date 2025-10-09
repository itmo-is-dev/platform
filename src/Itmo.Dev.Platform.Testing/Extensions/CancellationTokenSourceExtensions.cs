using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace System.Threading;

public static class CancellationTokenSourceExtensions
{
    /// <summary>
    ///     Cancels cts after specified timeout if debugger is not attached.
    ///     NOTE THAT THIS MAY LEAD TO INFINITE WAIT WHEN RUNNING TESTS IN DEBUG
    /// </summary>
    public static void CancelAfterDebug(this CancellationTokenSource cts, TimeSpan delay)
    {
        cts.CancelAfter(Debugger.IsAttached ? Timeout.InfiniteTimeSpan : delay);
    }
}
