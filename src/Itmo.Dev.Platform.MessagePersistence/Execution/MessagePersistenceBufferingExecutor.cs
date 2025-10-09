using Itmo.Dev.Platform.MessagePersistence.Buffering;
using Itmo.Dev.Platform.MessagePersistence.Exceptions;
using Itmo.Dev.Platform.MessagePersistence.Models;
using Itmo.Dev.Platform.MessagePersistence.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence.Execution;

internal class MessagePersistenceBufferingExecutor : IMessagePersistenceBufferingExecutor
{
    private readonly MessagePersistenceRegistry _registry;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessagePersistenceInternalRepository _repository;

    public MessagePersistenceBufferingExecutor(
        MessagePersistenceRegistry registry,
        IServiceProvider serviceProvider,
        IMessagePersistenceInternalRepository repository)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
        _repository = repository;
    }

    public async Task ExecuteAsync(
        string messageName,
        string bufferingStepName,
        IEnumerable<SerializedMessage> messages,
        CancellationToken cancellationToken)
    {
        var record = _registry.GetRecord(messageName);

        if (record.BufferGroup is null)
            throw MessagePersistenceException.RegistryRecordMissingBufferingGroup(messageName);

        var bufferingGroup = _registry.GetBufferingGroup(record.BufferGroup);
        var step = bufferingGroup.FindStep(bufferingStepName);

        if (step is null)
            throw MessagePersistenceException.BufferingStepNotFound(record.BufferGroup, bufferingStepName);

        var stepPublisher = (IBufferingStepPublisher)_serviceProvider.GetRequiredKeyedService(
            step.PublisherType,
            bufferingGroup.Name);

        var messagesArray = messages.ToArray();

        foreach (SerializedMessage message in messagesArray)
        {
            message.State = MessageState.Published;
            message.BufferingStep = bufferingStepName;
        }

        await stepPublisher.PublishAsync(messagesArray, cancellationToken);
        await _repository.UpdateAsync(messagesArray, cancellationToken);
    }
}
