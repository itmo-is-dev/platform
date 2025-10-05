using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence.Models;

internal record BufferedMessageValue(SerializedMessage Message)
{
    public static BufferedMessageValue FromSerializedMessage(SerializedMessage message)
    {
        return new BufferedMessageValue(message);
    }
}
