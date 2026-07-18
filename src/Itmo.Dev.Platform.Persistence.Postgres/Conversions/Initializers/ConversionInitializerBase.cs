using Npgsql.Internal;
using Npgsql.Internal.Postgres;

namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions.Initializers;

internal abstract class ConversionInitializerBase<TSource, TPrimitive>(
    IPlatformPostgresConverter<TSource, TPrimitive> converter)
    : IPlatformConverterInitializer
{
    public Type SourceType { get; } = typeof(TSource);

    public void ConfigureMapping(
        TypeInfoMappingCollection collection,
        DataTypeName? dataTypeName,
        PgSerializerOptions options)
    {
        PgTypeId? typeId = dataTypeName;

        if (typeId is null)
        {
            typeId = options.GetDefaultTypeInfo(typeof(TPrimitive))?.PgTypeId;
        }

        if (typeId is null)
        {
            throw new InvalidOperationException($"Writing values of type {typeof(TPrimitive).Name} not supported");
        }

        dataTypeName = options.GetDataTypeName(typeId.Value);

        RegisterMapping(
            collection,
            dataTypeName.Value,
            (_, mapping, _) => mapping.CreateInfo(options, CreateConverter(dataTypeName.Value, options)));
    }

    public void ConfigureArrayMapping(
        TypeInfoMappingCollection collection,
        DataTypeName? arrayDataTypeName,
        PgSerializerOptions options)
    {
        PgTypeId? arrayTypeId = arrayDataTypeName;

        if (arrayTypeId is null)
        {
            arrayTypeId = options.GetDefaultTypeInfo(typeof(TPrimitive).MakeArrayType())?.PgTypeId;
        }

        if (arrayTypeId is null)
        {
            throw new InvalidOperationException($"Writing collections of type {typeof(TPrimitive).Name} not supported");
        }

        var elementDataTypeName = options.GetDataTypeName(options.GetArrayElementTypeId(arrayTypeId.Value));

        RegisterMapping(
            collection,
            elementDataTypeName,
            (_, mapping, _) => mapping.CreateInfo(options, CreateConverter(elementDataTypeName, options)));

        RegisterArrayMapping(
            collection,
            elementDataTypeName);
    }

    protected abstract void RegisterMapping(
        TypeInfoMappingCollection collection,
        DataTypeName dataTypeName,
        TypeInfoFactory factory);

    protected abstract void RegisterArrayMapping(
        TypeInfoMappingCollection collection,
        DataTypeName dataTypeName);

    private PgConverter CreateConverter(DataTypeName dataTypeName, PgSerializerOptions options)
    {
        var underlyingConverter = options
            .GetTypeInfo(typeof(TPrimitive), dataTypeName)
            ?.GetResolution(default(TPrimitive))
            .Converter;

        if (underlyingConverter is null)
        {
            throw new InvalidOperationException($"Writing values of type {typeof(TPrimitive).Name} is not supported");
        }

        return new PlatformPgConverterAdapter<TSource, TPrimitive>(
            converter,
            (PgConverter<TPrimitive>)underlyingConverter);
    }
}
