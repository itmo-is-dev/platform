using Google.Protobuf.WellKnownTypes;

namespace Itmo.Dev.Platform.Testing.Tools.Comparers;

public sealed class TimeComparer : IEqualityComparer<DateTimeOffset>, IEqualityComparer<Timestamp>
{
    public static readonly TimeComparer Instance = new();

    public static readonly TimeSpan Delta = TimeSpan.FromSeconds(1);

    private TimeComparer() { }

    public bool Equals(DateTimeOffset x, DateTimeOffset y)
        => (x - y).Duration() < Delta;

    public int GetHashCode(DateTimeOffset obj)
        => obj.GetHashCode();

    public bool Equals(Timestamp? x, Timestamp? y) => (x, y) switch
    {
        (null, null) => true,
        (null, not null) => false,
        (not null, null) => false,
        _ => Equals(x.ToDateTimeOffset(), y.ToDateTimeOffset()),
    };

    public int GetHashCode(Timestamp obj)
        => obj.GetHashCode();
}
