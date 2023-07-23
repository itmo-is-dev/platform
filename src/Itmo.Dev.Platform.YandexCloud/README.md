# Itmo.Dev.Platform.YandexCloud

Platform extensions for working with YandexCloud

## LockBox

### Configuration

Use `AddYandexCloudConfigurationAsync(this WebApplicationBuilder builder)` method for reading configuration
from YandexCloud's LockBox

Configuration parameters:

```json
{
  "Platform": {
    "Environment": "string",
    "YandexCloud": {
      "ServiceUri": "string",
      "LockBox": {
        "SecretId": "string"
      }
    }
  }
}
```

| Path                                  | Description                                         | Respected values   |
|---------------------------------------|-----------------------------------------------------|--------------------|
| Platform:Environment                  | Running environment identifier                      | Local, YandexCloud |
| Platform:YandexCloud:ServiceUri       | Uri, user for token fetching                        |                    |
| Platform:YandexCloud:LockBox:SecretId | Id of LockBox secrets where configuration is stored |                    |
