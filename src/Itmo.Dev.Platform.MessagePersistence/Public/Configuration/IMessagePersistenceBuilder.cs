using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence;

public static partial class MessagePersistenceConfiguration
{
    public static class General
    {
        public interface IDefaultPublisherStep
        {
            IPersistenceStep WithDefaultPublisherOptions(
                Action<OptionsBuilder<MessagePersistencePublisherOptions>> action);

            IPersistenceStep WithDefaultPublisherOptions(string sectionPath) => WithDefaultPublisherOptions(builder =>
            {
                builder.BindConfiguration(sectionPath);
            });
        }

        public interface IPersistenceStep
        {
            IBufferingGroupStep UsePersistenceConfigurator(IMessagePersistencePersistenceConfigurator configurator);
        }

        public interface IBufferingGroupStep : IMessagePersistenceBuilder
        {
            IBufferingGroupStep AddBufferingGroup(Func<Buffering.INameStep, Buffering.IFinalStep> action);
        }
    }
}

public interface IMessagePersistenceBuilder
{
    IMessagePersistenceBuilder AddMessage(Func<Config.Message.INameStep, Config.Message.IFinalStep> configuration);
}
