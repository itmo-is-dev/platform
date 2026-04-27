using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Kafka.Configuration.Builders;

internal class KafkaConfigurationBuilder :
    IKafkaConfigurationOptionsSelector,
    IKafkaConfigurationBuilder
{
    private readonly IServiceCollection _services;

    public KafkaConfigurationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IKafkaConfigurationBuilder ConfigureOptions(Action<OptionsBuilder<PlatformKafkaOptions>> action)
    {
        var builder = _services
            .AddOptions<PlatformKafkaOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        action.Invoke(builder);

        return this;
    }

    public IKafkaConfigurationBuilder ConfigureServices(Action<IServiceCollection> action)
    {
        action(_services);
        return this;
    }
}
