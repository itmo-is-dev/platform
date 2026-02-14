namespace Itmo.Dev.Platform.MessagePersistence;

public readonly record struct PayloadVersion(long Value)
{
    public static readonly PayloadVersion Default = new(0);
    
    public static implicit operator PayloadVersion(long value) => new(value);
}
