using FluentAssertions;
using System.Diagnostics;
using Xunit;

namespace Itmo.Dev.Platform.Common.Tests;

public class ChunkAsyncTests
{
    [Fact]
    public async Task ChunkAsync_ShouldReturnValuesWithCorrectTimeout_WhenBatchIsNotFull()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        var element = 1;
        var enumerable = GenerateInfiniteEnumerable(cts.Token, element);
        var chunkSize = 100;
        var timeout = TimeSpan.FromSeconds(2);

        // Act
        var start = Stopwatch.GetTimestamp();

        await foreach (var chunk in enumerable.ChunkAsync(chunkSize, timeout).WithCancellation(cts.Token))
        {
            var elapsed = Stopwatch.GetElapsedTime(start);

            // Assert
            chunk.Should().ContainSingle().Which.Should().Be(element);
            elapsed.Should().BeCloseTo(timeout, TimeSpan.FromMilliseconds(500));

            await cts.CancelAsync();

            return;
        }
    }

    private async IAsyncEnumerable<int> GenerateInfiniteEnumerable(
        CancellationToken cancellationToken,
        params int[] elements)
    {
        foreach (int element in elements)
        {
            yield return element;
        }

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
