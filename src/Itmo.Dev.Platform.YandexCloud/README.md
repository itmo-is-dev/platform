# Itmo.Dev.Platform.YandexCloud

Platform extensions for working with YandexCloud

Use `AddPlatformYandexCloudAsync` on your host builder to register library.

## Authorization

Configure a method of authorization to YandexCloud API.\
If no explicit method is configured, null method will be used, all calls to YandexCloud API will fail.

### Configuration

```json
{
  "Platform": {
    "YandexCloud": {
      "Authorization": {
        "VirtualMachine": {
          "IsEnabled": bool
          "ServiceUri": string,
          "MinRemainingTokenLifetime": timespan
        }
      }
    }
  }
}
```

see [MSDN for timespan string formats](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings)

| Path                                                                        | Description                                                                                                                                                                                        |
|-----------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Platform:YandexCloud:Authorization:VirtualMachine:IsEnabled                 | Enables authorization through service account bound to VM                                                                                                                                          |
| Platform:YandexCloud:Authorization:VirtualMachine:ServiceUri                | YandexCloud's internal service uri for accessing VM info (default: http://169.254.169.254/)                                                                                                        |
| Platform:YandexCloud:Authorization:VirtualMachine:MinRemainingTokenLifetime | Minimal remaining lifetime of token when it is retrieved for any operation, if token will expire prior to DateTimeOffset.UtcNow + TimeSpan.FromSeconds(threshold), the new token will be generated |

## LockBox

Adds a configuration provider that loads values from YandexCloud's Lockbox.

### Configuration

Configuration parameters:

```json
{
  "Platform": {
    "YandexCloud": {
      "LockBox": {
        "IsEnabled": bool,
        "SecretId": string,
        "LockboxOptionsPollingDelay": timespan
      }
    }
  }
}
```

see [MSDN for timespan string formats](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings)

| Path                                                    | Description                                                   |
|---------------------------------------------------------|---------------------------------------------------------------|
| Platform:YandexCloud:Lockbox:IsEnabled                  | Enables Lockbox                                               |
| Platform:YandexCloud:Lockbox:SecretId                   | Id of Lockbox secret, from which configuration will be loaded |
| Platform:YandexCloud:Lockbox:LockboxOptionsPollingDelay | An interval of configuration reload                           |
