using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.Common.Extensions;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUtcDateTimeProvider();
builder.Services.AddSingleton(new JsonSerializerSettings());

var app = builder.Build();


await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.UsePlatformBackgroundTasksAsync(default);
}

app.Run();

public partial class Program;