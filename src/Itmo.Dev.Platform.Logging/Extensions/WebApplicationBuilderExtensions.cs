using Sentry.AspNetCore;
using Sentry.AspNetCore.Grpc;

namespace Itmo.Dev.Platform.Logging.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddPlatformSentry(this WebApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("Sentry:Enabled").Get<bool>())
        {
            builder.WebHost.UseSentry(sentryBuilder =>
            {
                sentryBuilder.AddGrpc();
                sentryBuilder.AddSentryOptions(options => options.Environment = builder.Environment.EnvironmentName);
            });
        }

        return builder;
    }
}