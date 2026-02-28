using System.Diagnostics.CodeAnalysis;

namespace Itmo.Dev.Platform.Common.Tools;

public readonly record struct Optional<T>(
    T? Value,
    [property: MemberNotNullWhen(true, nameof(Optional<>.Value))]
    bool HasValue)
{
    public static implicit operator Optional<T?>(T? value) => Optional.Some(value);
}

public static class Optional
{
    public static Optional<T> None<T>() => new(default, false);

    public static Optional<T> Some<T>(T value) => new(value, true);
}
