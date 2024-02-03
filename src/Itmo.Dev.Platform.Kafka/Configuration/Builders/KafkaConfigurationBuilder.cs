using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    public IKafkaConfigurationBuilder ConfigureOptions(
        IConfiguration configuration,
        Action<PlatformKafkaOptions>? action = null)
    {
        var builder = Services.AddOptions<PlatformKafkaOptions>()
            .Bind(configuration)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if (action is not null)
            builder.Configure(action);

        return this;
    }

    public IKafkaConfigurationBuilder ConfigureOptions(Action<PlatformKafkaOptions> action)
    {
        Services.AddOptions<PlatformKafkaOptions>()
            .Configure(action)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return this;
    }
}