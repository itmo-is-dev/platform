using System.Diagnostics;

namespace Itmo.Dev.Platform.Kafka.Tools;

internal static class PlatformKafkaActivitySource
{
    public const string Name = "Itmo.Dev.Platform.Kafka";

    public static readonly ActivitySource Value = new(Name);
}
