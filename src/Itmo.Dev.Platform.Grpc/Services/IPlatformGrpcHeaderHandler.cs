using Grpc.Core;

namespace Itmo.Dev.Platform.Grpc.Services;

public interface IPlatformGrpcHeaderHandler
{
    ValueTask HandleHeadersAsync(IReadOnlyCollection<Metadata.Entry> headers, CancellationToken cancellationToken);
}