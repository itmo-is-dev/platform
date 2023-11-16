using Itmo.Dev.Platform.Common.Tools;

namespace Itmo.Dev.Platform.Common.Extensions;

public static class SemaphoreExtensions
{
    public static async ValueTask<SemaphoreSubscription> UseAsync(
        this SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        return new SemaphoreSubscription(semaphore);
    }
}