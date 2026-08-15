namespace Itmo.Dev.Platform.Options.Samples;

[OptionsType]
public sealed class SomeOptions
{
    public int Value { get; set; }
    
    public required string OtherValue { get; set; }
}
