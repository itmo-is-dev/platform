namespace Itmo.Dev.Platform.Common.Lifetime;

public interface IPlatformLifetimePostInitializer
{
    Task WaitForCompletionAsync(CancellationToken cancellationToken);

    Task OnAfterInitializedAsync(CancellationToken cancellationToken);
}