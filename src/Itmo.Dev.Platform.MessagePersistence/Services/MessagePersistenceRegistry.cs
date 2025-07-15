using Itmo.Dev.Platform.MessagePersistence.Exceptions;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Services;

internal class MessagePersistenceRegistry
{
    private readonly IOptionsMonitor<PersistedMessageOptions> _messageOptions;
    private readonly IOptionsMonitor<BufferGroupOptions> _bufferGroupOptions;

    public MessagePersistenceRegistry(
        IOptionsMonitor<PersistedMessageOptions> messageOptions,
        IOptionsMonitor<BufferGroupOptions> bufferGroupOptions)
    {
        _messageOptions = messageOptions;
        _bufferGroupOptions = bufferGroupOptions;
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
