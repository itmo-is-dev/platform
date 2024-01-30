using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration;

public class MessagePersistencePersistenceOptions : IValidatableObject
{
    public string SchemaName { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(SchemaName))
            yield return new ValidationResult("Schema name must be specified in persistence options");
    }
}