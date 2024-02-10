using Grpc.Core;
using Itmo.Dev.Platform.Grpc.Clients;

namespace Itmo.Dev.Platform.Grpc.Samples.Clients;

public class HeaderProvider : IPlatformGrpcHeaderProvider
{
    public IEnumerable<Metadata.Entry> GetHeaders()
    {
        yield return new Metadata.Entry("x-iid-sample", "sample");
    }
}