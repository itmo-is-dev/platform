using Itmo.Dev.Platform.Observability.Logging.Enrichers;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Exceptions;
using Serilog.Settings.Configuration;

namespace Itmo.Dev.Platform.Observability.Logging;

internal class SerilogConfigurationPlugin : IObservabilityConfigurationPlugin
{
    private readonly PlatformLoggingOptions _options;
    private readonly IEnumerable<ISerilogConfigurationPlugin> _plugins;
    private readonly ILogger<SerilogConfigurationPlugin> _logger;

    public SerilogConfigurationPlugin(
        IOptions<PlatformLoggingOptions> options,
        IEnumerable<ISerilogConfigurationPlugin> plugins,
        ILogger<SerilogConfigurationPlugin> logger)
    {
        _plugins = plugins;
        _logger = logger;
        _options = options.Value;
    }

    public void Configure(WebApplicationBuilder builder)
    {
        if (_options.Serilog?.GetChildren().Count() is not > 0)
        {
            _logger.LogInformation("Serilog logging is not configured");
            return;
        }

        var configuration = new LoggerConfiguration();
        var readerOptions = new ConfigurationReaderOptions { SectionName = "" };

        configuration = configuration
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.With<ActivityLogEnricher>();

        configuration = configuration
            .ReadFrom.Configuration(_options.Serilog, readerOptions);

        configuration = configuration.WriteTo.Console(
            outputTemplate: "{Timestamp:T} [{Level:u3}] {SourceContext} {Message}{NewLine}{Exception}");

        configuration = _plugins.Aggregate(
            configuration,
            (conf, plugin) => plugin.Configure(builder, conf));

        Log.Logger = configuration.CreateLogger();
        builder.Host.UseSerilog();

        _logger.LogInformation("Serilog logging initialized");
    }
}