using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Persistence.Postgres.Models;

public class PostgresConnectionOptions : IValidatableObject
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Database { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SslMode { get; set; } = string.Empty;

    public bool Pooling { get; set; } = true;

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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Host))
            yield return new ValidationResult("No Postgres host specified");

        if (Port is 0)
            yield return new ValidationResult("No Postgres port specified");

        if (string.IsNullOrEmpty(Database))
            yield return new ValidationResult("No Postgres database specified");

        if (string.IsNullOrEmpty(Username))
            yield return new ValidationResult("No Postgres username specified");

        if (string.IsNullOrEmpty(Password))
            yield return new ValidationResult("No Postgres password specified");

        if (MaximumPoolSize < 1)
        {
            yield return new ValidationResult(
                $"Invalid {nameof(PostgresConnectionOptions)}.{nameof(MaximumPoolSize)} = {MaximumPoolSize}");
        }
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