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
      "MinRemainingTokenLifetimeSeconds": int,
      "LockBox": {
        "SecretId": "string"
      }
    }
  }
}
```

| Path                                                  | Description                                                                                                                                                                                        | Respected values   |
|-------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------|
| Platform:Environment                                  | Running environment identifier                                                                                                                                                                     | Local, YandexCloud |
| Platform:YandexCloud:ServiceUri                       | Uri, used for token fetching                                                                                                                                                                       |                    |
| Platform:YandexCloud:MinRemainingTokenLifetimeSeconds | Minimal remaining lifetime of token when it is retrieved for any operation, if token will expire prior to DateTimeOffset.UtcNow + TimeSpan.FromSeconds(threshold), the new token will be generated |                    |
| Platform:YandexCloud:LockBox:SecretId                 | Id of LockBox secrets where configuration is stored                                                                                                                                                |                    |
