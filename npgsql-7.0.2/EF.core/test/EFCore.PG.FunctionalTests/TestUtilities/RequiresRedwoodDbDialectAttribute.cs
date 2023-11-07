namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

// EnterpriseDB Team
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequiresRedwoodDbDialectAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync() => new(TestEnvironment.IsRedwoodDbDialect);

    public string SkipReason => "Requires EDB Postgres Advanced Server with Oracle support (redwood db_dialect)";
}
