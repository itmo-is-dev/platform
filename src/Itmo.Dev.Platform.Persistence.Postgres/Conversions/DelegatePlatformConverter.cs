namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions;

public sealed class DelegatePlatformConverter<TSource, TPrimitive>(
    Func<TPrimitive, TSource> wrap,
    Func<TSource, TPrimitive> unwrap)
    : IPlatformPostgresConverter<TSource, TPrimitive>
{
    public TSource Wrap(TPrimitive value) => wrap(value);

    public TPrimitive Unwrap(TSource value) => unwrap(value);
}
