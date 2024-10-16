using Itmo.Dev.Platform.Common.Options;
using Microsoft.Extensions.Options;
using Sentry.AspNetCore;
using Sentry.AspNetCore.Grpc;
using Sentry.Infrastructure;
using Sentry.OpenTelemetry;
using System.Net;

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

        SentrySdk.Init(options =>
        {
            _options.Configuration?.Bind(options);

            options.TracesSampleRate = 1.0;
            options.Environment = _platformOptions.Environment ?? builder.Environment.EnvironmentName;

            if (_options.WebProxyUri is not null)
            {
                options.HttpProxy = new WebProxy(_options.WebProxyUri)
                {
                    Credentials = new NetworkCredential(_options.WebProxyUsername, _options.WebProxyPassword),
                };

                _logger.LogInformation("Sentry web proxy initialized = {ProxyUri}", _options.WebProxyUri.Host);
            }

            options.DiagnosticLogger = new ConsoleAndTraceDiagnosticLogger(options.DiagnosticLevel);

            options.UseOpenTelemetry();
        });

        builder.WebHost.UseSentry(sentry =>
        {
            sentry.AddSentryOptions(x => x.InitializeSdk = false);
            sentry.AddGrpc();
        });

        _logger.LogInformation("Sentry initialized");
    }
}