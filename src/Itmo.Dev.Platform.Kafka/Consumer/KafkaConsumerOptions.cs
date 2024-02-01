using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Kafka.Consumer;

public class KafkaConsumerOptions : IValidatableObject
{
    public bool IsDisabled { get; set; }

    public TimeSpan DisabledConsumerTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public string Topic { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string InstanceId { get; set; } = string.Empty;

    public int ParallelismDegree { get; set; } = 1;

    public int BufferSize { get; set; } = 1;

    public TimeSpan BufferWaitLimit { get; set; }

    public bool ReadLatest { get; set; }

    public KafkaConsumerOptions WithGroup(string group)
    {
        Group = group;
        return this;
    }

    public KafkaConsumerOptions WithInstanceId(string instanceId)
    {
        InstanceId = instanceId;
        return this;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Topic))
            yield return new ValidationResult("Topic must be specified for consumer configuration");

        if (string.IsNullOrEmpty(Group))
            yield return new ValidationResult($"Group must be specified for topic = {Topic} consumer");

        if (ParallelismDegree < 1)
        {
            string message = $"Invalid parallelism degree = {ParallelismDegree} (must be >= 1) for topic = {Topic} consumer";
            yield return new ValidationResult(message);
        }

        if (BufferSize < 1)
        {
            string message = $"Invalid buffer size = {BufferSize} (must be >= 1) for topic = {Topic} consumer";
            yield return new ValidationResult(message);
        }
    }
}