using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Events;

internal class EventPublisher : IEventPublisher
{
    private readonly IServiceProvider _provider;

    public EventPublisher(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ValueTask PublishAsync<T>(T evt) where T : IEvent
    {
        return PublishAsync(evt, default);
    }

    public ValueTask PublishAsync<T>(IEnumerable<T> events) where T : IEvent
    {
        return PublishAsync(events, default);
    }

    public async ValueTask PublishAsync<T>(T evt, CancellationToken cancellationToken) where T : IEvent
    {
        var handlers = _provider.GetRequiredService<IEnumerable<IEventHandler<T>>>();

        foreach (IEventHandler<T> handler in handlers)
        {
            await handler.HandleAsync(evt, cancellationToken);
        }
    }

    public async ValueTask PublishAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken) where T : IEvent
    {
        var handlers = _provider.GetRequiredService<IEnumerable<IEventHandler<T>>>();

        foreach (T evt in events)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            foreach (IEventHandler<T> handler in handlers)
            {
                await handler.HandleAsync(evt, cancellationToken);
            }
        }
    }
}