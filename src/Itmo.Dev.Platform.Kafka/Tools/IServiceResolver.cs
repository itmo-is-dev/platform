namespace Itmo.Dev.Platform.Kafka.Tools;

public interface IServiceResolver<out T>
{
    T Resolve(IServiceProvider provider);
}