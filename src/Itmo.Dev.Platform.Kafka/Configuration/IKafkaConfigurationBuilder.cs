using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Configuration;

public interface IKafkaConfigurationOptionsSelector
{
    IKafkaConfigurationBuilder ConfigureOptions(Action<OptionsBuilder<PlatformKafkaOptions>> action);

    IKafkaConfigurationBuilder ConfigureOptions(
        IConfiguration configuration,
        Action<PlatformKafkaOptions>? action = null)
    {
        return ConfigureOptions(builder =>
        {
            builder.Bind(configuration);

            if (action is not null)
                builder.Configure(action);
        });
    }

    IKafkaConfigurationBuilder ConfigureOptions(Action<PlatformKafkaOptions> action)
    {
        return ConfigureOptions(builder => builder.Configure(action));
    }

    IKafkaConfigurationBuilder ConfigureOptions(string sectionPath)
    {
        return ConfigureOptions(builder => builder.BindConfiguration(sectionPath));
    }
}

public interface IKafkaConfigurationBuilder
{
    IServiceCollection Services { get; }
}
