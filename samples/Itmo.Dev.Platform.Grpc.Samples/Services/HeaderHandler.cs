using Grpc.Core;
using Itmo.Dev.Platform.Grpc.Services;

namespace Itmo.Dev.Platform.Grpc.Samples.Clients;

public class HeaderHandler : IPlatformGrpcHeaderHandler
{
    public ValueTask HandleHeadersAsync(
        IReadOnlyCollection<Metadata.Entry> headers,
        CancellationToken cancellationToken)
    {
        foreach (Metadata.Entry header in headers)
        {
            Console.WriteLine($"Key = {header.Key}, Value = {header.Value}");
        }

        return ValueTask.CompletedTask;
    }
}