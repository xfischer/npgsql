using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types;

[TestFixture(MultiplexingMode.NonMultiplexing, EDBDbType.Json)]
[TestFixture(MultiplexingMode.NonMultiplexing, EDBDbType.Jsonb)]
[TestFixture(MultiplexingMode.Multiplexing, EDBDbType.Json)]
[TestFixture(MultiplexingMode.Multiplexing, EDBDbType.Jsonb)]
public class JsonTests : MultiplexingTestBase
{
    [Test]
    public async Task As_string()
        => await AssertType("""{"K": "V"}""", """{"K": "V"}""", PostgresType, EDBDbType, isDefaultForWriting: false);

    [Test]
    public async Task As_string_long()
    {
        await using var conn = CreateConnection();

        var value = new StringBuilder()
            .Append(@"{""K"": """)
            .Append('x', conn.Settings.WriteBufferSize)
            .Append(@"""}")
            .ToString();

        await AssertType(value, value, PostgresType, EDBDbType, isDefaultForWriting: false);
    }

    [Test]
    public async Task As_string_with_GetTextReader()
    {
        // EnterpriseDB force datasource without opt-ins
        using var ds = CreateDataSource(b => b.DisableDynamicJson());
        using var conn = await ds.OpenConnectionAsync();
        await using var cmd = new EDBCommand($$"""SELECT '{"K": "V"}'::{{PostgresType}}""", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        reader.Read();
        using var textReader = await reader.GetTextReaderAsync(0);
        Assert.That(await textReader.ReadToEndAsync(), Is.EqualTo(@"{""K"": ""V""}"));
    }

    [Test]
    public async Task As_char_array()
        => await AssertType("""{"K": "V"}""".ToCharArray(), """{"K": "V"}""", PostgresType, EDBDbType, isDefault: false);

    [Test]
    public async Task As_bytes()
        => await AssertType("""{"K": "V"}"""u8.ToArray(), """{"K": "V"}""", PostgresType, EDBDbType, isDefault: false);

    [Test]
    public async Task Write_as_ReadOnlyMemory_of_byte()
    {
        // EnterpriseDB force datasource without opt-ins
        using var ds = CreateDataSource(b => b.DisableDynamicJson());
        using var con = await ds.OpenConnectionAsync();
        await AssertTypeWrite(connection: con, () => new ReadOnlyMemory<byte>("""{"K": "V"}"""u8.ToArray()), """{"K": "V"}""", PostgresType, EDBDbType,
            isDefault: false);
    }

    [Test]
    public async Task Write_as_ArraySegment_of_char()
    {
        // EnterpriseDB force datasource without opt-ins
        using var ds = CreateDataSource(b => b.DisableDynamicJson());
        using var con = await ds.OpenConnectionAsync();
        await AssertTypeWrite(connection: con, () => new ArraySegment<char>("""{"K": "V"}""".ToCharArray()), """{"K": "V"}""", PostgresType, EDBDbType,
            isDefault: false);
    }

    [Test]
    public async Task As_MemoryStream()
    {
        // EnterpriseDB force datasource without opt-ins
        using var ds = CreateDataSource(b => b.DisableDynamicJson());
        using var con = await ds.OpenConnectionAsync();
        await AssertTypeWrite(connection: con, () => new MemoryStream("""{"K": "V"}"""u8.ToArray()), """{"K": "V"}""", PostgresType, EDBDbType, isDefault: false);
    }

    [Test]
    public async Task As_JsonDocument()
        => await AssertType(
            JsonDocument.Parse("""{"K": "V"}"""),
            IsJsonb ? """{"K": "V"}""" : """{"K":"V"}""",
            PostgresType,
            EDBDbType,
            isDefault: false,
            comparer: (x, y) => x.RootElement.GetProperty("K").GetString() == y.RootElement.GetProperty("K").GetString());

    [Test, IssueLink("https://github.com/npgsql/npgsql/issues/5540")]
    public async Task As_JsonDocument_with_null_root()
        => await AssertType(
            JsonDocument.Parse("null"),
            "null",
            PostgresType,
            EDBDbType,
            isDefault: false,
            comparer: (x, y) => x.RootElement.ValueKind == y.RootElement.ValueKind,
            skipArrayCheck: true);

    [Test]
    public async Task As_JsonElement_with_null_root()
        => await AssertType(
            JsonDocument.Parse("null").RootElement,
            "null",
            PostgresType,
            EDBDbType,
            isDefault: false,
            comparer: (x, y) => x.ValueKind == y.ValueKind,
            skipArrayCheck: true);

    [Test]
    public async Task As_JsonDocument_supported_only_with_SystemTextJson()
    {
        await using var slimDataSource = new EDBSlimDataSourceBuilder(ConnectionString).Build();

        await AssertTypeUnsupported(
            JsonDocument.Parse("""{"K": "V"}"""),
            """{"K": "V"}""",
            PostgresType,
            slimDataSource);
    }

    [Test]
    public Task Roundtrip_string()
        => AssertType(
            @"{""p"": 1}",
            @"{""p"": 1}",
            PostgresType,
            EDBDbType,
            isDefault: false,
            isEDBDbTypeInferredFromClrType: false);

    [Test]
    public Task Roundtrip_char_array()
        => AssertType(
            @"{""p"": 1}".ToCharArray(),
            @"{""p"": 1}",
            PostgresType,
            EDBDbType,
            isDefault: false,
            isEDBDbTypeInferredFromClrType: false);

    [Test]
    public Task Roundtrip_byte_array()
        => AssertType(
            Encoding.ASCII.GetBytes(@"{""p"": 1}"),
            @"{""p"": 1}",
            PostgresType,
            EDBDbType,
            isDefault: false,
            isEDBDbTypeInferredFromClrType: false);

    [Test]
    [IssueLink("https://github.com/npgsql/npgsql/issues/2811")]
    [IssueLink("https://github.com/npgsql/efcore.pg/issues/1177")]
    [IssueLink("https://github.com/npgsql/efcore.pg/issues/1082")]
    public async Task Can_read_two_json_documents()
    {
        await using var conn = await OpenConnectionAsync();

        JsonDocument car;
        await using (var cmd = new EDBCommand("""SELECT '{"key" : "foo"}'::jsonb""", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            reader.Read();
            car = reader.GetFieldValue<JsonDocument>(0);
        }

        await using (var cmd = new EDBCommand("""SELECT '{"key" : "bar"}'::jsonb""", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            reader.Read();
            reader.GetFieldValue<JsonDocument>(0);
        }

        Assert.That(car.RootElement.GetProperty("key").GetString(), Is.EqualTo("foo"));
    }

    [Test]
    public Task Roundtrip_JsonObject()
        => AssertType(
            new JsonObject { ["Bar"] = 8 },
            IsJsonb ? """{"Bar": 8}""" : """{"Bar":8}""",
            PostgresType,
            EDBDbType,
            // By default we map JsonObject to jsonb
            isDefaultForWriting: IsJsonb,
            isDefaultForReading: false,
            isEDBDbTypeInferredFromClrType: false,
            comparer: (x, y) => x.ToString() == y.ToString());

    [Test]
    public Task Roundtrip_JsonArray()
        => AssertType(
#if NET8_0
            // Necessary until we drop STJ 8.0, see https://github.com/dotnet/runtime/pull/103733
            new JsonArray { (JsonValue)1, (JsonValue)2, (JsonValue)3 },
#else
            new JsonArray { 1, 2, 3 },
#endif
            IsJsonb ? "[1, 2, 3]" : "[1,2,3]",
            PostgresType,
            EDBDbType,
            // By default we map JsonArray to jsonb
            isDefaultForWriting: IsJsonb,
            isDefaultForReading: false,
            isEDBDbTypeInferredFromClrType: false,
            comparer: (x, y) => x.ToString() == y.ToString());

    [Test]
    [IssueLink("https://github.com/npgsql/npgsql/issues/4537")]
    public async Task Write_jsonobject_array_without_npgsqldbtype()
    {
        // By default we map JsonObject to jsonb
        if (!IsJsonb)
            return;

        await using var conn = await OpenConnectionAsync();
        var tableName = await TestUtil.CreateTempTable(conn, "key SERIAL PRIMARY KEY, ingredients json[]");

        await using var cmd = new EDBCommand { Connection = conn };

        var jsonObject1 = new JsonObject
        {
            { "name", "value1" },
            { "amount", 1 },
            { "unit", "ml" }
        };

        var jsonObject2 = new JsonObject
        {
            { "name", "value2" },
            { "amount", 2 },
            { "unit", "g" }
        };

        cmd.CommandText = $"INSERT INTO {tableName} (ingredients) VALUES (@p)";
        cmd.Parameters.Add(new("p", new[] { jsonObject1, jsonObject2 }));
        await cmd.ExecuteNonQueryAsync();
    }

    public JsonTests(MultiplexingMode multiplexingMode, EDBDbType npgsqlDbType)
        : base(multiplexingMode)
    {
        if (npgsqlDbType == EDBDbType.Jsonb)
            using (var conn = OpenConnection())
                TestUtil.MinimumPgVersion(conn, "9.4.0", "JSONB data type not yet introduced");

        EDBDbType = npgsqlDbType;
    }

    bool IsJsonb => EDBDbType == EDBDbType.Jsonb;
    string PostgresType => IsJsonb ? "jsonb" : "json";
    readonly EDBDbType EDBDbType;
}
