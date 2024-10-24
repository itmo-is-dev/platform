using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Grpc.Clients.Options;

public class PlatformGrpcClientOptions : IValidatableObject
{
    public Uri? Address { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Address is null)
            yield return new ValidationResult("Client address must be specified");
    }
}