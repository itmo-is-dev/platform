using Microsoft.Build.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Platform.Options.MSBuild.Extensions;

public static class LoggingExtensions
{
    private static readonly AsyncLocal<bool> UseDebugLoggingValue = new();
    private static readonly AsyncLocal<string?> DebugScopeValue = new();

    public static void LogDebugMessage(
        this TaskLoggingHelper log,
        [StringSyntax("CompositeFormat")] string template,
        params object?[] args)
    {
        if (UseDebugLoggingValue.Value is false)
            return;

        template = DebugScopeValue.Value is null
            ? template
            : $"[{DebugScopeValue.Value}] {template}";

        log.LogWarning(template, args);
    }

    public static UseDebugScopeDisposable UseDebugScope(this TaskLoggingHelper log, string scope)
    {
        var previous = DebugScopeValue.Value;
        DebugScopeValue.Value = previous is null ? scope : $"{previous}:{scope}";
        return new UseDebugScopeDisposable(previous);
    }

    public static UseDebugLoggingDisposable UseDebugLogging(bool isEnabled)
    {
        UseDebugLoggingValue.Value = isEnabled;
        return new UseDebugLoggingDisposable();
    }

    public readonly record struct UseDebugLoggingDisposable : IDisposable
    {
        public void Dispose()
        {
            UseDebugLoggingValue.Value = false;
        }
    }

    public readonly record struct UseDebugScopeDisposable(string? Previous) : IDisposable
    {
        public void Dispose()
        {
            DebugScopeValue.Value = Previous;
        }
    }
}
