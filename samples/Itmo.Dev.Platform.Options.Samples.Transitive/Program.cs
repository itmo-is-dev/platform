using Itmo.Dev.Platform.Observability;
using Itmo.Dev.Platform.Options.Samples;

var builder = WebApplication.CreateBuilder();

builder.AddPlatformObservability();
builder.Services.RegisterOptions();
