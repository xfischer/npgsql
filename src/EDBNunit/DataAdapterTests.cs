using System;
using System.Data;
using System.Web.UI.WebControls;
using EnterpriseDB.EDBClient;
using EDBTypes;
using NUnit.Framework;


namespace DOTNET
{

    [TestFixture]
    public class DataAdapterTests
    {

        private EDBConnection 	_conn = null;        

        [SetUp]
        protected void SetUp()
        {
			string connectionString = System.Configuration.ConfigurationSettings.AppSettings["connectionString"];
			_conn = new EDBConnection(connectionString);
        }

        [TearDown]
        protected void TearDown()
        {
            if (_conn.State != ConnectionState.Closed)
                _conn.Close();
        }


        [Test]
        public void FB8070_1()
        {
            _conn.Open();
            EDBCommand com = new EDBCommand("", _conn);

            com.CommandText = "create table Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com = new EDBCommand("", _conn);

            com.CommandText = "create or replace procedure quoteproc(abc in integer)\n"
                + "is\n"
                + "declare\n"
                + "i integer:=0;\n"
                + "begin\n"
                + "while i < abc loop\n"
                + "insert into Quote values(1, 't');\n"
                + "i := i+1;\n"
                + "end loop;\n"
                + "end;\n";

            com.ExecuteNonQuery();

            com = new EDBCommand("quoteproc(:a)", _conn);
            com.CommandType = CommandType.StoredProcedure;

            com.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
            com.Parameters[0].Value = 2000;
            com.Prepare();
            com.ExecuteNonQuery();

            Console.WriteLine("Data inserted");
            DataSet ds = new DataSet();
            Console.WriteLine("selecting data");
            EDBDataAdapter da = new EDBDataAdapter("select * from Quote", _conn);
            da.Fill(ds);
            Console.WriteLine("selected data");
            Console.WriteLine("filled data=" + ds.Tables[0].Rows.Count);

            Console.WriteLine("Values selected");
            com = new EDBCommand("drop table Quote", _conn);
            com.ExecuteNonQuery();

            com = new EDBCommand("drop procedure quoteproc", _conn);
            com.ExecuteNonQuery();
            GC.Collect();
            _conn.Close();


        }


        [Test]
        public void InsertWithDataSet()
        {

            _conn.Open();

            DataSet ds = new DataSet();

            EDBDataAdapter da = new EDBDataAdapter("select * from tableb", _conn);

            da.InsertCommand = new EDBCommand("insert into tableb(field_int2, field_timestamp, field_numeric) values (:a, :b, :c)", _conn);

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


            DataSet ds2 = ds.GetChanges();

            da.Update(ds2);

            ds.Merge(ds2);
            ds.AcceptChanges();
            

            EDBDataReader dr2 = new EDBCommand("select * from tableb where field_serial > 4", _conn).ExecuteReader();
            //EDBDataReader dr2 = new EDBCommand("select * from tableb", _conn).ExecuteReader();


            Assert.AreEqual(true, dr2.Read());
            Assert.AreEqual(4, dr2[1]);
            Assert.AreEqual(7.3000000M, dr2[3]);
            dr2.Close();

            new EDBCommand("delete from tableb where field_serial > 4", _conn).ExecuteNonQuery();



        }

        [Test]
        public void FillWithEmptyResultset()
        {

            _conn.Open();

            DataSet ds = new DataSet();

            EDBDataAdapter da = new EDBDataAdapter("select * from tableb where field_serial = -1", _conn);


            da.Fill(ds);

            Assert.AreEqual(1, ds.Tables.Count);
            Assert.AreEqual(4, ds.Tables[0].Columns.Count);
            Assert.AreEqual("field_serial", ds.Tables[0].Columns[0].ColumnName);
            Assert.AreEqual("field_int2", ds.Tables[0].Columns[1].ColumnName);
            Assert.AreEqual("field_timestamp", ds.Tables[0].Columns[2].ColumnName);
            Assert.AreEqual("field_numeric", ds.Tables[0].Columns[3].ColumnName);

        }

        [Test]
        public void UpdateLettingNullFieldValue()
        {

            _conn.Open();
            
            EDBCommand command = new EDBCommand("insert into tableb(field_int2) values (2)", _conn);
            command.ExecuteNonQuery();
            

            DataSet ds = new DataSet();

            EDBDataAdapter da = new EDBDataAdapter("select * from tableb where field_serial = (select max(field_serial) from tableb)", _conn);
			da.UpdateCommand = new EDBCommand("update tableb set field_int2 = :a, field_timestamp = :b, field_numeric = :c where field_serial = :d", _conn);

            da.UpdateCommand.Parameters.Add(new EDBParameter("a", DbType.Int16));

            da.UpdateCommand.Parameters.Add(new EDBParameter("b", DbType.DateTime));

            da.UpdateCommand.Parameters.Add(new EDBParameter("c", DbType.Decimal));
            
            da.UpdateCommand.Parameters.Add(new EDBParameter("d", EDBDbType.Bigint));

            da.UpdateCommand.Parameters[0].Direction = ParameterDirection.Input;
            da.UpdateCommand.Parameters[1].Direction = ParameterDirection.Input;
            da.UpdateCommand.Parameters[2].Direction = ParameterDirection.Input;
            da.UpdateCommand.Parameters[3].Direction = ParameterDirection.Input;

            da.UpdateCommand.Parameters[0].SourceColumn = "field_int2";
            da.UpdateCommand.Parameters[1].SourceColumn = "field_timestamp";
            da.UpdateCommand.Parameters[2].SourceColumn = "field_numeric";
            da.UpdateCommand.Parameters[3].SourceColumn = "field_serial";

            da.Fill(ds);
            
            DataTable dt = ds.Tables[0];

            DataRow dr = ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1];
            
            dr["field_int2"] = 4;
            
            DataSet ds2 = ds.GetChanges();

            da.Update(ds2);

            ds.Merge(ds2);
            ds.AcceptChanges();


            EDBDataReader dr2 = new EDBCommand("select * from tableb where field_serial = (select max(field_serial) from tableb)", _conn).ExecuteReader();
            dr2.Read();
            
            Assert.AreEqual(4, dr2["field_int2"]);           

        }

		[Test]
		public void TestDSNotNull()
		{
			_conn.Open();
			DataSet ds = new DataSet();
			EDBDataAdapter da = new EDBDataAdapter("select * from emp",_conn);
				
			da.Fill(ds);           
			Console.WriteLine(ds.Tables[0].Rows.Count.ToString());    
            
			Assert.IsNotNull(ds);			
			
		}

        [Test]
        public void FillWithDuplicateColumnName()
        {
            _conn.Open();
            DataSet ds = new DataSet();

            EDBDataAdapter da = new EDBDataAdapter("select field_serial, field_serial from tableb", _conn);

            da.Fill(ds);

        }


        //[Test]
		public void FB8070_2()
		{
			_conn.Open();
			EDBCommand com=new EDBCommand("",_conn);

			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com=new EDBCommand("",_conn);

			com.CommandText="create or replace procedure quoteproc(abc in integer)\n"
				+"is\n"
				+"declare\n"
				+"i integer:=0;\n"
				+"begin\n"
				+"while i < abc loop\n"
				+"insert into Quote values(1, 't');\n"
				+"i := i+1;\n"
				+"end loop;\n"
				+"end;\n";

			com.ExecuteNonQuery();
			
			com=new EDBCommand("quoteproc(:a)",_conn);
			com.CommandType=CommandType.StoredProcedure;
			
			com.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Integer));
			com.Parameters[0].Value=20000;
			com.Prepare();
			com.ExecuteNonQuery();
			
			Console.WriteLine("Data inserted");
			DataSet ds=new DataSet();
			Console.WriteLine("selecting data");
			EDBDataAdapter da =new EDBDataAdapter("select * from Quote",_conn);
			da.Fill(ds);
			Console.WriteLine("selected data");
			Console.WriteLine("filled data="+ ds.Tables[0].Rows.Count);

			Console.WriteLine("Values selected");
			com=new EDBCommand("drop table Quote",_conn);
			com.ExecuteNonQuery();
			com=new EDBCommand("drop procedure quoteproc",_conn);
			com.ExecuteNonQuery();
			GC.Collect();
			_conn.Close();
			
		}

        //Redundent case	[Test]
		public void FB8070_3()
		{
			_conn.Open();
			EDBCommand com=new EDBCommand("",_conn);

			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com=new EDBCommand("",_conn);

			com.CommandText="create or replace procedure quoteproc(abc in integer)\n"
				+"is\n"
				+"declare\n"
				+"i integer:=0;\n"
				+"begin\n"
				+"while i < abc loop\n"
				+"insert into Quote values(1, 't');\n"
				+"i := i+1;\n"
				+"end loop;\n"
				+"end;\n";

			com.ExecuteNonQuery();
			
			com=new EDBCommand("quoteproc(:a)",_conn);
			com.CommandType=CommandType.StoredProcedure;
			
			com.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Integer));
			com.Parameters[0].Value=200000;
			com.Prepare();
			com.ExecuteNonQuery();
			
			Console.WriteLine("Data inserted");
			DataSet ds=new DataSet();
			Console.WriteLine("selecting data");
			EDBDataAdapter da =new EDBDataAdapter("select * from Quote",_conn);
			da.Fill(ds);
			Console.WriteLine("selected data");
			Console.WriteLine("filled data="+ ds.Tables[0].Rows.Count);

			Console.WriteLine("Values selected");
			com=new EDBCommand("drop table Quote",_conn);
			com.ExecuteNonQuery();
			com=new EDBCommand("drop procedure quoteproc",_conn);
			com.ExecuteNonQuery();
			GC.Collect();
			_conn.Close();
			
		}
	//Redundent case	[Test]
		public void FB8070_4()
		{
			_conn.Open();
			EDBCommand com=new EDBCommand("",_conn);

			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com=new EDBCommand("",_conn);

			com.CommandText="create or replace procedure quoteproc(abc in integer)\n"
				+"is\n"
				+"declare\n"
				+"i integer:=0;\n"
				+"begin\n"
				+"while i < abc loop\n"
				+"insert into Quote values(1, 't');\n"
				+"i := i+1;\n"
				+"end loop;\n"
				+"end;\n";

			com.ExecuteNonQuery();
            			
			com=new EDBCommand("quoteproc(:a)",_conn);
			com.CommandType=CommandType.StoredProcedure;
            com.CommandTimeout = 1500;
			
			com.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Integer));
			com.Parameters[0].Value=1000000;
			com.Prepare();
			com.ExecuteNonQuery();
			
			Console.WriteLine("Data inserted");
			DataSet ds=new DataSet();
			Console.WriteLine("selecting data");
			EDBDataAdapter da =new EDBDataAdapter("select * from Quote",_conn);
			da.Fill(ds);
			Console.WriteLine("selected data");
			Console.WriteLine("filled data="+ ds.Tables[0].Rows.Count);

			Console.WriteLine("Values selected");
			com=new EDBCommand("drop table Quote",_conn);
			com.ExecuteNonQuery();
			com=new EDBCommand("drop procedure quoteproc",_conn);
			com.ExecuteNonQuery();
			GC.Collect();
			_conn.Close();
		}
        //Redundent case	[Test]
		public void FB8070_5()
		{
			_conn.Open();
			EDBCommand com=new EDBCommand("",_conn);

			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com=new EDBCommand("",_conn);

			com.CommandText="create or replace procedure quoteproc(abc in integer)\n"
				+"is\n"
				+"declare\n"
				+"i integer:=0;\n"
				+"begin\n"
				+"while i < abc loop\n"
				+"insert into Quote values(1, 't');\n"
				+"i := i+1;\n"
				+"end loop;\n"
				+"end;\n";

			com.ExecuteNonQuery();
			
			com=new EDBCommand("quoteproc(:a)",_conn);
			com.CommandType=CommandType.StoredProcedure;
            com.CommandTimeout = 1500;		
			com.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Integer));
			com.Parameters[0].Value=1000000;
			com.Prepare();
			com.ExecuteNonQuery();
			
			Console.WriteLine("Data inserted");
			DataSet ds=new DataSet();
			Console.WriteLine("selecting data");
			EDBDataAdapter da =new EDBDataAdapter("select * from Quote",_conn);
			da.Fill(ds);
			Console.WriteLine("selected data");
			Console.WriteLine("filled data="+ ds.Tables[0].Rows.Count);

			Console.WriteLine("Values selected");
			com=new EDBCommand("drop table Quote",_conn);
			com.ExecuteNonQuery();
			com=new EDBCommand("drop procedure quoteproc",_conn);
			com.ExecuteNonQuery();
			GC.Collect();
			_conn.Close();
			
		}

		[Test]
		public void _AdapFillSchemaMapped()
		{
			_conn.Open();

		/*	DataSet ds= new DataSet();

			EDBDataAdapter da=new EDBDataAdapter("select * from emp limit 1",_conn);
			try
			{
				da.FillSchema(ds,SchemaType.Mapped);
			}

			catch(Exception exp)
			{
				Assert.Fail(exp.Message);
				_conn.Close();
			}
            */
		}

		[Test]
		public void _AdapFillSchemaSource()
		{
			_conn.Open();

			DataSet ds= new DataSet();

			EDBDataAdapter da=new EDBDataAdapter("select * from emp limit 1",_conn);
			try
			{
		//		da.FillSchema(ds,SchemaType.Source);
			}

			catch(Exception exp)
			{
				Assert.Fail(exp.Message);
				_conn.Close();
			}
			
			_conn.Close();

		}

		[Test]
		public void AdapFillSchemaDataTableSourceColumnNameAccess()
		{
			_conn.Open();

			DataSet ds= new DataSet();

			EDBDataAdapter da=new EDBDataAdapter("select * from emp limit 1",_conn);
			try
			{
				da.FillSchema(ds,SchemaType.Source);
				DataTable dt =new DataTable("testtab");
				da.FillSchema(dt,SchemaType.Source);

				Assert.AreEqual("job".ToUpper(),dt.Columns[2].ColumnName.ToUpper());
			}

			catch(Exception exp)
			{
				_conn.Close();
			}

			
			_conn.Close();

		}

		[Test]
		public void AdapFillSchemaDataTableSourceColumnType()
		{
			_conn.Open();

			DataSet ds= new DataSet();

			EDBDataAdapter da=new EDBDataAdapter("select * from emp limit 1",_conn);
			try
			{
				da.FillSchema(ds,SchemaType.Source);
				DataTable dt =new DataTable("testtab");
				da.FillSchema(dt,SchemaType.Source);

				Assert.AreEqual("system.decimal".ToUpper(),dt.Columns[0].DataType.FullName.ToUpper());
				Assert.AreEqual("system.string".ToUpper(),dt.Columns[1].DataType.FullName.ToUpper());
				Assert.AreEqual("system.string".ToUpper(),dt.Columns[2].DataType.FullName.ToUpper());
				Assert.AreEqual("system.decimal".ToUpper(),dt.Columns[3].DataType.FullName.ToUpper());
				Assert.AreEqual("system.datetime".ToUpper(),dt.Columns[4].DataType.FullName.ToUpper());
				Assert.AreEqual("system.decimal".ToUpper(),dt.Columns[5].DataType.FullName.ToUpper());
				Assert.AreEqual("system.decimal".ToUpper(),dt.Columns[6].DataType.FullName.ToUpper());
				Assert.AreEqual("system.decimal".ToUpper(),dt.Columns[7].DataType.FullName.ToUpper());
								

			}

			catch(Exception exp)
			{
				_conn.Close();
			}

			
			_conn.Close();

		}


		[Test]
		public void AdapFillSchemaDataTableSourcePrimaryKey()
		{
			_conn.Open();

			DataSet ds= new DataSet();

			EDBDataAdapter da=new EDBDataAdapter("select * from emp limit 1",_conn);
			try
			{
				da.FillSchema(ds,SchemaType.Source);
				DataTable dt =new DataTable("testtab");
				da.FillSchema(dt,SchemaType.Source);

				Assert.AreEqual("empno".ToUpper(),dt.PrimaryKey.GetValue(0).ToString().ToUpper());
			}

			catch(Exception exp)
			{
				_conn.Close();
			}

			
			_conn.Close();

		}


		
    }
}
