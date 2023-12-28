namespace Itmo.Dev.Platform.Events;

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    ValueTask HandleAsync(TEvent evt, CancellationToken cancellationToken);
}