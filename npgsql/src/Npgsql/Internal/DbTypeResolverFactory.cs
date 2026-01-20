using System.Diagnostics.CodeAnalysis;

namespace EnterpriseDB.EDBClient.Internal;

[Experimental(EDBDiagnostics.DbTypeResolverExperimental)]
public abstract class DbTypeResolverFactory
{
    public abstract IDbTypeResolver CreateDbTypeResolver(EDBDatabaseInfo databaseInfo);
}
