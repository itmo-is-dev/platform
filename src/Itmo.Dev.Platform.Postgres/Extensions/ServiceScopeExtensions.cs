using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.Postgres.Extensions;

public static class ServiceScopeExtensions
{
    public static Task UsePlatformMigrationsAsync(this IServiceScope scope)
    {
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();

        return Task.CompletedTask;
    }
}