using System.Runtime.CompilerServices;

namespace Itmo.Dev.Platform.Kafka.Tools;

public static class TestConsumerGroup
{
    public static string GetName(
        object? postfix = null,
        [CallerFilePath] string? callerFile = null,
        [CallerMemberName] string? callerMethod = null)
    {
        var className = Path.GetFileNameWithoutExtension(callerFile);
        return $"{className}_{callerMethod}_{postfix?.ToString() ?? "validate"}";
    }
}
