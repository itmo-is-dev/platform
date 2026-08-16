using Itmo.Dev.Platform.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Persistence.Postgres.Models;

[OptionsType]
public class PostgresConnectionOptions
{
    [Required]
    public string Host { get; set; } = string.Empty;

    [Required]
    [Range(minimum: 1, maximum: int.MaxValue)]
    public int Port { get; set; }

    [Required]
    public string Database { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string SslMode { get; set; } = string.Empty;

    [DefaultValue(true)]
    public bool Pooling { get; set; } = true;

    [DefaultValue(10)]
    [Range(minimum: 1, maximum: int.MaxValue)]
    public int MaximumPoolSize { get; set; } = 10;

    public bool EnableConnectionProviderLogging { get; set; }

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

    public void ApplyTo(PostgresConnectionOptions options)
    {
        options.Host = Host;
        options.Port = Port;
        options.Database = Database;
        options.Username = Username;
        options.Password = Password;
        options.SslMode = SslMode;
        options.Pooling = Pooling;
        options.MaximumPoolSize = MaximumPoolSize;
        options.EnableConnectionProviderLogging = EnableConnectionProviderLogging;
    }
}
