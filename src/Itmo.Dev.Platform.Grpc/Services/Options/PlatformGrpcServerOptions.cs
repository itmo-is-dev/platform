using Itmo.Dev.Platform.Options;

namespace Itmo.Dev.Platform.Grpc.Services.Options;

[OptionsType]
public class PlatformGrpcServerOptions
{
    public bool RecordMessageEvents { get; set; } = true;

    public bool RecordExceptions { get; set; } = true;
}