using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Platform.MessagePersistence.Configuration.General;

public interface IMessagePersistencePersistenceConfigurator
{
    void Apply(IServiceCollection collection);
}