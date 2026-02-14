using Itmo.Dev.Platform.MessagePersistence.Internal.Models;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence.Models;

internal record BufferedMessageValue(PersistedMessageModel Message)
{
    public static BufferedMessageValue FromSerializedMessage(PersistedMessageModel message)
    {
        return new BufferedMessageValue(message);
    }
}
