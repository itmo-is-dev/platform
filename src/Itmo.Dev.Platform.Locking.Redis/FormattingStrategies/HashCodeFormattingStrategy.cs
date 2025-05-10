namespace Itmo.Dev.Platform.Locking.Redis.FormattingStrategies;

internal class HashCodeFormattingStrategy : IKeyFormattingStrategy
{
    public string Format(object key)
    {
        return key.GetHashCode().ToString();
    }
}
