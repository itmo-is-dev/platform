using Itmo.Dev.Platform.Common.Exceptions;

// ReSharper disable once CheckNamespace
namespace Itmo.Dev.Platform.Enrichment;

public class PlatformEnrichmentException : PlatformException
{
    private PlatformEnrichmentException(string? message) : base(message) { }

    public static PlatformEnrichmentException ModelNotFound<TKey, TModel>(TKey key)
        where TKey : notnull
        where TModel : IEnrichedModel<TKey>
    {
        string message = $"Model of type {typeof(TModel).FullName} with key = {key} was not found";
        return new PlatformEnrichmentException(message);
    }
}
