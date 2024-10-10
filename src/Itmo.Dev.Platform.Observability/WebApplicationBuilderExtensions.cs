using Itmo.Dev.Platform.Common.Options;
using Itmo.Dev.Platform.Observability.Options;
using OpenTelemetry.Trace;
using Sentry.OpenTelemetry;
using Serilog;

namespace Itmo.Dev.Platform.Observability;

public static class WebApplicationBuilderExtensions
{
    public static void AddPlatformObservability(this WebApplicationBuilder builder)
    {
        var platformOptions = new PlatformOptions();
        builder.Configuration.GetSection("Platform").Bind(platformOptions);

        ConfigureTracing(builder, platformOptions);
        var sentryOptions = ConfigureSentry(builder, platformOptions);
        ConfigureLogging(builder, sentryOptions);
    }

    private static void ConfigureTracing(WebApplicationBuilder builder, PlatformOptions platformOptions)
    {
        var tracingOptions = new PlatformObservabilityTracingOptions();
        builder.Configuration.GetSection("Platform:Observability:Tracing").Bind(tracingOptions);

        if (tracingOptions.IsEnabled is false)
            return;

        builder.Services
            .AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddSource(platformOptions.ServiceName)
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation()
                .AddGrpcCoreInstrumentation()
                .AddGrpcClientInstrumentation());
    }


    private static PlatformObservabilitySentryOptions ConfigureSentry(
        WebApplicationBuilder builder,
        PlatformOptions platformOptions)
    {
        var sentrySection = builder.Configuration.GetSection("Platform:Observability:Sentry");

        var sentryOptions = new PlatformObservabilitySentryOptions();
        sentrySection.Bind(sentryOptions);

        if (sentryOptions.IsEnabled is false)
            return sentryOptions;

        builder.WebHost.UseSentry(options =>
        {
            sentrySection.Bind(options);

            options.Environment = platformOptions.Environment ?? builder.Environment.EnvironmentName;
            options.TracesSampleRate = 1.0;
            options.UseOpenTelemetry();
        });

        builder.Services.AddOpenTelemetry().WithTracing(x => x.AddSentry());

        return sentryOptions;
    }

    private static void ConfigureLogging(
        WebApplicationBuilder builder,
        PlatformObservabilitySentryOptions sentryOptions)
    {
        var configurationSection = builder.Configuration.GetSection("Platform:Observability:Logging");

        var loggingConfiguration = new LoggerConfiguration().ReadFrom.Configuration(configurationSection);

        if (sentryOptions.IsEnabled)
        {
            var sentrySection = builder.Configuration.GetSection("Platform:Observability:Sentry");
            loggingConfiguration.WriteTo.Sentry(o => sentrySection.Bind(o));
        }

        Log.Logger = loggingConfiguration.CreateLogger();
        builder.Host.UseSerilog();
    }
}