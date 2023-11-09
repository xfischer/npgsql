using System.Runtime.InteropServices;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

// EnterpriseDB Team
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class EDBRequiresRedwoodDbDialectAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync() => new(TestEnvironment.IsRedwoodDbDialect);

    public string SkipReason => "Requires EDB Postgres Advanced Server with Oracle support (redwood db_dialect)";
}


public sealed class IgnoreOnEPASFact : FactAttribute
{
    public IgnoreOnEPASFact()
    {
        if (TestEnvironment.IsVanillaPostgres)
        {
            Skip = "Ignore on Linux when run via AppVeyor";
        }
    }
}

