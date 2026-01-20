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
        var p = new EDBParameter { DbType = DbType.Binary, EDBDbType = EDBDbType.Bytea };
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
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "DbType");
        Assert.That(p.Direction, Is.EqualTo(ParameterDirection.Input), "Direction");
        Assert.That(p.IsNullable, Is.False, "IsNullable");
        Assert.That(p.ParameterName, Is.Empty, "ParameterName");
        Assert.That(p.Precision, Is.EqualTo(0), "Precision");
        Assert.That(p.Scale, Is.EqualTo(0), "Scale");
        Assert.That(p.Size, Is.EqualTo(0), "Size");
        Assert.That(p.SourceColumn, Is.Empty, "SourceColumn");
        Assert.That(p.SourceVersion, Is.EqualTo(DataRowVersion.Current), "SourceVersion");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "EDBDbType");
        Assert.That(p.Value, Is.Null, "Value");
    }

    [Test]
    public void Constructor2_Value_DateTime()
    {
        var value = new DateTime(2004, 8, 24);

        var p = new EDBParameter("address", value);
        Assert.That(p.DbType, Is.EqualTo(DbType.DateTime2), "B:DbType");
        Assert.That(p.Direction, Is.EqualTo(ParameterDirection.Input), "B:Direction");
        Assert.That(p.IsNullable, Is.False, "B:IsNullable");
        Assert.That(p.ParameterName, Is.EqualTo("address"), "B:ParameterName");
        Assert.That(p.Precision, Is.EqualTo(0), "B:Precision");
        Assert.That(p.Scale, Is.EqualTo(0), "B:Scale");
        //Assert.AreEqual (0, p.Size, "B:Size");
        Assert.That(p.SourceColumn, Is.Empty, "B:SourceColumn");
        Assert.That(p.SourceVersion, Is.EqualTo(DataRowVersion.Current), "B:SourceVersion");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Timestamp), "B:EDBDbType");
        Assert.That(p.Value, Is.EqualTo(value), "B:Value");
    }

    [Test]
    public void Constructor2_Value_DBNull()
    {
        var p = new EDBParameter("address", DBNull.Value);
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "B:DbType");
        Assert.That(p.Direction, Is.EqualTo(ParameterDirection.Input), "B:Direction");
        Assert.That(p.IsNullable, Is.False, "B:IsNullable");
        Assert.That(p.ParameterName, Is.EqualTo("address"), "B:ParameterName");
        Assert.That(p.Precision, Is.EqualTo(0), "B:Precision");
        Assert.That(p.Scale, Is.EqualTo(0), "B:Scale");
        Assert.That(p.Size, Is.EqualTo(0), "B:Size");
        Assert.That(p.SourceColumn, Is.Empty, "B:SourceColumn");
        Assert.That(p.SourceVersion, Is.EqualTo(DataRowVersion.Current), "B:SourceVersion");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "B:EDBDbType");
        Assert.That(p.Value, Is.EqualTo(DBNull.Value), "B:Value");
    }

    [Test]
    public void Constructor2_Value_null()
    {
        var p = new EDBParameter("address", null);
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "A:DbType");
        Assert.That(p.Direction, Is.EqualTo(ParameterDirection.Input), "A:Direction");
        Assert.That(p.IsNullable, Is.False, "A:IsNullable");
        Assert.That(p.ParameterName, Is.EqualTo("address"), "A:ParameterName");
        Assert.That(p.Precision, Is.EqualTo(0), "A:Precision");
        Assert.That(p.Scale, Is.EqualTo(0), "A:Scale");
        Assert.That(p.Size, Is.EqualTo(0), "A:Size");
        Assert.That(p.SourceColumn, Is.Empty, "A:SourceColumn");
        Assert.That(p.SourceVersion, Is.EqualTo(DataRowVersion.Current), "A:SourceVersion");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "A:EDBDbType");
        Assert.That(p.Value, Is.Null, "A:Value");
    }

    [Test]
    //.ctor (String, EDBDbType, Int32, String, ParameterDirection, bool, byte, byte, DataRowVersion, object)
    public void Constructor7()
    {
        var p1 = new EDBParameter("p1Name", EDBDbType.Varchar, 20,
            "srcCol", ParameterDirection.InputOutput, false, 0, 0,
            DataRowVersion.Original, "foo");
        Assert.That(p1.DbType, Is.EqualTo(DbType.String), "DbType");
        Assert.That(p1.Direction, Is.EqualTo(ParameterDirection.InputOutput), "Direction");
        Assert.That(p1.IsNullable, Is.EqualTo(false), "IsNullable");
        //Assert.AreEqual (999, p1.LocaleId, "#");
        Assert.That(p1.ParameterName, Is.EqualTo("p1Name"), "ParameterName");
        Assert.That(p1.Precision, Is.EqualTo(0), "Precision");
        Assert.That(p1.Scale, Is.EqualTo(0), "Scale");
        Assert.That(p1.Size, Is.EqualTo(20), "Size");
        Assert.That(p1.SourceColumn, Is.EqualTo("srcCol"), "SourceColumn");
        Assert.That(p1.SourceColumnNullMapping, Is.EqualTo(false), "SourceColumnNullMapping");
        Assert.That(p1.SourceVersion, Is.EqualTo(DataRowVersion.Original), "SourceVersion");
        Assert.That(p1.EDBDbType, Is.EqualTo(EDBDbType.Varchar), "EDBDbType");
        //Assert.AreEqual (3210, p1.EDBValue, "#");
        Assert.That(p1.Value, Is.EqualTo("foo"), "Value");
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

        Assert.That(actual.Value, Is.EqualTo(expected.Value));
        Assert.That(actual.ParameterName, Is.EqualTo(expected.ParameterName));

        Assert.That(actual.DbType, Is.EqualTo(expected.DbType));
        Assert.That(actual.EDBDbType, Is.EqualTo(expected.EDBDbType));
        Assert.That(actual.DataTypeName, Is.EqualTo(expected.DataTypeName));

        Assert.That(actual.Direction, Is.EqualTo(expected.Direction));
        Assert.That(actual.IsNullable, Is.EqualTo(expected.IsNullable));
        Assert.That(actual.Precision, Is.EqualTo(expected.Precision));
        Assert.That(actual.Scale, Is.EqualTo(expected.Scale));
        Assert.That(actual.Size, Is.EqualTo(expected.Size));

        Assert.That(actual.SourceVersion, Is.EqualTo(expected.SourceVersion));
        Assert.That(actual.SourceColumn, Is.EqualTo(expected.SourceColumn));
        Assert.That(actual.SourceColumnNullMapping, Is.EqualTo(expected.SourceColumnNullMapping));
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

        Assert.That(actual.Value, Is.EqualTo(expected.Value));
        Assert.That(actual.TypedValue, Is.EqualTo(expected.TypedValue));
        Assert.That(actual.ParameterName, Is.EqualTo(expected.ParameterName));

        Assert.That(actual.DbType, Is.EqualTo(expected.DbType));
        Assert.That(actual.EDBDbType, Is.EqualTo(expected.EDBDbType));
        Assert.That(actual.DataTypeName, Is.EqualTo(expected.DataTypeName));

        Assert.That(actual.Direction, Is.EqualTo(expected.Direction));
        Assert.That(actual.IsNullable, Is.EqualTo(expected.IsNullable));
        Assert.That(actual.Precision, Is.EqualTo(expected.Precision));
        Assert.That(actual.Scale, Is.EqualTo(expected.Scale));
        Assert.That(actual.Size, Is.EqualTo(expected.Size));

        Assert.That(actual.SourceVersion, Is.EqualTo(expected.SourceVersion));
        Assert.That(actual.SourceColumn, Is.EqualTo(expected.SourceColumn));
        Assert.That(actual.SourceColumnNullMapping, Is.EqualTo(expected.SourceColumnNullMapping));
    }

    #endregion

    [Test] // bug #320196
    public void Parameter_null()
    {
        var param = new EDBParameter("param", EDBDbType.Numeric);
        Assert.That(param.Scale, Is.EqualTo(0), "#A1");
        param.Value = DBNull.Value;
        Assert.That(param.Scale, Is.EqualTo(0), "#A2");

        param = new EDBParameter("param", EDBDbType.Integer);
        Assert.That(param.Scale, Is.EqualTo(0), "#B1");
        param.Value = DBNull.Value;
        Assert.That(param.Scale, Is.EqualTo(0), "#B2");
    }

    [Test]
    public void Parameter_type()
    {
        EDBParameter p;

        // If Type is not set, then type is inferred from the value
        // assigned. The Type should be inferred everytime Value is assigned
        // If value is null or DBNull, then the current Type should be reset to Text.
        p = new EDBParameter { Value = "" };
        Assert.That(p.DbType, Is.EqualTo(DbType.String), "#A1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Text), "#A2");
        p.Value = DBNull.Value;
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#B1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#B2");
        p.Value = 1;
        Assert.That(p.DbType, Is.EqualTo(DbType.Int32), "#C1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Integer), "#C2");
        p.Value = DBNull.Value;
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#D1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#D2");
        p.Value = new byte[] { 0x0a };
        Assert.That(p.DbType, Is.EqualTo(DbType.Binary), "#E1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Bytea), "#E2");
        p.Value = null;
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#F1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#F2");
        p.Value = DateTime.Now;
        Assert.That(p.DbType, Is.EqualTo(DbType.DateTime2), "#G1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Timestamp), "#G2");
        p.Value = null;
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#H1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#H2");

        // If DbType is set, then the EDBDbType should not be
        // inferred from the value assigned.
        p = new EDBParameter();
        p.DbType = DbType.DateTime;
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz), "#I1");
        p.Value = 1;
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz), "#I2");
        p.Value = null;
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz), "#I3");
        p.Value = DBNull.Value;
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz), "#I4");

        // If EDBDbType is set, then the DbType should not be
        // inferred from the value assigned.
        p = new EDBParameter();
        p.EDBDbType = EDBDbType.Bytea;
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Bytea), "#J1");
        p.Value = 1;
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Bytea), "#J2");
        p.Value = null;
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Bytea), "#J3");
        p.Value = DBNull.Value;
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Bytea), "#J4");
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
    public void ParameterName()
    {
        var p = new EDBParameter();
        p.ParameterName = "name";
        Assert.That(p.ParameterName, Is.EqualTo("name"), "#A:ParameterName");
        Assert.That(p.SourceColumn, Is.Empty, "#A:SourceColumn");

        p.ParameterName = null;
        Assert.That(p.ParameterName, Is.Empty, "#B:ParameterName");
        Assert.That(p.SourceColumn, Is.Empty, "#B:SourceColumn");

        p.ParameterName = " ";
        Assert.That(p.ParameterName, Is.EqualTo(" "), "#C:ParameterName");
        Assert.That(p.SourceColumn, Is.Empty, "#C:SourceColumn");

        p.ParameterName = " name ";
        Assert.That(p.ParameterName, Is.EqualTo(" name "), "#D:ParameterName");
        Assert.That(p.SourceColumn, Is.Empty, "#D:SourceColumn");

        p.ParameterName = string.Empty;
        Assert.That(p.ParameterName, Is.Empty, "#E:ParameterName");
        Assert.That(p.SourceColumn, Is.Empty, "#E:SourceColumn");
    }

    [Test]
    public void ResetDbType()
    {
        EDBParameter p;

        //Parameter with an assigned value but no DbType specified
        p = new EDBParameter("foo", 42);
        p.ResetDbType();
        Assert.That(p.DbType, Is.EqualTo(DbType.Int32), "#A:DbType");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Integer), "#A:EDBDbType");
        Assert.That(p.Value, Is.EqualTo(42), "#A:Value");

        p.DbType = DbType.DateTime; //assigning a DbType
        Assert.That(p.DbType, Is.EqualTo(DbType.DateTime), "#B:DbType1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz), "#B:SqlDbType1");
        p.ResetDbType();
        Assert.That(p.DbType, Is.EqualTo(DbType.Int32), "#B:DbType2");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Integer), "#B:SqlDbtype2");

        //Parameter with an assigned EDBDbType but no specified value
        p = new EDBParameter("foo", EDBDbType.Integer);
        p.ResetDbType();
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#C:DbType");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#C:EDBDbType");

        p.EDBDbType = EDBDbType.TimestampTz; //assigning a EDBDbType
        Assert.That(p.DbType, Is.EqualTo(DbType.DateTime), "#D:DbType1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.TimestampTz), "#D:SqlDbType1");
        p.ResetDbType();
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#D:DbType2");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#D:SqlDbType2");

        p = new EDBParameter();
        p.Value = DateTime.MaxValue;
        Assert.That(p.DbType, Is.EqualTo(DbType.DateTime2), "#E:DbType1");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Timestamp), "#E:SqlDbType1");
        p.Value = null;
        p.ResetDbType();
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#E:DbType2");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#E:SqlDbType2");

        p = new EDBParameter("foo", EDBDbType.Varchar);
        p.Value = DateTime.MaxValue;
        p.ResetDbType();
        Assert.That(p.DbType, Is.EqualTo(DbType.DateTime2), "#F:DbType");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Timestamp), "#F:EDBDbType");
        Assert.That(p.Value, Is.EqualTo(DateTime.MaxValue), "#F:Value");

        p = new EDBParameter("foo", EDBDbType.Varchar);
        p.Value = DBNull.Value;
        p.ResetDbType();
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#G:DbType");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#G:EDBDbType");
        Assert.That(p.Value, Is.EqualTo(DBNull.Value), "#G:Value");

        p = new EDBParameter("foo", EDBDbType.Varchar);
        p.Value = null;
        p.ResetDbType();
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#G:DbType");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#G:EDBDbType");
        Assert.That(p.Value, Is.Null, "#G:Value");
    }

    [Test]
    public void ParameterName_retains_prefix()
        => Assert.That(new EDBParameter("@p", DbType.String).ParameterName, Is.EqualTo("@p"));

    [Test]
    public void SourceColumn()
    {
        var p = new EDBParameter();
        p.SourceColumn = "name";
        Assert.That(p.ParameterName, Is.Empty, "#A:ParameterName");
        Assert.That(p.SourceColumn, Is.EqualTo("name"), "#A:SourceColumn");

        p.SourceColumn = null;
        Assert.That(p.ParameterName, Is.Empty, "#B:ParameterName");
        Assert.That(p.SourceColumn, Is.Empty, "#B:SourceColumn");

        p.SourceColumn = " ";
        Assert.That(p.ParameterName, Is.Empty, "#C:ParameterName");
        Assert.That(p.SourceColumn, Is.EqualTo(" "), "#C:SourceColumn");

        p.SourceColumn = " name ";
        Assert.That(p.ParameterName, Is.Empty, "#D:ParameterName");
        Assert.That(p.SourceColumn, Is.EqualTo(" name "), "#D:SourceColumn");

        p.SourceColumn = string.Empty;
        Assert.That(p.ParameterName, Is.Empty, "#E:ParameterName");
        Assert.That(p.SourceColumn, Is.Empty, "#E:SourceColumn");
    }

    [Test]
    public void Bug1011100_EDBDbType()
    {
        var p = new EDBParameter();
        p.Value = DBNull.Value;
        Assert.That(p.DbType, Is.EqualTo(DbType.Object), "#A:DbType");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Unknown), "#A:EDBDbType");

        // Now change parameter value.
        // Note that as we didn't explicitly specified a dbtype, the dbtype property should change when
        // the value changes...

        p.Value = 8;

        Assert.That(p.DbType, Is.EqualTo(DbType.Int32), "#A:DbType");
        Assert.That(p.EDBDbType, Is.EqualTo(EDBDbType.Integer), "#A:EDBDbType");

        //Assert.That(3510, p.Value, "#A:Value");
        //p.EDBDbType = EDBDbType.Varchar;
        //Assert.That(DbType.String, p.DbType, "#B:DbType");
        //Assert.That(EDBDbType.Varchar, p.EDBDbType, "#B:EDBDbType");
        //Assert.That(3510, p.Value, "#B:Value");
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

        Assert.That(newParam.Value, Is.EqualTo(param.Value));
        Assert.That(newParam.Precision, Is.EqualTo(param.Precision));
        Assert.That(newParam.Scale, Is.EqualTo(param.Scale));
        Assert.That(newParam.Size, Is.EqualTo(param.Size));
        Assert.That(newParam.Direction, Is.EqualTo(param.Direction));
        Assert.That(newParam.IsNullable, Is.EqualTo(param.IsNullable));
        Assert.That(newParam.ParameterName, Is.EqualTo(param.ParameterName));
        Assert.That(newParam.TrimmedName, Is.EqualTo(param.TrimmedName));
        Assert.That(newParam.SourceColumn, Is.EqualTo(param.SourceColumn));
        Assert.That(newParam.SourceVersion, Is.EqualTo(param.SourceVersion));
        Assert.That(newParam.EDBValue, Is.EqualTo(param.EDBValue));
        Assert.That(newParam.SourceColumnNullMapping, Is.EqualTo(param.SourceColumnNullMapping));
        Assert.That(newParam.EDBValue, Is.EqualTo(param.EDBValue));

    }

    [Test]
    public void Precision_via_interface()
    {
        var parameter = new EDBParameter();
        var paramIface = (IDbDataParameter)parameter;

        paramIface.Precision = 42;

        Assert.That(paramIface.Precision, Is.EqualTo((byte)42));
    }

    [Test]
    public void Precision_via_base_class()
    {
        var parameter = new EDBParameter();
        var paramBase = (DbParameter)parameter;

        paramBase.Precision = 42;

        Assert.That(paramBase.Precision, Is.EqualTo((byte)42));
    }

    [Test]
    public void Scale_via_interface()
    {
        var parameter = new EDBParameter();
        var paramIface = (IDbDataParameter)parameter;

        paramIface.Scale = 42;

        Assert.That(paramIface.Scale, Is.EqualTo((byte)42));
    }

    [Test]
    public void Scale_via_base_class()
    {
        var parameter = new EDBParameter();
        var paramBase = (DbParameter)parameter;

        paramBase.Scale = 42;

        Assert.That(paramBase.Scale, Is.EqualTo((byte)42));
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
        var param = generic ? new EDBParameter<object> { Value = "value" } : new EDBParameter { Value = "value" };
        param.ResolveTypeInfo(DataSource.CurrentReloadableState.SerializerOptions, null);
        param.GetResolutionInfo(out var typeInfo, out _, out _);
        Assert.That(typeInfo, Is.Not.Null);

        // Make sure we don't reset the type info when setting DBNull.
        param.Value = DBNull.Value;
        param.GetResolutionInfo(out var secondTypeInfo, out _, out _);
        Assert.That(secondTypeInfo, Is.SameAs(typeInfo));

        // Make sure we don't resolve a different type info either.
        param.ResolveTypeInfo(DataSource.CurrentReloadableState.SerializerOptions, null);
        param.GetResolutionInfo(out var thirdTypeInfo, out _, out _);
        Assert.That(thirdTypeInfo, Is.SameAs(secondTypeInfo));
    }

    [Test]
    public void DBNull_followed_by_non_null_reresolves([Values]bool generic)
    {
        var param = generic ? new EDBParameter<object> { Value = DBNull.Value } : new EDBParameter { Value = DBNull.Value };
        param.ResolveTypeInfo(DataSource.CurrentReloadableState.SerializerOptions, null);
        param.GetResolutionInfo(out var typeInfo, out _, out var pgTypeId);
        Assert.That(typeInfo, Is.Not.Null);
        Assert.That(pgTypeId.IsUnspecified, Is.True);

        param.Value = "value";
        param.GetResolutionInfo(out var secondTypeInfo, out _, out _);
        Assert.That(secondTypeInfo, Is.Null);

        // Make sure we don't resolve the same type info either.
        param.ResolveTypeInfo(DataSource.CurrentReloadableState.SerializerOptions, null);
        param.GetResolutionInfo(out var thirdTypeInfo, out _, out _);
        Assert.That(thirdTypeInfo, Is.Not.SameAs(typeInfo));
    }

    [Test]
    public void Changing_value_type_reresolves([Values]bool generic)
    {
		var param = generic ? new EDBParameter<object> { Value = "value" } : new EDBParameter { Value = "value" };
        param.ResolveTypeInfo(DataSource.CurrentReloadableState.SerializerOptions, null);
        param.GetResolutionInfo(out var typeInfo, out _, out _);
        Assert.That(typeInfo, Is.Not.Null);

        param.Value = 1;
        param.GetResolutionInfo(out var secondTypeInfo, out _, out _);
        Assert.That(secondTypeInfo, Is.Null);

        // Make sure we don't resolve a different type info either.
        param.ResolveTypeInfo(DataSource.CurrentReloadableState.SerializerOptions, null);
        param.GetResolutionInfo(out var thirdTypeInfo, out _, out _);
        Assert.That(thirdTypeInfo, Is.Not.SameAs(typeInfo));
    }

    [Test]
    public void DataTypeName_prioritized_over_NpgsqlDbType([Values]bool generic)
    {
        var param = generic ? new EDBParameter<object>
        {
            EDBDbType = EDBDbType.Integer,
            DataTypeName = "text",
            Value = "value"
        } : new EDBParameter
        {
            EDBDbType = EDBDbType.Integer,
            DataTypeName = "text",
            Value = "value"
        };
        param.ResolveTypeInfo(DataSource.CurrentReloadableState.SerializerOptions, null);
        param.GetResolutionInfo(out var typeInfo, out _, out _);
        Assert.That(typeInfo, Is.Not.Null);
        Assert.That(typeInfo.PgTypeId, Is.EqualTo(DataSource.CurrentReloadableState.SerializerOptions.TextPgTypeId));
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
        Assert.That(EDBDbType.Variant, param.EDBDbType, "#1");
        Assert.That(DbType.Object, param.DbType, "#2");
    }

    [Test]
    public void LocaleId ()
    {
        EDBParameter parameter = new EDBParameter ();
        Assert.AreEqual (0, parameter.LocaleId, "#1");
        parameter.LocaleId = 15;
        Assert.That(15, parameter.LocaleId, "#2");
    }
#endif

    [OneTimeSetUp]
    public async Task Bootstrap()
    {
        // Bootstrap datasource.
        await using (var _ = await OpenConnectionAsync()) {}
    }
}
