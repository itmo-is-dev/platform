using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Configuration.Builders;

internal class KafkaConfigurationBuilder :
    IKafkaConfigurationOptionsSelector,
    IKafkaConfigurationBuilder
{
    public KafkaConfigurationBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public IKafkaConfigurationBuilder ConfigureOptions(Action<OptionsBuilder<PlatformKafkaOptions>> configuration)
    {
        var builder = Services.AddOptions<PlatformKafkaOptions>().ValidateOnStart();
        configuration.Invoke(builder);

        return this;
    }
}