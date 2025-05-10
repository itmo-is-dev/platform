namespace Itmo.Dev.Platform.Locking;

public interface ILockingKeyFormatter
{
    string Format(object key);
}
