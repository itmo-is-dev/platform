#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Itmo.Dev.Platform.Testing.Behavioural;

[AttributeUsage(AttributeTargets.Class)]
public sealed class StepAttribute : Attribute
{
    public StepAttribute(string? name)
    {
        Name = name;
    }

    public string? Name { get; }
}
