using FluentAssertions;
using Itmo.Dev.Platform.Events.Tests.Events;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Itmo.Dev.Platform.Events.Tests;

public class EventPublisherTests
{
    [Fact]
    public async Task PublishAsync_ShouldInvokeEventHandler_WhenHandlerIsRegistered()
    {
        // Arrange
        var collection = new ServiceCollection();
        collection.AddPlatformEvents(builder => builder.AddHandlersFromAssemblyContaining<IAssemblyMarker>());

        var provider = collection.BuildServiceProvider();

        using var scope = provider.CreateScope();

        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var evt = new TestEvent();

        // Act
        await publisher.PublishAsync(evt);

        // Assert
        
        var handler = scope.ServiceProvider
            .GetRequiredService<IEnumerable<IEventHandler<TestEvent>>>()
            .Single();

        handler.Should().BeOfType<TestEventHandler>().Which.IsHandled.Should().BeTrue();
    }
}