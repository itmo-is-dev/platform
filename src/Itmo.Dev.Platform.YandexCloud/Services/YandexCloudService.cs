using System.Net;
using System.Text;
using Itmo.Dev.Platform.YandexCloud.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Itmo.Dev.Platform.YandexCloud.Services;

internal class YandexCloudService
{
    private readonly Uri _baseUri;

    public YandexCloudService(IConfiguration configuration)
    {
        var uri = configuration.GetSection("Platform:YandexCloud:InstanceUri");
        _baseUri = new Uri(uri.Value ?? string.Empty);
    }

    public async Task<string> GetTokenAsync()
    {
        const string requestUri = "computeMetadata/v1/instance/service-accounts/default/token";

        using var httpClient = new HttpClient { BaseAddress = _baseUri };

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri)
        {
            Headers =
            {
                { "Metadata-Flavor", "Google" },
            },
        };

        HttpResponseMessage resp = await httpClient.SendAsync(request);

        if (resp.StatusCode is not HttpStatusCode.OK)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Unable to receive IAM service account token from Yandex Compute Cloud.");
            stringBuilder.AppendLine($"HTTP Status code: {resp.StatusCode:D)}");
            string body = await resp.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body) is false)
            {
                stringBuilder.AppendLine("HTTP Response:");
                stringBuilder.AppendLine(body);
            }

            throw new YandexCloudException(stringBuilder.ToString());
        }

        string content = await resp.Content.ReadAsStringAsync();
        JObject? jsonContent = JsonConvert.DeserializeObject<JObject>(content);

        if (jsonContent is null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Unable to parse IAM service account token from Yandex Compute Cloud.");
            stringBuilder.AppendLine("Cannot parse JSON. Original response:");
            stringBuilder.AppendLine(content);

            throw new YandexCloudException(stringBuilder.ToString());
        }

        if (jsonContent.TryGetValue("access_token", StringComparison.Ordinal, out JToken? accessToken))
        {
            return accessToken.ToString();
        }

        var parseStringBuilder = new StringBuilder();
        parseStringBuilder.AppendLine("Unable to parse IAM service account token from Yandex Compute Cloud.");
        parseStringBuilder.AppendLine("Cannot find access token in original response:");
        parseStringBuilder.AppendLine(content);

        throw new YandexCloudException(parseStringBuilder.ToString());
    }
}