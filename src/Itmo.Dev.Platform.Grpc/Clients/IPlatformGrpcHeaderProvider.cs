using Grpc.Core;

namespace Itmo.Dev.Platform.Grpc.Clients;

public interface IPlatformGrpcHeaderProvider
{
    IEnumerable<Metadata.Entry> GetHeaders();
}