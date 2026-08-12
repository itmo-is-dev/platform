using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Platform.Options;

[OptionsType]
public sealed class SampleOptions
{
    [Range(1, 100)]
    public int Value { get; set; }
    
    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan? Time { get; set; }
}
