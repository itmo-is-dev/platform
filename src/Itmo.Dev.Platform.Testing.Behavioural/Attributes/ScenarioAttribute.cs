#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Itmo.Dev.Platform.Testing.Behavioural;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ScenarioAttribute : Attribute
{
    public string? DisplayName { get; set; }

    public string? Skip { get; set; }

    public int Timeout { get; set; }
}
