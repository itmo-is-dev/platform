namespace Itmo.Dev.Platform.MessagePersistence.Tests.Models;

public sealed record TestMessage<TKey, TValue>(TKey Key, TValue Value);
