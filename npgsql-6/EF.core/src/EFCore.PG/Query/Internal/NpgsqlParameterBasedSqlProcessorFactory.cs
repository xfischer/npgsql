using Microsoft.EntityFrameworkCore.Query;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query.Internal;

public class EDBParameterBasedSqlProcessorFactory : IRelationalParameterBasedSqlProcessorFactory
{
    private readonly RelationalParameterBasedSqlProcessorDependencies _dependencies;

    public EDBParameterBasedSqlProcessorFactory(
        RelationalParameterBasedSqlProcessorDependencies dependencies)
        => _dependencies = dependencies;

    public virtual RelationalParameterBasedSqlProcessor Create(bool useRelationalNulls)
        => new EDBParameterBasedSqlProcessor(_dependencies, useRelationalNulls);
}