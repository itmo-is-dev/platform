namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class GeneratePrimitiveConverterAttribute<TValueObject, TPrimitive> : Attribute;
