# Itmo.Dev.Platform.Observability

## Usage

Call `AddPlatformObservability` extension method on your `WebApplicationBuilder`.

## Configuration

### Tracing

```json
{
  "Platform": {
    "Observability": {
      "Tracing": {
        "IsEnabled": bool
      }
    }
  }
}
```

### Logging

```json
{
  "Platform": {
    "Observability": {
      "Logging": {
        "Serilog": {
          // Your serilog configuration}
        }
      }
    }
  }
```

### Sentry

```json
{
  "Platform": {
    "Observability": {
      "Sentry": {
        "IsEnabled": bool
        "Configuration": {
          // Your Sentry configuration
        }
      }
    }
  }
}
```