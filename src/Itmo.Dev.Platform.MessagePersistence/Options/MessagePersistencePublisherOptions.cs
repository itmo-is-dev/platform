using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.MessagePersistence.Options;

public class MessagePersistencePublisherOptions
{
    internal bool IsInitialized { get; set; }
    
    internal List<string> MessageNames { get; set; } = [];

    [Range(minimum: 1, maximum: int.MaxValue)]
    public int BatchSize { get; set; }

    [Range(typeof(TimeSpan), minimum: "00:00:00.500", maximum: "23:59:59")]
    public TimeSpan PollingDelay { get; set; }
}
