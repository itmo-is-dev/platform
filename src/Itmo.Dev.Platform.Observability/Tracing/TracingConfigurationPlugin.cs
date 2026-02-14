using Itmo.Dev.Platform.Common.Options;
using Itmo.Dev.Platform.Kafka.Tools;
using Itmo.Dev.Platform.MessagePersistence.Internal.Tools;
using Itmo.Dev.Platform.Observability.Tracing.Processors;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Itmo.Dev.Platform.Observability.Tracing;

internal class TracingConfigurationPlugin : IObservabilityConfigurationPlugin
{
    private readonly PlatformTracingOptions _options;
    private readonly PlatformOptions _platformOptions;
    private readonly IEnumerable<ITracingConfigurationPlugin> _plugins;
    private readonly ILogger<TracingConfigurationPlugin> _logger;

    public TracingConfigurationPlugin(
        IOptions<PlatformTracingOptions> options,
        IOptions<PlatformOptions> platformOptions,
        IEnumerable<ITracingConfigurationPlugin> plugins,
        ILogger<TracingConfigurationPlugin> logger)
    {
        _plugins = plugins;
        _logger = logger;
        _options = options.Value;
        _platformOptions = platformOptions.Value;
    }

    public void Configure(WebApplicationBuilder builder)
    {
        if (_options.IsEnabled is false)
        {
            _logger.LogInformation("OpenTelemetry tracing is disabled");
            return;
        }

        builder.Services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .ConfigureResource(x => x.AddService(_platformOptions.ServiceName))
                    .AddSource(PlatformKafkaActivitySource.Name)
                    .AddSource(MessagePersistenceActivitySource.Name)
                    .SetSampler(new AlwaysOnSampler())
                    .AddAspNetCoreInstrumentation(x => x.RecordException = true)
                    .AddGrpcCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddNpgsql()
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.FilterHttpRequestMessage = message => message.Version.Major >= 1;
                        options.RecordException = true;
                    });

                tracing
                    .AddProcessor<DbStatementActivityFilter>()
                    .AddProcessor<MetricsActivityFilter>();

                foreach (string source in _options.Sources ?? [])
                {
                    tracing.AddSource(source);
                }

                foreach (ITracingConfigurationPlugin plugin in _plugins)
                {
                    plugin.Configure(builder, tracing);
                }
            });

        _logger.LogInformation("OpenTelemetry tracing initialized");
    }
}
