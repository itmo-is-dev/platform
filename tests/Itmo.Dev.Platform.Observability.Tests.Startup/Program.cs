#pragma warning disable CA1506
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Grpc.Clients;
using Itmo.Dev.Platform.Grpc.Services;
using Itmo.Dev.Platform.Observability;
using Itmo.Dev.Platform.Observability.Tests;
using Itmo.Dev.Platform.Observability.Tests.Startup;
using Itmo.Dev.Platform.Observability.Tests.Startup.Controllers;
using Itmo.Dev.Platform.Observability.Tests.Startup.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

bool isGrpcServer =
    bool.Parse(Environment.GetEnvironmentVariable(TestingConstants.IsGrpcServiceEnvVariableName) ?? "false");

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Platform:ServiceName"] = "test",
    ["Platform:Observability:Tracing:IsEnabled"] = "true",
});

builder.Services.AddUtcDateTimeProvider();
builder.Services.AddSingleton(new JsonSerializerSettings());
builder.Services.AddSingleton<TestingActivityProcessor>();

builder.Services.AddPlatform();

builder.Services.AddOpenTelemetry().WithTracing(x => x.AddConsoleExporter());

if (isGrpcServer)
{
    builder.Services.AddPlatformGrpcServices(grpc => grpc
        .AddInterceptor<ParentSpanInterceptor>());

    builder.WebHost.UseUrls(TestingConstants.GrpcServiceUrl);
    builder.WebHost.UseKestrel(x => x.ConfigureEndpointDefaults(e => e.Protocols = HttpProtocols.Http2));
}
else
{
    builder.AddPlatformObservability(observability => observability
        .AddTracingPlugin<TestTracingPlugin>());

    builder.Services.AddPlatformGrpcClients(grpc => grpc
        .AddService(service => service
            .Called("test")
            .WithConfiguration("TestServer")
            .WithClient<HelloService.HelloServiceClient>()
            .WithInterceptor<DebugClientInterceptor>()));

    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing => tracing
            .AddProcessor(p => p.GetRequiredService<TestingActivityProcessor>()));

    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["TestServer:Address"] = TestingConstants.GrpcServiceUrl,
    });

    builder.WebHost.UseUrls(TestingConstants.HttpServiceUrl);
}

WebApplication app = builder.Build();

app.UseRouting();

if (isGrpcServer)
{
    app.MapGrpcReflectionService();
    app.MapGrpcService<HelloController>();
}
else
{
    app.MapGet("hello",
        async ([FromQuery] string name, [FromServices] HelloService.HelloServiceClient client) =>
        {
            var request = new HelloRequest { Name = name };
            var response = await client.HelloAsync(request);

            return Results.Ok(response.Message);
        });
}

app.UsePlatformObservability();

app.Run();

public partial class Program;
