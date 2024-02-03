using Confluent.Kafka;
using Itmo.Dev.Platform.Kafka.Configuration;

namespace Itmo.Dev.Platform.Kafka.Tests.Extensions;

public static class KafkaConfigurationOptionsSelectorExtensions
{
    public static IKafkaConfigurationBuilder ConfigureTestOptions(
        this IKafkaConfigurationOptionsSelector selector,
        string host)
    {
        return selector.ConfigureOptions(options =>
        {
            options.Host = host;
            options.SecurityProtocol = SecurityProtocol.Plaintext;
        });
    }
}