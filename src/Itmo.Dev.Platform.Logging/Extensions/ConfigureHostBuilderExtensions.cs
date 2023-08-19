using Serilog;

namespace Itmo.Dev.Platform.Logging.Extensions;

public static class ConfigureHostBuilderExtensions
{
    public static IHostBuilder UseSerilogForAppLogs(this ConfigureHostBuilder hostBuilder, IConfiguration configuration)
    {
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration);

        if (configuration.GetSection("Sentry:Enabled").Get<bool>())
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Sentry(options =>
            {
                configuration.GetSection("Sentry").Bind(options);
            });
        }

        Log.Logger = loggerConfiguration.CreateLogger();
        return hostBuilder.UseSerilog();
    }
}