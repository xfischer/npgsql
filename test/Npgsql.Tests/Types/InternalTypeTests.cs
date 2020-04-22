using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types
{
    public class InternalTypeTests : TestBase
    {
        [Test]
        public void ReadInternalChar()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT typdelim FROM pg_type WHERE typname='int4'", conn))
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                Assert.That(reader.GetChar(0), Is.EqualTo(','));
                Assert.That(reader.GetValue(0), Is.EqualTo(','));
                Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(','));
                Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(char)));
            }
        }

        [Test]
        [TestCase(EDBDbType.Regtype)]
        [TestCase(EDBDbType.Regconfig)]
        public void InternalUintTypes(EDBDbType EDBDbType)
        {
            const uint expected = 8u;
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p", conn))
            {
                cmd.Parameters.AddWithValue("p", EDBDbType, expected);
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(uint)));
                    Assert.That(reader.GetValue(0), Is.EqualTo(expected));
                }
            }
        }

        [Test]
        public void Tid()
        {
            var expected = new EDBTid(3, 5);
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT '(1234,40000)'::tid, @p::tid";
                cmd.Parameters.AddWithValue("p", EDBDbType.Tid, expected);
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    Assert.AreEqual(1234, reader.GetFieldValue<EDBTid>(0).BlockNumber);
                    Assert.AreEqual(40000, reader.GetFieldValue<EDBTid>(0).OffsetNumber);
                    Assert.AreEqual(expected.BlockNumber, reader.GetFieldValue<EDBTid>(1).BlockNumber);
                    Assert.AreEqual(expected.OffsetNumber, reader.GetFieldValue<EDBTid>(1).OffsetNumber);
                }
            }
        }
    }
}
