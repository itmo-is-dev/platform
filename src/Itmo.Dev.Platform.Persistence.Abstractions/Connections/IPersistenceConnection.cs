using Itmo.Dev.Platform.Persistence.Abstractions.Commands;

namespace Itmo.Dev.Platform.Persistence.Abstractions.Connections;

public interface IPersistenceConnection : IAsyncDisposable
{
    IPersistenceCommand CreateCommand(string query);
}