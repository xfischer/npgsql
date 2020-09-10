using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using EnterpriseDB.EDBClient.Util;
using EDBTypes;
using NUnit.Framework;

namespace EnterpriseDB.EDBClient.Tests.Types
{
    /// <summary>
    /// Tests on PostgreSQL numeric types
    /// </summary>
    /// <summary>
    /// http://www.postgresql.org/docs/current/static/datatype-numeric.html
    /// </summary>
    public class NumericTypeTests : TestBase
    {
        [Test]
        public void Int16()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3, @p4, @p5", conn))
            {
                var p1 = new EDBParameter("p1", EDBDbType.Smallint);
                var p2 = new EDBParameter("p2", DbType.Int16);
                var p3 = new EDBParameter("p3", DbType.Byte);
                var p4 = new EDBParameter { ParameterName = "p4", Value = (short)8 };
                var p5 = new EDBParameter { ParameterName = "p5", Value = (byte)8  };
                Assert.That(p4.EDBDbType, Is.EqualTo(EDBDbType.Smallint));
                Assert.That(p4.DbType, Is.EqualTo(DbType.Int16));
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                cmd.Parameters.Add(p4);
                cmd.Parameters.Add(p5);
                p1.Value = p2.Value = p3.Value = (long)8;
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetInt16(i), Is.EqualTo(8));
                        Assert.That(reader.GetInt32(i), Is.EqualTo(8));
                        Assert.That(reader.GetInt64(i), Is.EqualTo(8));
                        Assert.That(reader.GetByte(i), Is.EqualTo(8));
                        Assert.That(reader.GetFloat(i), Is.EqualTo(8.0f));
                        Assert.That(reader.GetDouble(i), Is.EqualTo(8.0d));
                        Assert.That(reader.GetDecimal(i), Is.EqualTo(8.0m));
                        Assert.That(reader.GetValue(i), Is.EqualTo(8));
                        Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(8));
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(short)));
                        Assert.That(reader.GetDataTypeName(i), Is.EqualTo("smallint"));
                    }
                }
            }
        }

        [Test]
        public void Int32()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3", conn))
            {
                var p1 = new EDBParameter("p1", EDBDbType.Integer);
                var p2 = new EDBParameter("p2", DbType.Int32);
                var p3 = new EDBParameter { ParameterName = "p3", Value = 8 };
                Assert.That(p3.EDBDbType, Is.EqualTo(EDBDbType.Integer));
                Assert.That(p3.DbType, Is.EqualTo(DbType.Int32));
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                p1.Value = p2.Value = (long)8;
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetInt32(i),                 Is.EqualTo(8));
                        Assert.That(reader.GetInt64(i),                 Is.EqualTo(8));
                        Assert.That(reader.GetInt16(i),                 Is.EqualTo(8));
                        Assert.That(reader.GetByte(i),                  Is.EqualTo(8));
                        Assert.That(reader.GetFloat(i),                 Is.EqualTo(8.0f));
                        Assert.That(reader.GetDouble(i),                Is.EqualTo(8.0d));
                        Assert.That(reader.GetDecimal(i),               Is.EqualTo(8.0m));
                        Assert.That(reader.GetValue(i),                 Is.EqualTo(8));
                        Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(8));
                        Assert.That(reader.GetFieldType(i),             Is.EqualTo(typeof(int)));
                        Assert.That(reader.GetDataTypeName(i),          Is.EqualTo("integer"));
                    }
                }
            }
        }

        [Test, Description("Tests some types which are aliased to UInt32")]
        [TestCase(EDBDbType.Oid, TestName="OID")]
        [TestCase(EDBDbType.Xid, TestName="XID")]
        [TestCase(EDBDbType.Cid, TestName="CID")]
        public void UInt32(EDBDbType EDBDbType)
        {
            var expected = 8u;
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p", conn))
            {
                cmd.Parameters.Add(new EDBParameter("p", EDBDbType) { Value = expected });
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    Assert.That(reader[0], Is.EqualTo(expected));
                    Assert.That(reader.GetProviderSpecificValue(0), Is.EqualTo(expected));
                    Assert.That(reader.GetFieldType(0), Is.EqualTo(typeof(uint)));
                }
            }
        }

        [Test]
        public void Int64()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3", conn))
            {
                var p1 = new EDBParameter("p1", EDBDbType.Bigint);
                var p2 = new EDBParameter("p2", DbType.Int64);
                var p3 = new EDBParameter { ParameterName = "p3", Value = (long)8 };
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                p1.Value = p2.Value = (short)8;
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetInt64(i),                 Is.EqualTo(8));
                        Assert.That(reader.GetInt16(i),                 Is.EqualTo(8));
                        Assert.That(reader.GetInt32(i),                 Is.EqualTo(8));
                        Assert.That(reader.GetByte(i),                  Is.EqualTo(8));
                        Assert.That(reader.GetFloat(i),                 Is.EqualTo(8.0f));
                        Assert.That(reader.GetDouble(i),                Is.EqualTo(8.0d));
                        Assert.That(reader.GetDecimal(i),               Is.EqualTo(8.0m));
                        Assert.That(reader.GetValue(i),                 Is.EqualTo(8));
                        Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(8));
                        Assert.That(reader.GetFieldType(i),             Is.EqualTo(typeof(long)));
                        Assert.That(reader.GetDataTypeName(i),          Is.EqualTo("bigint"));
                    }
                }
            }
        }

        [Test]
        public void Double()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3", conn))
            {
                const double expected = 4.123456789012345;
                var p1 = new EDBParameter("p1", EDBDbType.Double);
                var p2 = new EDBParameter("p2", DbType.Double);
                var p3 = new EDBParameter {ParameterName = "p3", Value = expected};
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                p1.Value = p2.Value = expected;
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetDouble(i), Is.EqualTo(expected).Within(10E-07));
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(double)));
                    }
                }
            }
        }

        [Test]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        public void DoubleSpecial(double value)
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p", conn))
            {
                cmd.Parameters.AddWithValue("p", EDBDbType.Double, value);
                var actual = cmd.ExecuteScalar();
                Assert.That(actual, Is.EqualTo(value));
            }
        }

        [Test]
        public void Float()
        {
            const float expected = .123456F;
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3", conn))
            {
                var p1 = new EDBParameter("p1", EDBDbType.Real);
                var p2 = new EDBParameter("p2", DbType.Single);
                var p3 = new EDBParameter {ParameterName = "p3", Value = expected};
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                p1.Value = p2.Value = expected;
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    for (var i = 0; i < cmd.Parameters.Count; i++)
                    {
                        Assert.That(reader.GetFloat(i), Is.EqualTo(expected).Within(10E-07));
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof(float)));
                    }
                }
            }
        }

        [Test]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        public void DoubleFloat(double value)
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p", conn))
            {
                cmd.Parameters.AddWithValue("p", EDBDbType.Real, value);
                var actual = cmd.ExecuteScalar();
                Assert.That(actual, Is.EqualTo(value));
            }
        }

        [Test, Description("Tests handling of numeric overflow when writing data")]
        [TestCase(EDBDbType.Smallint, 1 + short.MaxValue)]
        [TestCase(EDBDbType.Smallint, 1L + short.MaxValue)]
        [TestCase(EDBDbType.Smallint, 1F + short.MaxValue)]
        [TestCase(EDBDbType.Smallint, 1D + short.MaxValue)]
        [TestCase(EDBDbType.Integer, 1L + int.MaxValue)]
        [TestCase(EDBDbType.Integer, 1F + int.MaxValue)]
        [TestCase(EDBDbType.Integer, 1D + int.MaxValue)]
        [TestCase(EDBDbType.Bigint, 1F + long.MaxValue)]
        [TestCase(EDBDbType.Bigint, 1D + long.MaxValue)]
        public void WriteOverflow(EDBDbType type, object value)
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p1", conn))
            {
                var p1 = new EDBParameter("p1", type)
                {
                    Value = value
                };
                cmd.Parameters.Add(p1);
                Assert.Throws<OverflowException>(() =>
                {
                    using (var reader = cmd.ExecuteReader()) { }
                });
            }
        }

        static IEnumerable<TestCaseData> ReadOverflowTestCases
        {
            get
            {
                yield return new TestCaseData(EDBDbType.Smallint, 1D + byte.MaxValue){ };
            }
        }
        [Test, Description("Tests handling of numeric overflow when reading data")]
        [TestCase((byte)0, EDBDbType.Smallint, 1D + byte.MaxValue)]
        [TestCase((sbyte)0, EDBDbType.Smallint, 1D + sbyte.MaxValue)]
        [TestCase((byte)0, EDBDbType.Integer, 1D + byte.MaxValue)]
        [TestCase((short)0, EDBDbType.Integer, 1D + short.MaxValue)]
        [TestCase((byte)0, EDBDbType.Bigint, 1D + byte.MaxValue)]
        [TestCase((short)0, EDBDbType.Bigint, 1D + short.MaxValue)]
        [TestCase(0, EDBDbType.Bigint, 1D + int.MaxValue)]
        public void ReadOverflow<T>(T readingType, EDBDbType type, double value)
        {
            var typeString = GetTypeAsString(type);
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand($"SELECT {value}::{typeString}", conn))
            {
                Assert.Throws<OverflowException>(() =>
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        Assert.True(reader.Read());
                        reader.GetFieldValue<T>(0);
                    }
                });
            }

            string GetTypeAsString(EDBDbType dbType)
                => dbType switch
                {
                    EDBDbType.Smallint => "int2",
                    EDBDbType.Integer  => "int4",
                    EDBDbType.Bigint   => "int8",
                    _                     => throw new NotSupportedException()
                };
        }

        // Older tests

        [Test]
        public void DoubleWithoutPrepared()
        {
            using (var conn = OpenConnection())
            using (var command = new EDBCommand("select :field_float8", conn))
            {
                command.Parameters.Add(new EDBParameter(":field_float8", EDBDbType.Double));
                var x = 1d/7d;
                command.Parameters[0].Value = x;
                var valueReturned = command.ExecuteScalar();
                Assert.That(valueReturned, Is.EqualTo(x).Within(100).Ulps);
            }
        }

        [Test]
        public void NumberConversionWithCulture()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("select :p1", conn))
            using (new CultureSetter(new CultureInfo("es-ES")))
            {
                var parameter = new EDBParameter("p1", EDBDbType.Double) { Value = 5.5 };
                cmd.Parameters.Add(parameter);
                var result = cmd.ExecuteScalar();
                Assert.AreEqual(5.5, result);
            }
        }

        [Test]
        public void TestMoney([Values(PrepareOrNot.Prepared, PrepareOrNot.NotPrepared)] PrepareOrNot prepare)
        {
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "select '1'::MONEY, '12345'::MONEY / 100, '123456789012345'::MONEY / 100";
                if (prepare == PrepareOrNot.Prepared)
                    cmd.Prepare();
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    Assert.AreEqual(1M, reader.GetValue(0));
                    Assert.AreEqual(123.45M, reader.GetValue(1));
                    Assert.AreEqual(1234567890123.45M, reader.GetValue(2));
                }
            }
        }
    }
}
