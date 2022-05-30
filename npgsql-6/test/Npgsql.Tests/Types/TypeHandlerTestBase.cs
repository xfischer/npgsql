using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types
{
    public abstract class TypeHandlerTestBase<T> : MultiplexingTestBase
    {
        readonly EDBDbType? _npgsqlDbType;
        readonly string? _typeName;

        protected TypeHandlerTestBase(MultiplexingMode multiplexingMode, EDBDbType? npgsqlDbType, string? typeName)
            : base(multiplexingMode)
            => (_npgsqlDbType, _typeName) = (npgsqlDbType, typeName);

        [Test]
        [TestCaseSource("TestCases")]
        public async Task Read(string query, T expected)
        {
            using var conn = await OpenConnectionAsync();
            using var cmd = new EDBCommand($"SELECT {query}", conn);

            Assert.That(await cmd.ExecuteScalarAsync(), Is.EqualTo(expected));
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task Write(string query, T expected)
        {
            var parameter = new EDBParameter<T>("p", expected);

            if (_npgsqlDbType != null)
                parameter.EDBDbType = _npgsqlDbType.Value;

            if (_typeName != null)
                parameter.DataTypeName = _typeName;

            using var conn = await OpenConnectionAsync();
            using var cmd = new EDBCommand($"SELECT {query}::text = @p::text", conn)
            {
                Parameters = { parameter }
            };

            Assert.That(await cmd.ExecuteScalarAsync(), Is.True);
        }
    }
}
