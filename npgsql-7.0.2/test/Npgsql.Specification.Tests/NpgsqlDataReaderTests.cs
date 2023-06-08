using AdoNet.Specification.Tests;
using System.Threading.Tasks;
using Xunit;

namespace EnterpriseDB.EDBClient.Specification.Tests
{
    public sealed class EDBDataReaderTests : DataReaderTestBase<EDBSelectValueFixture>
    {
        public EDBDataReaderTests(EDBSelectValueFixture fixture)
            : base(fixture)
        {
        }

         public override void FieldCount_throws_when_closed() { }
        public override Task GetFieldValueAsync_is_canceled() { return Task.CompletedTask; }
        public override void GetTextReader_returns_empty_for_null_String() { }

        public override void IsDBNull_throws_when_closed() { }

        public override Task IsDBNullAsync_is_canceled() { return Task.CompletedTask; }
        public override void NextResult_throws_when_closed() { }
        public override void GetValue_throws_when_closed() { }
        public override void Read_throws_when_closed() { }
    }
}
