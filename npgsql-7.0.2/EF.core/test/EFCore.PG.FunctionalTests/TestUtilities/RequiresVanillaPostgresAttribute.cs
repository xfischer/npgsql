namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

// EnterpriseDB Team
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequiresVanillaPostgresAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync() => new(!TestEnvironment.IsRedwoodDbDialect);

    public string SkipReason => "Requires Postgres or EDB Postgres Advanced Server with postgres support (postgres db_dialect)";
}
