using Itmo.Dev.Platform.MessagePersistence.Internal.Models;
using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Npgsql;

namespace Itmo.Dev.Platform.MessagePersistence.Postgres.Plugins;

internal class MappingPlugin : IPostgresDataSourcePlugin
{
    public void Configure(NpgsqlDataSourceBuilder builder)
    {
        builder.MapEnum<MessageState>("persisted_message_state");
    }
}