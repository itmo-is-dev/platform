using Grpc.Core;

namespace Itmo.Dev.Platform.Grpc.Extensions;

public static class CallOptionsExtensions
{
    public static CallOptions WithAppendedHeaders(this CallOptions options, IEnumerable<Metadata.Entry> entries)
    {
        return options.WithHeaders(options.Headers is null
            ? new Metadata { entries }
            : new Metadata { entries.Concat(options.Headers).DistinctBy(x => x.Key) });
    }
}