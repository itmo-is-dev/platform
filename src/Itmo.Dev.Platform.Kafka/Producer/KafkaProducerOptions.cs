using Itmo.Dev.Platform.Options;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Kafka.Producer;

[OptionsType]
public class KafkaProducerOptions
{
    [Required]
    public required string Topic { get; init; }

    [DefaultValue(1_000_000)]
    [Range(minimum: 1, maximum: int.MaxValue)]
    public int MessageMaxBytes { get; } = 1_000_000;
}
