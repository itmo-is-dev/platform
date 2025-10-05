using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Platform.MessagePersistence.Options;

internal class PersistedMessageOptions
{
    internal bool IsInitialized { get; set; }

    [NotNull]
    [Required]
    public Type? KeyType { get; set; }

    [NotNull]
    [Required]
    public Type? ValueType { get; set; }

    public string? BufferGroup { get; set; }
}
