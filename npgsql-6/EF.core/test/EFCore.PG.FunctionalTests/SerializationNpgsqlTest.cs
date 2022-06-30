using Microsoft.EntityFrameworkCore;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL;

public class SerializationNpgsqlTest : SerializationTestBase<F1NpgsqlFixture>
{
    public SerializationNpgsqlTest(F1NpgsqlFixture fixture)
        : base(fixture)
    {
    }
}