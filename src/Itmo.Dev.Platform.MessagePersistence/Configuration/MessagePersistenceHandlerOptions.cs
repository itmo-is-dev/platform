using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration;

public class MessagePersistenceHandlerOptions : IValidatableObject
{
    public int BatchSize { get; set; }

    public TimeSpan PollingDelay { get; set; }

    public MessageHandleResult DefaultHandleResult { get; set; }

    public int? RetryCount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (BatchSize < 1)
            yield return new ValidationResult("Batch size must be greater or equal to 1");

        if (PollingDelay <= TimeSpan.Zero)
            yield return new ValidationResult("Polling delay must be specified in polling configuration");

        if (RetryCount is < 0)
            yield return new ValidationResult("Retry count must not be negative");
    }
}