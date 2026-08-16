using Confluent.Kafka;
using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Kafka.Configuration;

[OptionsType]
public class PlatformKafkaOptions : IValidatableObject
{
    [Required]
    public required string Host { get; set; }

    public SecurityProtocol SecurityProtocol { get; set; }

    public string? SslCaPem { get; set; }

    public SaslMechanism? SaslMechanism { get; set; }

    public string? SaslUsername { get; set; }

    public string? SaslPassword { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SecurityProtocol is SecurityProtocol.Ssl && string.IsNullOrEmpty(SslCaPem))
        {
            yield return new ValidationResult(
                "SslCaPem must be specified for Ssl protocol",
                [nameof(SecurityProtocol), nameof(SslCaPem)]);
        }

        if (SecurityProtocol is SecurityProtocol.SaslPlaintext or SecurityProtocol.SaslSsl)
        {
            if (string.IsNullOrEmpty(SaslUsername))
            {
                yield return new ValidationResult(
                    "SaslUsername must be specified for SaslPlaintext protocol",
                    [nameof(SaslUsername)]);
            }

            if (string.IsNullOrEmpty(SaslPassword))
            {
                yield return new ValidationResult(
                    "SaslPassword must be specified for SaslPlaintext protocol",
                    [nameof(SaslPassword)]);
            }
        }

        if (SecurityProtocol is SecurityProtocol.Ssl or SecurityProtocol.SaslSsl)
        {
            if (string.IsNullOrEmpty(SslCaPem))
            {
                yield return new ValidationResult(
                    "SslCaPem must be specified for SaslSsl protocol",
                    [nameof(SslCaPem)]);
            }
        }
    }
}
