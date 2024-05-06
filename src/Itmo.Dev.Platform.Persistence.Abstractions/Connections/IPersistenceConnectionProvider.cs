namespace Itmo.Dev.Platform.Persistence.Abstractions.Connections;

public interface IPersistenceConnectionProvider
{
    ValueTask<IPersistenceConnection> GetConnectionAsync(CancellationToken cancellationToken);
}