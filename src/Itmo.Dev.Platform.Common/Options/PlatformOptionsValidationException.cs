using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.Common.Options;

internal class PlatformOptionsValidationException : OptionsValidationException
{
    public PlatformOptionsValidationException(OptionsValidationException exception)
        : base(exception.OptionsName, exception.OptionsType, exception.Failures) { }

    public override string Message => $"""
    Name: {OptionsName}
    Type: {OptionsType}
    Error: {base.Message}
    """;
}
