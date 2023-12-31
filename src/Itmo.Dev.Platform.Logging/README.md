# Itmo.Dev.Platform.Common

Package contains methods to connect Serilog and Sink to Sentry.


Usage example:
```csharp
builder.AddPlatformSentry();
builder.Host.AddPlatformSerilog(builder.Configuration);

// ...

var app = builder.Build();

app.UseRouting();
// UsePlatformSentryTracing should be placed right after UseRouting middleware
app.UsePlatformSentryTracing(builder.Configuration);
```

Sentry application settings example:
```json
{
  "Sentry": {
    "Enabled": true,
    "Environment": "Production",
    "Dsn": "",
    "Debug": false,
    "TracesSampleRate": 1.0,
    "MinimumEventLevel": "Warning"
  }
}
```

Serilog application settings example:
```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File"
    ],
    "MinimumLevel": {
      "Default": "Verbose",
      "Override": {
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Grpc.AspNetCore.Server.Model.Internal": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:T} [{Level:u3}] {SourceContext} {Message}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Serilogs/${ApplicationName}/AppLogs_.log",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:o} [{Level:u3}] {SourceContext} {Message}{NewLine}{Exception}",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```


N.B.

Don't forget to add required Sinks:
 - Serilog.Sinks.Console
 - Serilog.Sinks.File