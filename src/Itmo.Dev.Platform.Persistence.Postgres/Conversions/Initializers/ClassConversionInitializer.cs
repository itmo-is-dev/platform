using Npgsql.Internal;
using Npgsql.Internal.Postgres;

namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions.Initializers;

internal sealed class ClassConversionInitializer<TSource, TPrimitive>(
    IPlatformPostgresConverter<TSource, TPrimitive> converter)
    : ConversionInitializerBase<TSource, TPrimitive>(converter)
    where TSource : class
{
    protected override void RegisterMapping(
        TypeInfoMappingCollection collection,
        DataTypeName dataTypeName,
        TypeInfoFactory factory)
    {
        collection.AddType<TSource>(dataTypeName, factory);
    }

    protected override void RegisterArrayMapping(TypeInfoMappingCollection collection, DataTypeName dataTypeName)
    {
        collection.AddArrayType<TSource>(dataTypeName);
    }
}
