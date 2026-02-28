using FluentAssertions;
using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.Testing.Behavioural.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Testing.Behavioural.MessagePersistence.Steps;

public sealed class MessageNotPersisted<TMessage> : IThenStep<ITestContext>
    where TMessage : IPersistedMessage
{
    public required TMessage Message { get; init; }

    public async ValueTask ExecuteAsync(ITestContext context, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = context.CreateScope();
        IMessagePersistenceService service = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();

        var query = PersistedMessageQuery.Build(builder => builder
            .WithMinCreatedAt(context.ScenarioStartTimestamp));

        TMessage? message = await service
            .QueryAsync<TMessage>(query, cancellationToken)
            .Where(message => message.Payload == Message.Payload)
            .FirstOrDefaultAsync(cancellationToken);

        message.Should().BeNull();
    }
}
