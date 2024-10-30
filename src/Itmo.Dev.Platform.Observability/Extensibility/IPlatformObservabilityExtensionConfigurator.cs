using Itmo.Dev.Platform.Observability.Logging;
using Itmo.Dev.Platform.Observability.Tracing;

namespace Itmo.Dev.Platform.Observability.Extensibility;

public interface IPlatformObservabilityExtensionConfigurator
{
    IPlatformObservabilityExtensionConfigurator AddSerilogPlugin<T>()
        where T : class, ISerilogConfigurationPlugin;

    IPlatformObservabilityExtensionConfigurator AddTracingPlugin<T>()
        where T : class, ITracingConfigurationPlugin;
}