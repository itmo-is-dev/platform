using Itmo.Dev.Platform.Common.Options;
using Microsoft.Extensions.Options;
using Sentry.AspNetCore;
using Sentry.AspNetCore.Grpc;
using Sentry.OpenTelemetry;

namespace Itmo.Dev.Platform.Observability.Sentry;

internal class SentryObservabilityPlugin : IObservabilityConfigurationPlugin
{
    private readonly PlatformObservabilitySentryOptions _options;
    private readonly PlatformOptions _platformOptions;
    private readonly ILogger<SentryObservabilityPlugin> _logger;

    public SentryObservabilityPlugin(
        IOptions<PlatformObservabilitySentryOptions> options,
        IOptions<PlatformOptions> platformOptions,
        ILogger<SentryObservabilityPlugin> logger)
    {
        _logger = logger;
        _options = options.Value;
        _platformOptions = platformOptions.Value;
    }

    public void Configure(WebApplicationBuilder builder)
    {
        if (_options.IsEnabled is false)
        {
            _logger.LogInformation("Sentry is disabled");
            return;
        }

        builder.WebHost.UseSentry(sentry =>
        {
            sentry.AddSentryOptions(options =>
            {
                _options.Configuration?.Bind(options);

                options.InitializeSdk = true;
                options.TracesSampleRate = 1.0;
                options.Environment = _platformOptions.Environment ?? builder.Environment.EnvironmentName;

                options.UseOpenTelemetry();
            });

            sentry.AddGrpc();
        });

        _logger.LogInformation("Sentry initialized");
    }
}