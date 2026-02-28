using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Platform.Testing.Tools.Comparers;

public sealed class DoubleComparer : IEqualityComparer<double?>
{
    public static readonly DoubleComparer Instance = new();

    public static readonly double Delta = 0.001;

    private DoubleComparer() { }

    public bool Equals(double? x, double? y) => (x, y) switch
    {
        (null, null) => true,
        (null, not null) => false,
        (not null, null) => false,
        _ => Math.Abs(x.Value - y.Value) < Delta,
    };

    public int GetHashCode([DisallowNull] double? obj) => obj.GetHashCode();
}
