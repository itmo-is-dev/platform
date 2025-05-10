namespace Itmo.Dev.Platform.Locking.Redis;

public interface IKeyFormattingStrategy
{
    string Format(object key);
}
