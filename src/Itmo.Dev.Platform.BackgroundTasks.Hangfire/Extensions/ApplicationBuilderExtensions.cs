using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace Itmo.Dev.Platform.BackgroundTasks.Hangfire.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UsePlatformBackgroundTasksHangfire(this ApplicationBuilder builder)
    {
        builder.UseHangfireDashboard();
        builder.UseEndpoints(e => e.MapHangfireDashboard());
    }
}