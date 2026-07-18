using Npgsql.Internal;
using Npgsql.Internal.Postgres;

namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions.Initializers;

internal sealed class StructConversionInitializer<TSource, TPrimitive>(
    IPlatformPostgresConverter<TSource, TPrimitive> converter)
    : ConversionInitializerBase<TSource, TPrimitive>(converter)
    where TSource : struct
{
    protected override void RegisterMapping(
        TypeInfoMappingCollection collection,
        DataTypeName dataTypeName,
        TypeInfoFactory factory)
    {
        collection.AddStructType<TSource>(dataTypeName, factory);
    }

    protected override void RegisterArrayMapping(
        TypeInfoMappingCollection collection,
        DataTypeName dataTypeName)
    {
        collection.AddStructArrayType<TSource>(dataTypeName);
    }
}
