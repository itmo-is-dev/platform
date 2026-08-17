using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Hangfire.Postgres.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Postgres.Extensions;
using Itmo.Dev.Platform.Kafka.Extensions;
using Itmo.Dev.Platform.Observability;
using Itmo.Dev.Platform.Options.Samples;

var builder = WebApplication.CreateBuilder();

builder.AddPlatformObservability();
builder.Services.RegisterOptions();

builder.Services.AddPlatformKafka(kafka => kafka.ConfigureOptions("Kafka"));

builder.Services.AddPlatformBackgroundTasks(x
    => x.UsePostgresPersistence("Postgres")
        .ConfigureScheduling("Scheduling")
        .UseHangfireScheduling(h => h.ConfigureOptions("Hangfire").UsePostgresJobStorage())
        .ConfigureExecution("Execution"));

builder.Build().Run();
