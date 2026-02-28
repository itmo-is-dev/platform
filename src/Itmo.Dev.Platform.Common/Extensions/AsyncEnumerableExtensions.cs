using Itmo.Dev.Platform.Common.Extensions.Iterators;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

public static class AsyncEnumerableExtensions
{
    /// <param name="enumerable">Chunked async enumerable</param>
    /// <typeparam name="T">Type of chunked values</typeparam>
    extension<T>(IAsyncEnumerable<T> enumerable)
    {
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public async Task AsTask(CancellationToken cancellationToken)
        {
            await foreach (T value in enumerable.WithCancellation(cancellationToken))
            {
                _ = value;
            }
        }

        /// <summary>
        ///     Chunks an async enumerable
        /// </summary>
        /// <param name="count">Chunk max size (>1)</param>
        /// <param name="timeout">
        ///     Wait time for when enumerator is not completed.
        ///     If enumerator would not complete before that time span,
        ///     existing values for current chunk are yielded. 
        /// </param>
        /// <returns>Async enumerable of chunks</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <see cref="count"/> is less than or equal to 1,
        ///     or <see cref="timeout"/> is less than <see cref="timeoutChunkSpan"/>
        /// </exception>
        public IAsyncEnumerable<IReadOnlyList<T>> ChunkAsync(
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
        public IAsyncEnumerable<IReadOnlyList<T>> ChunkAsync(
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

            return new ChunkedAsyncEnumerable<T>(enumerable, count, timeout, timeoutChunkSpan);
        }

        public async IAsyncEnumerable<T2> SelectAwaitParallel<T2>(
            ParallelOptions options,
            Func<T, CancellationToken, ValueTask<T2>> selector)
        {
            var channel = Channel.CreateUnbounded<T2>();

            var parallelTask = Parallel.ForEachAsync(
                enumerable,
                options,
                async (element, cancellationToken) =>
                {
                    var selected = await selector.Invoke(element, cancellationToken);
                    await channel.Writer.WriteAsync(selected, cancellationToken);
                });

            var completionTask = parallelTask.ContinueWith(_ => channel.Writer.Complete(), options.CancellationToken);

            await foreach (var element in channel.Reader.ReadAllAsync(options.CancellationToken))
            {
                yield return element;
            }

            if (parallelTask.Exception is not null)
            {
                ExceptionDispatchInfo.Throw(parallelTask.Exception);
            }

            await parallelTask;
            await completionTask;
        }
    }
}
