namespace Itmo.Dev.Platform.MessagePersistence.Tests.Models;

public record TestMessage<TKey, TValue>(TKey Key, TValue Value);