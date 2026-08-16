namespace Itmo.Dev.Platform.Options;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ProducesOptionRegistrationAttribute<TOptions> : Attribute
{
    public string SectionName { get; init; } = string.Empty;

    public string SectionParameterName { get; init; } = string.Empty;
}
