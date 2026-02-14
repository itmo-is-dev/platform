using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence.Models;

internal record BufferedMessageKey(string Key)
{
    public static BufferedMessageKey FromSerializedMessage(PersistedMessageModel message)
    {
        return new BufferedMessageKey(message.Key);
    }
}
