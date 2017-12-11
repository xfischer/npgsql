#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using EnterpriseDB.EDBClient;
using EDBTypes;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DOTNET
{
    /// <summary>
    /// Tests on PostgreSQL numeric types
    /// </summary>
    /// <summary>
    /// http://www.postgresql.org/docs/current/static/datatype-numeric.html
    /// </summary>
    public class NumericTypeTests
    {
        [Test]
        public void Int16()
        {
            using (var conn = TestUtil.openDB())
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
                        Assert.That(reader.GetDataTypeName(i), Is.EqualTo("int2"));
                    }
                }
            }
        }

        [Test]
        public void Int32()
        {
            using (var conn = TestUtil.openDB())
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
                        Assert.That(reader.GetDataTypeName(i),          Is.EqualTo("int4"));
                    }
                }
            }
        }

        [Test, Description("Tests some types which are aliased to UInt32")]
        [TestCase(EDBDbType.Oid, TestName="OID")]
        [TestCase(EDBDbType.Xid, TestName="XID")]
        [TestCase(EDBDbType.Cid, TestName="CID")]
        public void UInt32(EDBDbType npgsqlDbType)
        {
            var expected = 8u;
            using (var conn = TestUtil.openDB())
            using (var cmd = new EDBCommand("SELECT @p", conn))
            {
                cmd.Parameters.Add(new EDBParameter("p", npgsqlDbType) { Value = expected });
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
            using (var conn = TestUtil.openDB())
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
                        Assert.That(reader.GetDataTypeName(i),          Is.EqualTo("int8"));
                    }
                }
            }
        }

        [Test]
        public void Double()
        {
            using (var conn = TestUtil.openDB())
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
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof (double)));
                    }
                }
            }
        }

        [Test]
        public void Float()
        {
            const float expected = .123456F;
            using (var conn = TestUtil.openDB())
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
                        Assert.That(reader.GetFieldType(i), Is.EqualTo(typeof (float)));
                    }
                }
            }
        }

        [Test]
        public void Numeric()
        {
            using (var conn = TestUtil.openDB())
            {
                using (var cmd = new EDBCommand("SELECT '-1234567.890123'::numeric", conn))
                {
                    var result = cmd.ExecuteScalar();
                    Assert.AreEqual(-1234567.890123M, result);
                }

                using (var cmd = new EDBCommand("SELECT '" + string.Join("", Enumerable.Range(0, 131072).Select(i => "1")) + "." + string.Join("", Enumerable.Range(0, 16383).Select(i => "1")) + "'::numeric::text", conn))
                using (var rdr = cmd.ExecuteReader())
                {
                    rdr.Read();
                }

                var decimals = new decimal[] { 499.0M / 375.0M, 0, 1, -1, 2, -2, decimal.MaxValue, decimal.MinValue, 9999, 10000, -0.0001M, 0.00001M, 0.00000000111143243221M, 4372894738294782934.5832947839247M, 7483927483400000000000M };

                using (var cmd = new EDBCommand("SELECT " + string.Join(", ", Enumerable.Range(0, decimals.Length).Select(i => "@p" + i.ToString())), conn))
                {
                    for (var i = 0; i < decimals.Length; i++)
                    {
                        cmd.Parameters.Add(new EDBParameter("p" + i, EDBDbType.Numeric) { Value = decimals[i] });
                    }
                    using (var rdr = cmd.ExecuteReader())
                    {
                        rdr.Read();
                        for (var i = 0; i < decimals.Length; i++)
                            Assert.AreEqual(decimals[i], rdr.GetValue(i));
                    }
                }

                using (var cmd = new EDBCommand("SELECT @p1, @p2, @p3, @p4", conn))
                {
                    var p1 = new EDBParameter("p1", EDBDbType.Numeric);
                    var p2 = new EDBParameter("p2", DbType.Decimal);
                    var p3 = new EDBParameter("p3", DbType.VarNumeric);
                    var p4 = new EDBParameter { ParameterName = "p4", Value = (decimal)8 };
                    cmd.Parameters.Add(p1);
                    cmd.Parameters.Add(p2);
                    cmd.Parameters.Add(p3);
                    cmd.Parameters.Add(p4);
                    p1.Value = p2.Value = p3.Value = 8;
                    using (var reader = cmd.ExecuteReader()) {
                        reader.Read();

                        for (var i = 0; i < cmd.Parameters.Count; i++)
                        {
                            Assert.That(reader.GetDecimal(i),               Is.EqualTo(8.0m));
                            Assert.That(reader.GetInt32(i),                 Is.EqualTo(8));
                            Assert.That(reader.GetInt64(i),                 Is.EqualTo(8));
                            Assert.That(reader.GetInt16(i),                 Is.EqualTo(8));
                            Assert.That(reader.GetByte(i),                  Is.EqualTo(8));
                            Assert.That(reader.GetFloat(i),                 Is.EqualTo(8.0f));
                            Assert.That(reader.GetDouble(i),                Is.EqualTo(8.0d));
                            Assert.That(reader.GetValue(i),                 Is.EqualTo(8));
                            Assert.That(reader.GetProviderSpecificValue(i), Is.EqualTo(8));
                            Assert.That(reader.GetFieldType(i),             Is.EqualTo(typeof(decimal)));
                            Assert.That(reader.GetDataTypeName(i),          Is.EqualTo("numeric"));
                        }
                    }
                }
            }
        }

        // Older tests

        [Test]
        public void DoubleWithoutPrepared()
        {
            using (var conn = TestUtil.openDB())
            using (var command = new EDBCommand("select :field_float8", conn))
            {
                command.Parameters.Add(new EDBParameter(":field_float8", EDBDbType.Double));
                double x = 1d/7d;
                command.Parameters[0].Value = x;
                var valueReturned = command.ExecuteScalar();
                Assert.That(valueReturned, Is.EqualTo(x).Within(100).Ulps);
            }
        }

        [Test]
        public void PrecisionScaleNumericSupport()
        {
            using (var conn = TestUtil.openDB())
            using (var command = new EDBCommand("SELECT -4.3::NUMERIC", conn))
            using (var dr = command.ExecuteReader())
            {
                dr.Read();
                var result = dr.GetDecimal(0);
                Assert.AreEqual(-4.3000000M, result);
                //Assert.AreEqual(11, result.Precision);
                //Assert.AreEqual(7, result.Scale);
            }
        }

        [Test]
        public void NumberConversionWithCulture()
        {
            //using (var conn = TestUtil.openDB())
            //using (var cmd = new EDBCommand("select :p1", conn))
            //using (new CultureSetter(new CultureInfo("es-ES")))
            //{
            //    var parameter = new EDBParameter("p1", EDBDbType.Double) { Value = 5.5 };
            //    cmd.Parameters.Add(parameter);
            //    var result = cmd.ExecuteScalar();
            //    Assert.AreEqual(5.5, result);
            //}
        }

        [Test]
        public void TestMoney([Values(PrepareOrNot.Prepared, PrepareOrNot.NotPrepared)] PrepareOrNot prepare)
        {
            using (var conn = TestUtil.openDB())
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
