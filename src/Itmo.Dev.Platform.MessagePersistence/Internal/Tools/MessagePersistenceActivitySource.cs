using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Itmo.Dev.Platform.MessagePersistence.Internal.Tools;

internal static class MessagePersistenceActivitySource
{
    /// <summary>
    ///     Required to be const, as Observability package depends on compile-time const string inlining
    /// </summary>
    public const string Name = "Itmo.Dev.Platform.MessagePersistence";

    public static readonly ActivitySource Value = new(Name);
    public static readonly Meter Meter = new(Name);
}
