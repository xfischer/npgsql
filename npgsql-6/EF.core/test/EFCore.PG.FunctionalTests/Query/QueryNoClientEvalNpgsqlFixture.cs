using Microsoft.EntityFrameworkCore.TestUtilities;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query;

public class QueryNoClientEvalNpgsqlFixture : NorthwindQueryNpgsqlFixture<NoopModelCustomizer>
{
}