namespace Itmo.Dev.Platform.Common.Lifetime.Services;

internal class PlatformLifetime : IPlatformLifetime
{
    private readonly IEnumerable<IPlatformLifetimeInitializer> _initializers;
    private readonly IEnumerable<IPlatformLifetimePostInitializer> _postInitializers;

    private readonly object _lock = new object();
    private Task? _whenAllTask;

    public PlatformLifetime(
        IEnumerable<IPlatformLifetimeInitializer> initializers,
        IEnumerable<IPlatformLifetimePostInitializer> postInitializers)
    {
        _initializers = initializers;
        _postInitializers = postInitializers;
    }

    public Task WaitOnInitializedAsync(CancellationToken cancellationToken)
    {
        if (_whenAllTask is not null)
            return _whenAllTask;

        lock (_lock)
        {
            if (_whenAllTask is not null)
                return _whenAllTask;

            var initializerTasks = _initializers
                .Select(initializer => initializer.WaitForCompletionAsync(cancellationToken));

            var postInitializerTasks = _postInitializers
                .Select(initializer => initializer.WaitForCompletionAsync(cancellationToken));

            return _whenAllTask = Task.WhenAll(initializerTasks.Concat(postInitializerTasks));
        }
    }
}