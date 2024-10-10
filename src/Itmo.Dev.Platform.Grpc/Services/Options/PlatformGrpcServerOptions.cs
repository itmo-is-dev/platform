namespace Itmo.Dev.Platform.Grpc.Services.Options;

public class PlatformGrpcServerOptions
{
    public bool RecordMessageEvents { get; set; } = true;

    public bool RecordExceptions { get; set; } = true;
}