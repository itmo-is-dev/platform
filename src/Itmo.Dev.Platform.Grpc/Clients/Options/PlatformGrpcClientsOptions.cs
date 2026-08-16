using Itmo.Dev.Platform.Options;
using System.ComponentModel;

namespace Itmo.Dev.Platform.Grpc.Clients.Options;

[OptionsType]
public class PlatformGrpcClientsOptions
{
    [DefaultValue(true)]
    public bool RecordMessageEvents { get; set; } = true;

    [DefaultValue(true)]
    public bool RecordExceptions { get; set; } = true;
}