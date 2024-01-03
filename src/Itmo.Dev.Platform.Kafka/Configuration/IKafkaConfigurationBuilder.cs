using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Configuration;

public interface IKafkaConfigurationOptionsSelector
{
    IKafkaConfigurationBuilder ConfigureOptions(Action<OptionsBuilder<KafkaConfiguration>> configuration);
}

public interface IKafkaConfigurationBuilder
{
    IServiceCollection Services { get; }
}