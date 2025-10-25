using Itmo.Dev.Platform.Observability.Tests.Startup.Models;
using Newtonsoft.Json;
using OpenTelemetry;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Observability.Tests.Startup.Tools;

public class TestingActivityProcessor : BaseProcessor<Activity>
{
    private readonly List<Activity> _activities = [];

    public IReadOnlyCollection<Activity> Activities => _activities;

    public override void OnStart(Activity data)
    {
        var spanInfo = new SpanInfo(
            data.SpanId.ToString(),
            data);

        File.AppendAllText(
            Environment.GetEnvironmentVariable(TestingConstants.SpanIdFileVariableName) ?? string.Empty,
            JsonConvert.SerializeObject(spanInfo) + Environment.NewLine);

        _activities.Add(data);
    }
}
