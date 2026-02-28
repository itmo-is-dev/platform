using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Common.Extensions.Iterators;

internal sealed class ChunkedAsyncEnumerable<T>(
    IAsyncEnumerable<T> enumerable,
    int count,
    TimeSpan timeout,
    TimeSpan timeoutChunkSpan
)
    : IAsyncEnumerable<IReadOnlyList<T>>
{
    public async IAsyncEnumerator<IReadOnlyList<T>> GetAsyncEnumerator(CancellationToken cancellationToken)
    {
        var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var enumerator = enumerable
            .WithCancellation(cancellationSource.Token)
            .ConfigureAwait(false)
            .GetAsyncEnumerator();

        var enumeratorTask = default(ConfiguredValueTaskAwaitable<bool>);
        var buffer = new List<T>();

        try
        {
            while (true)
            {
                enumeratorTask = enumerator.MoveNextAsync();
                var awaiter = enumeratorTask.GetAwaiter();

                var remainingDelay = timeout;

                while (awaiter.IsCompleted is false && remainingDelay > TimeSpan.Zero)
                {
                    var delay = remainingDelay <= timeoutChunkSpan
                        ? remainingDelay
                        : timeoutChunkSpan;

                    await Task.Delay(delay, cancellationToken);

                    remainingDelay -= delay;
                }

                if (awaiter.IsCompleted is false && buffer.Count > 0)
                {
                    yield return buffer.ToArray();
                    buffer.Clear();
                }

                var hasNext = awaiter.IsCompleted
                    ? awaiter.GetResult()
                    : await enumeratorTask;

                if (hasNext is false)
                {
                    if (buffer.Count > 0)
                        yield return buffer;

                    yield break;
                }

                buffer.Add(enumerator.Current);

                if (buffer.Count < count)
                    continue;

                yield return buffer.ToArray();
                buffer.Clear();
            }
        }
        finally
        {
            try
            {
                await cancellationSource.CancelAsync();
                await enumeratorTask;
            }
            catch
            {
                // We are not interested in any exceptions here
                // because the fetch operation was scheduled while
                // no one expects it.
            }

            cancellationSource.Dispose();
            await enumerator.DisposeAsync();
        }
    }
}
