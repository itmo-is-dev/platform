namespace Itmo.Dev.Platform.Enrichment;

public interface IEnrichedModel<out TKey>
    where TKey : notnull
{
    TKey Key { get; }
}