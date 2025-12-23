using Itmo.Dev.Platform.Persistence.Abstractions.Exceptions;

namespace Itmo.Dev.Platform.Persistence.Postgres.Exceptions;

public sealed class PlatformPersistencePostgresException : PlatformPersistenceException
{
    private PlatformPersistencePostgresException(string message) : base(message) { }

    internal static PlatformPersistencePostgresException DuplicateParameter(string parameterName)
    {
        return new PlatformPersistencePostgresException($"""
        Postgres commands cannot contain duplicate parameters.
        Duplicate parameter name = {parameterName}.
        """);
    }
}
