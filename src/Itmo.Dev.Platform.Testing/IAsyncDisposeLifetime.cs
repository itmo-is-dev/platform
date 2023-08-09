using Xunit;

namespace Itmo.Dev.Platform.Testing;

public interface IAsyncDisposeLifetime : IAsyncLifetime
{
    Task IAsyncLifetime.InitializeAsync()
    {
        return Task.CompletedTask;
    }
}