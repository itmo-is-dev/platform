using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.MessagePersistence.Options;

public class MessagePersistenceHandlerOptions : IValidatableObject
{
    public MessageHandleResultKind DefaultHandleResult { get; set; }

    public int? RetryCount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RetryCount is < 0)
            yield return new ValidationResult("Retry count must not be negative", [nameof(RetryCount)]);
    }
}
