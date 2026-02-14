using FluentAssertions;
using System.Collections;
using Xunit;

namespace Itmo.Dev.Platform.Common.Tests.EnumerableExtensions;

#pragma warning disable CA2244
public class WindowedGroupByTests
{
    public static TheoryData<Model<int, int>[], ModelGroupingResult<int, int>> WindowedGroupingScenarios => new()
    {
        (
            [
                (1, 1),
                (1, 2),
                (1, 3),
                (1, 4)
            ],
            new()
            {
                [1] = [1, 2, 3, 4],
            }
        ),
        (
            [
                (1, 1),
                (1, 2),
                (2, 3),
                (2, 4)
            ],
            new()
            {
                [1] = [1, 2],
                [2] = [3, 4],
            }
        ),
        (
            [
                (1, 1),
                (2, 2),
                (1, 3),
                (2, 4)
            ],
            new()
            {
                [1] = [1],
                [2] = [2],
                [1] = [3],
                [2] = [4],
            }
        ),
        (
            [
                (1, 1),
                (1, 2),
                (2, 3),
                (1, 4),
                (2, 5),
                (2, 6),
            ],
            new()
            {
                [1] = [1, 2],
                [2] = [3],
                [1] = [4],
                [2] = [5, 6],
            }
        ),
    };

    [Theory]
    [MemberData(nameof(WindowedGroupingScenarios))]
    public void WindowedGroupBy_ShouldReturnCorrectGroupings(
        Model<int, int>[] source,
        ModelGroupingResult<int, int> expectedGroupings)
    {
        // Arrange
        expectedGroupings = expectedGroupings
            .OrderBy(x => x.Key + x.Values.Sum())
            .ToArray();

        // Act
        var groupings = source
            .WindowedGroupBy(model => model.Key, model => model.Value)
            .OrderBy(x => x.Key + x.Sum())
            .ToArray();

        // Assert
        groupings.Length.Should().Be(expectedGroupings.Length);

        foreach (var (grouping, expectedGrouping) in groupings.Zip(expectedGroupings))
        {
            grouping.Key.Should().Be(expectedGrouping.Key);
            grouping.Should().BeEquivalentTo(expectedGrouping.Values);
        }
    }

    public readonly record struct Model<TKey, TValue>(TKey Key, TValue Value)
    {
        public static implicit operator Model<TKey, TValue>((TKey key, TValue value) tuple)
            => new(tuple.key, tuple.value);
    }

    public readonly record struct ModelGrouping<TKey, TValue>(TKey Key, IEnumerable<TValue> Values);

    public class ModelGroupingResult<TKey, TValue> : IEnumerable<ModelGrouping<TKey, TValue>>
    {
        private readonly List<ModelGrouping<TKey, TValue>> _groupings;

        public ModelGroupingResult(List<ModelGrouping<TKey, TValue>>? groupings = null)
        {
            _groupings = groupings ?? [];
        }

        public TValue[] this[TKey key] { set => _groupings.Add(new ModelGrouping<TKey, TValue>(key, value)); }

        public static implicit operator ModelGroupingResult<TKey, TValue>(ModelGrouping<TKey, TValue>[] groupings)
            => new(groupings.ToList());

        public int Length => _groupings.Count;

        public IEnumerator<ModelGrouping<TKey, TValue>> GetEnumerator()
            => _groupings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable)_groupings).GetEnumerator();
    }

    private static Model<TKey, TValue> CreateModel<TKey, TValue>(TKey key, TValue value) => new(key, value);

    private static ModelGrouping<TKey, TValue> CreateGrouping<TKey, TValue>(TKey key, TValue[] values)
        => new(key, values);
}
