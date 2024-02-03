using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Samples.Producer;

public static class Extensions
{
    public static IServiceCollection AddProducer(this IServiceCollection collection, IConfiguration configuration)
    {
        return collection.AddPlatformKafka(builder => builder
            .ConfigureOptions(configuration.GetSection("Kafka"))
            .AddProducer(b => b
                .WithKey<MessageKey>()
                .WithValue<MessageValue>()
                .WithConfiguration(configuration.GetSection("Kafka:Producers:Message"))
                .SerializeKeyWithNewtonsoft()
                .SerializeValueWithNewtonsoft()));
    }

    public static IServiceCollection AddOutboxProducer(this IServiceCollection collection, IConfiguration configuration)
    {
        return collection.AddPlatformKafka(builder => builder
            .ConfigureOptions(configuration.GetSection("Kafka"))
            .AddProducer(b => b
                .WithKey<MessageKey>()
                .WithValue<MessageValue>()
                .WithConfiguration(configuration.GetSection("Kafka:Producers:Message"))
                .SerializeKeyWithNewtonsoft()
                .SerializeValueWithNewtonsoft()
                .WithOutbox()));
    }
}