using Itmo.Dev.Platform.Postgres.Plugins;
using Npgsql;

namespace Itmo.Dev.Platform.MessagePersistence.Persistence.Plugins;

internal class MappingPlugin : IDataSourcePlugin
{
    public void Configure(NpgsqlDataSourceBuilder builder)
    {
        builder.MapEnum<MessageState>("persisted_message_state");
    }
}