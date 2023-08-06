namespace Itmo.Dev.Platform.Kafka.QualifiedServices;

public interface IServiceResolver<out T>
{
    T Resolve(IServiceProvider provider);
}