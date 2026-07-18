namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions;

public interface IPlatformPostgresConverter<TSource, TPrimitive>
{
    TSource Wrap(TPrimitive value);

    TPrimitive Unwrap(TSource value);
}
