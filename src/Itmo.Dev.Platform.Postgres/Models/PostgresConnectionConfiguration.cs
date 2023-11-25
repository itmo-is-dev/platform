namespace Itmo.Dev.Platform.Postgres.Models;

public class PostgresConnectionConfiguration
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Database { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SslMode { get; set; } = string.Empty;

    public bool Pooling { get; set; } = true;

    public int MaximumPoolSize { get; set; } = 10;

    public string ToConnectionString()
    {
        return $"Host={Host};" +
               $"Port={Port};" +
               $"Database={Database};" +
               $"Username={Username};" +
               $"Password={Password};" +
               $"Ssl Mode={SslMode};" +
               $"Pooling={Pooling};" +
               $"Maximum Pool Size={MaximumPoolSize}";
    }
}