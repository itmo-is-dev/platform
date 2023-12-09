namespace Itmo.Dev.Platform.Locking;

public interface ILockingService
{
    ValueTask<ILockHandle> AcquireAsync(object key, CancellationToken cancellationToken);
}