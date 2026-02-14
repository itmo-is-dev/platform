using Itmo.Dev.Platform.MessagePersistence.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Execution.FailureProcessors;

internal class MessageHandleFailureProcessorContext : IDisposable
{
    private readonly List<Exception> _exceptions = [];

    public MessageHandleFailureProcessorContext(MessagePersistenceHandlerOptions handlerOptions)
    {
        HandlerOptions = handlerOptions;
    }

    public MessagePersistenceHandlerOptions HandlerOptions { get; }

    public void AddException(Exception exception)
    {
        _exceptions.Add(exception);
    }

#pragma warning disable CA1065
    public void Dispose()
    {
        if (_exceptions is not [])
            throw new AggregateException(_exceptions);
    }
#pragma warning restore CA1065
}
