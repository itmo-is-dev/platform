using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.BackgroundTasks.Postgres.Configuration;

[OptionsType]
public class BackgroundTaskPersistenceOptions
{
    [Required]
    [MinLength(1)]
    public string SchemaName { get; set; } = string.Empty;

    public void ApplyTo(BackgroundTaskPersistenceOptions options)
    {
        options.SchemaName = SchemaName;
    }
}