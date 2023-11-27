using Itmo.Dev.Platform.Common.Tools;
using Itmo.Dev.Platform.Postgres.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Postgres.Connection;

internal class DataSourceConnectionFactory : IPostgresConnectionFactory
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly PostgresConnectionConfiguration _configuration;
    private readonly ILogger<DataSourceConnectionFactory> _logger;

    public DataSourceConnectionFactory(
        NpgsqlDataSource dataSource,
        IOptions<PostgresConnectionConfiguration> configuration,
        ILogger<DataSourceConnectionFactory> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public NpgsqlConnection CreateConnection()
    {
        if (_configuration.EnableConnectionProviderLogging && _logger.IsEnabled(LogLevel.Trace))
        {
            var stackTrace = new StackTrace();
            var formattedStackTrace = new FormattedStackTrace(stackTrace, 1);

            _logger.LogTrace(
                "Creating connection at\n{Trace}",
                formattedStackTrace);
        }
        
        return _dataSource.CreateConnection();
    }
}