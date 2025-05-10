using FluentAssertions;
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
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                    return i;
                })
            .SingleAsync();

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public async Task SelectAwaitParallel_ShouldReturnAllValues_WhenSomeTasksFinishEarlier()
    {
        // Arrange
        var enumerable = new[] { 1, 5, 2, 3, 5, 1, 2, 3, 4, 1 };

        // Act
        var result = await enumerable
            .ToAsyncEnumerable()
            .SelectAwaitParallel(
                new ParallelOptions { MaxDegreeOfParallelism = 5 },
                async (i, token) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(i), token);
                    return i;
                })
            .CountAsync();

        // Assert
        result.Should().Be(enumerable.Length);
    }

    [Fact]
    public async Task SelectAwaitParallel_ShouldRethrowException_WhenRaisedByHandler()
    {
        // Arrange
        const string message = "aboba";

        var enumerable = new[] { 1, 2 }.ToAsyncEnumerable();

        // Act
        var action = async () => await enumerable
            .SelectAwaitParallel<int, int>(
                new ParallelOptions { MaxDegreeOfParallelism = 5 },
                (_, _) => throw new Exception(message))
            .ToArrayAsync();

        // Assert
        await action.Should()
            .ThrowAsync<AggregateException>()
            .Where(e => e.InnerExceptions.All(ie => ie.Message.Equals(message, StringComparison.OrdinalIgnoreCase)));
    }
}
