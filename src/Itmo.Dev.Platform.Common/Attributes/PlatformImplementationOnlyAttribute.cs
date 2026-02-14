namespace Itmo.Dev.Platform.Common.Attributes;

[AttributeUsage(AttributeTargets.Interface)]
public sealed class PlatformImplementationOnlyAttribute : Attribute
{
    public PlatformImplementationOnlyAttribute(string message)
    {
        Message = message;
    }

    public string Message { get; }
}
