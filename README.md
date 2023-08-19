# Itmo.Dev.Platform

- [Itmo.Dev.Platform.Common](src/Itmo.Dev.Platform.Common/README.md)
- [Itmo.Dev.Platform.YandexCloud](src/Itmo.Dev.Platform.YandexCloud/README.md)
- [Itmo.Dev.Platform.Kafka](src/Itmo.Dev.Platform.Kafka/README.md)
- [Itmo.Dev.Platform.Postgres](src/Itmo.Dev.Platform.Postgres/README.md)
- [Itmo.Dev.Platform.Logging](src/Itmo.Dev.Platform.Logging/README.md)

## Package source installation

1. Add `nuget.config` file next to your `*.sln` file (if one does not exists)
2. Prepare your GitHub personal access token with `packages:read` permission
3. Add reference to `itmo-is-dev` GitHub nuget repository
    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
        <packageSources>
            <add key="github" value="https://nuget.pkg.github.com/itmo-is-dev/index.json"/>
        </packageSources>
         <packageSourceMapping>
              <packageSource key="github">
                  <package pattern="Itmo.Dev.Platform*" />
              </packageSource>
          </packageSourceMapping>
    </configuration>
    ```
4. When prompted for authorization, use your github username as username, and generated PAT as password

Alternatively, you can use CLI to add package source. Code below will add source into your global config.
If you want to add it into local config, you should run it with option `--configfile nuget.config`

```shell
dotnet nuget add source --username YOUR_USERNAME --password YOUR_GITHUB_PAT --store-password-in-clear-text --name github "https://nuget.pkg.github.com/itmo-is-dev/index.json"
```

### <p style="color: red">WARNING! </p>

Adding source into local config, will result in credentials be written into local file \
Be aware that it may lead to credentials leaking into VSC \
You can store credentials separately from source definition, in global nuget.config