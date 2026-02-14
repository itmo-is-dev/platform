using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence;

public interface IMessagePersistencePersistenceConfigurator
{
    void Apply(IServiceCollection collection);
}