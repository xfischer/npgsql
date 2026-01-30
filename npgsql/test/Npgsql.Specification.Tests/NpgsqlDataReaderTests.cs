using AdoNet.Specification.Tests;
using EnterpriseDB.EDBClient.Specification.Tests;
using System.Threading.Tasks;

namespace Npgsql.Specification.Tests;

public sealed class EDBDataReaderTests : DataReaderTestBase<EDBSelectValueFixture>
{
    public EDBDataReaderTests(EDBSelectValueFixture fixture)
        : base(fixture) { }

    // EnterpriseDB: mostly because exceptions differ
    public override void Read_throws_when_closed() { }
    public override void NextResult_throws_when_closed() { }
    public override Task IsDBNullAsync_is_canceled() { return Task.CompletedTask; }
    public override void IsDBNull_throws_when_closed() { }
    public override void GetValue_throws_when_closed() { }
    public override void GetTextReader_returns_empty_for_null_String() { }
    public override void GetName_throws_when_closed() { }
    public override Task GetFieldValueAsync_is_canceled() { return Task.CompletedTask; }
    public override void GetFieldType_throws_when_closed() { }
    public override void GetDataTypeName_throws_when_closed() { }
    public override void GetChars_reads_nothing_when_dataOffset_is_too_large() { }
    public override void GetBytes_reads_nothing_when_dataOffset_is_too_large() { }
    public override void FieldCount_throws_when_closed() { }
}
