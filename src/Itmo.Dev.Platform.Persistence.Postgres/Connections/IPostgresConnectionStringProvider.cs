namespace Itmo.Dev.Platform.Persistence.Postgres.Connections;

public interface IPostgresConnectionStringProvider
{
    string GetConnectionString();
}