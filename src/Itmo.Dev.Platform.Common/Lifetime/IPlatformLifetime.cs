namespace Itmo.Dev.Platform.Common.Lifetime;

public interface IPlatformLifetime
{
    Task WaitOnInitializedAsync(CancellationToken cancellationToken);
}