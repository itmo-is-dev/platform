using Itmo.Dev.Platform.MessagePersistence;
using Itmo.Dev.Platform.Testing.Behavioural.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Testing.Behavioural.MessagePersistence.Steps;

public sealed class MessagePersisted<TMessage> : IThenStep<ITestContext>
    where TMessage : IPersistedMessage
{
    public required TMessage Message { get; init; }
    public required TimeSpan Timeout { get; init; }

    public async ValueTask ExecuteAsync(ITestContext context, CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = context.CreateScope();
        IMessagePersistenceService service = scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();

        var query = PersistedMessageQuery.Build(builder => builder
            .WithMinCreatedAt(context.ScenarioStartTimestamp));

        using var cts = new CancellationTokenSource(Timeout);

        while (cts.IsCancellationRequested is false)
        {
            bool messageExists = await service
                .QueryAsync<TMessage>(query, cancellationToken)
                .AnyAsync(message => message.Payload.Equals(Message.Payload), cancellationToken);

            if (messageExists is false)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
            else
            {
                return;
            }
        }

        throw new InvalidOperationException($"Failed to find message {typeof(TMessage).Name} = {Message.Payload}");
    }
}
