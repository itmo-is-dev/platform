using Itmo.Dev.Platform.Postgres.Connection;
using Npgsql;
using System.Data;

namespace Itmo.Dev.Platform.Postgres.Transactions;

public class PostgresTransactionProvider : IPostgresTransactionProvider
{
    private readonly IPostgresConnectionProvider _connectionProvider;

    public PostgresTransactionProvider(IPostgresConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<NpgsqlTransaction> CreateTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        return await connection.BeginTransactionAsync(isolationLevel, cancellationToken);
    }
}