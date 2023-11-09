namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

// EnterpriseDB Team
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class EDBRequiresVanillaPostgresAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync() => new(TestEnvironment.IsVanillaPostgres);

    public string SkipReason => "EPAS in redwood mode not supported - requires Postgres or EDB Postgres Advanced Server with postgres support (postgres db_dialect)";
}
