namespace Itmo.Dev.Platform.Locking.FormattingStrategies;

internal class HashCodeLockingKeyFormatter : ILockingKeyFormatter
{
    public string Format(object key)
    {
        return key.GetHashCode().ToString();
    }
}
