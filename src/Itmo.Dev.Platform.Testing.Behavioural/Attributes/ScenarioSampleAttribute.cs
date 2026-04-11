using System.Reflection;
using Xunit.Sdk;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130
namespace Itmo.Dev.Platform.Testing.Behavioural;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
[DataDiscoverer(
    typeName: "Itmo.Dev.Platform.Testing.Behavioural.ScenarioSampleDataDiscoverer",
    assemblyName: "Itmo.Dev.Platform.Testing.Behavioural")]
public sealed class ScenarioSampleAttribute : Attribute
{
    public ScenarioSampleAttribute(params object?[] parameters)
    {
        Parameters = parameters;
    }

    public object?[] Parameters { get; }

    public IEnumerable<object?[]> GetData(MethodInfo testMethod) => [Parameters];
}
