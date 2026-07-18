using FluentAssertions;
using Itmo.Dev.Platform.Persistence.Abstractions.Commands;
using Itmo.Dev.Platform.Persistence.Abstractions.Connections;
using Itmo.Dev.Platform.Persistence.Postgres.Tests.Fixtures;
using Itmo.Dev.Platform.Persistence.Postgres.Tests.Models;
using Itmo.Dev.Platform.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Xunit;

namespace Itmo.Dev.Platform.Persistence.Postgres.Tests;

[Collection(nameof(PostgresCollectionFixture))]
public sealed class ConversionTests(PostgresDatabaseFixture fixture) : IAsyncDisposeLifetime
{
    public Task DisposeAsync() => fixture.ResetAsync();

    public interface IConversionData
    {
        string PgTypeName { get; }

        void AddSourceParameter(IPersistenceCommand command, string parameterName);
        void AddSourceArrayParameter(IPersistenceCommand command, string parameterName);

        void AddPrimitiveParameter(IPersistenceCommand command, string parameterName);
        void AddPrimitiveArrayParameter(IPersistenceCommand command, string parameterName);

        void VerifySourceParameter(DbDataReader reader, string parameterName);
        void VerifySourceArrayParameter(DbDataReader reader, string parameterName);

        void VerifyPrimitiveParameter(DbDataReader reader, string parameterName);
        void VerifyPrimitiveArrayParameter(DbDataReader reader, string parameterName);
    }

    private class GenericConversionData<TSource, TPrimitive>(
        string typeName,
        TSource sourceValue,
        TPrimitive primitiveValue,
        Func<Type, Type> makeCollectionType)
        : IConversionData
    {
        public string PgTypeName { get; } = typeName;

        public void AddSourceParameter(IPersistenceCommand command, string parameterName)
            => command.AddParameter(parameterName, sourceValue);

        public void AddSourceArrayParameter(IPersistenceCommand command, string parameterName)
            => command.AddParameter(parameterName, CreateCollection(sourceValue));

        public void AddPrimitiveParameter(IPersistenceCommand command, string parameterName)
            => command.AddParameter(parameterName, primitiveValue);

        public void AddPrimitiveArrayParameter(IPersistenceCommand command, string parameterName)
            => command.AddParameter(parameterName, CreateCollection(primitiveValue));

        public void VerifySourceParameter(DbDataReader reader, string parameterName)
        {
            var actualValue = reader.GetFieldValue<TSource>(parameterName);
            actualValue.Should().Be(sourceValue);
        }

        public void VerifySourceArrayParameter(DbDataReader reader, string parameterName)
        {
            var actualValue = ReadCollection<TSource>(reader, parameterName);
            actualValue.Should().ContainSingle().Which.Should().Be(sourceValue);
        }

        public void VerifyPrimitiveParameter(DbDataReader reader, string parameterName)
        {
            TPrimitive actualValue = reader.GetFieldValue<TPrimitive>(parameterName);
            actualValue.Should().Be(primitiveValue);
        }

        public void VerifyPrimitiveArrayParameter(DbDataReader reader, string parameterName)
        {
            var actualValue = ReadCollection<TPrimitive>(reader, parameterName);
            actualValue.Should().ContainSingle().Which.Should().BeEquivalentTo(primitiveValue);
        }

        public override string ToString()
        {
            var collectionTypeFormat = makeCollectionType(typeof(TSource)) is { IsArray: false } collectionType
                ? collectionType.GetGenericTypeDefinition().Name
                : "[]";

            return $"{typeof(TSource).Name} <-> {typeof(TPrimitive).Name}::{PgTypeName} > {collectionTypeFormat}";
        }

        private IEnumerable<T> CreateCollection<T>(T value)
        {
            var collectionType = makeCollectionType(typeof(T));

            if (collectionType.IsArray)
            {
                var array = (T[])Array.CreateInstanceFromArrayType(collectionType, length: 1);
                array[0] = value;

                return array;
            }

            return (IEnumerable<T>)Activator.CreateInstance(collectionType, args: [new List<T> { value }])!;
        }

        private IEnumerable<T> ReadCollection<T>(DbDataReader reader, string parameterName)
        {
            var collectionType = makeCollectionType(typeof(T));

            // ReSharper disable once RedundantNameQualifier
            var readMethod = typeof(System.Data.DataReaderExtensions)
                .GetMethod("GetFieldValue", BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(collectionType);

            return (IEnumerable<T>)readMethod.Invoke(null, parameters: [reader, parameterName])!;
        }
    }

    public static TheoryData<IConversionData> ConversionData => new()
    {
        new GenericConversionData<LongId, long>(
            "bigint",
            new LongId(10),
            10,
            type => type.MakeArrayType()),

        new GenericConversionData<LongId, long>(
            "bigint",
            new LongId(10),
            10,
            type => typeof(List<>).MakeGenericType(type)),

        new GenericConversionData<GuidId, Guid>(
            "uuid",
            new GuidId(Guid.Parse("cf9cf273-6cb0-47c5-92a7-4abe29035a75")),
            Guid.Parse("cf9cf273-6cb0-47c5-92a7-4abe29035a75"),
            type => type.MakeArrayType()),

        new GenericConversionData<GuidId, Guid>(
            "uuid",
            new GuidId(Guid.Parse("cf9cf273-6cb0-47c5-92a7-4abe29035a75")),
            Guid.Parse("cf9cf273-6cb0-47c5-92a7-4abe29035a75"),
            type => typeof(List<>).MakeGenericType(type)),

        new GenericConversionData<StringId, string>(
            "text",
            new StringId("aboba"),
            "aboba",
            type => type.MakeArrayType()),

        new GenericConversionData<StringId, string>(
            "text",
            new StringId("aboba"),
            "aboba",
            type => typeof(List<>).MakeGenericType(type)),
    };

    [Theory]
    [MemberData(nameof(ConversionData))]
    public async Task GetFieldValue_ShouldReturnWrappedValue_WhenConverterConfigured(IConversionData data)
    {
        // Arrange
        await using var scope = fixture.Scope;

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(default);

        // Act
        await using var command = connection.CreateCommand("SELECT :value as value");
        data.AddPrimitiveParameter(command, "value");

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        // Assert
        data.VerifySourceParameter(reader, "value");
    }

    [Theory]
    [MemberData(nameof(ConversionData))]
    public async Task AddParameter_ShouldAddCorrectUnderlyingValue_WhenConversionConfigured(IConversionData data)
    {
        // Arrange
        await using var scope = fixture.Scope;

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(default);

        // Act
        await using var command = connection.CreateCommand("SELECT :value as value");
        data.AddSourceParameter(command, "value");

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        // Assert
        data.VerifyPrimitiveParameter(reader, "value");
    }

    [Theory]
    [MemberData(nameof(ConversionData))]
    public async Task AddParameterGetFieldValue_ShouldCorrectlyProcessValue_WhenConversionConfigured(
        IConversionData data)
    {
        // Arrange
        await using var scope = fixture.Scope;

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(default);

        // Act
        await using var command = connection.CreateCommand("SELECT :value as value");
        data.AddSourceParameter(command, "value");

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        // Assert
        data.VerifySourceParameter(reader, "value");
    }

    [Theory]
    [MemberData(nameof(ConversionData))]
    public async Task CollectionGetFieldValue_ShouldReturnWrappedValue_WhenConverterConfigured(IConversionData data)
    {
        // Arrange
        await using var scope = fixture.Scope;

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(default);

        // Act
        await using var command = connection.CreateCommand($"SELECT array[:value] as value");
        data.AddPrimitiveParameter(command, "value");

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        // Assert
        data.VerifySourceArrayParameter(reader, "value");
    }

    [Theory]
    [MemberData(nameof(ConversionData))]
    public async Task CollectionAddParameter_ShouldAddCorrectUnderlyingValue_WhenConversionConfigured(
        IConversionData data)
    {
        // Arrange
        await using var scope = fixture.Scope;

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(default);

        // Act
        await using var command = connection.CreateCommand("SELECT value FROM unnest(:values) as source(value)");
        data.AddSourceArrayParameter(command, "values");

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        // Assert
        data.VerifyPrimitiveParameter(reader, "value");
    }

    [Theory]
    [MemberData(nameof(ConversionData))]
    public async Task CollectionAddParameterGetFieldValue_ShouldCorrectlyProcessValue_WhenConversionConfigured(
        IConversionData data)
    {
        // Arrange
        await using var scope = fixture.Scope;

        var connectionProvider = scope.ServiceProvider.GetRequiredService<IPersistenceConnectionProvider>();
        await using var connection = await connectionProvider.GetConnectionAsync(default);

        // Act
        await using var command = connection.CreateCommand("SELECT values FROM (VALUES (:values)) as source(values)");
        data.AddSourceArrayParameter(command, "values");

        await using var reader = await command.ExecuteReaderAsync(default);
        await reader.ReadAsync();

        // Assert
        data.VerifySourceArrayParameter(reader, "values");
    }
}
