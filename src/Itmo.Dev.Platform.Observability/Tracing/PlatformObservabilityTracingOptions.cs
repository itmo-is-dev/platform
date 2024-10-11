namespace Itmo.Dev.Platform.Observability.Tracing;

internal class PlatformObservabilityTracingOptions
{
    public bool IsEnabled { get; set; }

    public string[]? Sources { get; set; }
}