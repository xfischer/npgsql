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


#define NET_2_0

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Xml;
using EnterpriseDB.EDBClient;
using EDBTypes;

using NUnit.Framework;

namespace DOTNET
{
    [TestFixture]
    public class ParameterTest
    {
        [Test, Description("Makes sure that when EDBDbType or Value/EDBValue are set, DbType and EDBDbType are set accordingly")]
        public void ImplicitSettingOfDbTypes()
        {
            var p = new EDBParameter("p", DbType.Int32);
            Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Integer));

            // As long as EDBDbType/DbType aren't set explicitly, infer them from Value
            p = new EDBParameter("p", 8);
            Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Integer));
            Assert.That(p.DbType, Is.EqualTo(DbType.Int32));

            p.Value = 3.0;
            Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Double));
            Assert.That(p.DbType, Is.EqualTo(DbType.Double));

            p.EDBDbType = EDBDbType.Bytea;
            Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Bytea));
            Assert.That(p.DbType, Is.EqualTo(DbType.Binary));

            p.Value = "dont_change";
            Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Bytea));
            Assert.That(p.DbType, Is.EqualTo(DbType.Binary));

            p = new EDBParameter("p", new int[0]);
            Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Array | EDBDbType.Integer));
            Assert.That(p.DbType, Is.EqualTo(DbType.Object));
        }

        // Older tests

        /// <summary>
        /// Test which validates that Clear() indeed cleans up the parameters in a command so they can be added to other commands safely.
        /// </summary>
        [Test]
        public void EDBParameterCollectionClearTest()
        {
            var p = new EDBParameter();
            var c1 = new EDBCommand();
            var c2 = new EDBCommand();
            c1.Parameters.Add(p);
            Assert.AreEqual(1, c1.Parameters.Count);
            Assert.AreEqual(0, c2.Parameters.Count);
            c1.Parameters.Clear();
            Assert.AreEqual(0, c1.Parameters.Count);
            c2.Parameters.Add(p);
            Assert.AreEqual(0, c1.Parameters.Count);
            Assert.AreEqual(1, c2.Parameters.Count);
        }

        #region Constructors

        [Test]
        public void Constructor1()
        {
            var p = new EDBParameter();
            Assert.AreEqual(DbType.Object, p.DbType, "DbType");
            Assert.AreEqual(ParameterDirection.Input, p.Direction, "Direction");
            Assert.IsFalse(p.IsNullable, "IsNullable");
#if NET_2_0
            //Assert.AreEqual (0, p.LocaleId, "LocaleId");
#endif
            Assert.AreEqual(string.Empty, p.ParameterName, "ParameterName");
            Assert.AreEqual(0, p.Precision, "Precision");
            Assert.AreEqual(0, p.Scale, "Scale");
            Assert.AreEqual(0, p.Size, "Size");
            Assert.AreEqual(string.Empty, p.SourceColumn, "SourceColumn");
#if NET_2_0
            Assert.IsFalse(p.SourceColumnNullMapping, "SourceColumnNullMapping");
#endif
#if NET451
            Assert.AreEqual(DataRowVersion.Current, p.SourceVersion, "SourceVersion");
#endif
            Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "EDBDbType");
#if NET_2_0
            Assert.IsNull(p.EDBValue, "EDBValue");
#endif
            Assert.IsNull(p.Value, "Value");
#if NET_2_0
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionDatabase, "XmlSchemaCollectionDatabase");
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionName, "XmlSchemaCollectionName");
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionOwningSchema, "XmlSchemaCollectionOwningSchema");
#endif
        }

        [Test]
        public void Constructor2_Value_DateTime()
        {
            var value = new DateTime(2004, 8, 24);

            var p = new EDBParameter("address", value);
            Assert.AreEqual(DbType.DateTime, p.DbType, "B:DbType");
            Assert.AreEqual(ParameterDirection.Input, p.Direction, "B:Direction");
            Assert.IsFalse(p.IsNullable, "B:IsNullable");
#if NET_2_0
            //Assert.AreEqual (0, p.LocaleId, "B:LocaleId");
#endif
            Assert.AreEqual("address", p.ParameterName, "B:ParameterName");
            Assert.AreEqual(0, p.Precision, "B:Precision");
            Assert.AreEqual(0, p.Scale, "B:Scale");
            //Assert.AreEqual (0, p.Size, "B:Size");
            Assert.AreEqual(string.Empty, p.SourceColumn, "B:SourceColumn");
#if NET_2_0
            Assert.IsFalse(p.SourceColumnNullMapping, "B:SourceColumnNullMapping");
#endif
#if NET451
            Assert.AreEqual(DataRowVersion.Current, p.SourceVersion, "B:SourceVersion");
#endif
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "B:EDBDbType");
#if NET_2_0
            // FIXME
            //Assert.AreEqual (new SqlDateTime (value), p.EDBValue, "B:EDBValue");
#endif
            Assert.AreEqual(value, p.Value, "B:Value");
#if NET_2_0
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionDatabase, "B:XmlSchemaCollectionDatabase");
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionName, "B:XmlSchemaCollectionName");
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionOwningSchema, "B:XmlSchemaCollectionOwningSchema");
#endif
        }

        [Test]
        public void Constructor2_Value_DBNull()
        {
            var p = new EDBParameter("address", DBNull.Value);
            Assert.AreEqual(DbType.Object, p.DbType, "B:DbType");
            Assert.AreEqual(ParameterDirection.Input, p.Direction, "B:Direction");
            Assert.IsFalse(p.IsNullable, "B:IsNullable");
#if NET_2_0
            //Assert.AreEqual (0, p.LocaleId, "B:LocaleId");
#endif
            Assert.AreEqual("address", p.ParameterName, "B:ParameterName");
            Assert.AreEqual(0, p.Precision, "B:Precision");
            Assert.AreEqual(0, p.Scale, "B:Scale");
            Assert.AreEqual(0, p.Size, "B:Size");
            Assert.AreEqual(string.Empty, p.SourceColumn, "B:SourceColumn");
#if NET_2_0
            Assert.IsFalse(p.SourceColumnNullMapping, "B:SourceColumnNullMapping");
#endif
#if NET451
            Assert.AreEqual(DataRowVersion.Current, p.SourceVersion, "B:SourceVersion");
#endif
            Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "B:EDBDbType");
#if NET_2_0
            // FIXME
            //Assert.AreEqual (SqlString.Null, p.EDBValue, "B:EDBValue");
#endif
            Assert.AreEqual(DBNull.Value, p.Value, "B:Value");
#if NET_2_0
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionDatabase, "B:XmlSchemaCollectionDatabase");
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionName, "B:XmlSchemaCollectionName");
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionOwningSchema, "B:XmlSchemaCollectionOwningSchema");
#endif
        }

        [Test]
        public void Constructor2_Value_Null()
        {
            var p = new EDBParameter("address", (Object) null);
            Assert.AreEqual(DbType.Object, p.DbType, "A:DbType");
            Assert.AreEqual(ParameterDirection.Input, p.Direction, "A:Direction");
            Assert.IsFalse(p.IsNullable, "A:IsNullable");
#if NET_2_0
            //Assert.AreEqual (0, p.LocaleId, "A:LocaleId");
#endif
            Assert.AreEqual("address", p.ParameterName, "A:ParameterName");
            Assert.AreEqual(0, p.Precision, "A:Precision");
            Assert.AreEqual(0, p.Scale, "A:Scale");
            Assert.AreEqual(0, p.Size, "A:Size");
            Assert.AreEqual(string.Empty, p.SourceColumn, "A:SourceColumn");
#if NET_2_0
            Assert.IsFalse(p.SourceColumnNullMapping, "A:SourceColumnNullMapping");
#endif
#if NET451
            Assert.AreEqual(DataRowVersion.Current, p.SourceVersion, "A:SourceVersion");
#endif
            Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "A:EDBDbType");
#if NET_2_0
            Assert.IsNull(p.EDBValue, "A:EDBValue");
#endif
            Assert.IsNull(p.Value, "A:Value");
#if NET_2_0
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionDatabase, "A:XmlSchemaCollectionDatabase");
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionName, "A:XmlSchemaCollectionName");
            //Assert.AreEqual (string.Empty, p.XmlSchemaCollectionOwningSchema, "A:XmlSchemaCollectionOwningSchema");
#endif
        }

#if NET_2_0
#if NET451
        [Test]
        //.ctor (String, EDBDbType, Int32, String, ParameterDirection, bool, byte, byte, DataRowVersion, object)
        public void Constructor7()
        {
            var p1 = new EDBParameter("p1Name", EDBDbType.Varchar, 20,
                                         "srcCol", ParameterDirection.InputOutput, false, 0, 0,
                                         DataRowVersion.Original, "foo");
            Assert.AreEqual(DbType.String, p1.DbType, "DbType");
            Assert.AreEqual(ParameterDirection.InputOutput, p1.Direction, "Direction");
            Assert.AreEqual(false, p1.IsNullable, "IsNullable");
            //Assert.AreEqual (999, p1.LocaleId, "#");
            Assert.AreEqual("p1Name", p1.ParameterName, "ParameterName");
            Assert.AreEqual(0, p1.Precision, "Precision");
            Assert.AreEqual(0, p1.Scale, "Scale");
            Assert.AreEqual(20, p1.Size, "Size");
            Assert.AreEqual("srcCol", p1.SourceColumn, "SourceColumn");
            Assert.AreEqual(false, p1.SourceColumnNullMapping, "SourceColumnNullMapping");
            Assert.AreEqual(DataRowVersion.Original, p1.SourceVersion, "SourceVersion");
            Assert.AreEqual(EDBDbType.Varchar, p1.EDBDbType, "EDBDbType");
            //Assert.AreEqual (3210, p1.EDBValue, "#");
            Assert.AreEqual("foo", p1.Value, "Value");
            //Assert.AreEqual ("database", p1.XmlSchemaCollectionDatabase, "XmlSchemaCollectionDatabase");
            //Assert.AreEqual ("name", p1.XmlSchemaCollectionName, "XmlSchemaCollectionName");
            //Assert.AreEqual ("schema", p1.XmlSchemaCollectionOwningSchema, "XmlSchemaCollectionOwningSchema");
        }
#endif
#endif

        #endregion

#if NeedsPorting

        [Test]
#if NET_2_0
        [Category ("NotWorking")]
#endif
        public void InferType_Char()
        {
            Char value = 'X';

#if NET_2_0
            String string_value = "X";

            EDBParameter p = new EDBParameter ();
            p.Value = value;
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#A:EDBDbType");
            Assert.AreEqual (DbType.String, p.DbType, "#A:DbType");
            Assert.AreEqual (string_value, p.Value, "#A:Value");

            p = new EDBParameter ();
            p.Value = value;
            Assert.AreEqual (value, p.Value, "#B:Value1");
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#B:EDBDbType");
            Assert.AreEqual (string_value, p.Value, "#B:Value2");

            p = new EDBParameter ();
            p.Value = value;
            Assert.AreEqual (value, p.Value, "#C:Value1");
            Assert.AreEqual (DbType.String, p.DbType, "#C:DbType");
            Assert.AreEqual (string_value, p.Value, "#C:Value2");

            p = new EDBParameter ("name", value);
            Assert.AreEqual (value, p.Value, "#D:Value1");
            Assert.AreEqual (DbType.String, p.DbType, "#D:DbType");
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#D:EDBDbType");
            Assert.AreEqual (string_value, p.Value, "#D:Value2");

            p = new EDBParameter ("name", 5);
            p.Value = value;
            Assert.AreEqual (value, p.Value, "#E:Value1");
            Assert.AreEqual (DbType.String, p.DbType, "#E:DbType");
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#E:EDBDbType");
            Assert.AreEqual (string_value, p.Value, "#E:Value2");

            p = new EDBParameter ("name", EDBDbType.Text);
            p.Value = value;
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#F:EDBDbType");
            Assert.AreEqual (value, p.Value, "#F:Value");
#else
            EDBParameter p = new EDBParameter();
            try
            {
                p.Value = value;
                Assert.Fail("#1");
            }
            catch (ArgumentException ex)
            {
                // The parameter data type of Char is invalid
                Assert.AreEqual(typeof(ArgumentException), ex.GetType(), "#2");
                Assert.IsNull(ex.InnerException, "#3");
                Assert.IsNotNull(ex.Message, "#4");
                Assert.IsNull(ex.ParamName, "#5");
            }
#endif
        }

        [Test]
#if NET_2_0
        [Category ("NotWorking")]
#endif
        public void InferType_CharArray()
        {
            Char[] value = new Char[] { 'A', 'X' };

#if NET_2_0
            String string_value = "AX";

            EDBParameter p = new EDBParameter ();
            p.Value = value;
            Assert.AreEqual (value, p.Value, "#A:Value1");
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#A:EDBDbType");
            Assert.AreEqual (DbType.String, p.DbType, "#A:DbType");
            Assert.AreEqual (string_value, p.Value, "#A:Value2");

            p = new EDBParameter ();
            p.Value = value;
            Assert.AreEqual (value, p.Value, "#B:Value1");
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#B:EDBDbType");
            Assert.AreEqual (string_value, p.Value, "#B:Value2");

            p = new EDBParameter ();
            p.Value = value;
            Assert.AreEqual (value, p.Value, "#C:Value1");
            Assert.AreEqual (DbType.String, p.DbType, "#C:DbType");
            Assert.AreEqual (string_value, p.Value, "#C:Value2");

            p = new EDBParameter ("name", value);
            Assert.AreEqual (value, p.Value, "#D:Value1");
            Assert.AreEqual (DbType.String, p.DbType, "#D:DbType");
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#D:EDBDbType");
            Assert.AreEqual (string_value, p.Value, "#D:Value2");

            p = new EDBParameter ("name", 5);
            p.Value = value;
            Assert.AreEqual (value, p.Value, "#E:Value1");
            Assert.AreEqual (DbType.String, p.DbType, "#E:DbType");
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#E:EDBDbType");
            Assert.AreEqual (string_value, p.Value, "#E:Value2");

            p = new EDBParameter ("name", EDBDbType.Text);
            p.Value = value;
            Assert.AreEqual (EDBDbType.Text, p.EDBDbType, "#F:EDBDbType");
            Assert.AreEqual (value, p.Value, "#F:Value");
#else
            EDBParameter p = new EDBParameter();
            try
            {
                p.Value = value;
                Assert.Fail("#1");
            }
            catch (FormatException)
            {
                // appears to be bug in .NET 1.1 while constructing
                // exception message
            }
            catch (ArgumentException ex)
            {
                // The parameter data type of Char[] is invalid
                Assert.AreEqual(typeof(ArgumentException), ex.GetType(), "#2");
                Assert.IsNull(ex.InnerException, "#3");
                Assert.IsNotNull(ex.Message, "#4");
                Assert.IsNull(ex.ParamName, "#5");
            }
#endif
        }

#endif

        [Test]
        [Ignore("")]
        public void InferType_Invalid()
        {
            var notsupported = new object[]
                                        {
                                            UInt16.MaxValue,
                                            UInt32.MaxValue,
                                            UInt64.MaxValue,
                                            SByte.MaxValue,
                                            new EDBParameter()
                                        };

            var param = new EDBParameter();

            for (var i = 0; i < notsupported.Length; i++)
            {
                try
                {
                    param.Value = notsupported[i];
                    Assert.Fail("#A1:" + i);
                }
                catch (FormatException)
                {
                    // appears to be bug in .NET 1.1 while
                    // constructing exception message
                }
                catch (ArgumentException ex)
                {
                    // The parameter data type of ... is invalid
                    Assert.AreEqual(typeof(ArgumentException), ex.GetType(), "#A2");
                    Assert.IsNull(ex.InnerException, "#A3");
                    Assert.IsNotNull(ex.Message, "#A4");
                    Assert.IsNull(ex.ParamName, "#A5");
                }
            }
        }

#if NeedsPorting
        [Test]
        public void InferType_Object()
        {
            Object value = new Object();

            EDBParameter param = new EDBParameter();
            param.Value = value;
            Assert.AreEqual(EDBDbType.Variant, param.EDBDbType, "#1");
            Assert.AreEqual(DbType.Object, param.DbType, "#2");
        }
#endif

#if NeedsPorting
#if NET_2_0
        [Test]
        public void LocaleId ()
        {
            EDBParameter parameter = new EDBParameter ();
            Assert.AreEqual (0, parameter.LocaleId, "#1");
            parameter.LocaleId = 15;
            Assert.AreEqual(15, parameter.LocaleId, "#2");
        }
#endif
#endif

        [Test] // bug #320196
        public void ParameterNullTest()
        {
            var param = new EDBParameter("param", EDBDbType.Numeric);
            Assert.AreEqual(0, param.Scale, "#A1");
            param.Value = DBNull.Value;
            Assert.AreEqual(0, param.Scale, "#A2");

            param = new EDBParameter("param", EDBDbType.Integer);
            Assert.AreEqual(0, param.Scale, "#B1");
            param.Value = DBNull.Value;
            Assert.AreEqual(0, param.Scale, "#B2");
        }

        [Test]
        [Ignore("")]
        public void ParameterType()
        {
            EDBParameter p;

            // If Type is not set, then type is inferred from the value
            // assigned. The Type should be inferred everytime Value is assigned
            // If value is null or DBNull, then the current Type should be reset to Text.
            p = new EDBParameter();
            Assert.AreEqual(DbType.String, p.DbType, "#A1");
            Assert.AreEqual(EDBDbType.Text, p.EDBDbType, "#A2");
            p.Value = DBNull.Value;
            Assert.AreEqual(DbType.String, p.DbType, "#B1");
            Assert.AreEqual(EDBDbType.Text, p.EDBDbType, "#B2");
            p.Value = 1;
            Assert.AreEqual(DbType.Int32, p.DbType, "#C1");
            Assert.AreEqual(EDBDbType.Integer, p.EDBDbType, "#C2");
            p.Value = DBNull.Value;
#if NET_2_0
            Assert.AreEqual(DbType.String, p.DbType, "#D1");
            Assert.AreEqual(EDBDbType.Text, p.EDBDbType, "#D2");
#else
            Assert.AreEqual(DbType.Int32, p.DbType, "#D1");
            Assert.AreEqual(EDBDbType.Integer, p.EDBDbType, "#D2");
#endif
            p.Value = new byte[] {0x0a};
            Assert.AreEqual(DbType.Binary, p.DbType, "#E1");
            Assert.AreEqual(EDBDbType.Bytea, p.EDBDbType, "#E2");
            p.Value = null;
#if NET_2_0
            Assert.AreEqual(DbType.String, p.DbType, "#F1");
            Assert.AreEqual(EDBDbType.Text, p.EDBDbType, "#F2");
#else
            Assert.AreEqual(DbType.Binary, p.DbType, "#F1");
            Assert.AreEqual(EDBDbType.VarBinary, p.EDBDbType, "#F2");
#endif
            p.Value = DateTime.Now;
            Assert.AreEqual(DbType.DateTime, p.DbType, "#G1");
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#G2");
            p.Value = null;
#if NET_2_0
            Assert.AreEqual(DbType.String, p.DbType, "#H1");
            Assert.AreEqual(EDBDbType.Text, p.EDBDbType, "#H2");
#else
            Assert.AreEqual(DbType.DateTime, p.DbType, "#H1");
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#H2");
#endif

            // If DbType is set, then the EDBDbType should not be
            // inferred from the value assigned.
            p = new EDBParameter();
            p.DbType = DbType.DateTime;
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#I1");
            p.Value = 1;
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#I2");
            p.Value = null;
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#I3");
            p.Value = DBNull.Value;
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#I4");

            // If EDBDbType is set, then the DbType should not be
            // inferred from the value assigned.
            p = new EDBParameter();
            p.EDBDbType = EDBDbType.Bytea;
            Assert.AreEqual(EDBDbType.Bytea, p.EDBDbType, "#J1");
            p.Value = 1;
            Assert.AreEqual(EDBDbType.Bytea, p.EDBDbType, "#J2");
            p.Value = null;
            Assert.AreEqual(EDBDbType.Bytea, p.EDBDbType, "#J3");
            p.Value = DBNull.Value;
            Assert.AreEqual(EDBDbType.Bytea, p.EDBDbType, "#J4");
        }

        [Test]
        [Ignore("")]
        public void ParameterName()
        {
            var p = new EDBParameter();
            p.ParameterName = "name";
            Assert.AreEqual("name", p.ParameterName, "#A:ParameterName");
            Assert.AreEqual(string.Empty, p.SourceColumn, "#A:SourceColumn");

            p.ParameterName = null;
            Assert.AreEqual(string.Empty, p.ParameterName, "#B:ParameterName");
            Assert.AreEqual(string.Empty, p.SourceColumn, "#B:SourceColumn");

            p.ParameterName = " ";
            Assert.AreEqual(" ", p.ParameterName, "#C:ParameterName");
            Assert.AreEqual(string.Empty, p.SourceColumn, "#C:SourceColumn");

            p.ParameterName = " name ";
            Assert.AreEqual(" name ", p.ParameterName, "#D:ParameterName");
            Assert.AreEqual(string.Empty, p.SourceColumn, "#D:SourceColumn");

            p.ParameterName = string.Empty;
            Assert.AreEqual(string.Empty, p.ParameterName, "#E:ParameterName");
            Assert.AreEqual(string.Empty, p.SourceColumn, "#E:SourceColumn");
        }

#if NET_2_0
        [Test]
        public void ResetDbType()
        {
            EDBParameter p;

            //Parameter with an assigned value but no DbType specified
            p = new EDBParameter("foo", 42);
            p.ResetDbType();
            Assert.AreEqual(DbType.Int32, p.DbType, "#A:DbType");
            Assert.AreEqual(EDBDbType.Integer, p.EDBDbType, "#A:EDBDbType");
            Assert.AreEqual(42, p.Value, "#A:Value");

            p.DbType = DbType.DateTime; //assigning a DbType
            Assert.AreEqual(DbType.DateTime, p.DbType, "#B:DbType1");
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#B:SqlDbType1");
            p.ResetDbType();
            Assert.AreEqual(DbType.Int32, p.DbType, "#B:DbType2");
            Assert.AreEqual(EDBDbType.Integer, p.EDBDbType, "#B:SqlDbtype2");

            //Parameter with an assigned EDBDbType but no specified value
            p = new EDBParameter("foo", EDBDbType.Integer);
            p.ResetDbType();
            Assert.AreEqual(DbType.Object, p.DbType, "#C:DbType");
            Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "#C:EDBDbType");

            p.DbType = DbType.DateTime; //assigning a EDBDbType
            Assert.AreEqual(DbType.DateTime, p.DbType, "#D:DbType1");
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#D:SqlDbType1");
            p.ResetDbType();
            Assert.AreEqual(DbType.Object, p.DbType, "#D:DbType2");
            Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "#D:SqlDbType2");

            p = new EDBParameter();
            p.Value = DateTime.MaxValue;
            Assert.AreEqual(DbType.DateTime, p.DbType, "#E:DbType1");
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#E:SqlDbType1");
            p.Value = null;
            p.ResetDbType();
            Assert.AreEqual(DbType.Object, p.DbType, "#E:DbType2");
            Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "#E:SqlDbType2");

            p = new EDBParameter("foo", EDBDbType.Varchar);
            p.Value = DateTime.MaxValue;
            p.ResetDbType();
            Assert.AreEqual(DbType.DateTime, p.DbType, "#F:DbType");
            Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#F:EDBDbType");
            Assert.AreEqual(DateTime.MaxValue, p.Value, "#F:Value");

            p = new EDBParameter("foo", EDBDbType.Varchar);
            p.Value = DBNull.Value;
            p.ResetDbType();
            Assert.AreEqual(DbType.Object, p.DbType, "#G:DbType");
            Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "#G:EDBDbType");
            Assert.AreEqual(DBNull.Value, p.Value, "#G:Value");

            p = new EDBParameter("foo", EDBDbType.Varchar);
            p.Value = null;
            p.ResetDbType();
            Assert.AreEqual(DbType.Object, p.DbType, "#G:DbType");
            Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "#G:EDBDbType");
            Assert.IsNull(p.Value, "#G:Value");
        }

#endif

        [Test]
        [Ignore("")]
        public void SourceColumn()
        {
            var p = new EDBParameter();
            p.SourceColumn = "name";
            Assert.AreEqual(string.Empty, p.ParameterName, "#A:ParameterName");
            Assert.AreEqual("name", p.SourceColumn, "#A:SourceColumn");

            p.SourceColumn = null;
            Assert.AreEqual(string.Empty, p.ParameterName, "#B:ParameterName");
            Assert.AreEqual(string.Empty, p.SourceColumn, "#B:SourceColumn");

            p.SourceColumn = " ";
            Assert.AreEqual(string.Empty, p.ParameterName, "#C:ParameterName");
            Assert.AreEqual(" ", p.SourceColumn, "#C:SourceColumn");

            p.SourceColumn = " name ";
            Assert.AreEqual(string.Empty, p.ParameterName, "#D:ParameterName");
            Assert.AreEqual(" name ", p.SourceColumn, "#D:SourceColumn");

            p.SourceColumn = string.Empty;
            Assert.AreEqual(string.Empty, p.ParameterName, "#E:ParameterName");
            Assert.AreEqual(string.Empty, p.SourceColumn, "#E:SourceColumn");
        }

        [Test]
        public void Bug1011100EDBDbTypeTest()
        {
            var p = new EDBParameter();
            p.Value = DBNull.Value;
            Assert.AreEqual(DbType.Object, p.DbType, "#A:DbType");
            Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "#A:EDBDbType");

            // Now change parameter value.
            // Note that as we didn't explicitly specified a dbtype, the dbtype property should change when
            // the value changes...

            p.Value = 8;

            Assert.AreEqual(DbType.Int32, p.DbType, "#A:DbType");
            Assert.AreEqual(EDBDbType.Integer, p.EDBDbType, "#A:EDBDbType");

            //Assert.AreEqual(3510, p.Value, "#A:Value");
            //p.EDBDbType = EDBDbType.Varchar;
            //Assert.AreEqual(DbType.String, p.DbType, "#B:DbType");
            //Assert.AreEqual(EDBDbType.Varchar, p.EDBDbType, "#B:EDBDbType");
            //Assert.AreEqual(3510, p.Value, "#B:Value");
        }

        [Test]
        public void ParameterCollectionHashLookupParameterRenameBug()
        {
            using (var command = new EDBCommand())
            {
                // Put plenty of parameters in the collection to turn on hash lookup functionality.
                for (int i = 0 ; i < 10 ; i++)
                {
                    command.Parameters.AddWithValue(string.Format("p{0:00}", i + 1), EDBDbType.Text, string.Format("String parameter value {0}", i + 1));
                }

                // Make sure both hash lookups have been generated.
                Assert.AreEqual(command.Parameters["p03"].ParameterName, "p03");
                Assert.AreEqual(command.Parameters["P03"].ParameterName, "p03");

                // Rename the target parameter.
                command.Parameters["p03"].ParameterName = "a_new_name";

                try
                {
                    // Try to exploit the hash lookup bug.
                    // If the bug exists, the hash lookups will be out of sync with the list, and be unable
                    // to find the parameter by its new name.
                    Assert.IsTrue(command.Parameters.IndexOf("a_new_name") >= 0);
                }
                catch (Exception e)
                {
                    throw new Exception("EDBParameterCollection hash lookup/parameter rename bug detected", e);
                }
            }
        }

        [Test]
        public void EDBParameterCloneTest()
        {

            var param = new EDBParameter();

            param.Value = 5;
            param.Precision = 1;
            param.Scale = 1;
            param.Size = 1;
            param.Direction = ParameterDirection.Input;
            param.IsNullable = true;
            param.ParameterName = "parameterName";
            param.SourceColumn = "source_column";
#if NET451
            param.SourceVersion = DataRowVersion.Current;
#endif
            param.EDBValue = 5;
            param.SourceColumnNullMapping = false;

            var newParam = param.Clone();

            Assert.AreEqual(param.Value, newParam.Value);
            Assert.AreEqual(param.Precision, newParam.Precision);
            Assert.AreEqual(param.Scale, newParam.Scale);
            Assert.AreEqual(param.Size, newParam.Size);
            Assert.AreEqual(param.Direction, newParam.Direction);
            Assert.AreEqual(param.IsNullable, newParam.IsNullable);
            Assert.AreEqual(param.ParameterName, newParam.ParameterName);
            Assert.AreEqual(param.SourceColumn, newParam.SourceColumn);
#if NET451
            Assert.AreEqual(param.SourceVersion, newParam.SourceVersion);
#endif
            Assert.AreEqual(param.EDBValue, newParam.EDBValue);
            Assert.AreEqual(param.SourceColumnNullMapping, newParam.SourceColumnNullMapping);
            Assert.AreEqual(param.EDBValue, newParam.EDBValue);

        }

        [Test]
        public void CleanName()
        {
            var param = new EDBParameter();
            var command = new EDBCommand();
            command.Parameters.Add(param);

            param.ParameterName = "";

            // These should not throw exceptions
            Assert.AreEqual(0, command.Parameters.IndexOf(""));
            Assert.AreEqual("", param.CleanName);
        }
    }
}
