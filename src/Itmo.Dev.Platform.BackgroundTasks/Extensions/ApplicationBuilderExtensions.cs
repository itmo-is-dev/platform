using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace Itmo.Dev.Platform.BackgroundTasks.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UsePlatformBackgroundTasks(this ApplicationBuilder builder)
    {
        builder.UseHangfireDashboard();
        builder.UseEndpoints(e => e.MapHangfireDashboard());
    }
}