namespace Itmo.Dev.Platform.Locking;

public interface ILockingKeyFormatterProvider
{
    ILockingKeyFormatter GetFormatter(object key);
}
