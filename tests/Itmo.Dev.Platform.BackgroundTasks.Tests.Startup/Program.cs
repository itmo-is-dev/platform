using Itmo.Dev.Platform.Common.Extensions;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUtcDateTimeProvider();
builder.Services.AddSingleton(new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto,
});

builder.Services.AddPlatform();

var app = builder.Build();

app.Run();

public partial class Program;