namespace Itmo.Dev.Platform.Logging.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePlatformSentryTracing(this IApplicationBuilder builder, IConfiguration configuration)
    {
        return configuration.GetSection("Sentry:Enabled").Get<bool>()
            ? builder.UseSentryTracing() 
            : builder;
    }
}