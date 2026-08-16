using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;

[OptionsType]
public class MessagePersistencePostgresOptions
{
    [Required]
    [MinLength(1)]
    public string SchemaName { get; set; } = string.Empty;

    public void ApplyTo(MessagePersistencePostgresOptions options)
    {
        options.SchemaName = SchemaName;
    }
}