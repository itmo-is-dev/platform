using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Kafka.Samples.ConsumerInbox;

public static class Extensions
{
    public static IServiceCollection AddInboxConsumer(this IServiceCollection collection, IConfiguration configuration)
    {
        return collection.AddPlatformKafka(builder => builder
            .ConfigureOptions(configuration.GetSection("Kafka"))
            .AddConsumer(b => b
                .WithKey<MessageKey>()
                .WithValue<MessageValue>()
                .WithConfiguration(configuration.GetSection("Kafka:Consumers:Message"))
                .DeserializeKeyWithNewtonsoft()
                .DeserializeValueWithNewtonsoft()
                .HandleInboxWith<InboxHandler>()));
    }
}