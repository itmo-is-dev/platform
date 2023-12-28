namespace Itmo.Dev.Platform.Events.Tests.Events;

public class TestEventHandler : IEventHandler<TestEvent>
{
    public bool IsHandled { get; private set; }
    
    public ValueTask HandleAsync(TestEvent evt, CancellationToken cancellationToken)
    {
        IsHandled = true;
        return ValueTask.CompletedTask;
    }
}