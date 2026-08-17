using NJsonSchema.Generation;

namespace Itmo.Dev.Platform.Options.MSBuild.Tasks.CurrentSchemaGeneration;

public readonly record struct SchemaConfigurationContext(
    JsonSchemaResolver SchemaResolver,
    JsonSchemaGenerator SchemaGenerator);
