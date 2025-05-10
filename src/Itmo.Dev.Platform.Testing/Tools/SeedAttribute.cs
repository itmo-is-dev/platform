namespace Itmo.Dev.Platform.Testing.Tools;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class SeedAttribute : Attribute
{
    public SeedAttribute(int value)
    {
        Value = value;
    }

    public int Value { get; }
}