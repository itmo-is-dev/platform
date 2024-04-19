using Microsoft.Extensions.Hosting;

namespace Itmo.Dev.Platform.Common.Lifetime.Services;

internal class PlatformLifetimeInitializerService : IHostedService
{
    private readonly IEnumerable<IPlatformLifetimeInitializer> _initializers;
    private readonly IEnumerable<IPlatformLifetimePostInitializer> _postInitializers;

    public PlatformLifetimeInitializerService(
        IEnumerable<IPlatformLifetimeInitializer> initializers,
        IEnumerable<IPlatformLifetimePostInitializer> postInitializers)
    {
        _initializers = initializers;
        _postInitializers = postInitializers;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(_initializers
            .Select(initializer => initializer.InitializeAsync(cancellationToken)));

        await Task.WhenAll(_postInitializers
            .Select(initializer => initializer.OnAfterInitializedAsync(cancellationToken)));
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}