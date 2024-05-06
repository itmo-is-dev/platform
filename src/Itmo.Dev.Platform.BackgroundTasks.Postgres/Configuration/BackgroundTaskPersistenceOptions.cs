using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;

public class BackgroundTaskPersistenceOptions : IValidatableObject
{
    public string SchemaName { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(SchemaName))
        {
            yield return new ValidationResult(
                $"{nameof(BackgroundTaskPersistenceOptions)}.{nameof(SchemaName)} is undefined");
        }
    }

    public void ApplyTo(BackgroundTaskPersistenceOptions options)
    {
        options.SchemaName = SchemaName;
    }
}