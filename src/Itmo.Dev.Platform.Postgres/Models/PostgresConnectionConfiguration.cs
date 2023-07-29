namespace Itmo.Dev.Platform.Postgres.Models;

public class PostgresConnectionConfiguration
{
    public string Host { get; init; } = string.Empty;

    public int Port { get; init; }

    public string Database { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string SslMode { get; init; } = string.Empty;

    public bool Pooling { get; init; } = true;

    public string ToConnectionString()
    {
        return $"Host={Host};" +
               $"Port={Port};" +
               $"Database={Database};" +
               $"Username={Username};" +
               $"Password={Password};" +
               $"Ssl Mode={SslMode};" +
               $"Pooling={Pooling}";
    }
}