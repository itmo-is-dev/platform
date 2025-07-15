namespace Itmo.Dev.Platform.Kafka.Tests.Tools;

public record TestMessage<TKey, TValue>(TKey Key, TValue Value);