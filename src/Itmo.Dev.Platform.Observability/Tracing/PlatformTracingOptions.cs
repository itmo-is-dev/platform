namespace Itmo.Dev.Platform.Observability.Tracing;

internal class PlatformTracingOptions
{
    public bool IsEnabled { get; set; }

    public string[]? Sources { get; set; }
}