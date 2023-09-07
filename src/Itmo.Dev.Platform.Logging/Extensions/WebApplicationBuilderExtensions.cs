using Sentry.AspNetCore;
using Sentry.AspNetCore.Grpc;

namespace Itmo.Dev.Platform.Logging.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddPlatformSentry(this WebApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("Sentry:Enabled").Get<bool>() is false)
        {
            return builder;
        }

        var environment = builder.Configuration.GetSection("Sentry:Environment").Get<string?>();
        
        if (string.IsNullOrWhiteSpace(environment))
        {
            environment = builder.Environment.EnvironmentName;
        }
            
        builder.WebHost.UseSentry(sentryBuilder =>
        {
            sentryBuilder.AddGrpc();
            sentryBuilder.AddSentryOptions(options => options.Environment = environment);
        });

        return builder;
    }
}