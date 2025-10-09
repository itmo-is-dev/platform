using Itmo.Dev.Platform.Common.Exceptions;

namespace Itmo.Dev.Platform.MessagePersistence.Exceptions;

public class MessagePersistenceException : PlatformException
{
    private MessagePersistenceException(string message, Exception? innerException = null)
        : base(message, innerException) { }

    internal static MessagePersistenceException RegistryRecordNotFound(string messageName)
    {
        string message = $"Could not find registry record for '{messageName}' message";
        return new MessagePersistenceException(message);
    }

    internal static MessagePersistenceException RegistryRecordMissingBufferingGroup(string messageName)
    {
        string message = $"Could not find buffering group for '{messageName}' message while trying to buffer";
        return new MessagePersistenceException(message);
    }

    internal static MessagePersistenceException BufferingGroupNotFound(string groupName)
    {
        string message = $"Could not find buffering group '{groupName}'";
        return new MessagePersistenceException(message);
    }

    internal static MessagePersistenceException BufferingStepNotFound(string groupName, string stepName)
    {
        string message = $"Could not find buffering step '{stepName}' in buffering group '{groupName}'";
        return new MessagePersistenceException(message);
    }

    internal static MessagePersistenceException MessageHandleFailed(string messageName, long messageId)
    {
        string message = $"Failed to handle message {messageName} with id = {messageId}";
        return new MessagePersistenceException(message);
    }
}
