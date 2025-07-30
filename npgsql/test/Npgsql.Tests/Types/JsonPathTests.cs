using System.Data;
using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;
using static EnterpriseDB.EDBClient.Tests.TestUtil;

namespace EnterpriseDB.EDBClient.Tests.Types;

public class JsonPathTests(MultiplexingMode multiplexingMode) : MultiplexingTestBase(multiplexingMode)
{
    static readonly object[] ReadWriteCases =
    [
        new object[] { "'$'", "$" },
        new object[] { "'$\"varname\"'", "$\"varname\"" }
    ];

    [Test]
    [TestCase("$")]
    [TestCase("$\"varname\"")]
    public async Task JsonPath(string jsonPath)
    {
        using var conn = await OpenConnectionAsync();
        MinimumPgVersion(conn, "12.0", "The jsonpath type was introduced in PostgreSQL 12");
        await AssertType(
            jsonPath, jsonPath, "jsonpath", EDBDbType.JsonPath, isDefaultForWriting: false, isEDBDbTypeInferredFromClrType: false,
            inferredDbType: DbType.Object);
    }

    [Test]
    [TestCaseSource(nameof(ReadWriteCases))]
    public async Task Read(string query, string expected)
    {
        using var conn = await OpenConnectionAsync();
        MinimumPgVersion(conn, "12.0", "The jsonpath type was introduced in PostgreSQL 12");

        using var cmd = new EDBCommand($"SELECT {query}::jsonpath", conn);
        using var rdr = await cmd.ExecuteReaderAsync();

        rdr.Read();
        Assert.That(rdr.GetFieldValue<string>(0), Is.EqualTo(expected));
        Assert.That(rdr.GetTextReader(0).ReadToEnd(), Is.EqualTo(expected));
    }

    [Test]
    [TestCaseSource(nameof(ReadWriteCases))]
    public async Task Write(string query, string expected)
    {
        using var conn = await OpenConnectionAsync();
        MinimumPgVersion(conn, "12.0", "The jsonpath type was introduced in PostgreSQL 12");

        using var cmd = new EDBCommand($"SELECT 'Passed' WHERE @p::text = {query}::text", conn) { Parameters = { new EDBParameter("p", EDBDbType.JsonPath) { Value = expected } } };
        using var rdr = await cmd.ExecuteReaderAsync();

        Assert.True(rdr.Read());
    }
}