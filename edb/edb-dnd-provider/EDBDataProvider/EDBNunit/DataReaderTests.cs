using System;
using System.Data;
using System.Web.UI.WebControls;
using EnterpriseDB.EDBClient;
using EDBTypes;
using NUnit.Framework;

namespace NUnit
{

    [TestFixture]
    public class DataReaderTests
    {

        private EDBConnection 	_conn = null;
		string connectionString = string.Empty;
        
        [SetUp]
        protected void SetUp()
        {
			connectionString = System.Configuration.ConfigurationSettings.AppSettings["connectionString"];
			_conn = new EDBConnection(connectionString);
        }

        [TearDown]
        protected void TearDown()
        {
            if (_conn.State != ConnectionState.Closed)
                _conn.Close();
        }

        [Test]
        public void GetBoolean()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select * from tablea where field_serial = 4;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();
            Boolean result = dr.GetBoolean(4);
            Assert.AreEqual(true, result);

        }


        [Test]
        public void GetChars()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tablea where field_serial = 1;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();
            Char[] result = new Char[6];


            Int64 a = dr.GetChars(1, 0, result, 0, 6);

            Assert.AreEqual("Random", new String(result));
            /*ConsoleWriter cw = new ConsoleWriter(Console.Out);

            cw.WriteLine(result);*/


        }

        [Test]
        public void GetInt32()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tablea where field_serial = 2;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();


            Int32 result = dr.GetInt32(2);

            //ConsoleWriter cw = new ConsoleWriter(Console.Out);

            //cw.WriteLine(result.GetType().Name);
            Assert.AreEqual(4, result);

        }


        [Test]
        public void GetInt16()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tableb where field_serial = 1;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();

            Int16 result = dr.GetInt16(1);

            Assert.AreEqual(2, result);

        }


        [Test]
        public void GetDecimal()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tableb where field_serial = 3;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();

            Decimal result = dr.GetDecimal(3);


            Assert.AreEqual(4.2300000M, result);

        }




        [Test]
        public void GetDouble()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tabled where field_serial = 2;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();

            //Double result = Double.Parse(dr.GetInt32(2).ToString());
            Double result = dr.GetDouble(2);

            Assert.AreEqual(.123456789012345D, result);

        }


        [Test]
        public void GetFloat()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tabled where field_serial = 1;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();

            //Single result = Single.Parse(dr.GetInt32(2).ToString());
            Single result = dr.GetFloat(1);

            Assert.AreEqual(.123456F, result);

        }


        [Test]
        public void GetString()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tablea where field_serial = 1;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();

            String result = dr.GetString(1);

            Assert.AreEqual("Random text", result);

        }


        [Test]
        public void GetStringWithParameter()
        {
           /* _conn.Open();
            EDBCommand command = new EDBCommand("select * from tablea where field_text = :value;", _conn);

            String test = "Random text";
            EDBParameter param = new EDBParameter();
            param.ParameterName = "value";
            param.DbType = DbType.String;
            //param.EDBDbType = EDBDbType.Text;
            param.Size = test.Length;
            param.Value = test;
            command.Parameters.Add(param);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();

            String result = dr.GetString(1);

            Assert.AreEqual(test, result);*/

        }

        [Test]
        public void GetStringWithQuoteWithParameter()
        {
            /*_conn.Open();
            EDBCommand command = new EDBCommand("select * from tablea where field_text = :value;", _conn);

            String test = "Text with ' single quote";
            EDBParameter param = new EDBParameter();
            param.ParameterName = "value";
            param.DbType = DbType.String;
            //param.EDBDbType = EDBDbType.Text;
            param.Size = test.Length;
            param.Value = test;
            command.Parameters.Add(param);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();

            String result = dr.GetString(1);

            Assert.AreEqual(test, result);*/

        }


        [Test]
        public void GetValueByName()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tablea where field_serial = 1;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();

            String result = (String) dr["field_text"];

            Assert.AreEqual("Random text", result);

        }

        [Test]
        public void GetValueFromEmptyResultset()
        {
            /*_conn.Open();
            EDBCommand command = new EDBCommand("select * from tablea where field_text = :value;", _conn);

            String test = "Text single quote";
            EDBParameter param = new EDBParameter();
            param.ParameterName = "value";
            param.DbType = DbType.String;
            //param.EDBDbType = EDBDbType.Text;
            param.Size = test.Length;
            param.Value = test;
            command.Parameters.Add(param);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();


            // This line should throw the invalid operation exception as the datareader will
            // have an empty resultset.
            Console.WriteLine(dr.IsDBNull(1));*/


        }


        [Test]
        public void TestOverlappedParameterNames()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select * from tablea where field_serial = :a or field_serial = :aa", _conn);
            command.Parameters.Add(new EDBParameter("a", DbType.Int32, 4, "a"));
            command.Parameters.Add(new EDBParameter("aa", DbType.Int32, 4, "aa"));

            command.Parameters[0].Value = 2;
            command.Parameters[1].Value = 3;

            EDBDataReader dr = command.ExecuteReader();

        }

        [Test]
        public void TestNonExistentParameterName()
        {
            /*_conn.Open();

            EDBCommand command = new EDBCommand("select * from tablea where field_serial = :a or field_serial = :aa", _conn);
            command.Parameters.Add(new EDBParameter(":b", DbType.Int32, 4, "b"));
            command.Parameters.Add(new EDBParameter(":aa", DbType.Int32, 4, "aa"));

            command.Parameters[0].Value = 2;
            command.Parameters[1].Value = 3;

            EDBDataReader dr = command.ExecuteReader();*/


        }




        [Test]
        public void UseDataAdapter()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("select * from tablea", _conn);

            EDBDataAdapter da = new EDBDataAdapter();

            da.SelectCommand = command;

            DataSet ds = new DataSet();

            da.Fill(ds);

            //ds.WriteXml("TestUseDataAdapter.xml");


        }

        [Test]
        public void UseDataAdapterEDBConnectionConstructor()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("select * from tablea", _conn);

            command.Connection = _conn;

            EDBDataAdapter da = new EDBDataAdapter(command);

            DataSet ds = new DataSet();

            da.Fill(ds);

            //ds.WriteXml("TestUseDataAdapterEDBConnectionConstructor.xml");


        }

        [Test]
        public void UseDataAdapterStringEDBConnectionConstructor()
        {

            _conn.Open();


            EDBDataAdapter da = new EDBDataAdapter("select * from tablea", _conn);

            DataSet ds = new DataSet();

            da.Fill(ds);

            //ds.WriteXml("TestUseDataAdapterStringEDBConnectionConstructor.xml");


        }


        [Test]
        public void UseDataAdapterStringStringConstructor()
        {

            _conn.Open();


            EDBDataAdapter da = new EDBDataAdapter("select * from tablea", connectionString);

            DataSet ds = new DataSet();

            da.Fill(ds);

            ds.WriteXml("TestUseDataAdapterStringStringConstructor.xml");


        }

        [Test]
        public void UseDataAdapterStringStringConstructor2()
        {

            _conn.Open();


            EDBDataAdapter da = new EDBDataAdapter("select * from tableb", connectionString);

            DataSet ds = new DataSet();

            da.Fill(ds);

            ds.WriteXml("TestUseDataAdapterStringStringConstructor2.xml");


        }

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


        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadPastDataReaderEnd()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tablea;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            while (dr.Read())
                ;

            Object o = dr[0];

        }

        [Test]
        public void IsDBNull()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select field_text from tablea;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();
            Assert.AreEqual(false, dr.IsDBNull(0));
            dr.Read();
            Assert.AreEqual(true, dr.IsDBNull(0));


        }

        [Test]
        public void IsDBNullFromScalar()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select max(field_serial) from tablea;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();
            Assert.AreEqual(false, dr.IsDBNull(0));

        }



        [Test]
        public void TypesNames()
        {
            _conn.Open();
            EDBCommand command = new EDBCommand("select * from tablea where 1 = 2;", _conn);

            EDBDataReader dr = command.ExecuteReader();

            dr.Read();

            Assert.AreEqual("int4", dr.GetDataTypeName(0));
            Assert.AreEqual("text", dr.GetDataTypeName(1));
            Assert.AreEqual("int4", dr.GetDataTypeName(2));
            Assert.AreEqual("int8", dr.GetDataTypeName(3));
            Assert.AreEqual("bool", dr.GetDataTypeName(4));

            dr.Close();

            command.CommandText = "select * from tableb where 1 = 2";

            dr = command.ExecuteReader();

            dr.Read();

            Assert.AreEqual("int4", dr.GetDataTypeName(0));
            Assert.AreEqual("int2", dr.GetDataTypeName(1));
            Assert.AreEqual("timestamp", dr.GetDataTypeName(2));
            Assert.AreEqual("numeric", dr.GetDataTypeName(3));



        }
		
		[Test]
		public void testgetByte() 
		{
			/*_conn=TestUtil.openDB();
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
			_conn.Open();
			EDBCommand	command=new EDBCommand("create table DataSetTest(field_int2 int2, field_timestamp timestamp, field_numeric numeric);",_conn);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest", _conn);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest(field_int2, field_timestamp, field_numeric) " + 
				" values (:a, :b, :c)", _conn);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));
	
			da.InsertCommand.Parameters.Add(new EDBParameter("b", DbType.DateTime));
			
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

			command=new EDBCommand("select * from DataSetTest",_conn);
			EDBDataReader Reader=command.ExecuteReader();
			Assert.IsTrue(Reader.HasRows);
			
		    command=new EDBCommand("drop table DataSetTest;",_conn);
			command.ExecuteNonQuery();
		}
		

		


		[Test]
		public void DataSetTableByIndex()
		{	
			_conn.Open();
			EDBCommand	command=new EDBCommand("create table DataSetTest1(field_int2 int2);",_conn);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest1", _conn);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest1(field_int2) " + 
				" values (:a)", _conn);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));
	
			
			
			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			
			da.Fill(ds);
	
			DataTable dt = ds.Tables[0];
	
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			
			dt.Rows.Add(dr);
			da.Update(dt);

			command=new EDBCommand("select * from DataSetTest1",_conn);
			EDBDataReader Reader=command.ExecuteReader();
			Assert.IsTrue(Reader.HasRows);
			
			command=new EDBCommand("drop table DataSetTest1;",_conn);
			command.ExecuteNonQuery();
		}

		[Test]
		public void DataSetTableColumnByIndex()
		{	
			_conn.Open();
			EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",_conn);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", _conn);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", _conn);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));
	
			
			
			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			
			da.Fill(ds);
	
			DataTable dt = ds.Tables[0];
	
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			
			dt.Rows.Add(dr);
			da.Update(dt);
			
			command=new EDBCommand("select * from DataSetTest3",_conn);
			EDBDataReader Reader=command.ExecuteReader();
			Assert.IsTrue(Reader.HasRows);
			Assert.AreEqual("field_int2",ds.Tables[0].Columns[0].ToString());
			command=new EDBCommand("drop table DataSetTest3;",_conn);
			command.ExecuteNonQuery();
		}


		[Test]
		public void DataSetTableColumnByName()
		{	
			_conn.Open();
			EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",_conn);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", _conn);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", _conn);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));
	
			
			
			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			
			da.Fill(ds);
	
			DataTable dt = ds.Tables[0];
	
			DataRow dr = dt.NewRow();
			dr["field_int2"] = 4;
			
			dt.Rows.Add(dr);
			da.Update(dt);
			
			command=new EDBCommand("select * from DataSetTest3",_conn);
			EDBDataReader Reader=command.ExecuteReader();
			Assert.IsTrue(Reader.HasRows);
			Assert.AreEqual("field_int2",ds.Tables[0].Columns["field_int2"].ToString());
			//Console.WriteLine(ds.Tables[0].Columns["field_int2"].ToString());
			command=new EDBCommand("drop table DataSetTest3;",_conn);
			command.ExecuteNonQuery();
			
		}

		
		[Test]
		public void DataSetCopyTest()
		{	
			_conn.Open();
			EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",_conn);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", _conn);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", _conn);
			
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
			command=new EDBCommand("drop table DataSetTest3;",_conn);
			command.ExecuteNonQuery();
			
			
		}
		

		[Test]
		public void DataSetNameTest()
		{	
			_conn.Open();
			EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",_conn);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", _conn);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", _conn);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));
	
			
			
			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";
			
			da.Fill(ds);
	
			ds.DataSetName="ds";
			
			//Console.WriteLine(ds.Namespace);
			Assert.AreEqual("ds",ds.DataSetName);
			command=new EDBCommand("drop table DataSetTest3;",_conn);
			command.ExecuteNonQuery();
			
				
		}
	
		[Test]
		public void DataSetClearTest()
		{	_conn.Open();
			EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",_conn);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", _conn);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", _conn);
			
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

			
			command=new EDBCommand("drop table DataSetTest3;",_conn);
			command.ExecuteNonQuery();
			
				
		}

		[Test]
		public void DataSetNameSpaceTest()
		{
				_conn.Open();
			EDBCommand	command=new EDBCommand("create table DataSetTest3(field_int2 int2);",_conn);
			command.ExecuteNonQuery();
			
			DataSet ds = new DataSet();
			
		
			EDBDataAdapter da = new EDBDataAdapter("select * from DataSetTest3", _conn);
	
			da.InsertCommand = new EDBCommand("insert into DataSetTest3(field_int2) " + 
				" values (:a)", _conn);
			
			da.InsertCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));
	
			
			
			da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
	
			da.InsertCommand.Parameters[0].SourceColumn = "field_int2";

			da.Fill(ds);
		
			
			//Console.WriteLine(ds.Namespace);
			ds.Namespace="TestNameSpace";
			Console.WriteLine("TestNameSpace",ds.Namespace);
			Assert.AreEqual("TestNameSpace",ds.Namespace);
			command=new EDBCommand("drop table DataSetTest3;",_conn);
			command.ExecuteNonQuery();
			
			
				
		}
	

	
    }
}
