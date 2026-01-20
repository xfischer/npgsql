using System;
using System.Data;
using EDBTypes;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

#pragma warning disable CS8602
[TestFixture]
[NonParallelizable]
public class EDBDataReaderTests : EPASTestBase
{

    private EDBConnection? con = null;
    string connectionString = string.Empty;

    [SetUp]
    protected void SetUp()
    {
        connectionString = ConnectionString;
        con = new EDBConnection(connectionString);
    }

    [TearDown]
    protected void TearDown()
    {
        if (con.State != ConnectionState.Closed)
            con.Close();
    }

    [Test]
    public void GetBoolean()
    {
        con.Open();

        var command = new EDBCommand("select * from tablea where field_serial = 4;", con);

        var dr = command.ExecuteReader();

        dr.Read();
        var result = dr.GetBoolean(4);
        Assert.That(result);

    }

    [Test]
    public void GetChars()
    {
        con.Open();
        var command = new EDBCommand("select * from tablea where field_serial = 1;", con);

        var dr = command.ExecuteReader();

        dr.Read();
        var result = new char[6];

        var a = dr.GetChars(1, 0, result, 0, 6);

        Assert.That(new string(result), Is.EqualTo("Random"));

    }

    [Test]
    public void GetInt32()
    {
        con.Open();
        var command = new EDBCommand("select * from tablea where field_serial = 2;", con);

        var dr = command.ExecuteReader();

        dr.Read();

        var result = dr.GetInt32(2);

        Assert.That(result, Is.EqualTo(4));

    }

    [Test]
    public void GetInt16()
    {
        con.Open();
        var command = new EDBCommand("select * from tableb where field_serial = 1;", con);

        var dr = command.ExecuteReader();

        dr.Read();

        var result = dr.GetInt16(1);

        Assert.That(result, Is.EqualTo(2));

    }

    [Test]
    public void GetDecimal()
    {
        con.Open();
        var command = new EDBCommand("select * from tableb where field_serial = 3;", con);

        var dr = command.ExecuteReader();

        dr.Read();

        var result = dr.GetDecimal(3);

        Assert.That(result, Is.EqualTo(4.2300000M));

    }

    [Test]
    public void GetDouble()
    {
        con.Open();
        var command = new EDBCommand("select * from tabled where field_serial = 2;", con);

        var dr = command.ExecuteReader();

        dr.Read();

        var result = dr.GetDouble(2);

        Assert.That(result, Is.EqualTo(0.123456789012345D));

    }

    [Test]
    public void GetFloat()
    {
        con.Open();
        var command = new EDBCommand("select * from tabled where field_serial = 1;", con);

        var dr = command.ExecuteReader();

        dr.Read();

        var result = dr.GetFloat(1);

        Assert.That(result, Is.EqualTo(.123456F));

    }

    [Test]
    public void GetString()
    {
        con.Open();
        var command = new EDBCommand("select * from tablea where field_serial = 1;", con);

        var dr = command.ExecuteReader();

        dr.Read();

        var result = dr.GetString(1);

        Assert.That(result, Is.EqualTo("Random text"));

    }

    [Test]
    public void GetStringWithParameter()
    {
        con.Open();
        var command = new EDBCommand("select * from tablea where field_text = :value;", con);

        var test = "Random text";
        var param = new EDBParameter
        {
            ParameterName = "value",
            DbType = DbType.String,
            Size = test.Length,
            Value = test
        };
        command.Parameters.Add(param);

        var dr = command.ExecuteReader();

        dr.Read();

        var result = dr.GetString(1);

        Assert.That(result, Is.EqualTo(test));

    }

    [Test]
    public void GetStringWithQuoteWithParameter()
    {
        con.Open();
        var command = new EDBCommand("select * from tablea where field_text = :value;", con);

        var test = "Text with ' single quote";
        var param = new EDBParameter
        {
            ParameterName = "value",
            DbType = DbType.String,
            Size = test.Length,
            Value = test
        };
        command.Parameters.Add(param);

        var dr = command.ExecuteReader();

        dr.Read();

        var result = dr.GetString(1);

        Assert.That(result, Is.EqualTo(test));

    }


    [Test]
    public void GetValueByName()
    {
        con.Open();
        var command = new EDBCommand("select * from tablea where field_serial = 1;", con);

        var dr = command.ExecuteReader();

        dr.Read();

        var result = (string)dr["field_text"];

        Assert.That(result, Is.EqualTo("Random text"));

    }

    [Test]
    public void GetValueFromEmptyResultset()
    {
        con.Open();
        var command = new EDBCommand("select * from tablea where field_text = :value;", con);

        var test = "Text single quote";
        var param = new EDBParameter
        {
            ParameterName = "value",
            DbType = DbType.String,
            Size = test.Length,
            Value = test
        };
        command.Parameters.Add(param);

        var dr = command.ExecuteReader();

        dr.Read();

        try
        {
            // This line should throw the invalid operation exception as the datareader will
            // have an empty resultset.
            Console.WriteLine(dr.IsDBNull(1));
            Assert.Fail("This line should throw the invalid operation exception as the datareader will have an empty resultset.");
        }
        catch (InvalidOperationException)
        {
        }

    }

    [Test]
    public void TestOverlappedParameterNames()
    {
        con.Open();

        var command = new EDBCommand("select * from tablea where field_serial = :a or field_serial = :aa", con);
        command.Parameters.Add(new EDBParameter("a", DbType.Int32, 4, "a"));
        command.Parameters.Add(new EDBParameter("aa", DbType.Int32, 4, "aa"));

        command.Parameters[0].Value = 2;
        command.Parameters[1].Value = 3;

        var dr = command.ExecuteReader();

    }

    [Test]
    public void TestNonExistentParameterName()
    {
        con.Open();

        var command = new EDBCommand("select * from tablea where field_serial = :a or field_serial = :aa", con);
        command.Parameters.Add(new EDBParameter(":b", DbType.Int32, 4, "b"));
        command.Parameters.Add(new EDBParameter(":aa", DbType.Int32, 4, "aa"));

        command.Parameters[0].Value = 2;
        command.Parameters[1].Value = 3;
        try
        {
            var dr = command.ExecuteReader();
            Assert.Fail("Execution should fail as we are bind a non-existing parameter");
        }
        catch (PostgresException)
        {
        }

    }

    [Test]
    public void UseDataAdapter()
    {

        con.Open();

        var command = new EDBCommand("select * from tablea", con);

        var da = new EDBDataAdapter
        {
            SelectCommand = command
        };

        var ds = new DataSet();

        da.Fill(ds);

        Assert.That(ds.Tables.Count, Is.EqualTo(1));
        Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(5));

    }

    [Test]
    public void UseDataAdapterEDBConnectionConstructor()
    {

        con.Open();

        var command = new EDBCommand("select * from tablea", con)
        {
            Connection = con
        };

        var da = new EDBDataAdapter(command);

        var ds = new DataSet("testtablea");

        da.Fill(ds);

        Assert.That(ds.Tables.Count, Is.EqualTo(1));
        Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(5));

    }

    [Test]
    public void UseDataAdapterStringEDBConnectionConstructor()
    {

        con.Open();

        var da = new EDBDataAdapter("select * from tablea", connectionString);

        var ds = new DataSet();

        da.Fill(ds);

        Assert.That(ds.Tables.Count, Is.EqualTo(1));
        Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(5));

    }

    [Test]
    public void UseDataAdapterStringStringConstructor2()
    {

        con.Open();

        var da = new EDBDataAdapter("select * from tableb", connectionString);

        var ds = new DataSet();

        da.Fill(ds);

        //ds.WriteXml("TestUseDataAdapterStringStringConstructor2.xml");
        Assert.That(ds.Tables.Count, Is.EqualTo(1));
        Assert.That(ds.Tables[0].Rows.Count, Is.EqualTo(4));
    }

    [Test]
    //[ExpectedException(typeof(InvalidOperationException))]
    public void ReadPastDataReaderEnd()
    {
        con.Open();
        var command = new EDBCommand("select * from tablea;", con);

        var dr = command.ExecuteReader();

        while (dr.Read())
            ;
        object o;
        Assert.Throws<InvalidOperationException>(() => o = dr[0]);

    }

    [Test]
    public void IsDBNull()
    {
        con.Open();
        var command = new EDBCommand("select field_text from tablea;", con);

        var dr = command.ExecuteReader();

        dr.Read();
        Assert.That(dr.IsDBNull(0), Is.False);
        dr.Read();
        Assert.That(dr.IsDBNull(0), Is.True);

    }

    [Test]
    public void IsDBNullFromScalar()
    {
        con.Open();
        var command = new EDBCommand("select max(field_serial) from tablea;", con);

        var dr = command.ExecuteReader();

        dr.Read();
        Assert.That(dr.IsDBNull(0), Is.False);

    }

    [Test]
    public void TypesNames()
    {
        con.Open();
        var command = new EDBCommand("select * from tablea where 1 = 2;", con);

        var dr = command.ExecuteReader();

        dr.Read();

        var t0 = dr.GetDataTypeName(0);
        var t1 = dr.GetDataTypeName(1);
        var t2 = dr.GetDataTypeName(2);
        var t3 = dr.GetDataTypeName(3);
        var t4 = dr.GetDataTypeName(4);

        Assert.That(t0, Is.EqualTo("integer"));
        Assert.That(t1, Is.EqualTo("text"));
        Assert.That(t2, Is.EqualTo("integer"));
        Assert.That(t3, Is.EqualTo("bigint"));
        Assert.That(t4, Is.EqualTo("boolean"));

        dr.Close();

        command.CommandText = "select * from tableb where 1 = 2";

        dr = command.ExecuteReader();

        dr.Read();
        t0 = dr.GetDataTypeName(0);
        t1 = dr.GetDataTypeName(1);
        t2 = dr.GetDataTypeName(2);
        t3 = dr.GetDataTypeName(3);

        Assert.That(t0, Is.EqualTo("integer"));
        Assert.That(t1, Is.EqualTo("smallint"));
        Assert.That(t2, Is.EqualTo("timestamp without time zone"));
        Assert.That(t3, Is.EqualTo("numeric(11, 7)"));

        dr.Close();
    }

    [Test]
    public void TestgetByte()
    {
        /*_conn=OpenConnection();
			TestUtil.createTempTable(_conn,"TESTTAB","a VARCHAR, b INT4");
			EDBCommand Command=new EDBCommand("",_conn);
			Command.CommandText="INSERT INTO TESTTAB VALUES('V1',1)";
			Command.ExecuteNonQuery();
			Command.CommandText="INSERT INTO TESTTAB VALUES('V2',2)";
			Command.ExecuteNonQuery();

			
			 Command = new EDBCommand("",_conn);
			Command.CommandText="select * from TESTTAB";
			EDBDataReader Reader = Command.ExecuteReader();
			
			Assert.That(Reader.Read());
			Assert.That(1,Reader.GetByte(0));


			
			Command=new EDBCommand("",_conn);
			Command.CommandText="DROP Table TESTTAB";
			Command.CommandType=CommandType.Text;
			Command.ExecuteNonQuery();
			TestUtil.closeDB(_conn);
			Assert.That(1,Reader.GetByte(0));*/

        con?.Dispose();
    }


    [Test]
    public void AddRowWithDataSet()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest");
        var command = new EDBCommand("create table DataSetTest(field_int2 int2, field_timestamp timestamp, field_numeric numeric);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest(field_int2, field_timestamp, field_numeric) " +
                " values (:a, :b, :c)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        //da.InsertCommand.Parameters.Add(new EDBParameter("b", DbType.DateTime));

        da.InsertCommand.Parameters.Add(new EDBParameter("b", EDBDbType.Timestamp));

        da.InsertCommand.Parameters.Add(new EDBParameter("c", DbType.Decimal));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
        da.InsertCommand.Parameters[1].Direction = ParameterDirection.Input;
        da.InsertCommand.Parameters[2].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
        da.InsertCommand.Parameters[1].SourceColumn = "field_timestamp";
        da.InsertCommand.Parameters[2].SourceColumn = "field_numeric";

        da.Fill(ds);

        var dt = ds.Tables[0];

        var dr = dt.NewRow();
        dr["field_int2"] = 4;
        dr["field_timestamp"] = new DateTime(2003, 03, 03, 14, 0, 0);
        dr["field_numeric"] = 7.3M;

        dt.Rows.Add(dr);
        da.Update(dt);

        command = new EDBCommand("select * from DataSetTest", con);
        var Reader = command.ExecuteReader();
        Assert.That(Reader.HasRows);
        Reader.Close();

        command = new EDBCommand("drop table DataSetTest;", con);
        command.ExecuteNonQuery();
    }

    [Test]
    public void DataSetTableByIndex()
    {
        con.Open();
        var command = new EDBCommand("create table DataSetTest1(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest1", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest1(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        var dt = ds.Tables[0];

        var dr = dt.NewRow();
        dr["field_int2"] = 4;

        dt.Rows.Add(dr);
        da.Update(dt);

        command = new EDBCommand("select * from DataSetTest1", con);
        var Reader = command.ExecuteReader();
        Assert.That(Reader.HasRows);
        Reader.Close();

        command = new EDBCommand("drop table DataSetTest1;", con);
        command.ExecuteNonQuery();
    }

    [Test]
    public void DataSetTableColumnByIndex()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");

        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        var dt = ds.Tables[0];

        var dr = dt.NewRow();
        dr["field_int2"] = 4;

        dt.Rows.Add(dr);
        da.Update(dt);

        command = new EDBCommand("select * from DataSetTest3", con);
        var Reader = command.ExecuteReader();
        Assert.That(Reader.HasRows);
        Reader.Close();
        Assert.That(ds.Tables[0].Columns[0].ToString(), Is.EqualTo("field_int2"));
        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();
    }

    [Test]
    public void DataSetTableColumnByName()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        var dt = ds.Tables[0];

        var dr = dt.NewRow();
        dr["field_int2"] = 4;

        dt.Rows.Add(dr);
        da.Update(dt);

        command = new EDBCommand("select * from DataSetTest3", con);
        var Reader = command.ExecuteReader();
        Assert.That(Reader.HasRows);
        Reader.Close();
        Assert.That(ds.Tables[0].Columns["field_int2"].ToString(), Is.EqualTo("field_int2"));
        //Console.WriteLine(ds.Tables[0].Columns["field_int2"].ToString());
        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();

    }

    [Test]
    public void DataSetCopyTest()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        var dt = ds.Tables[0];

        var dr = dt.NewRow();
        dr["field_int2"] = 4;

        dt.Rows.Add(dr);
        da.Update(dt);

        var ds2 = new DataSet();

        ds2 = ds.Copy();

        Assert.That(ds2.Tables[0].Columns[0].ToString(), Is.EqualTo("field_int2"));
        //Console.WriteLine(ds2.Tables[0].Columns["field_int2"].ToString());
        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();

    }

    [Test]
    public void DataSetNameTest()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        ds.DataSetName = "ds";

        //Console.WriteLine(ds.Namespace);
        Assert.That(ds.DataSetName, Is.EqualTo("ds"));
        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();

    }

    [Test]
    public void DataSetClearTest()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);
        var dt = ds.Tables[0];

        var dr = dt.NewRow();
        dr["field_int2"] = 4;

        dt.Rows.Add(dr);
        da.Update(dt);

        da.Fill(ds);

        //Console.WriteLine(ds.Tables[0].Rows.Count);
        Assert.That(ds.Tables[0].Rows.Count, Is.GreaterThan(0));
        ds.Clear();
        //Console.WriteLine(ds.Tables[0].Rows.Count);
        Assert.That(ds.Tables[0].Rows.Count.ToString(), Is.EqualTo("0"));

        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();

    }

    [Test]
    public void DataSetNameSpaceTest()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        ds.Namespace = "TestNameSpace";
        Assert.That(ds.Namespace, Is.EqualTo("TestNameSpace"));
        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();
    }

    [Test]
    public void DataSetWriteXML()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        ds.WriteXml("XMLTest.xml");

        Assert.That(File.Exists("XMLTest.xml"));

        File.Delete("XMLTest.xml");

        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();

    }

    [Test]
    public void DataSetReadXML()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        ds.WriteXml("XMLTest.xml");

        Assert.That(File.Exists("XMLTest.xml"));

        try
        {
            ds.ReadXml("XMLTest.xml");
        }

        catch (EDBException)
        {
            Assert.Fail("File not read");
        }

        finally
        {
            File.Delete("XMLTest.xml");
            command = new EDBCommand("drop table DataSetTest3;", con);
            command.ExecuteNonQuery();
        }

    }

    [Test]
    public void DataSetGetXmlTest()
    {
        // Create a DataSet with one table containing 
        // two columns and 10 rows.
        var dataSet = new DataSet("dataSet");
        var table = dataSet.Tables.Add("Items");
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("Item", typeof(string));

        // Add ten rows.
        DataRow row;
        for (var i = 0; i < 10; i++)
        {
            row = table.NewRow();
            row["id"] = i;
            row["Item"] = "Item" + i;
            table.Rows.Add(row);
        }
        // Display the DataSet contents as XML.

        //Console.WriteLine( dataSet.GetXml().Length );
        Assert.That(dataSet.GetXml().Length, Is.EqualTo(651));
    }

    [Test]
    public void DataSetWriteXMLSchema()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();


        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        ds.WriteXmlSchema("XMLSchemaTest.xml");

        Assert.That(File.Exists("XMLSchemaTest.xml"));

        File.Delete("XMLSchemaTest.xml");

        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();

    }

    [Test]
    public void DataSetReadXMLSchema()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);

        ds.WriteXmlSchema("XMLTest.xml");

        Assert.That(File.Exists("XMLTest.xml"));

        try
        {
            ds.ReadXmlSchema("XMLTest.xml");
        }

        catch (EDBException)
        {
            Assert.Fail("File not read");
        }

        finally
        {
            File.Delete("XMLTest.xml");
            command = new EDBCommand("drop table DataSetTest3;", con);
            command.ExecuteNonQuery();
        }

    }


    [Test]
    public void DataSetGetXmlSchemaTest()
    {
        // Create a DataSet with one table containing 
        // two columns and 10 rows.
        var dataSet = new DataSet("dataSet");
        var table = dataSet.Tables.Add("Items");
        table.Columns.Add("id", typeof(int));
        table.Columns.Add("Item", typeof(string));

        // Add ten rows.
        DataRow row;
        for (var i = 0; i < 10; i++)
        {
            row = table.NewRow();
            row["id"] = i;
            row["Item"] = "Item" + i;
            table.Rows.Add(row);
        }

        if (Environment.Version.Major == 1)
            Assert.That(dataSet.GetXmlSchema().Length, Is.EqualTo(673));
        else
        if (Environment.Version.Major == 2)
            Assert.That(dataSet.GetXmlSchema().Length, Is.EqualTo(718));
        dataSet.Prefix = "abc";
    }

    [Test]
    public void DataSetPrefixTest()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();


        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));



        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);
        //Console.WriteLine(ds.Namespace);
        ds.Prefix = "TestPrefix";
        //Console.WriteLine("TestPrefix",ds.Prefix);
        Assert.That(ds.Prefix, Is.EqualTo("TestPrefix"));
        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();

    }



    [Test]
    public void DataSetAddNewTableTest()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);
        Assert.That(ds.Tables.Count, Is.EqualTo(1));
        var dt = new DataTable();
        ds.Tables.Add(dt);
        Assert.That(ds.Tables.Count, Is.EqualTo(2));
        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();

    }

    [Test]
    public void DataSetRemoveTableByObjectTest()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);
        Assert.That(ds.Tables.Count, Is.EqualTo(1));
        var dt = new DataTable("TestTable");
        ds.Tables.Add(dt);
        Assert.That(ds.Tables.Count, Is.EqualTo(2));

        ds.Tables.Remove(dt);
        Assert.That(ds.Tables.Count, Is.EqualTo(1));

        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();
    }

    [Test]
    public void DataSetRemoveTableByNameTest()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();

        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);
        Assert.That(ds.Tables.Count, Is.EqualTo(1));
        var dt = new DataTable("TestTable");
        ds.Tables.Add(dt);
        Assert.That(ds.Tables.Count, Is.EqualTo(2));

        ds.Tables.Remove("TestTable");
        Assert.That(ds.Tables.Count, Is.EqualTo(1));

        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();
    }


    [Test]
    public void DataSetObjectClearanceTest()
    {
        con.Open();
        TestUtil.dropTable(con, "DataSetTest3");
        var command = new EDBCommand("create table DataSetTest3(field_int2 int2);", con);
        command.ExecuteNonQuery();

        var ds = new DataSet();


        var da = new EDBDataAdapter("select * from DataSetTest3", con)
        {
            InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " +
                " values (:a)", con)
        };

        da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));



        da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;

        da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

        da.Fill(ds);
        Assert.That(ds.Tables.Count, Is.EqualTo(1));
        var dt = new DataTable("TestTable");
        ds.Tables.Add(dt);
        Assert.That(ds.Tables.Count, Is.EqualTo(2));

        ds.Tables.Clear();

        Assert.That(ds.Tables.Count, Is.EqualTo(0));

        command = new EDBCommand("drop table DataSetTest3;", con);
        command.ExecuteNonQuery();


    }

    [Test, CancelAfter(5000)]
    public async Task EDB_EC_2716_TestReaderShouldNotHangAsync()
    {
        await con.OpenAsync();
        var callable_command = GetEC2716_Command(con);
        await callable_command.PrepareAsync();
        callable_command.Parameters[0].Value = 20;
        callable_command.Parameters[1].Value = 7369;
        await using var result = await callable_command.ExecuteReaderAsync();
        var fc = result.FieldCount;
        Console.WriteLine("Count: " + fc);
        while (await result.ReadAsync())
        {
        }
    }

    private static EDBCommand GetEC2716_Command(EDBConnection conn)
    {
        var callable_command = new EDBCommand("emp_query(:p_deptno,:p_empno,:p_ename,:p_job,:p_hiredate,:p_sal)", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));
        callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 7369));
        callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "SMITH"));
        callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));
        callable_command.Parameters.Add(new EDBParameter("p_hiredate", EDBTypes.EDBDbType.Date, 200, "p_hiredate", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));
        callable_command.Parameters.Add(new EDBParameter("p_sal", EDBTypes.EDBDbType.Numeric, 200, "p_sal", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));
        return callable_command;
    }

    [Test, CancelAfter(5000)]
    public void EDB_EC_2716_TestReaderShouldNotHangSync()
    {
        con.Open();
        var callable_command = GetEC2716_Command(con);
        callable_command.Prepare();
        callable_command.Parameters[0].Value = 20;
        callable_command.Parameters[1].Value = 7369;
        var result = callable_command.ExecuteReader();
        var fc = result.FieldCount;
        Console.WriteLine("Count: " + fc);
        Assert.That(result.Read());
    }

}
#pragma warning restore CS8602

