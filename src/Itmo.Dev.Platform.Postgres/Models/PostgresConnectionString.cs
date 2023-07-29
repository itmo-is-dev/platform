using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Postgres.Models;

internal class PostgresConnectionString : IDisposable
{
    private readonly IDisposable? _subscription;

    public PostgresConnectionString(IOptionsMonitor<PostgresConnectionConfiguration> options)
    {
        Value = options.CurrentValue.ToConnectionString();
        _subscription = options.OnChange(o => Value = o.ToConnectionString());
    }

    public string Value { get; private set; }

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}