using Confluent.Kafka;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Kafka.Configuration;

public class PlatformKafkaOptions : IValidatableObject
{
    public string Host { get; set; } = null!;

    public SecurityProtocol SecurityProtocol { get; set; }

    public string? SslCaPem { get; set; }

    public SaslMechanism? SaslMechanism { get; set; }

    public string? SaslUsername { get; set; }

    public string? SaslPassword { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Host))
            yield return new ValidationResult("Kafka host should be specified");

        if (SecurityProtocol is SecurityProtocol.Ssl)
        {
            if (string.IsNullOrEmpty(SslCaPem))
                yield return new ValidationResult("SslCaPem must be specified for Ssl protocol");
        }

        if (SecurityProtocol is SecurityProtocol.SaslPlaintext)
        {
            if (string.IsNullOrEmpty(SaslUsername))
                yield return new ValidationResult("SaslUsername must be specified for SaslPlaintext protocol");

            if (string.IsNullOrEmpty(SaslPassword))
                yield return new ValidationResult("SaslPassword must be specified for SaslPlaintext protocol");
        }

        if (SecurityProtocol is SecurityProtocol.SaslSsl)
        {
            if (string.IsNullOrEmpty(SslCaPem))
                yield return new ValidationResult("SslCaPem must be specified for SaslSsl protocol");

            if (string.IsNullOrEmpty(SaslUsername))
                yield return new ValidationResult("SaslUsername must be specified for SaslSsl protocol");

            if (string.IsNullOrEmpty(SaslPassword))
                yield return new ValidationResult("SaslPassword must be specified for SaslSsl protocol");
        }
    }
}