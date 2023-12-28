namespace Itmo.Dev.Platform.Events;

public interface IEventsConfigurationBuilder
{
    IEventsConfigurationBuilder AddHandlersFromAssemblyContaining<T>();
}