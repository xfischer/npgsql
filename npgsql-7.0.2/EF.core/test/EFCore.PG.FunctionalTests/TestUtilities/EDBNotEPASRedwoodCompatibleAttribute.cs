namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

// EnterpriseDB Team
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class EDBNotEPASRedwoodCompatibleAttribute : Attribute, ITestCondition
{
    private readonly string _context;

    public EDBNotEPASRedwoodCompatibleAttribute(string context = null)
    {
        _context = context;
    }
    public ValueTask<bool> IsMetAsync() => new(TestEnvironment.IsVanillaPostgres);

    public string SkipReason => $"{(string.IsNullOrWhiteSpace(_context) ? "" : _context + " / ")} EPAS in redwood mode not supported";
}
