using System;
using System.Text;
using System.Text.Json;
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
        => await AssertType(@"{""K"": ""V""}", @"{""K"": ""V""}", PostgresType, EDBDbType, isDefaultForWriting: false);

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
        await using var conn = await OpenConnectionAsync();
        await using var cmd = new EDBCommand($@"SELECT '{{""K"": ""V""}}'::{PostgresType}", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        reader.Read();
        using var textReader = await reader.GetTextReaderAsync(0);
        Assert.That(await textReader.ReadToEndAsync(), Is.EqualTo(@"{""K"": ""V""}"));
    }

    [Test]
    public async Task As_char_array()
        => await AssertType(@"{""K"": ""V""}".ToCharArray(), @"{""K"": ""V""}", PostgresType, EDBDbType, isDefault: false);

    [Test]
    public async Task As_bytes()
        => await AssertType(Encoding.ASCII.GetBytes(@"{""K"": ""V""}"), @"{""K"": ""V""}", PostgresType, EDBDbType, isDefault: false);

    [Test]
    public async Task Write_as_ArraySegment_of_char()
        => await AssertTypeWrite(
            new ArraySegment<char>(@"{""K"": ""V""}".ToCharArray()), @"{""K"": ""V""}", PostgresType, EDBDbType, isDefault: false);

    [Test]
    public async Task As_JsonDocument()
        => await AssertType(
            JsonDocument.Parse(@"{""K"": ""V""}"),
            IsJsonb ? @"{""K"": ""V""}" : @"{""K"":""V""}",
            PostgresType,
            EDBDbType,
            isDefault: false,
            comparer: (x, y) => x.RootElement.GetProperty("K").GetString() == y.RootElement.GetProperty("K").GetString());

    [Test]
    public async Task As_poco()
        => await AssertType(
            new WeatherForecast
            {
                Date = new DateTime(2019, 9, 1),
                Summary = "Partly cloudy",
                TemperatureC = 10
            },
            // Warning: in theory jsonb order and whitespace may change across versions
            IsJsonb
                ? @"{""Date"": ""2019-09-01T00:00:00"", ""Summary"": ""Partly cloudy"", ""TemperatureC"": 10}"
                : @"{""Date"":""2019-09-01T00:00:00"",""TemperatureC"":10,""Summary"":""Partly cloudy""}",
            PostgresType,
            EDBDbType,
            isDefault: false);

    [Test]
    public async Task As_poco_long()
    {
        using var conn = CreateConnection();
        var bigString = new string('x', Math.Max(conn.Settings.ReadBufferSize, conn.Settings.WriteBufferSize));

        await AssertType(
            new WeatherForecast
            {
                Date = new DateTime(2019, 9, 1),
                Summary = bigString,
                TemperatureC = 10
            },
            // Warning: in theory jsonb order and whitespace may change across versions
            IsJsonb
                ? @"{""Date"": ""2019-09-01T00:00:00"", ""Summary"": """ + bigString + @""", ""TemperatureC"": 10}"
                : @"{""Date"":""2019-09-01T00:00:00"",""TemperatureC"":10,""Summary"":""" + bigString + @"""}",
            PostgresType,
            EDBDbType,
            isDefault: false);
    }

    record WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; } = "";
    }

    [Test]
    [IssueLink("https://github.com/npgsql/npgsql/issues/2811")]
    [IssueLink("https://github.com/npgsql/efcore.pg/issues/1177")]
    [IssueLink("https://github.com/npgsql/efcore.pg/issues/1082")]
    public async Task Can_read_two_json_documents()
    {
        using var conn = await OpenConnectionAsync();

        JsonDocument car;
        using (var cmd = new EDBCommand(@"SELECT '{""key"" : ""foo""}'::jsonb", conn))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            reader.Read();
            car = reader.GetFieldValue<JsonDocument>(0);
        }

        using (var cmd = new EDBCommand(@"SELECT '{""key"" : ""bar""}'::jsonb", conn))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            reader.Read();
            reader.GetFieldValue<JsonDocument>(0);
        }

        Assert.That(car.RootElement.GetProperty("key").GetString(), Is.EqualTo("foo"));
    }

    public JsonTests(MultiplexingMode multiplexingMode, EDBDbType npgsqlDbType)
        : base(multiplexingMode)
    {
        using (var conn = OpenConnection())
            TestUtil.MinimumPgVersion(conn, "9.4.0", "JSONB data type not yet introduced");
        EDBDbType = npgsqlDbType;
    }

    bool IsJsonb => EDBDbType == EDBDbType.Jsonb;
    string PostgresType => IsJsonb ? "jsonb" : "json";
    readonly EDBDbType EDBDbType;
}
