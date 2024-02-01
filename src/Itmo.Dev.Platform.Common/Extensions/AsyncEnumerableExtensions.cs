using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Common.Extensions;

public static class AsyncEnumerableExtensions
{
    /// <summary>
    ///     Chunks an async enumerable
    /// </summary>
    /// <param name="enumerable">Chunked async enumerable</param>
    /// <param name="count">Chunk max size (>1)</param>
    /// <param name="timeout">
    ///     Wait time for when enumerator is not completed.
    ///     If enumerator would not complete before that time span,
    ///     existing values for current chunk are yielded. 
    /// </param>
    /// <typeparam name="T">Type of chunked values</typeparam>
    /// <returns>Async enumerable of chunks</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <see cref="count"/> is less than or equal to 1,
    ///     or <see cref="timeout"/> is less than <see cref="timeoutChunkSpan"/>
    /// </exception>
    public static IAsyncEnumerable<IReadOnlyList<T>> ChunkAsync<T>(
        this IAsyncEnumerable<T> enumerable,
        int count,
        TimeSpan timeout)
    {
        if (count is 1)
            return enumerable.Select(x => new[] { x });

        return enumerable.ChunkAsync(count, timeout, timeoutChunkSpan: timeout);
    }

    /// <inheritdoc cref="ChunkAsync{T}(System.Collections.Generic.IAsyncEnumerable{T},int,System.TimeSpan)"/>
    /// <param name="timeoutChunkSpan">
    ///     Time span that <see cref="timeout"/> are split into.
    ///     When enumerator completes in any of  <see cref="timeoutChunkSpan"/> spans, its result 
    ///     continues to process.
    /// </param>
    public static IAsyncEnumerable<IReadOnlyList<T>> ChunkAsync<T>(
        this IAsyncEnumerable<T> enumerable,
        int count,
        TimeSpan timeout,
        TimeSpan timeoutChunkSpan)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count is 1)
            return enumerable.Select(x => new[] { x });

        if (timeout < timeoutChunkSpan)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeoutChunkSpan),
                "Timeout chunk span cannot be greater than timeout");
        }

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