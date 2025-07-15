using Itmo.Dev.Platform.MessagePersistence.Models;

namespace Itmo.Dev.Platform.Kafka.MessagePersistence.Models;

internal record BufferedMessageKey(string Key)
{
    public static BufferedMessageKey FromSerializedMessage(SerializedMessage message)
    {
        return new BufferedMessageKey(message.Key);
    }
}
