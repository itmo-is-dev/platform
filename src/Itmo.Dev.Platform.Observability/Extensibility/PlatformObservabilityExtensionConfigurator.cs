using Itmo.Dev.Platform.Observability.Logging;
using Itmo.Dev.Platform.Observability.Tracing;

namespace Itmo.Dev.Platform.Observability.Extensibility;

internal class PlatformObservabilityExtensionConfigurator : IPlatformObservabilityExtensionConfigurator
{
    private readonly IServiceCollection _collection;

    public PlatformObservabilityExtensionConfigurator(IServiceCollection collection)
    {
        _collection = collection;
    }

    public IPlatformObservabilityExtensionConfigurator AddSerilogPlugin<T>()
        where T : class, ISerilogConfigurationPlugin
    {
        _collection.AddSingleton<ISerilogConfigurationPlugin, T>();
        return this;
    }

    public IPlatformObservabilityExtensionConfigurator AddTracingPlugin<T>()
        where T : class, ITracingConfigurationPlugin
    {
        _collection.AddSingleton<ITracingConfigurationPlugin, T>();
        return this;
    }
}