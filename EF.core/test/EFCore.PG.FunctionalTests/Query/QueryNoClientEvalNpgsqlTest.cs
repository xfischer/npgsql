using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Query
{
    public class QueryNoClientEvalNpgsqlTest : QueryNoClientEvalTestBase<QueryNoClientEvalNpgsqlFixture>
    {
        public QueryNoClientEvalNpgsqlTest(QueryNoClientEvalNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
