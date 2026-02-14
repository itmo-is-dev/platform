using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Options;

internal class PersistedMessageOptions
{
    internal bool IsInitialized { get; set; }

    [NotNull]
    [Required]
    public Type? MessageType { get; set; }

    [NotNull]
    [Required]
    public Type? PayloadType { get; set; }

    public PayloadVersion Version { get; set; } = PayloadVersion.Default;

    [NotNull]
    [Required]
    public Func<IPersistedMessagePayload, IPersistedMessage>? Factory { get; set; }

    public string? BufferGroup { get; set; }
}
