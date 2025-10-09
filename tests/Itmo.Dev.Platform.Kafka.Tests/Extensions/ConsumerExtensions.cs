using Confluent.Kafka;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Kafka.Tests.Extensions;

public static class ConsumerExtensions
{
    [StackTraceHidden]
    public static ConsumeResult<TKey, TValue> ConsumeWithCancellationMessage<TKey, TValue>(
        this IConsumer<TKey, TValue> consumer,
        string message,
        CancellationToken cancellationToken)
    {
        try
        {
            return consumer.Consume(cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            throw new OperationCanceledException(message, e);
        }
    }
}
