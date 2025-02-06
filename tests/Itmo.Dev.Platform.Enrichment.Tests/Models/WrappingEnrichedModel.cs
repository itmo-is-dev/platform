namespace Itmo.Dev.Platform.Enrichment.Tests.Models;

public class WrappingEnrichedModel : IEnrichedModel<int>
{
    public WrappingEnrichedModel(int key, EnrichedModel innerModel)
    {
        Key = key;
        InnerModel = innerModel;
    }

    public int Key { get; }

    public EnrichedModel InnerModel { get; }

    public string? OuterValue { get; set; }
}
