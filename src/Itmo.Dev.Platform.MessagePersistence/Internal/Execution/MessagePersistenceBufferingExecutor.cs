using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Internal.Buffering;
using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.MessagePersistence.Internal.Persistence;
using Itmo.Dev.Platform.MessagePersistence.Internal.Services;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Execution;

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
        IEnumerable<PersistedMessageModel> messages,
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

        using var activity = MessagePersistenceActivitySource.Value
            .StartActivity(
                name: MessagePersistenceConstants.Tracing.SpanName,
                ActivityKind.Internal,
                parentContext: default,
                tags: new Dictionary<string, object?>
                {
                    [MessagePersistenceConstants.Tracing.MessageNameTag] = messageName,
                    [MessagePersistenceConstants.Tracing.MessageBufferingStepTag] = bufferingStepName,
                })
            .WithDisplayName($"[buffer] {messageName}");

        foreach (PersistedMessageModel message in messagesArray)
        {
            message.State = MessageState.Published;
            message.BufferingStep = bufferingStepName;
        }

        await stepPublisher.PublishAsync(messagesArray, cancellationToken);
        await _repository.UpdateAsync(messagesArray, cancellationToken);
    }
}
