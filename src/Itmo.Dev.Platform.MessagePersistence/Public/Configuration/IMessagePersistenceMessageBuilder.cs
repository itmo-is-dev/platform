using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence;

public static partial class MessagePersistenceConfiguration
{
    public static partial class Message
    {
        public interface INameStep
        {
            IConfigurationStep Called(string messageName);
        }

        public interface IConfigurationStep
        {
            IMessageStep WithConfiguration(Action<OptionsBuilder<MessagePersistenceHandlerOptions>> action);

            IMessageStep WithConfiguration(
                IConfiguration configuration,
                Action<MessagePersistenceHandlerOptions>? options = null) => WithConfiguration(builder =>
            {
                builder.Bind(configuration);

                if (options is not null)
                    builder.Configure(options);
            });

            IMessageStep WithConfiguration(string sectionPath) => WithConfiguration(builder =>
                builder.BindConfiguration(sectionPath));
        }

        public interface IMessageStep
        {
            IHandlerStep<TMessage> WithMessage<TMessage>()
                where TMessage : IPersistedMessage<TMessage>;
        }

        public interface IHandlerStep<TMessage>
            where TMessage : IPersistedMessage
        {
            IBufferingGroupStep HandleBy<THandler>()
                where THandler : class, IPersistedMessageHandler<TMessage>;

            IBufferingGroupStep HandleBy<THandler>(
                Func<IServiceProvider, string, THandler> implementationFactory)
                where THandler : class, IPersistedMessageHandler<TMessage>;
        }

        public interface IBufferingGroupStep : IFinalStep
        {
            IFinalStep WithBufferingGroup(string bufferingGroupName);
        }

        public interface IFinalStep
        {
            void Build();
        }
    }
}
