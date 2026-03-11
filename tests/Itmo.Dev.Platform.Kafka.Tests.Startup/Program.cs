using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Observability;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Platform:ServiceName"] = "Test",
    ["Platform:Observability:Tracing:IsEnabled"] = "true",
});

builder.AddPlatformObservability();
builder.Services.AddOpenTelemetry().WithTracing(tracing => tracing.AddConsoleExporter());

builder.Services.AddLogging(x => x.AddSerilog());
builder.Services.AddOptions();

builder.Services.AddPlatform(x => x.WithNewtonsoftSerialization());

var app = builder.Build();

app.UsePlatformObservability();

app.Run();

public partial class Program;
