using Itmo.Dev.Platform.MessagePersistence.Internal.Buffering;
using Itmo.Dev.Platform.MessagePersistence.Internal.Execution.FailureProcessors;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Options;

internal class BufferStepOptions : IValidatableObject
{
    [NotNull]
    [Required]
    public string? Name { get; set; }

    [NotNull]
    [Required]
    public Type? PublisherType { get; set; }
    
    public IMessageHandleFailureProcessor? FailureProcessor { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PublisherType.GetInterfaces().Contains(typeof(IBufferingStepPublisher)) is false)
        {
            yield return new ValidationResult(
                $"Type '{PublisherType}' is invalid publisher type",
                [nameof(PublisherType)]);
        }
    }
}
