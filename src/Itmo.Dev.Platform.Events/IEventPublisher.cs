namespace Itmo.Dev.Platform.Events;

public interface IEventPublisher
{
    ValueTask PublishAsync<T>(T evt) where T : IEvent;

    ValueTask PublishAsync<T>(IEnumerable<T> events) where T : IEvent;

    ValueTask PublishAsync<T>(T evt, CancellationToken cancellationToken) where T : IEvent;

    ValueTask PublishAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken) where T : IEvent;
}