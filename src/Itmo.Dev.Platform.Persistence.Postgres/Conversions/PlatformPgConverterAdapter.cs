using Npgsql.Internal;

namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions;

internal sealed class PlatformPgConverterAdapter<TSource, TPrimitive>(
    IPlatformPostgresConverter<TSource, TPrimitive> converter,
    PgConverter<TPrimitive> primitiveConverter)
    : PgBufferedConverter<TSource>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
        => primitiveConverter.CanConvert(format, out bufferRequirements);

    public override Size GetSize(SizeContext context, TSource value, ref object? writeState)
        => primitiveConverter.GetSize(context, converter.Unwrap(value)!, ref writeState);

    protected override bool IsDbNullValue(TSource? value, ref object? writeState)
        => value is null;

    protected override TSource ReadCore(PgReader reader)
        => converter.Wrap(primitiveConverter.Read(reader));

    protected override void WriteCore(PgWriter writer, TSource value)
    {
        var primitive = converter.Unwrap(value);

        if (primitive is not null)
        {
            primitiveConverter.Write(writer, primitive);
        }
    }
}
