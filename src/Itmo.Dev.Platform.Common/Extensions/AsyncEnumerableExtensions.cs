using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Common.Extensions;

internal static class AsyncEnumerableExtensions
{
    public static IAsyncEnumerable<IReadOnlyList<T>> ChunkAsync<T>(
        this IAsyncEnumerable<T> enumerable,
        int count,
        TimeSpan timeout)
    {
        if (count <= 1)
            throw new ArgumentOutOfRangeException(nameof(count));

        return AsyncEnumerable.Create(ChunkInternal);

        async IAsyncEnumerator<IReadOnlyList<T>> ChunkInternal(CancellationToken cancellationToken)
        {
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default);

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

                    if (awaiter.IsCompleted is false && timeout > TimeSpan.Zero)
                    {
                        await Task.Delay(timeout, cancellationToken);
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
                    cancellationSource.Cancel();
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
}