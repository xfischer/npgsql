using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query
{
    public class TPTInheritanceQueryNpgsqlTest : TPTInheritanceQueryTestBase<TPTInheritanceQueryNpgsqlFixture>
    {
        public TPTInheritanceQueryNpgsqlTest(TPTInheritanceQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
