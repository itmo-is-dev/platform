using Itmo.Dev.Platform.Persistence.Postgres.Conversions.Initializers;
using Npgsql.Internal;
using Npgsql.Internal.Postgres;

namespace Itmo.Dev.Platform.Persistence.Postgres.Conversions;

internal sealed class PlatformPgResolverFactory(
    IEnumerable<IPlatformConverterInitializer> converterInitializers)
    : PgTypeInfoResolverFactory
{
    private readonly IReadOnlyDictionary<Type, IPlatformConverterInitializer> _converterInitializers =
        converterInitializers.ToDictionary(initializer => initializer.SourceType);

    public override IPgTypeInfoResolver CreateResolver() => new Resolver(_converterInitializers);

    public override IPgTypeInfoResolver CreateArrayResolver() => new ArrayResolver(_converterInitializers);

    private sealed class Resolver(
        IReadOnlyDictionary<Type, IPlatformConverterInitializer> converterInitializers)
        : IPgTypeInfoResolver
    {
        private readonly TypeInfoMappingCollection _mappingCollection = new();

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
        {
            if (type is null || converterInitializers.TryGetValue(GetRawType(type), out var initializer) is false)
                return null;

            if (_mappingCollection.Find(type, dataTypeName, options) is not { } typeInfo)
            {
                initializer.ConfigureMapping(_mappingCollection, dataTypeName, options);
                typeInfo = _mappingCollection.Find(type, dataTypeName, options);
            }

            return typeInfo;
        }
    }

    private sealed class ArrayResolver(
        IReadOnlyDictionary<Type, IPlatformConverterInitializer> converterInitializers)
        : IPgTypeInfoResolver
    {
        private readonly TypeInfoMappingCollection _mappingCollection = new();

        public PgTypeInfo? GetTypeInfo(Type? arrayType, DataTypeName? dataTypeName, PgSerializerOptions options)
        {
            if (arrayType is null)
                return null;

            var elementType = FindElementType(arrayType);

            if (elementType is null || converterInitializers.TryGetValue(elementType, out var initializer) is false)
                return null;

            if (_mappingCollection.Find(arrayType, dataTypeName, options) is not { } typeInfo)
            {
                initializer.ConfigureArrayMapping(_mappingCollection, dataTypeName, options);
                typeInfo = _mappingCollection.Find(arrayType, dataTypeName, options);
            }

            return typeInfo;
        }

        private static Type? FindElementType(Type type)
        {
            if (type.HasElementType is true)
                return type.GetElementType()!;

            var listInterface = type
                .GetInterfaces()
                .SingleOrDefault(i =>
                    i.IsConstructedGenericType
                    && i.GetGenericTypeDefinition().IsAssignableTo(typeof(IList<>)));

            var elementType = listInterface?.GenericTypeArguments.SingleOrDefault();

            return elementType is null ? elementType : GetRawType(elementType);
        }
    }

    private static Type GetRawType(Type type)
    {
        if (type.IsConstructedGenericType
            && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GenericTypeArguments.Single();
        }

        return type;
    }
}
