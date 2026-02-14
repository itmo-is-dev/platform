using Itmo.Dev.Platform.MessagePersistence.Internal.Options;
using Itmo.Dev.Platform.MessagePersistence.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence;

public static partial class MessagePersistenceConfiguration
{
    public static class Buffering
    {
        public interface INameStep
        {
            IConfigurationStep Called(string name);
        }

        public interface IConfigurationStep
        {
            IBufferingStepStep WithPublisherConfiguration(
                Action<OptionsBuilder<MessagePersistencePublisherOptions>> action);

            IBufferingStepStep WithPublisherConfiguration(string sectionName)
            {
                return WithPublisherConfiguration(builder => builder.BindConfiguration(
                    sectionName,
                    binder => binder.BindNonPublicProperties = false));
            }
        }

        public interface IBufferingStepStep : IFinalStep
        {
            internal IServiceCollection Services { get; }

            internal string BufferGroupName { get; }

            internal IBufferingStepStep WithStep(BufferStepOptions step);
        }

        public interface IFinalStep;
    }
}
