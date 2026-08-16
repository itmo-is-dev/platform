using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Grpc.Clients.Options;

[OptionsType]
public class PlatformGrpcClientOptions
{
    [Required]
    public required Uri Address { get; set; }
}