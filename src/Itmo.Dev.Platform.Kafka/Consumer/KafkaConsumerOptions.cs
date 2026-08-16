using Itmo.Dev.Platform.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Kafka.Consumer;

[OptionsType]
public class KafkaConsumerOptions : IValidatableObject
{
    public bool IsDisabled { get; set; }

    [Required]
    public required string Topic { get; set; }

    [Required]
    public required string Group { get; set; }

    public string InstanceId { get; set; } = string.Empty;

    [DefaultValue(1)]
    [Range(minimum: 1, maximum: int.MaxValue)]
    public int ParallelismDegree { get; set; } = 1;

    [DefaultValue(1)]
    [Range(minimum: 1, maximum: int.MaxValue)]
    public int BufferSize { get; set; } = 1;

    [DefaultValue("00:00:00")]
    public TimeSpan BufferWaitLimit { get; set; } = TimeSpan.Zero;

    public bool ReadLatest { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (BufferSize > 1 && BufferWaitLimit <= TimeSpan.Zero)
        {
            yield return new ValidationResult(
                $"Invalid buffer wait limit = {BufferWaitLimit} (must be > TimeSpan.Zero) for topic = {Topic} consumer");
        }
    }
}
