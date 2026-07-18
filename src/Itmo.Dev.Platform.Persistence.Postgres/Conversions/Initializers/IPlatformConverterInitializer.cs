using Npgsql.Internal;
using Npgsql.Internal.Postgres;

namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions.Initializers;

internal interface IPlatformConverterInitializer
{
    Type SourceType { get; }

    void ConfigureMapping(
        TypeInfoMappingCollection collection,
        DataTypeName? dataTypeName,
        PgSerializerOptions options);

    void ConfigureArrayMapping(
        TypeInfoMappingCollection collection,
        DataTypeName? arrayDataTypeName,
        PgSerializerOptions options);
}
