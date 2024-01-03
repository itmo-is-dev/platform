using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Configuration;

internal class KafkaConfigurationBuilder :
    IKafkaConfigurationOptionsSelector,
    IKafkaConfigurationBuilder
{
    public KafkaConfigurationBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public IKafkaConfigurationBuilder ConfigureOptions(Action<OptionsBuilder<KafkaConfiguration>> configuration)
    {
        var builder = Services.AddOptions<KafkaConfiguration>().ValidateOnStart();
        configuration.Invoke(builder);

        return this;
    }
}