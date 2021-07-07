using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlMigrationBuilderTest
    {
        [Fact]
        public void IsNpgsql_when_using_Npgsql()
        {
            var migrationBuilder = new MigrationBuilder("EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL");
            Assert.True(migrationBuilder.IsNpgsql());
        }

        [Fact]
        public void Not_IsNpgsql_when_using_different_provider()
        {
            var migrationBuilder = new MigrationBuilder("Microsoft.EntityFrameworkCore.InMemory");
            Assert.False(migrationBuilder.IsNpgsql());
        }
    }
}
