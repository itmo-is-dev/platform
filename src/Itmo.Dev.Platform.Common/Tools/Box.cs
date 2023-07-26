namespace Itmo.Dev.Platform.Common.Tools;

internal class Box<T>
{
    public Box(T value)
    {
        Value = value;
    }

    public T Value { get; set; }
}