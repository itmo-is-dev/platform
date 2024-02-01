using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Kafka.Producer;

public class KafkaProducerOptions : IValidatableObject
{
    public string Topic { get; init; } = string.Empty;

    public int MessageMaxBytes { get; } = 1_000_000;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Topic))
            yield return new ValidationResult("Topic name must be specified");
    }
}