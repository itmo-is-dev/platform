using Itmo.Dev.Platform.YandexCloud.Exceptions;
using Itmo.Dev.Platform.YandexCloud.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Itmo.Dev.Platform.YandexCloud.Services;

internal class YandexCloudLockBoxService
{
    private readonly YandexCloudTokenProvider _tokenProvider;

    public YandexCloudLockBoxService(YandexCloudTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    internal async Task<LockBoxEntry[]> GetEntries(string secretId, CancellationToken cancellationToken)
    {
        const string baseUrl = "https://payload.lockbox.api.cloud.yandex.net/";
        string requestUri = $"/lockbox/v1/secrets/{secretId}/payload";

        var token = await _tokenProvider.GetTokenAsync(cancellationToken);

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(baseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        HttpResponseMessage resp = await httpClient.SendAsync(request, cancellationToken);

        if (resp.StatusCode is not HttpStatusCode.OK)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Unable to receive secrets from Yandex LockBox.");
            stringBuilder.AppendLine($"HTTP Status code: {resp.StatusCode:D}");

            string body = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(body) is false)
            {
                stringBuilder.AppendLine("HTTP Response:");
                stringBuilder.AppendLine(body);
            }

            throw new YandexCloudException(stringBuilder.ToString());
        }

        string respBody = await resp.Content.ReadAsStringAsync(cancellationToken);

        try
        {
            LockBoxEntry[]? entries = JsonConvert
                .DeserializeObject<JObject>(respBody)?
                .GetValue("entries", StringComparison.Ordinal)?
                .ToObject<LockBoxEntry[]>();

            if (entries?.Any() is not true)
            {
                throw new YandexCloudException("Secrets cannot be null or empty");
            }

            return entries;
        }
        catch
        {
            throw new YandexCloudException("Yandex LockBox have not returned any secrets");
        }
    }
}