using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Configuration;

public interface IKafkaConfigurationOptionsSelector
{
    IKafkaConfigurationBuilder ConfigureOptions(
        IConfiguration configuration,
        Action<PlatformKafkaOptions>? action = null);

    IKafkaConfigurationBuilder ConfigureOptions(Action<PlatformKafkaOptions> action);
}

public interface IKafkaConfigurationBuilder
{
    IServiceCollection Services { get; }
}