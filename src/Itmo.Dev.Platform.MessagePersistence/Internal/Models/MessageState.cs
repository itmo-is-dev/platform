namespace Itmo.Dev.Platform.MessagePersistence.Internal.Models;

internal enum MessageState
{
    Pending,
    Published,
    Completed,
    Failed,
}