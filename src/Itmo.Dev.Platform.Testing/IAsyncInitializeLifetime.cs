using Xunit;

namespace Itmo.Dev.Platform.Testing;

public interface IAsyncInitializeLifetime : IAsyncLifetime
{
    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
