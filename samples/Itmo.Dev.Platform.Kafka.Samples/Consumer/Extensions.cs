using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Samples.Consumer;

public static class Extensions
{
    public static IServiceCollection AddConsumer(this IServiceCollection collection, IConfiguration configuration)
    {
        return collection.AddPlatformKafka(builder => builder
            .ConfigureOptions(configuration.GetSection("Kafka"))
            .AddConsumer(b => b
                .WithKey<MessageKey>()
                .WithValue<MessageValue>()
                .WithConfiguration(configuration.GetSection("Kafka:Consumers:Message"))
                .DeserializeKeyWithNewtonsoft()
                .DeserializeValueWithNewtonsoft()
                .HandleWith<ConsumerHandler>()));
    }
}