using Itmo.Dev.Platform.Options;
using System.ComponentModel;

namespace Itmo.Dev.Platform.Grpc.Services.Options;

[OptionsType]
public class PlatformGrpcServerOptions
{
    [DefaultValue(true)]
    public bool RecordMessageEvents { get; set; } = true;

    [DefaultValue(true)]
    public bool RecordExceptions { get; set; } = true;
}