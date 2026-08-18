using NJsonSchema.Generation;

namespace Itmo.Dev.Platform.Options.MSBuild.Extensions;

public static class JsonSchemaGeneratorSettingsExtensions
{
    extension(JsonSchemaGeneratorSettings settings)
    {
        public JsonSchemaGeneratorSettings AddProcessor(ISchemaProcessor processor)
        {
            settings.SchemaProcessors.Add(processor);
            return settings;
        }
    }
}
