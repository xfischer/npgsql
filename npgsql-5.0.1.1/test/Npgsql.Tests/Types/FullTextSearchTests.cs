using System;
using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types
{
    public class FullTextSearchTests : MultiplexingTestBase
    {
        public FullTextSearchTests(MultiplexingMode multiplexingMode)
            : base(multiplexingMode) { }

        [Test]
        public async Task TsVector()
        {
            using (var conn = await OpenConnectionAsync())
            using (var cmd = conn.CreateCommand())
            {
                var inputVec = EDBTsVector.Parse(" a:12345C  a:24D a:25B b c d 1 2 a:25A,26B,27,28");

                cmd.CommandText = "Select :p";
                cmd.Parameters.AddWithValue("p", inputVec);
                var outputVec = await cmd.ExecuteScalarAsync();
                Assert.AreEqual(inputVec.ToString(), outputVec!.ToString());
            }
        }

        [Test]
        public async Task TsQuery()
        {
            using var conn = await OpenConnectionAsync();
            var queryString = conn.PostgreSqlVersion < new Version(9, 6)
                ? "'a' & !( 'c' | 'd' ) & !!'a' & 'b' | 'ä' | 'f'"
                : "'a' & !( 'c' | 'd' ) & !!'a' & 'b' | 'ä' | 'x' <-> 'y' | 'x' <10> 'y' | 'd' <0> 'e' | 'f'";
            var query = EDBTsQuery.Parse(queryString);

            using var cmd = new EDBCommand("SELECT @s::tsquery, @q, @q::text", conn)
            {
                Parameters =
                {
                    new EDBParameter("s", queryString),
                    new EDBParameter("q", query),
                }
            };

            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            Assert.AreEqual(query.ToString(), reader.GetFieldValue<EDBTsQuery>(0).ToString());
            Assert.AreEqual(query.ToString(), reader.GetFieldValue<EDBTsQuery>(1).ToString());
            Assert.AreEqual(queryString, reader.GetFieldValue<string>(2));
        }
    }
}
