using Itmo.Dev.Platform.MessagePersistence.Internal.Options;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Services;

internal class MessagePersistenceRegistry
{
    private readonly IOptionsMonitor<PersistedMessageOptions> _messageOptions;
    private readonly IOptionsMonitor<BufferGroupOptions> _bufferGroupOptions;
    private readonly IOptionsMonitor<MessagePersistenceOptions> _messagePersistenceOptions;

    public MessagePersistenceRegistry(
        IOptionsMonitor<PersistedMessageOptions> messageOptions,
        IOptionsMonitor<BufferGroupOptions> bufferGroupOptions,
        IOptionsMonitor<MessagePersistenceOptions> messagePersistenceOptions)
    {
        _messageOptions = messageOptions;
        _bufferGroupOptions = bufferGroupOptions;
        _messagePersistenceOptions = messagePersistenceOptions;
    }

    public string GetMessageName(Type messageType)
    {
        var options = _messagePersistenceOptions.CurrentValue;

        if (options.MessageNames.TryGetValue(messageType, out var messageName) is false)
            throw MessagePersistenceException.RegistryRecordNotFound(messageType);

        return messageName;
    }

    public PersistedMessageOptions GetRecord(string messageName)
    {
        var options = _messageOptions.Get(messageName);

        if (options.IsInitialized is false)
            throw MessagePersistenceException.RegistryRecordNotFound(messageName);

        return options;
    }

    public BufferGroupOptions GetBufferingGroup(string groupName)
    {
        var options = _bufferGroupOptions.Get(groupName);

        if (options.IsInitialized is false)
            throw MessagePersistenceException.BufferingGroupNotFound(groupName);

        return options;
    }
}
