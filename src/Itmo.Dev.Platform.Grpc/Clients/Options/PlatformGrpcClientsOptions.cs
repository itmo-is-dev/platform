namespace Itmo.Dev.Platform.Grpc.Clients.Options;

public class PlatformGrpcClientsOptions
{
    public bool RecordMessageEvents { get; set; } = true;

    public bool RecordExceptions { get; set; } = true;
}