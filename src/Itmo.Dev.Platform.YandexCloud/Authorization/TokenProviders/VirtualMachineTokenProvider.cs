using Itmo.Dev.Platform.YandexCloud.Authorization.Options;
using Itmo.Dev.Platform.YandexCloud.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace Itmo.Dev.Platform.YandexCloud.Authorization.TokenProviders;

internal class VirtualMachineTokenProvider : IYandexCloudTokenProvider, IDisposable
{
    private readonly YandexCloudVirtualMachineAuthorizationOptions _options;
    private readonly SemaphoreSlim _semaphore;

    private TokenInfo? _tokenInfo;

    public VirtualMachineTokenProvider(YandexCloudVirtualMachineAuthorizationOptions options)
    {
        _options = options;
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var minExpires = now + _options.MinRemainingTokenLifetime;

        if (_tokenInfo is not null && _tokenInfo.Value.Expires > minExpires)
            return _tokenInfo.Value.AccessToken;

        await _semaphore.WaitAsync(cancellationToken);

        if (_tokenInfo is not null && _tokenInfo.Value.Expires > minExpires)
            return _tokenInfo.Value.AccessToken;

        try
        {
            _tokenInfo = await GetNewTokenAsync(cancellationToken);
            return _tokenInfo.Value.AccessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }

    private async Task<TokenInfo> GetNewTokenAsync(CancellationToken cancellationToken)
    {
        const string requestUri = "computeMetadata/v1/instance/service-accounts/default/token";

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = _options.ServiceUri;

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("Metadata-Flavor", "Google");

        HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken);

        if (resp.StatusCode is not HttpStatusCode.OK)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Unable to receive IAM service account token from Yandex Compute Cloud.");
            stringBuilder.AppendLine($"HTTP Status code: {resp.StatusCode:D}");

            string body = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(body) is false)
            {
                stringBuilder.AppendLine("HTTP Response:");
                stringBuilder.AppendLine(body);
            }

            throw new YandexCloudException(stringBuilder.ToString());
        }

        string content = await resp.Content.ReadAsStringAsync(cancellationToken);
        JObject? jsonContent = JsonConvert.DeserializeObject<JObject>(content);

        if (jsonContent is null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Unable to parse IAM service account token from Yandex Compute Cloud.");
            stringBuilder.AppendLine("Cannot parse JSON. Original response:");
            stringBuilder.AppendLine(content);

            throw new YandexCloudException(stringBuilder.ToString());
        }

        if (jsonContent.TryGetValue("access_token", StringComparison.Ordinal, out JToken? accessToken) is false
            || jsonContent.TryGetValue("expires_in", StringComparison.Ordinal, out JToken? expiresIn) is false)
        {
            var parseStringBuilder = new StringBuilder();
            parseStringBuilder.AppendLine("Unable to parse IAM service account token from Yandex Compute Cloud.");
            parseStringBuilder.AppendLine("Cannot find access token in original response:");
            parseStringBuilder.AppendLine(content);

            throw new YandexCloudException(parseStringBuilder.ToString());
        }

        var expires = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(expiresIn.Value<int>());

        return new TokenInfo(accessToken.ToString(), expires);
    }

    private record struct TokenInfo(string AccessToken, DateTimeOffset Expires);
}