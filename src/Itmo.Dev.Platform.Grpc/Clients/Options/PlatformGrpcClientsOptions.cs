using Itmo.Dev.Platform.Options;

namespace Itmo.Dev.Platform.Grpc.Clients.Options;

[OptionsType]
public class PlatformGrpcClientsOptions
{
    public bool RecordMessageEvents { get; set; } = true;

    public bool RecordExceptions { get; set; } = true;
}