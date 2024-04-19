namespace Itmo.Dev.Platform.Common.Lifetime;

public interface IPlatformLifetimeInitializer
{
    Task WaitForCompletionAsync(CancellationToken cancellationToken);

    Task InitializeAsync(CancellationToken cancellationToken);
}