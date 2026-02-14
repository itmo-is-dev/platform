using Itmo.Dev.Platform.Common.Exceptions;

namespace Itmo.Dev.Platform.MessagePersistence;

public class MessagePersistenceException : PlatformException
{
    private MessagePersistenceException(string message, Exception? innerException = null)
        : base(message, innerException) { }

    internal static MessagePersistenceException RegistryRecordNotFound(Type messageType)
    {
        string message = $"Could not find registry record for '{messageType}' message";
        return new MessagePersistenceException(message);
    }

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

    internal static MessagePersistenceException CouldNotFindMigrator(string messageName, PayloadVersion version)
    {
        string message = $"Could not find correct migrator for message {messageName} with version {version}";
        return new MessagePersistenceException(message);
    }

    internal static MessagePersistenceException FailedToDeserializePayload(
        string messageName,
        PayloadVersion version,
        object? actualValue)
    {
        string message = $"""
        Failed to deserialize payload for message {messageName} with version {version}, actual value: {actualValue}
        """;

        return new MessagePersistenceException(message);
    }

    internal static MessagePersistenceException InvalidPreviousPayload(
        string messageName,
        PayloadVersion fromVersion,
        PayloadVersion toVersion,
        Type expectedType,
        Type actualType)
    {
        string message = $"""
        Failed to migrate message {messageName} from version {fromVersion} to {toVersion}. 
        Invalid previous value, expected type: {expectedType}, actual: {actualType}
        """;

        return new MessagePersistenceException(message);
    }

    internal static MessagePersistenceException InvalidPayloadType(Type expectedType, Type actualPayload)
    {
        string message = $"Invalid payload type, expected: {expectedType}, actual: {actualPayload}";
        return new MessagePersistenceException(message);
    }
}
