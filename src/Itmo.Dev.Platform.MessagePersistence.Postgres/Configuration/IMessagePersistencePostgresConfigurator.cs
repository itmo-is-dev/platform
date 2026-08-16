using Itmo.Dev.Platform.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Configuration;

public static partial class MessagePersistencePostgresConfiguration
{
    public interface IOptionsStep
    {
        IFinalStep ConfigureOptions(
            Action<OptionsBuilder<MessagePersistencePostgresOptions>> action);

        [ProducesOptionRegistration<MessagePersistencePostgresOptions>(SectionParameterName = nameof(sectionPath))]
        IFinalStep ConfigureOptions(string sectionPath)
            => ConfigureOptions(builder => builder.BindConfiguration(sectionPath));
    }

    public interface IFinalStep;
}
