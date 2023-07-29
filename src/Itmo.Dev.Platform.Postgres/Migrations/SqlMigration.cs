using FluentMigrator;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace Itmo.Dev.Platform.Postgres.Migrations;

public abstract class SqlMigration : IMigration
{
    public void GetUpExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = GetUpSql(context.ServiceProvider),
        });
    }

    public void GetDownExpressions(IMigrationContext context)
    {
        context.Expressions.Add(new ExecuteSqlStatementExpression
        {
            SqlStatement = GetDownSql(context.ServiceProvider),
        });
    }

    public object ApplicationContext => throw new NotSupportedException();

    public string ConnectionString => throw new NotSupportedException();

    protected abstract string GetUpSql(IServiceProvider serviceProvider);

    protected abstract string GetDownSql(IServiceProvider serviceProvider);
}