using Grpc.Core;

namespace Itmo.Dev.Platform.Grpc.Extensions;

public static class MetadataExtensions
{
    public static Metadata Add(this Metadata metadata, IEnumerable<Metadata.Entry> entries)
    {
        foreach (Metadata.Entry entry in entries)
        {
            metadata.Add(entry);
        }

        return metadata;
    }
}