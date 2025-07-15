using Itmo.Dev.Platform.Common.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUtcDateTimeProvider();
builder.Services.AddSingleton(new JsonSerializerSettings());
builder.Services.AddLogging(x => x.AddSerilog());
builder.Services.AddOptions();

builder.Services.AddSingleton(p => p.GetRequiredService<IOptions<JsonSerializerSettings>>().Value);

builder.Services.AddPlatform();

var app = builder.Build();

app.Run();

public partial class Program;