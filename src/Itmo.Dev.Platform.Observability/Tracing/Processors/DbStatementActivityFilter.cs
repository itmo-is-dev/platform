using Itmo.Dev.Platform.Observability.Extensions;
using OpenTelemetry;
using System.Buffers;
using System.Diagnostics;

namespace Itmo.Dev.Platform.Observability.Tracing.Processors;

internal sealed class DbStatementActivityFilter : BaseProcessor<Activity>
{
    private const string DbStatementTagName = "db.statement";

    private static readonly SearchValues<string> IgnoredStatementParts = SearchValues.Create(
        [
            "information_schema",
            "\"public\".\"VersionInfo\"",
            "\"hangfire\".",
        ],
        StringComparison.OrdinalIgnoreCase);

    public override void OnEnd(Activity data)
    {
        if (data is not { OperationName: "postgres", Recorded: true })
            return;

        if (data.TryGetTag(DbStatementTagName, out var statement) is false)
            return;

        if (statement.AsSpan().ContainsAny(IgnoredStatementParts))
        {
            data.Suppress();
        }
    }
}
