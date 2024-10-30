using Itmo.Dev.Platform.Common.Options;
using Itmo.Dev.Platform.Observability.Sentry.ExceptionFilters;
using Microsoft.Extensions.Options;
using Sentry.AspNetCore;
using Sentry.AspNetCore.Grpc;
using Sentry.Infrastructure;
using Sentry.OpenTelemetry;
using System.Net;

namespace Itmo.Dev.Platform.Observability.Sentry;

internal class SentryConfigurationPlugin : IObservabilityConfigurationPlugin
{
    private readonly PlatformSentryOptions _options;
    private readonly PlatformOptions _platformOptions;
    private readonly ILogger<SentryConfigurationPlugin> _logger;

    public SentryConfigurationPlugin(
        IOptions<PlatformSentryOptions> options,
        IOptions<PlatformOptions> platformOptions,
        ILogger<SentryConfigurationPlugin> logger)
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
                options.TracesSampleRate = 1.0;
                options.Environment = _platformOptions.Environment ?? builder.Environment.EnvironmentName;

                options.DiagnosticLogger = new ConsoleAndTraceDiagnosticLogger(options.DiagnosticLevel);

                _options.Configuration?.Bind(options);

                options.UseOpenTelemetry();
                options.AddExceptionFilter(new CancelledExceptionFilter());
            });

            sentry.AddGrpc();
        });

        _logger.LogInformation("Sentry initialized");
    }
}