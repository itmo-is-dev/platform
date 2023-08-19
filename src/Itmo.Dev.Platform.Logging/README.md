# Itmo.Dev.Platform.Common

Package contains method to connect Serilog and Sink to Sentry.

### Prerequisites
- Sentry could be configured in project.

Sentry application settings example:
```json
{
  "Sentry": {
    "Enabled": true,
    "Dsn": "",
    "Debug": false,
    "TracesSampleRate": 1.0,
    "MinimumEventLevel": "Warning"
  }
}
```

Usage example:
```csharp
builder.Host.UseSerilogForAppLogs(builder.Configuration);
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
        ...
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:T} [{Level:u3}] {Message}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Serilogs/${ApplicationName}/AppLogs_.log",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:o} [{Level:u3}] {Message}{NewLine}{Exception}",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```