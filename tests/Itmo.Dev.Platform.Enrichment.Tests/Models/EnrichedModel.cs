namespace Itmo.Dev.Platform.Enrichment.Tests.Models;

public class EnrichedModel : IEnrichedModel<int>
{
    public EnrichedModel(int key)
    {
        Key = key;
    }

    public int Key { get; }

    public string? Value { get; set; }
}