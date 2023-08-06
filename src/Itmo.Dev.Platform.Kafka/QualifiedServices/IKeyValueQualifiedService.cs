namespace Itmo.Dev.Platform.Kafka.QualifiedServices;

public interface IKeyValueQualifiedService<TKey, TValue, out TService> : IServiceResolver<TService> { }