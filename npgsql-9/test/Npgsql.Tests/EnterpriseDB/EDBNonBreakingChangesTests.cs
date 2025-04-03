using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types;


// EnterpriseDB : Tests added after default AOT optins added (see EC-3060)
public class EDBNonBreakingChangesTests : TestBase
{
    [OneTimeSetUp]
    public async Task Init()
    {
        using var con = new EDBConnection(TestUtil.ConnectionString);
        await con.OpenAsync();
        await CreateSampleTableAsync(con);
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        using var con = await DataSource.OpenConnectionAsync();
        //await DropSampleTableAsync(con);
    }

    private async static Task CreateSampleTableAsync(EDBConnection connection)
    {
        await DropSampleTableAsync(connection);

        var createTableScript = """
    CREATE TABLE test_dynamicjson (
        data JSONB
    )
    """;
        using EDBCommand createCommand = new(createTableScript, connection);
        await createCommand.ExecuteNonQueryAsync();
    }

    private static async Task DropSampleTableAsync(EDBConnection connection)
    {
        var dropTableScript = """
    DROP TABLE IF EXISTS test_dynamicjson
    """;
        using EDBCommand dropCommand = new(dropTableScript, connection);
        await dropCommand.ExecuteNonQueryAsync();
    }

    class MyPoco
    {
        public int A { get; set; }
        public int B { get; set; }
    }


    [Test]
    public async Task EnableDynamicJson_EnabledByDefault_DataSource()
    {
        using var con = await DataSource.OpenConnectionAsync();


        // Write a POCO to a jsonb column:
        var myPoco1 = new MyPoco { A = 8, B = 9 };

        await using var command1 = new EDBCommand("INSERT INTO test_dynamicjson (data) VALUES ($1)", con)
        {
            Parameters = { new() { Value = myPoco1, EDBDbType = EDBDbType.Jsonb } }
        };
        Assert.DoesNotThrowAsync(command1.ExecuteNonQueryAsync);

        // Read jsonb data as a POCO:
        await using var command2 = new EDBCommand("SELECT data FROM test_dynamicjson", con);
        await using var reader = await command2.ExecuteReaderAsync();
        Assert.IsTrue(reader.Read());

        var myPoco2 = reader.GetFieldValue<MyPoco>(0);

        await reader.CloseAsync();
    }

    [Test]
    public async Task EnableDynamicJson_EnabledByDefault_Connection()
    {
        using var con = new EDBConnection(TestUtil.ConnectionString);
        await con.OpenAsync();
        ;

        // Write a POCO to a jsonb column:
        var myPoco1 = new MyPoco { A = 8, B = 9 };

        await using var command1 = new EDBCommand("INSERT INTO test_dynamicjson (data) VALUES ($1)", con)
        {
            Parameters = { new() { Value = myPoco1, EDBDbType = EDBDbType.Jsonb } }
        };
        Assert.DoesNotThrowAsync(command1.ExecuteNonQueryAsync);

        // Read jsonb data as a POCO:
        await using var command2 = new EDBCommand("SELECT data FROM test_dynamicjson", con);
        await using var reader = await command2.ExecuteReaderAsync();
        Assert.IsTrue(reader.Read());

        var myPoco2 = reader.GetFieldValue<MyPoco>(0);

        await reader.CloseAsync();
    }


    public EDBNonBreakingChangesTests()
    {
        //DataSource = CreateDataSource(b=> b.EnableDynamicJson()); // Not needed as breaking change has been reverted by EC-3060
        DataSource = CreateDataSource();
    }

    protected override EDBDataSource DataSource { get; }
}
