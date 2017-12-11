using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient;
using EDBTypes;
using NUnit.Framework;

namespace DOTNET
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/rangetypes.html
    /// </remarks>
    class RangeTests
    {
        [Test, Description("Resolves a range type handler via the different pathways")]
        public void RangeTypeResolution()
        {
            var csb = new EDBConnectionStringBuilder(TestUtil.defaultConnectionString)
            {
                ApplicationName = nameof(RangeTypeResolution),  // Prevent backend type caching in TypeHandlerRegistry
                Pooling = false
            };

            using (var conn = TestUtil.openDB(csb))
            {
                // Resolve type by EDBDbType
                using (var cmd = new EDBCommand("SELECT @p", conn))
                {
                    cmd.Parameters.AddWithValue("p", EDBDbType.Range | EDBDbType.Integer, DBNull.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        Assert.That(reader.GetDataTypeName(0), Is.EqualTo("int4range"));
                    }
                }

                // Resolve type by ClrType (type inference)
                conn.ReloadTypes();
                using (var cmd = new EDBCommand("SELECT @p", conn))
                {
                    cmd.Parameters.Add(new EDBParameter { ParameterName = "p", Value = new EDBRange<int>(3, 5) });
                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        Assert.That(reader.GetDataTypeName(0), Is.EqualTo("int4range"));
                    }
                }

                // Resolve type by OID (read)
                conn.ReloadTypes();
                using (var cmd = new EDBCommand("SELECT int4range(3, 5)", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    Assert.That(reader.GetDataTypeName(0), Is.EqualTo("int4range"));
                }
            }
        }

        [Test]
        public void Range()
        {
            using (var conn = TestUtil.openDB())
            using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3, @p4", conn))
            {
                var p1 = new EDBParameter("p1", EDBDbType.Range | EDBDbType.Integer) { Value = EDBRange<int>.Empty };
                var p2 = new EDBParameter { ParameterName = "p2", Value = new EDBRange<int>(1, 10) };
                var p3 = new EDBParameter { ParameterName = "p3", Value = new EDBRange<int>(1, false, 10, false) };
                var p4 = new EDBParameter { ParameterName = "p4", Value = new EDBRange<int>(0, false, true, 10, false, false) };
                Assert.That(p2.EDBDbType, Is.EqualTo(EDBDbType.Range | EDBDbType.Integer));
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                cmd.Parameters.Add(p4);
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    Assert.That(reader[0].ToString(), Is.EqualTo("empty"));
                    Assert.That(reader[1].ToString(), Is.EqualTo("[1,11)"));
                    Assert.That(reader[2].ToString(), Is.EqualTo("[2,10)"));
                    Assert.That(reader[3].ToString(), Is.EqualTo("(,10)"));
                }
            }
        }

        [Test]
        public void RangeWithLongSubtype()
        {
            using (var conn = TestUtil.openDB())
            {
                conn.ExecuteNonQuery("CREATE TYPE pg_temp.textrange AS RANGE(subtype=text)");
                conn.ReloadTypes();
                Assert.That(conn.ExecuteScalar("SELECT 1"), Is.EqualTo(1));

                var value = new EDBRange<string>(
                    new string('a', conn.Settings.WriteBufferSize + 10),
                    new string('z', conn.Settings.WriteBufferSize + 10)
                    );

                //var value = new EDBRange<string>("bar", "foo");
                using (var cmd = new EDBCommand("SELECT @p", conn))
                {
                    cmd.Parameters.Add(new EDBParameter("p", EDBDbType.Range | EDBDbType.Text) {Value = value});
                    using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        reader.Read();
                        Assert.That(reader[0], Is.EqualTo(value));
                    }
                }
            }
        }

        [Test]
        public void TestRange()
        {
            using (var conn = TestUtil.openDB())
            using (var cmd = conn.CreateCommand())
            {
                object obj;

                cmd.CommandText = "select '[2,10)'::int4range";
                cmd.Prepare();
                obj = cmd.ExecuteScalar();
                Assert.AreEqual(new EDBRange<int>(2, true, false, 10, false, false), obj);

                cmd.CommandText = "select array['[2,10)'::int4range, '[3,9)'::int4range]";
                cmd.Prepare();
                obj = cmd.ExecuteScalar();
                Assert.AreEqual(new EDBRange<int>(3, true, false, 9, false, false), ((EDBRange<int>[])obj)[1]);
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            using (var conn = TestUtil.openDB())
                TestUtil.MinimumPgVersion(conn, "9.2.0");
        }
    }
}
