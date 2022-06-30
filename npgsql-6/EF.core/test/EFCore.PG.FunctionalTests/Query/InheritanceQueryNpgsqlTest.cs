using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class InheritanceQueryNpgsqlTest : InheritanceRelationalQueryTestBase<InheritanceQueryNpgsqlFixture>
{
    public InheritanceQueryNpgsqlTest(InheritanceQueryNpgsqlFixture fixture)
        : base(fixture)
    {
    }
}