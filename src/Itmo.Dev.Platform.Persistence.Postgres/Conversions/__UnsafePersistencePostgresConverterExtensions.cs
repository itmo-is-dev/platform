using Itmo.Dev.Platform.Persistence.Postgres.Configuration;

namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions;

// ReSharper disable once InconsistentNaming
#pragma warning disable CA1707
public static class __UnsafePersistencePostgresConverterExtensions
{
    public static void AddConverter<TValueObject, TPrimitive>(
        IPostgresPersistenceConfigurator configurator,
        Func<TPrimitive, TValueObject> wrap,
        Func<TValueObject, TPrimitive> unwrap)
        where TValueObject : class
    {
        configurator.WithConverter(new DelegatePlatformConverter<TValueObject, TPrimitive>(wrap, unwrap));
    }

    public static void AddStructConverter<TValueObject, TPrimitive>(
        IPostgresPersistenceConfigurator configurator,
        Func<TPrimitive, TValueObject> wrap,
        Func<TValueObject, TPrimitive> unwrap)
        where TValueObject : struct
    {
        configurator.WithStructConverter(new DelegatePlatformConverter<TValueObject, TPrimitive>(wrap, unwrap));
    }
}
