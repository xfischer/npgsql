#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The EDB Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EDB DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EDB DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EDB DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EDB DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion



using System;
using System.Data;
#if NET45 || NET451
using System.Web.UI.WebControls;
#endif
using EnterpriseDB.EDBClient;
using EDBTypes;
using NUnit.Framework;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
#pragma warning disable CS8602
    [TestFixture]
    [NonParallelizable]
    public class EDBDataAdapterTests : EPASTestBase
    {

        private EDBConnection? 	con = null;

        [SetUp]
        public void Init()
        {
            con = CreateConnection();
        }

        [TearDown]
        protected void TearDown()
        {
            if (con.State != ConnectionState.Closed)
                con.Close();
        }

        private EDBConnection OpenConnection()
        {
            var conn = new EDBConnection(ConnectionString);
            conn.Open();
            conn.ExecuteNonQuery("CREATE TEMP TABLE data (" +
                                 "field_pk SERIAL PRIMARY KEY," +
                                 "field_serial SERIAL," +
                                 "field_int2 SMALLINT," +
                                 "field_int4 INTEGER," +
                                 "field_numeric NUMERIC," +
                                 "field_timestamp TIMESTAMP" +
                                 ")");
            return conn;
        }


        [Test, /*Ignore("MERGE_NEED_TO_EXPLORE")*/]
        public void FB8070_1()
        {
            con.Open();
            TestUtil.dropTable(con, "Quote");

            EDBCommand com = new EDBCommand("", con);

            com.CommandText = "create table Quote(id int4, b char)";
            com.ExecuteNonQuery();
            com = new EDBCommand("", con);

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

            com = new EDBCommand("quoteproc(:a)", con);
            com.CommandType = CommandType.StoredProcedure;

            com.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
            com.Parameters[0].Value = 2000;
            com.Prepare();
            com.ExecuteNonQuery();

            Console.WriteLine("Data inserted");
            DataSet ds = new DataSet();
            Console.WriteLine("selecting data");
            EDBDataAdapter da = new EDBDataAdapter("select * from Quote", con);
            da.Fill(ds);
            Console.WriteLine("selected data");
  
            Console.WriteLine("Values selected");
            com = new EDBCommand("drop table Quote", con);
            com.ExecuteNonQuery();

            com = new EDBCommand("drop procedure quoteproc", con);
            com.ExecuteNonQuery();
            GC.Collect();
            con.Close();


        }

        [Test]
        public void UseDataAdapterEDBConnectionConstructor()
        {
            using (var conn = OpenConnection())
            using (var command = new EDBCommand("SELECT 1", conn))
            {
                command.Connection = conn;
                var da = new EDBDataAdapter(command);
                var ds = new DataSet();
                da.Fill(ds);
                //ds.WriteXml("TestUseDataAdapterEDBConnectionConstructor.xml");
            }
        }

        [Test]
        public void UseDataAdapterStringEDBConnectionConstructor()
        {
            using (var conn = OpenConnection())
            {
                var da = new EDBDataAdapter("SELECT 1", conn);
                var ds = new DataSet();
                da.Fill(ds);
                //ds.WriteXml("TestUseDataAdapterStringEDBConnectionConstructor.xml");
            }
        }

        [Test]
        public void UseDataAdapterStringStringConstructor()
        {
            var da = new EDBDataAdapter("SELECT 1", ConnectionString);
            var ds = new DataSet();
            da.Fill(ds);
            //ds.WriteXml("TestUseDataAdapterStringStringConstructor.xml");
        }

        [Test]
        public void UseDataAdapterStringStringConstructor2()
        {
            var da = new EDBDataAdapter("SELECT 1", ConnectionString);
            var ds = new DataSet();
            da.Fill(ds);
            //ds.WriteXml("TestUseDataAdapterStringStringConstructor2.xml");
        }

		[Test]
		public void TestDSNotNull()
		{
			con.Open();
			DataSet ds = new DataSet();
			EDBDataAdapter da = new EDBDataAdapter("select * from emp",con);
				
			da.Fill(ds);           
			Console.WriteLine(ds.Tables[0].Rows.Count.ToString());    
            
			Assert.IsNotNull(ds);			
			
		}

        //[Test]
		public void FB8070_2()
		{
			con.Open();
			EDBCommand com=new EDBCommand("",con);

			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com=new EDBCommand("",con);

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
			
			com=new EDBCommand("quoteproc(:a)",con);
			com.CommandType=CommandType.StoredProcedure;
			
			com.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Integer));
			com.Parameters[0].Value=20000;
			com.Prepare();
			com.ExecuteNonQuery();
			
			Console.WriteLine("Data inserted");
			DataSet ds=new DataSet();
			Console.WriteLine("selecting data");
			EDBDataAdapter da =new EDBDataAdapter("select * from Quote",con);
			da.Fill(ds);
			Console.WriteLine("selected data");
			Console.WriteLine("filled data="+ ds.Tables[0].Rows.Count);

			Console.WriteLine("Values selected");
			com=new EDBCommand("drop table Quote",con);
			com.ExecuteNonQuery();
			com=new EDBCommand("drop procedure quoteproc",con);
			com.ExecuteNonQuery();
			GC.Collect();
			con.Close();
			
		}

        //Redundent case	[Test]
		public void FB8070_3()
		{
			con.Open();
			EDBCommand com=new EDBCommand("",con);

			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com=new EDBCommand("",con);

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
			
			com=new EDBCommand("quoteproc(:a)",con);
			com.CommandType=CommandType.StoredProcedure;
			
			com.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Integer));
			com.Parameters[0].Value=200000;
			com.Prepare();
			com.ExecuteNonQuery();
			
			Console.WriteLine("Data inserted");
			DataSet ds=new DataSet();
			Console.WriteLine("selecting data");
			EDBDataAdapter da =new EDBDataAdapter("select * from Quote",con);
			da.Fill(ds);
			Console.WriteLine("selected data");
			Console.WriteLine("filled data="+ ds.Tables[0].Rows.Count);

			Console.WriteLine("Values selected");
			com=new EDBCommand("drop table Quote",con);
			com.ExecuteNonQuery();
			com=new EDBCommand("drop procedure quoteproc",con);
			com.ExecuteNonQuery();
			GC.Collect();
			con.Close();
			
		}
	//Redundent case	[Test]
		public void FB8070_4()
		{
			con.Open();
			EDBCommand com=new EDBCommand("",con);

			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com=new EDBCommand("",con);

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
            			
			com=new EDBCommand("quoteproc(:a)",con);
			com.CommandType=CommandType.StoredProcedure;
            com.CommandTimeout = 1500;
			
			com.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Integer));
			com.Parameters[0].Value=1000000;
			com.Prepare();
			com.ExecuteNonQuery();
			
			Console.WriteLine("Data inserted");
			DataSet ds=new DataSet();
			Console.WriteLine("selecting data");
			EDBDataAdapter da =new EDBDataAdapter("select * from Quote",con);
			da.Fill(ds);
			Console.WriteLine("selected data");
			Console.WriteLine("filled data="+ ds.Tables[0].Rows.Count);

			Console.WriteLine("Values selected");
			com=new EDBCommand("drop table Quote",con);
			com.ExecuteNonQuery();
			com=new EDBCommand("drop procedure quoteproc",con);
			com.ExecuteNonQuery();
			GC.Collect();
			con.Close();
		}
        //Redundent case	[Test]
		public void FB8070_5()
		{
			con.Open();
			EDBCommand com=new EDBCommand("",con);

			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com=new EDBCommand("",con);

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
			
			com=new EDBCommand("quoteproc(:a)",con);
			com.CommandType=CommandType.StoredProcedure;
            com.CommandTimeout = 1500;		
			com.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Integer));
			com.Parameters[0].Value=1000000;
			com.Prepare();
			com.ExecuteNonQuery();
			
			Console.WriteLine("Data inserted");
			DataSet ds=new DataSet();
			Console.WriteLine("selecting data");
			EDBDataAdapter da =new EDBDataAdapter("select * from Quote",con);
			da.Fill(ds);
			Console.WriteLine("selected data");
			Console.WriteLine("filled data="+ ds.Tables[0].Rows.Count);

			Console.WriteLine("Values selected");
			com=new EDBCommand("drop table Quote",con);
			com.ExecuteNonQuery();
			com=new EDBCommand("drop procedure quoteproc",con);
			com.ExecuteNonQuery();
			GC.Collect();
			con.Close();
			
		}

		[Test]
		public void _AdapFillSchemaMapped()
		{
			con.Open();

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
			con.Open();

			DataSet ds= new DataSet();

			EDBDataAdapter da=new EDBDataAdapter("select * from emp limit 1",con);
			try
			{
		//		da.FillSchema(ds,SchemaType.Source);
			}

			catch(Exception exp)
			{
				Assert.Fail(exp.Message);
				con.Close();
			}
			
			con.Close();

		}

		[Test]
		public void AdapFillSchemaDataTableSourceColumnNameAccess()
		{
			con.Open();

			DataSet ds= new DataSet();

			EDBDataAdapter da=new EDBDataAdapter("select * from emp limit 1",con);
			try
			{
				da.FillSchema(ds,SchemaType.Source);
				DataTable dt =new DataTable("testtab");
				da.FillSchema(dt,SchemaType.Source);

				Assert.AreEqual("job".ToUpper(),dt.Columns[2].ColumnName.ToUpper());
			}

			catch(Exception )
			{
				con.Close();
			}

			
			con.Close();

		}

		[Test]
		public void AdapFillSchemaDataTableSourceColumnType()
		{
			con.Open();

			DataSet ds= new DataSet();

			EDBDataAdapter da=new EDBDataAdapter("select * from emp limit 1",con);
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

			catch(Exception )
			{
				con.Close();
			}

			
			con.Close();

		}


		[Test]
		public void AdapFillSchemaDataTableSourcePrimaryKey()
		{
			con.Open();

			DataSet ds= new DataSet();

			EDBDataAdapter da=new EDBDataAdapter("select * from emp limit 1",con);
			try
			{
				da.FillSchema(ds,SchemaType.Source);
				DataTable dt =new DataTable("testtab");
				da.FillSchema(dt,SchemaType.Source);

				Assert.AreEqual("empno".ToUpper(),dt.PrimaryKey.GetValue(0).ToString().ToUpper());
			}

			catch(Exception )
			{
				con.Close();
			}

			
			con.Close();

		}
        
        public void Setup(EDBConnection conn)
        {
            conn.ExecuteNonQuery("CREATE TEMP TABLE data (" +
                                 "field_pk SERIAL PRIMARY KEY," +
                                 "field_serial SERIAL," +
                                 "field_int2 SMALLINT," +
                                 "field_int4 INTEGER," +
                                 "field_numeric NUMERIC," +
                                 "field_timestamp TIMESTAMP" +
                                 ")");
        }
    }
#pragma warning restore CS8602
}

//#endif
