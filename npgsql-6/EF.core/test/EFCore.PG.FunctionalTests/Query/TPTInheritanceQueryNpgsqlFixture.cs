using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class TPTInheritanceQueryNpgsqlFixture : TPTInheritanceQueryFixture
{
    protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
}