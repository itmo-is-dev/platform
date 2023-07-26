using Itmo.Dev.Platform.Common.Tools;
using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Common.Extensions;

internal static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<IAsyncEnumerable<T>> ChunkAsync<T>(
        this IAsyncEnumerable<T> enumerable,
        int count,
        TimeSpan timeout,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var enumerator = enumerable.GetAsyncEnumerator(cancellationToken);

        var finished = new Box<bool>(false);

        while (finished.Value is false)
        {
            yield return GetChunkAsync(enumerator, finished, count, timeout, cancellationToken);
        }
    }

    private static async IAsyncEnumerable<T> GetChunkAsync<T>(
        IAsyncEnumerator<T> enumerator,
        Box<bool> finished,
        int count,
        TimeSpan timeout,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ShouldProceed(IAsyncResult task, int yieldedCount1)
        {
            return cancellationToken.IsCancellationRequested is false
                   && task.IsCompleted is false
                   && yieldedCount1 < count;
        }

        var delayTask = Task.Delay(timeout, cancellationToken);
        var nextTask = enumerator.MoveNextAsync();

        var yieldedCount = 0;

        while (ShouldProceed(delayTask, yieldedCount))
        {
            if (nextTask.IsFaulted)
            {
                await nextTask;
                yield break;
            }

            if (nextTask.IsCompletedSuccessfully is false)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                continue;
            }

            var result = await nextTask;

            if (result is false)
            {
                finished.Value = true;
                yield break;
            }

            yieldedCount++;
            yield return enumerator.Current;

            nextTask = enumerator.MoveNextAsync();
        }
    }
}