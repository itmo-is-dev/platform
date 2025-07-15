// ReSharper disable once CheckNamespace

namespace Xunit;

public static class TheoryDataExtensions
{
    public static void Add<T1, T2>(this TheoryData<T1, T2> data, (T1, T2) tuple)
    {
        data.Add(tuple.Item1, tuple.Item2);
    }
}
