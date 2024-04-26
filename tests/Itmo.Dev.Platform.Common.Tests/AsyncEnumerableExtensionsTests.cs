using FluentAssertions;
using Itmo.Dev.Platform.Common.Extensions;
using Xunit;

namespace Itmo.Dev.Platform.Common.Tests;

public class AsyncEnumerableExtensionsTests
{
    [Fact]
    public async Task SelectAwaitParallel_ShouldReturnValues_WhenValuesSupplied()
    {
        // Arrange
        const int count = 10;

        var enumerable = Enumerable.Range(0, count).ToAsyncEnumerable();

        // Act
        var result = enumerable.SelectAwaitParallel(
            new ParallelOptions { MaxDegreeOfParallelism = count / 2 },
            async (i, cancellationToken) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(count - i), cancellationToken);
                return i;
            });

        var actualCount = await result.CountAsync();

        // Assert
        actualCount.Should().Be(count);
    }

    [Fact]
    public async Task SelectAwaitParallel_ShouldHaveSingleValue_WhenSourceEnumerableHasSingleValue()
    {
        // Arrange
        var enumerable = new[] { 123 }.ToAsyncEnumerable();

        // Act
        var result = await enumerable
            .SelectAwaitParallel(
                new ParallelOptions { MaxDegreeOfParallelism = 5 },
                async (i, token) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                    return i;
                })
            .SingleAsync();

        // Assert
        result.Should().NotBe(0);
    }
}