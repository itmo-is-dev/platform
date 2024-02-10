using Grpc.Core;
using System.Collections;

namespace Itmo.Dev.Platform.Grpc.Services;

internal class MetadataCollection : IReadOnlyCollection<Metadata.Entry>
{
    private readonly Metadata _metadata;

    public MetadataCollection(Metadata metadata)
    {
        _metadata = metadata;
    }

    public int Count => _metadata.Count;

    public IEnumerator<Metadata.Entry> GetEnumerator()
        => _metadata.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}