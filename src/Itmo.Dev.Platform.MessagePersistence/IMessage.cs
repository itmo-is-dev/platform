using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.MessagePersistence;

public interface IMessage<out TKey, out TValue>
{
    DateTimeOffset CreatedAt { get; }

    TKey Key { get; }

    TValue Value { get; }

    void SetResult(MessageHandleResult result);
}