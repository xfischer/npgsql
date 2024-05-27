using EDBTypes;
using NUnit.Framework;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Internal.Postgres;

namespace EnterpriseDB.EDBClient.Tests;

public class EDBParameterTest : TestBase
{
    [Test, Description("Makes sure that when EDBDbType or Value/EDBValue are set, DbType and EDBDbType are set accordingly")]
    public void Implicit_setting_of_DbType()
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

    [Test]
    public void DataTypeName()
    {
        using var conn = OpenConnection();
        using var cmd = new EDBCommand("SELECT @p", conn);
        var p1 = new EDBParameter { ParameterName = "p", Value = 8, DataTypeName = "integer" };
        cmd.Parameters.Add(p1);
        Assert.That(cmd.ExecuteScalar(), Is.EqualTo(8));
        // Purposefully try to send int as string, which should fail. This makes sure
        // the above doesn't work simply because of type inference from the CLR type.
        p1.DataTypeName = "text";
        Assert.That(() => cmd.ExecuteScalar(), Throws.Exception.TypeOf<InvalidCastException>());

        cmd.Parameters.Clear();

        var p2 = new EDBParameter<int> { ParameterName = "p", TypedValue = 8, DataTypeName = "integer" };
        cmd.Parameters.Add(p2);
        Assert.That(cmd.ExecuteScalar(), Is.EqualTo(8));
        // Purposefully try to send int as string, which should fail. This makes sure
        // the above doesn't work simply because of type inference from the CLR type.
        p2.DataTypeName = "text";
        Assert.That(() => cmd.ExecuteScalar(), Throws.Exception.TypeOf<InvalidCastException>());
    }

    [Test]
    public void Positional_parameter_is_positional()
    {
        var p = new EDBParameter(EDBParameter.PositionalName, 1);
        Assert.That(p.IsPositional, Is.True);

        var p2 = new EDBParameter(null, 1);
        Assert.That(p2.IsPositional, Is.True);
    }

    [Test]
    public void Infer_data_type_name_from_EDBDbType()
    {
        var p = new EDBParameter("par_field1", EDBDbType.Varchar, 50);
        Assert.That(p.DataTypeName, Is.EqualTo("character varying"));
    }

    [Test]
    public void Infer_data_type_name_from_DbType()
    {
        var p = new EDBParameter("par_field1", DbType.String , 50);
        Assert.That(p.DataTypeName, Is.EqualTo("text"));
    }

    [Test]
    public void Infer_data_type_name_from_EDBDbType_for_array()
    {
        var p = new EDBParameter("int_array", EDBDbType.Array | EDBDbType.Integer);
        Assert.That(p.DataTypeName, Is.EqualTo("integer[]"));
    }

    [Test]
    public void Infer_data_type_name_from_EDBDbType_for_built_in_range()
    {
        var p = new EDBParameter("numeric_range", EDBDbType.Range | EDBDbType.Numeric);
        Assert.That(p.DataTypeName, Is.EqualTo("numrange"));
    }

    [Test]
    public void Cannot_infer_data_type_name_from_EDBDbType_for_unknown_range()
    {
        var p = new EDBParameter("text_range", EDBDbType.Range | EDBDbType.Text);
        Assert.That(p.DataTypeName, Is.EqualTo(null));
    }

    [Test]
    public void Infer_data_type_name_from_ClrType()
    {
        var p = new EDBParameter("p1", Array.Empty<byte>());
        Assert.That(p.DataTypeName, Is.EqualTo("bytea"));
    }

    [Test]
    public void Setting_DbType_sets_EDBDbType()
    {
        var p = new EDBParameter();
        p.DbType = DbType.Binary;
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Bytea));
    }

    [Test]
    public void Setting_EDBDbType_sets_DbType()
    {
        var p = new EDBParameter();
        p.EDBDbType = EDBDbType.Bytea;
        Assert.That(p.DbType, Is.EqualTo(DbType.Binary));
    }

    [Test]
    public void Setting_value_does_not_change_DbType()
    {
        var p = new EDBParameter { DbType = DbType.String, EDBDbType = EDBDbType.Bytea };
        p.Value = 8;
        Assert.That(p.DbType, Is.EqualTo(DbType.Binary));
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Bytea));
    }

    // Older tests

    #region Constructors

    [Test]
    public void Constructor1()
    {
        var p = new EDBParameter();
        Assert.AreEqual(DbType.Object, p.DbType, "DbType");
        Assert.AreEqual(ParameterDirection.Input, p.Direction, "Direction");
        Assert.IsFalse(p.IsNullable, "IsNullable");
        Assert.AreEqual(string.Empty, p.ParameterName, "ParameterName");
        Assert.AreEqual(0, p.Precision, "Precision");
        Assert.AreEqual(0, p.Scale, "Scale");
        Assert.AreEqual(0, p.Size, "Size");
        Assert.AreEqual(string.Empty, p.SourceColumn, "SourceColumn");
        Assert.AreEqual(DataRowVersion.Current, p.SourceVersion, "SourceVersion");
        Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "EDBDbType");
        Assert.IsNull(p.Value, "Value");
    }

    [Test]
    public void Constructor2_Value_DateTime()
    {
        var value = new DateTime(2004, 8, 24);

        var p = new EDBParameter("address", value);
        Assert.AreEqual(DbType.DateTime2, p.DbType, "B:DbType");
        Assert.AreEqual(ParameterDirection.Input, p.Direction, "B:Direction");
        Assert.IsFalse(p.IsNullable, "B:IsNullable");
        Assert.AreEqual("address", p.ParameterName, "B:ParameterName");
        Assert.AreEqual(0, p.Precision, "B:Precision");
        Assert.AreEqual(0, p.Scale, "B:Scale");
        //Assert.AreEqual (0, p.Size, "B:Size");
        Assert.AreEqual(string.Empty, p.SourceColumn, "B:SourceColumn");
        Assert.AreEqual(DataRowVersion.Current, p.SourceVersion, "B:SourceVersion");
        Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "B:EDBDbType");
        Assert.AreEqual(value, p.Value, "B:Value");
    }

    [Test]
    public void Constructor2_Value_DBNull()
    {
        var p = new EDBParameter("address", DBNull.Value);
        Assert.AreEqual(DbType.Object, p.DbType, "B:DbType");
        Assert.AreEqual(ParameterDirection.Input, p.Direction, "B:Direction");
        Assert.IsFalse(p.IsNullable, "B:IsNullable");
        Assert.AreEqual("address", p.ParameterName, "B:ParameterName");
        Assert.AreEqual(0, p.Precision, "B:Precision");
        Assert.AreEqual(0, p.Scale, "B:Scale");
        Assert.AreEqual(0, p.Size, "B:Size");
        Assert.AreEqual(string.Empty, p.SourceColumn, "B:SourceColumn");
        Assert.AreEqual(DataRowVersion.Current, p.SourceVersion, "B:SourceVersion");
        Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "B:EDBDbType");
        Assert.AreEqual(DBNull.Value, p.Value, "B:Value");
    }

    [Test]
    public void Constructor2_Value_null()
    {
        var p = new EDBParameter("address", null);
        Assert.AreEqual(DbType.Object, p.DbType, "A:DbType");
        Assert.AreEqual(ParameterDirection.Input, p.Direction, "A:Direction");
        Assert.IsFalse(p.IsNullable, "A:IsNullable");
        Assert.AreEqual("address", p.ParameterName, "A:ParameterName");
        Assert.AreEqual(0, p.Precision, "A:Precision");
        Assert.AreEqual(0, p.Scale, "A:Scale");
        Assert.AreEqual(0, p.Size, "A:Size");
        Assert.AreEqual(string.Empty, p.SourceColumn, "A:SourceColumn");
        Assert.AreEqual(DataRowVersion.Current, p.SourceVersion, "A:SourceVersion");
        Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "A:EDBDbType");
        Assert.IsNull(p.Value, "A:Value");
    }

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

    [Test]
    public void Clone()
    {
        var expected = new EDBParameter
        {
            Value = 42,
            ParameterName = "TheAnswer",

            DbType = DbType.Int32,
            EDBDbType = EDBDbType.Integer,
            DataTypeName = "integer",

            Direction = ParameterDirection.InputOutput,
            IsNullable = true,
            Precision = 1,
            Scale = 2,
            Size = 4,

            SourceVersion = DataRowVersion.Proposed,
            SourceColumn = "source",
            SourceColumnNullMapping = true,
        };
        var actual = expected.Clone();

        Assert.AreEqual(expected.Value, actual.Value);
        Assert.AreEqual(expected.ParameterName, actual.ParameterName);

        Assert.AreEqual(expected.DbType, actual.DbType);
        Assert.AreEqual(expected.EDBDbType, actual.EDBDbType);
        Assert.AreEqual(expected.DataTypeName, actual.DataTypeName);

        Assert.AreEqual(expected.Direction, actual.Direction);
        Assert.AreEqual(expected.IsNullable, actual.IsNullable);
        Assert.AreEqual(expected.Precision, actual.Precision);
        Assert.AreEqual(expected.Scale, actual.Scale);
        Assert.AreEqual(expected.Size, actual.Size);

        Assert.AreEqual(expected.SourceVersion, actual.SourceVersion);
        Assert.AreEqual(expected.SourceColumn, actual.SourceColumn);
        Assert.AreEqual(expected.SourceColumnNullMapping, actual.SourceColumnNullMapping);
    }

    [Test]
    public void Clone_generic()
    {
        var expected = new EDBParameter<int>
        {
            TypedValue = 42,
            ParameterName = "TheAnswer",

            DbType = DbType.Int32,
            EDBDbType = EDBDbType.Integer,
            DataTypeName = "integer",

            Direction = ParameterDirection.InputOutput,
            IsNullable = true,
            Precision = 1,
            Scale = 2,
            Size = 4,

            SourceVersion = DataRowVersion.Proposed,
            SourceColumn ="source",
            SourceColumnNullMapping = true,
        };
        var actual = (EDBParameter<int>)expected.Clone();

        Assert.AreEqual(expected.Value, actual.Value);
        Assert.AreEqual(expected.TypedValue, actual.TypedValue);
        Assert.AreEqual(expected.ParameterName, actual.ParameterName);

        Assert.AreEqual(expected.DbType, actual.DbType);
        Assert.AreEqual(expected.EDBDbType, actual.EDBDbType);
        Assert.AreEqual(expected.DataTypeName, actual.DataTypeName);

        Assert.AreEqual(expected.Direction, actual.Direction);
        Assert.AreEqual(expected.IsNullable, actual.IsNullable);
        Assert.AreEqual(expected.Precision, actual.Precision);
        Assert.AreEqual(expected.Scale, actual.Scale);
        Assert.AreEqual(expected.Size, actual.Size);

        Assert.AreEqual(expected.SourceVersion, actual.SourceVersion);
        Assert.AreEqual(expected.SourceColumn, actual.SourceColumn);
        Assert.AreEqual(expected.SourceColumnNullMapping, actual.SourceColumnNullMapping);
    }

    #endregion

    [Test]
    [Ignore("Ignored in community")]
    public void InferType_invalid_throws()
    {
        var notsupported = new object[]
        {
            ushort.MaxValue,
            uint.MaxValue,
            ulong.MaxValue,
            sbyte.MaxValue,
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

    [Test] // bug #320196
    public void Parameter_null()
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
    [Ignore("Ignored in community")]
    public void Parameter_type()
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
        Assert.AreEqual(DbType.String, p.DbType, "#D1");
        Assert.AreEqual(EDBDbType.Text, p.EDBDbType, "#D2");
        p.Value = new byte[] { 0x0a };
        Assert.AreEqual(DbType.Binary, p.DbType, "#E1");
        Assert.AreEqual(EDBDbType.Bytea, p.EDBDbType, "#E2");
        p.Value = null;
        Assert.AreEqual(DbType.String, p.DbType, "#F1");
        Assert.AreEqual(EDBDbType.Text, p.EDBDbType, "#F2");
        p.Value = DateTime.Now;
        Assert.AreEqual(DbType.DateTime, p.DbType, "#G1");
        Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#G2");
        p.Value = null;
        Assert.AreEqual(DbType.String, p.DbType, "#H1");
        Assert.AreEqual(EDBDbType.Text, p.EDBDbType, "#H2");

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

    [Test, IssueLink("https://github.com/npgsql/npgsql/issues/5428")]
    public async Task Match_param_index_case_insensitively()
    {
        await using var conn = await OpenConnectionAsync();
        await using var cmd = new EDBCommand("SELECT @p,@P", conn);
        cmd.Parameters.AddWithValue("p", "Hello world");
        await cmd.ExecuteNonQueryAsync();
    }

    [Test]
    [Ignore("Ignored in community")]
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
        Assert.AreEqual(EDBDbType.TimestampTz, p.EDBDbType, "#B:SqlDbType1");
        p.ResetDbType();
        Assert.AreEqual(DbType.Int32, p.DbType, "#B:DbType2");
        Assert.AreEqual(EDBDbType.Integer, p.EDBDbType, "#B:SqlDbtype2");

        //Parameter with an assigned EDBDbType but no specified value
        p = new EDBParameter("foo", EDBDbType.Integer);
        p.ResetDbType();
        Assert.AreEqual(DbType.Object, p.DbType, "#C:DbType");
        Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "#C:EDBDbType");

        p.EDBDbType = EDBDbType.TimestampTz; //assigning a EDBDbType
        Assert.AreEqual(DbType.DateTime, p.DbType, "#D:DbType1");
        Assert.AreEqual(EDBDbType.TimestampTz, p.EDBDbType, "#D:SqlDbType1");
        p.ResetDbType();
        Assert.AreEqual(DbType.Object, p.DbType, "#D:DbType2");
        Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "#D:SqlDbType2");

        p = new EDBParameter();
        p.Value = DateTime.MaxValue;
        Assert.AreEqual(DbType.DateTime2, p.DbType, "#E:DbType1");
        Assert.AreEqual(EDBDbType.Timestamp, p.EDBDbType, "#E:SqlDbType1");
        p.Value = null;
        p.ResetDbType();
        Assert.AreEqual(DbType.Object, p.DbType, "#E:DbType2");
        Assert.AreEqual(EDBDbType.Unknown, p.EDBDbType, "#E:SqlDbType2");

        p = new EDBParameter("foo", EDBDbType.Varchar);
        p.Value = DateTime.MaxValue;
        p.ResetDbType();
        Assert.AreEqual(DbType.DateTime2, p.DbType, "#F:DbType");
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

    [Test]
    public void ParameterName_retains_prefix()
        => Assert.That(new EDBParameter("@p", DbType.String).ParameterName, Is.EqualTo("@p"));

    [Test]
    [Ignore("Ignored in community")]
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
    public void Bug1011100_EDBDbType()
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
    public void EDBParameter_Clone()
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
        param.SourceVersion = DataRowVersion.Current;
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
        Assert.AreEqual(param.TrimmedName, newParam.TrimmedName);
        Assert.AreEqual(param.SourceColumn, newParam.SourceColumn);
        Assert.AreEqual(param.SourceVersion, newParam.SourceVersion);
        Assert.AreEqual(param.EDBValue, newParam.EDBValue);
        Assert.AreEqual(param.SourceColumnNullMapping, newParam.SourceColumnNullMapping);
        Assert.AreEqual(param.EDBValue, newParam.EDBValue);

    }

    [Test]
    public void Precision_via_interface()
    {
        var parameter = new EDBParameter();
        var paramIface = (IDbDataParameter)parameter;

        paramIface.Precision = 42;

        Assert.AreEqual((byte)42, paramIface.Precision);
    }

    [Test]
    public void Precision_via_base_class()
    {
        var parameter = new EDBParameter();
        var paramBase = (DbParameter)parameter;

        paramBase.Precision = 42;

        Assert.AreEqual((byte)42, paramBase.Precision);
    }

    [Test]
    public void Scale_via_interface()
    {
        var parameter = new EDBParameter();
        var paramIface = (IDbDataParameter)parameter;

        paramIface.Scale = 42;

        Assert.AreEqual((byte)42, paramIface.Scale);
    }

    [Test]
    public void Scale_via_base_class()
    {
        var parameter = new EDBParameter();
        var paramBase = (DbParameter)parameter;

        paramBase.Scale = 42;

        Assert.AreEqual((byte)42, paramBase.Scale);
    }

    [Test]
    public void Null_value_throws()
    {
        using var connection = OpenConnection();
        using var command = new EDBCommand("SELECT @p", connection)
        {
            Parameters = { new EDBParameter("p", null) }
        };

        Assert.That(() => command.ExecuteReader(), Throws.InvalidOperationException);
    }

    [Test]
    public void Null_value_with_nullable_type()
    {
        using var connection = OpenConnection();
        using var command = new EDBCommand("SELECT @p", connection)
        {
            Parameters = { new EDBParameter<int?>("p", null) }
        };
        using var reader = command.ExecuteReader();

        Assert.That(reader.Read(), Is.True);
        Assert.That(reader.GetFieldValue<int?>(0), Is.Null);
    }

    [Test]
    public void DBNull_reuses_type_info([Values]bool generic)
    {
        // Bootstrap datasource.
        using (var _ = OpenConnection()) {}

        var param = generic ? new EDBParameter<object> { Value = "value" } : new EDBParameter { Value = "value" };
        param.ResolveTypeInfo(DataSource.SerializerOptions);
        param.GetResolutionInfo(out var typeInfo, out _, out _);
        Assert.That(typeInfo, Is.Not.Null);

        // Make sure we don't reset the type info when setting DBNull.
        param.Value = DBNull.Value;
        param.GetResolutionInfo(out var secondTypeInfo, out _, out _);
        Assert.That(typeInfo, Is.SameAs(secondTypeInfo));

        // Make sure we don't resolve a different type info either.
        param.ResolveTypeInfo(DataSource.SerializerOptions);
        param.GetResolutionInfo(out var thirdTypeInfo, out _, out _);
        Assert.That(secondTypeInfo, Is.SameAs(thirdTypeInfo));
    }

    [Test]
    public void DBNull_followed_by_non_null_reresolves([Values]bool generic)
    {
        // Bootstrap datasource.
        using (var _ = OpenConnection()) {}

        var param = generic ? new EDBParameter<object> { Value = DBNull.Value } : new EDBParameter { Value = DBNull.Value };
        param.ResolveTypeInfo(DataSource.SerializerOptions);
        param.GetResolutionInfo(out var typeInfo, out _, out var pgTypeId);
        Assert.That(typeInfo, Is.Not.Null);
        Assert.That(pgTypeId.IsUnspecified, Is.True);

        param.Value = "value";
        param.GetResolutionInfo(out var secondTypeInfo, out _, out _);
        Assert.That(secondTypeInfo, Is.Null);

        // Make sure we don't resolve the same type info either.
        param.ResolveTypeInfo(DataSource.SerializerOptions);
        param.GetResolutionInfo(out var thirdTypeInfo, out _, out _);
        Assert.That(typeInfo, Is.Not.SameAs(thirdTypeInfo));
    }

    [Test]
    public void Changing_value_type_reresolves([Values]bool generic)
    {
        // Bootstrap datasource.
        using (var _ = OpenConnection()) {}

        var param = generic ? new EDBParameter<object> { Value = "value" } : new EDBParameter { Value = "value" };
        param.ResolveTypeInfo(DataSource.SerializerOptions);
        param.GetResolutionInfo(out var typeInfo, out _, out _);
        Assert.That(typeInfo, Is.Not.Null);

        param.Value = 1;
        param.GetResolutionInfo(out var secondTypeInfo, out _, out _);
        Assert.That(secondTypeInfo, Is.Null);

        // Make sure we don't resolve a different type info either.
        param.ResolveTypeInfo(DataSource.SerializerOptions);
        param.GetResolutionInfo(out var thirdTypeInfo, out _, out _);
        Assert.That(typeInfo, Is.Not.SameAs(thirdTypeInfo));
    }

#if NeedsPorting
    [Test]
    [Category ("NotWorking")]
    public void InferType_Char()
    {
        Char value = 'X';

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
    }

    [Test]
    [Category ("NotWorking")]
    public void InferType_CharArray()
    {
        Char[] value = new Char[] { 'A', 'X' };

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
    }

    [Test]
    public void InferType_Object()
    {
        Object value = new Object();

        EDBParameter param = new EDBParameter();
        param.Value = value;
        Assert.AreEqual(EDBDbType.Variant, param.EDBDbType, "#1");
        Assert.AreEqual(DbType.Object, param.DbType, "#2");
    }

    [Test]
    public void LocaleId ()
    {
        EDBParameter parameter = new EDBParameter ();
        Assert.AreEqual (0, parameter.LocaleId, "#1");
        parameter.LocaleId = 15;
        Assert.AreEqual(15, parameter.LocaleId, "#2");
    }
#endif
}
