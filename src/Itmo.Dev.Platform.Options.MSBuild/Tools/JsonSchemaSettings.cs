using Itmo.Dev.Platform.Options.MSBuild.JsonSchemaProcessors;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;

namespace Itmo.Dev.Platform.Options.MSBuild.Tools;

public static class JsonSchemaSettings
{
    private const string TimeSpanPattern = """
    ^-?(\d+\.)?([01]?\d|2[0-3]):([0-5]?\d):([0-5]?\d)(\.\d{1,7})?$
    """;

    public static SystemTextJsonSchemaGeneratorSettings CreateDefault()
    {
        var settings = new SystemTextJsonSchemaGeneratorSettings();

        settings.TypeMappers.Add(new PrimitiveTypeMapper(
            typeof(TimeSpan),
            schema =>
            {
                schema.Type = JsonObjectType.String;
                schema.Format = null;
                schema.Pattern = TimeSpanPattern;
            }));

        settings.TypeMappers.Add(new PrimitiveTypeMapper(
            typeof(TimeSpan?),
            schema =>
            {
                schema.Type = JsonObjectType.String;
                schema.Format = null;
                schema.Pattern = TimeSpanPattern;
                schema.IsNullableRaw = true;
            }));

        settings.SchemaProcessors.Add(new ConfigurationSectionJsonSchemaProcessor());
        settings.SchemaProcessors.Add(new PublicPropertyJsonSchemaProcessor());

        return settings;
    }
}
