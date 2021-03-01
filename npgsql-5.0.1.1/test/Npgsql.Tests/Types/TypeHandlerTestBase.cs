using System.Threading.Tasks;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types
{
    public abstract class TypeHandlerTestBase<T> : MultiplexingTestBase
    {
        readonly EDBDbType? _EDBDbType;
        readonly string? _typeName;
        readonly string? _minVersion;

        protected TypeHandlerTestBase(MultiplexingMode multiplexingMode, EDBDbType? EDBDbType, string? typeName, string? minVersion = null)
            : base(multiplexingMode) => (_EDBDbType, _typeName, _minVersion) = (EDBDbType, typeName, minVersion);

        [OneTimeSetUp]
        public async Task MinimumPgVersion()
        {
            if (_minVersion is string minVersion)
            {
                using var conn = await OpenConnectionAsync();
                TestUtil.MinimumPgVersion(conn, minVersion);
            }
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task Read(string query, T expected)
        {
            using var conn = await OpenConnectionAsync();
            using var cmd = new EDBCommand($"SELECT {query}", conn);

            Assert.AreEqual(await cmd.ExecuteScalarAsync(), expected);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public async Task Write(string query, T expected)
        {
            var parameter = new EDBParameter<T>("p", expected);

            if (_EDBDbType != null)
                parameter.EDBDbType = _EDBDbType.Value;

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
