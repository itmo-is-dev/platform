using System.Diagnostics;

namespace Itmo.Dev.Platform.MessagePersistence.Tools;

public static class PlatformMessagePersistenceActivitySource
{
    /// <summary>
    ///     Required to be const, as Observability package depends on compile-time const string inlining
    /// </summary>
    public const string Name = "Itmo.Dev.Platform.MessagePersistence";

    public static readonly ActivitySource Value = new(Name);
}
