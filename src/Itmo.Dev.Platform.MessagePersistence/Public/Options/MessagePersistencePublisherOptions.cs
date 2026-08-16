using Itmo.Dev.Platform.Options;
using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.MessagePersistence.Options;

[OptionsType]
public class MessagePersistencePublisherOptions
{
    internal bool IsInitialized { get; set; }
    
    internal List<string> MessageNames { get; set; } = [];

    [Range(minimum: 1, maximum: int.MaxValue)]
    public int BatchSize { get; set; }

    [Range(typeof(TimeSpan), minimum: "00:00:00.050", maximum: "23:59:59")]
    public TimeSpan PollingDelay { get; set; }
}
