using Itmo.Dev.Platform.Common.Extensions;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPlatform(x => x
    .WithNewtonsoftSerialization(options => options.TypeNameHandling = TypeNameHandling.Auto));

var app = builder.Build();

app.Run();

public partial class Program;
