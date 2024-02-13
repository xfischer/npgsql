using System;
using System.Data;
#if NET45 || NET451
using System.Web.UI.WebControls;
#endif
using EnterpriseDB.EDBClient;
using EDBTypes;
using NUnit.Framework;
using System.IO;
using System.Configuration;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
#pragma warning disable CS8602
    [TestFixture]
    [NonParallelizable]
	public class EDBDataReaderTests : EPASTestBase
    {

		private EDBConnection? 	con = null;
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

			EDBCommand command = new EDBCommand("select * from tablea where field_serial = 4;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();
			Boolean result = dr.GetBoolean(4);
			Assert.AreEqual(true, result);

		}

		[Test]
		public void GetChars()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tablea where field_serial = 1;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();
            char[] result = new char[6];

			Int64 a = dr.GetChars(1, 0, result, 0, 6);

			Assert.AreEqual("Random", new string(result));

		}

		[Test]
		public void GetInt32()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tablea where field_serial = 2;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();

			Int32 result = dr.GetInt32(2);

			Assert.AreEqual(4, result);

		}

		[Test]
		public void GetInt16()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tableb where field_serial = 1;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();

			Int16 result = dr.GetInt16(1);

			Assert.AreEqual(2, result);

		}

		[Test]
		public void GetDecimal()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tableb where field_serial = 3;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();

			Decimal result = dr.GetDecimal(3);

			Assert.AreEqual(4.2300000M, result);

		}

		[Test]
		public void GetDouble()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tabled where field_serial = 2;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();
			
			Double result = dr.GetDouble(2);

			Assert.AreEqual(.123456789012345D, result);

		}

		[Test]
		public void GetFloat()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tabled where field_serial = 1;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();
			
			Single result = dr.GetFloat(1);

			Assert.AreEqual(.123456F, result);

		}

		[Test]
		public void GetString()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tablea where field_serial = 1;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();

			string result = dr.GetString(1);

			Assert.AreEqual("Random text", result);

		}

		[Test]
		public void GetStringWithParameter()
		{
			con.Open();
			 EDBCommand command = new EDBCommand("select * from tablea where field_text = :value;", con);

			 string test = "Random text";
			 EDBParameter param = new EDBParameter();
			 param.ParameterName = "value";
			 param.DbType = DbType.String;
			 param.Size = test.Length;
			 param.Value = test;
			 command.Parameters.Add(param);

			 EDBDataReader dr = command.ExecuteReader();

			 dr.Read();

			 string result = dr.GetString(1);

			 Assert.AreEqual(test, result);

		}

		[Test]
		public void GetStringWithQuoteWithParameter()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tablea where field_text = :value;", con);

			string test = "Text with ' single quote";
			EDBParameter param = new EDBParameter();
			param.ParameterName = "value";
			param.DbType = DbType.String;
			param.Size = test.Length;
			param.Value = test;
			command.Parameters.Add(param);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();

			string result = dr.GetString(1);

			Assert.AreEqual(test, result);

		}


		[Test]
		public void GetValueByName()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tablea where field_serial = 1;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();

			string result = (string) dr["field_text"];

			Assert.AreEqual("Random text", result);

		}

		[Test]
		public void GetValueFromEmptyResultset()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tablea where field_text = :value;", con);

			string test = "Text single quote";
			EDBParameter param = new EDBParameter();
			param.ParameterName = "value";
			param.DbType = DbType.String;
			param.Size = test.Length;
			param.Value = test;
			command.Parameters.Add(param);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();

			try
			{
				// This line should throw the invalid operation exception as the datareader will
				// have an empty resultset.
				Console.WriteLine(dr.IsDBNull(1));
				Assert.Fail("This line should throw the invalid operation exception as the datareader will have an empty resultset.");
			}
			catch (InvalidOperationException )
			{
			}

		}

		[Test]
		public void TestOverlappedParameterNames()
		{
			con.Open();

			EDBCommand command = new EDBCommand("select * from tablea where field_serial = :a or field_serial = :aa", con);
			command.Parameters.Add(new EDBParameter("a", DbType.Int32, 4, "a"));
			command.Parameters.Add(new EDBParameter("aa", DbType.Int32, 4, "aa"));

			command.Parameters[0].Value = 2;
			command.Parameters[1].Value = 3;

			EDBDataReader dr = command.ExecuteReader();

		}

		[Test]
		public void TestNonExistentParameterName()
		{
			con.Open();

			EDBCommand command = new EDBCommand("select * from tablea where field_serial = :a or field_serial = :aa", con);
			command.Parameters.Add(new EDBParameter(":b", DbType.Int32, 4, "b"));
			command.Parameters.Add(new EDBParameter(":aa", DbType.Int32, 4, "aa"));

			command.Parameters[0].Value = 2;
			command.Parameters[1].Value = 3;
			try
			{
				EDBDataReader dr = command.ExecuteReader();
				Assert.Fail("Execution should fail as we are bind a non-existing parameter");
			}
			catch(PostgresException)
			{
			}

		}

		[Test]
		public void UseDataAdapter()
		{

			con.Open();

			EDBCommand command = new EDBCommand("select * from tablea", con);

			EDBDataAdapter da = new EDBDataAdapter();

			da.SelectCommand = command;

			DataSet ds = new DataSet();

			da.Fill(ds);

			Assert.AreEqual(ds.Tables.Count,  1);
			Assert.AreEqual(ds.Tables[0].Rows.Count, 5);

		}

		[Test]
		public void UseDataAdapterEDBConnectionConstructor()
		{

			con.Open();

			EDBCommand command = new EDBCommand("select * from tablea", con);

			command.Connection = con;

			EDBDataAdapter da = new EDBDataAdapter(command);

			DataSet ds = new DataSet("testtablea");

			da.Fill(ds);

			Assert.AreEqual(ds.Tables.Count, 1);
			Assert.AreEqual(ds.Tables[0].Rows.Count, 5);

		}

		[Test]
		public void UseDataAdapterStringEDBConnectionConstructor()
		{

			con.Open();

			EDBDataAdapter da = new EDBDataAdapter("select * from tablea", connectionString);

			DataSet ds = new DataSet();

			da.Fill(ds);

			//ds.WriteXml("TestUseDataAdapterStringEDBConnectionConstructor.xml");
			Assert.AreEqual(ds.Tables.Count, 1);
			Assert.AreEqual(ds.Tables[0].Rows.Count, 5);

		}

		[Test]
		public void UseDataAdapterStringStringConstructor()
		{

			con.Open();

			EDBDataAdapter da = new EDBDataAdapter("select * from tablea", connectionString);

			DataSet ds = new DataSet();

			da.Fill(ds);

			//ds.WriteXml("TestUseDataAdapterStringStringConstructor.xml");
			Assert.AreEqual(ds.Tables.Count, 1);
			Assert.AreEqual(ds.Tables[0].Rows.Count, 5);
		}

		[Test]
		public void UseDataAdapterStringStringConstructor2()
		{

			con.Open();

			EDBDataAdapter da = new EDBDataAdapter("select * from tableb", connectionString);

			DataSet ds = new DataSet();

			da.Fill(ds);

			//ds.WriteXml("TestUseDataAdapterStringStringConstructor2.xml");
			Assert.AreEqual(ds.Tables.Count, 1);
			Assert.AreEqual(ds.Tables[0].Rows.Count, 4);
		}

#if NET45 || NET451

        [Test]
		public void DataGridWebControlSupport()
		{

			_conn.Open();

			EDBCommand command = new EDBCommand("select * from tablea;", _conn);

			EDBDataReader dr = command.ExecuteReader();

			DataGrid dg = new DataGrid();

			dg.DataSource = dr;
			dg.DataBind();
		}
#endif
        [Test]
		//[ExpectedException(typeof(InvalidOperationException))]
		public void ReadPastDataReaderEnd()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tablea;", con);

			EDBDataReader dr = command.ExecuteReader();

			while (dr.Read())
				;
			object o;
			Assert.Throws<InvalidOperationException>(() => o = dr[0]);

		}

		[Test]
		public void IsDBNull()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select field_text from tablea;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();
			Assert.AreEqual(false, dr.IsDBNull(0));
			dr.Read();
			Assert.AreEqual(true, dr.IsDBNull(0));

		}

		[Test]
		public void IsDBNullFromScalar()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select max(field_serial) from tablea;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();
			Assert.AreEqual(false, dr.IsDBNull(0));

		}

		[Test, /*Ignore("Needs Investigation")*/]
		public void TypesNames()
		{
			con.Open();
			EDBCommand command = new EDBCommand("select * from tablea where 1 = 2;", con);

			EDBDataReader dr = command.ExecuteReader();

			dr.Read();

            string t0 = dr.GetDataTypeName(0);
            string t1 = dr.GetDataTypeName(1);
            string t2 = dr.GetDataTypeName(2);
            string t3 = dr.GetDataTypeName(3);
            string t4 = dr.GetDataTypeName(4);

            Assert.AreEqual("integer", t0);
			Assert.AreEqual("text", t1);
			Assert.AreEqual("integer", t2);
			Assert.AreEqual("bigint", t3);
			Assert.AreEqual("boolean", t4);

			dr.Close();

			command.CommandText = "select * from tableb where 1 = 2";

			dr = command.ExecuteReader();

			dr.Read();
            t0 = dr.GetDataTypeName(0);
            t1 = dr.GetDataTypeName(1);
            t2 = dr.GetDataTypeName(2);
            t3 = dr.GetDataTypeName(3);

            Assert.AreEqual("integer", t0);
			Assert.AreEqual("smallint", t1);
			Assert.AreEqual("timestamp without time zone", t2);
			Assert.AreEqual("numeric(11, 7)", t3);

            dr.Close();
		}
		
		[Test]
		public void testgetByte() 
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
			
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(1,Reader.GetByte(0));


			
			Command=new EDBCommand("",_conn);
			Command.CommandText="DROP Table TESTTAB";
			Command.CommandType=CommandType.Text;
			Command.ExecuteNonQuery();
			TestUtil.closeDB(_conn);
			Assert.AreEqual(1,Reader.GetByte(0));*/
		
		}


		[Test]
		public void AddRowWithDataSet()
		{	
			con.Open();
            TestUtil.dropTable(con, "DataSetTest");
            EDBCommand	command=new EDBCommand("create table DataSetTest(field_int2 int2, field_timestamp timestamp, field_numeric numeric);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest(field_int2, field_timestamp, field_numeric) " + 
				" values (:a, :b, :c)", con);
			
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
	
			DataTable dt = ds.Tables[0];
	
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			dr["field_timestamp"] = new DateTime(2003, 03, 03, 14, 0, 0);
			dr["field_numeric"] = 7.3M;
			
			dt.Rows.Add(dr);
			da.Update(dt);

			command=new EDBCommand("select * from DataSetTest",con);
			EDBDataReader Reader=command.ExecuteReader();
			Assert.IsTrue(Reader.HasRows);
            Reader.Close();
			
			command=new EDBCommand("drop table DataSetTest;",con);
			command.ExecuteNonQuery();
		}

		[Test]
		public void DataSetTableByIndex()
		{	
			con.Open();
			EDBCommand	command=new EDBCommand("create table DataSetTest1(field_int2 int2);",con);
			command.ExecuteNonQuery();

			DataSet ds = new DataSet();

			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest1", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest1(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			
			da.Fill(ds);
	
			DataTable dt = ds.Tables[0];
	
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			
			dt.Rows.Add(dr);
			da.Update(dt);

			command=new EDBCommand("select * from DataSetTest1",con);
			EDBDataReader Reader=command.ExecuteReader();
			Assert.IsTrue(Reader.HasRows);
            Reader.Close();
			
			command=new EDBCommand("drop table DataSetTest1;",con);
			command.ExecuteNonQuery();
		}

		[Test]
		public void DataSetTableColumnByIndex()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");

            EDBCommand command =new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();

			DataSet ds = new DataSet();

			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			
			da.Fill(ds);
	
			DataTable dt = ds.Tables[0];
	
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			
			dt.Rows.Add(dr);
			da.Update(dt);
			
			command=new EDBCommand("select * from DataSetTest3",con);
			EDBDataReader Reader=command.ExecuteReader();
			Assert.IsTrue(Reader.HasRows);
            Reader.Close();
			Assert.AreEqual("field_int2",ds.Tables[0].Columns[0].ToString());
			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();
		}

		[Test]
		public void DataSetTableColumnByName()
		{	
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			
			da.Fill(ds);
	
			DataTable dt = ds.Tables[0];
	
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			
			dt.Rows.Add(dr);
			da.Update(dt);
			
			command=new EDBCommand("select * from DataSetTest3",con);
			EDBDataReader Reader=command.ExecuteReader();
			Assert.IsTrue(Reader.HasRows);
            Reader.Close();
			Assert.AreEqual("field_int2",ds.Tables[0].Columns["field_int2"].ToString());
			//Console.WriteLine(ds.Tables[0].Columns["field_int2"].ToString());
			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();
			
		}

		[Test]
		public void DataSetCopyTest()
		{	
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			
			da.Fill(ds);
	
			DataTable dt = ds.Tables[0];
	
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			
			dt.Rows.Add(dr);
			da.Update(dt);

			DataSet ds2=new DataSet();

			ds2=ds.Copy();

			Assert.AreEqual("field_int2",ds2.Tables[0].Columns[0].ToString());
			//Console.WriteLine(ds2.Tables[0].Columns["field_int2"].ToString());
			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();

		}

		[Test]
		public void DataSetNameTest()
		{	
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			
			da.Fill(ds);
	
			ds.DataSetName="ds";
			
			//Console.WriteLine(ds.Namespace);
			Assert.AreEqual("ds",ds.DataSetName);
			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();

		}

		[Test]
		public void DataSetClearTest()
		{
				con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);
			DataTable dt = ds.Tables[0];
	
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			
			dt.Rows.Add(dr);
			da.Update(dt);
			
			da.Fill(ds);
	
			//Console.WriteLine(ds.Tables[0].Rows.Count);
			Assert.IsNotNull(ds.Tables[0].Rows.Count);
			ds.Clear();
			//Console.WriteLine(ds.Tables[0].Rows.Count);
			Assert.AreEqual("0",ds.Tables[0].Rows.Count.ToString());

			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();

		}

		[Test]
		public void DataSetNameSpaceTest()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();

			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);

			//Console.WriteLine(ds.Namespace);
			ds.Namespace="TestNameSpace";
			Console.WriteLine("TestNameSpace",ds.Namespace);
			Assert.AreEqual("TestNameSpace",ds.Namespace);
			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();
		}

		[Test]
		public void DataSetWriteXML()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();

			DataSet ds = new DataSet();

			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);

			ds.WriteXml("XMLTest.xml");
			
			Assert.IsTrue( File.Exists("XMLTest.xml"));
		
			File.Delete("XMLTest.xml");

			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();

		}

		[Test]
		public void DataSetReadXML()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();

			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);

			ds.WriteXml("XMLTest.xml");
			
			Assert.IsTrue( File.Exists("XMLTest.xml"));

			try
			{
				ds.ReadXml("XMLTest.xml");
			}

			catch(EDBException )
			{
				Assert.Fail("File not read");
			}
		
			finally
			{
				File.Delete("XMLTest.xml");
				command=new EDBCommand("drop table DataSetTest3;",con);
				command.ExecuteNonQuery();
			}

		}

		[Test]
		public  void DataSetGetXmlTest()
		{
			// Create a DataSet with one table containing 
			// two columns and 10 rows.
			DataSet dataSet = new DataSet("dataSet");
			DataTable table = dataSet.Tables.Add("Items");
			table.Columns.Add("id", typeof(int));
			table.Columns.Add("Item", typeof(string));

			// Add ten rows.
			DataRow row;
			for(int i = 0; i <10;i++)
			{
				row = table.NewRow();
				row["id"]= i;
				row["Item"]= "Item" + i;
				table.Rows.Add(row);
			}
			// Display the DataSet contents as XML.
				
			//Console.WriteLine( dataSet.GetXml().Length );
			Assert.AreEqual(651,dataSet.GetXml().Length);
		}

		[Test]
		public void DataSetWriteXMLSchema()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
			EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
			
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);

			ds.WriteXmlSchema("XMLSchemaTest.xml");
			
			Assert.IsTrue( File.Exists("XMLSchemaTest.xml"));
		
			File.Delete("XMLSchemaTest.xml");

			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();

		}

		[Test]
		public void DataSetReadXMLSchema()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();

			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);

			ds.WriteXmlSchema("XMLTest.xml");
			
			Assert.IsTrue( File.Exists("XMLTest.xml"));

			try
			{
				ds.ReadXmlSchema("XMLTest.xml");
			}

			catch(EDBException )
			{
				Assert.Fail("File not read");
			}
		
			finally
			{
				File.Delete("XMLTest.xml");
				command=new EDBCommand("drop table DataSetTest3;",con);
				command.ExecuteNonQuery();
			}

		}
		
			
		[Test]
		public  void DataSetGetXmlSchemaTest()
		{
			// Create a DataSet with one table containing 
			// two columns and 10 rows.
			DataSet dataSet = new DataSet("dataSet");
			DataTable table = dataSet.Tables.Add("Items");
			table.Columns.Add("id", typeof(int));
			table.Columns.Add("Item", typeof(string));

			// Add ten rows.
			DataRow row;
			for(int i = 0; i <10;i++)
			{
				row = table.NewRow();
				row["id"]= i;
				row["Item"]= "Item" + i;
				table.Rows.Add(row);
			}

			if(Environment.Version.Major==1)
			Assert.AreEqual(673,dataSet.GetXmlSchema().Length);
			else
			if(Environment.Version.Major==2)
			Assert.AreEqual(718,dataSet.GetXmlSchema().Length);
			dataSet.Prefix="abc";
		}

		[Test]
		public void DataSetPrefixTest()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
			
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));
	
			
			
			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);
			//Console.WriteLine(ds.Namespace);
			ds.Prefix="TestPrefix";
			//Console.WriteLine("TestPrefix",ds.Prefix);
			Assert.AreEqual("TestPrefix",ds.Prefix);
			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();

		}



		[Test]
		public void DataSetAddNewTableTest()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();

			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);
			Assert.AreEqual(1,ds.Tables.Count);
			DataTable dt=new DataTable();
			ds.Tables.Add(dt);
			Assert.AreEqual(2,ds.Tables.Count);
			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();

		}

		[Test]
		public void DataSetRemoveTableByObjectTest()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();

			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);

			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);
			Assert.AreEqual(1,ds.Tables.Count);
			DataTable dt=new DataTable("TestTable");
			ds.Tables.Add(dt);
			Assert.AreEqual(2,ds.Tables.Count);

			ds.Tables.Remove(dt);
			Assert.AreEqual(1,ds.Tables.Count);

			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();
		}

		[Test]
		public void DataSetRemoveTableByNameTest()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();

			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);
			Assert.AreEqual(1,ds.Tables.Count);
			DataTable dt=new DataTable("TestTable");
			ds.Tables.Add(dt);
			Assert.AreEqual(2,ds.Tables.Count);

			ds.Tables.Remove("TestTable");
			Assert.AreEqual(1,ds.Tables.Count);

			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();
		}


		[Test]
		public void DataSetObjectClearanceTest()
		{
			con.Open();
            TestUtil.dropTable(con, "DataSetTest3");
            EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",con);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
			
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", con);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", con);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));
	
			
			
			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);
			Assert.AreEqual(1,ds.Tables.Count);
			DataTable dt=new DataTable("TestTable");
			ds.Tables.Add(dt);
			Assert.AreEqual(2,ds.Tables.Count);

			ds.Tables.Clear();

			Assert.AreEqual(0,ds.Tables.Count);

			command=new EDBCommand("drop table DataSetTest3;",con);
			command.ExecuteNonQuery();
			
			
		}

        [Test, Timeout(5000)]
        public async Task EDB_EC_2716_TestReaderShouldNotHangAsync()
        {
            await con.OpenAsync();
            var callable_command = GetEC2716_Command(con);
            await callable_command.PrepareAsync();
            callable_command.Parameters[0].Value = 20;
            callable_command.Parameters[1].Value = 7369;
            await using EDBDataReader result = await callable_command.ExecuteReaderAsync();
            int fc = result.FieldCount;
            Console.WriteLine("Count: " + fc);
            while (await result.ReadAsync())
            {
            }
        }

        private EDBCommand GetEC2716_Command(EDBConnection conn)
        {
            EDBCommand callable_command = new EDBCommand("emp_query(:p_deptno,:p_empno,:p_ename,:p_job,:p_hiredate,:p_sal)", conn);
                callable_command.CommandType = CommandType.StoredProcedure;
                callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));
                callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 7369));
                callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "SMITH"));
                callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                callable_command.Parameters.Add(new EDBParameter("p_hiredate", EDBTypes.EDBDbType.Date, 200, "p_hiredate", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                callable_command.Parameters.Add(new EDBParameter("p_sal", EDBTypes.EDBDbType.Numeric, 200, "p_sal", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            return callable_command;
        }

        [Test, Timeout(5000)]
        public void EDB_EC_2716_TestReaderShouldNotHangSync()
        {
            con.Open();
            var callable_command = GetEC2716_Command(con);
                callable_command.Prepare();
                callable_command.Parameters[0].Value = 20;
                callable_command.Parameters[1].Value = 7369;
                EDBDataReader result = callable_command.ExecuteReader();
                int fc = result.FieldCount;
                Console.WriteLine("Count: " + fc);
                while (result.Read())
                {
                }
        }

    }
#pragma warning restore CS8602
}
