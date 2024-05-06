using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;

public class MessagePersistencePostgresOptions : IValidatableObject
{
    public string SchemaName { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(SchemaName))
            yield return new ValidationResult("Schema name must be specified in persistence options");
    }

    public void ApplyTo(MessagePersistencePostgresOptions options)
    {
        options.SchemaName = SchemaName;
    }
}