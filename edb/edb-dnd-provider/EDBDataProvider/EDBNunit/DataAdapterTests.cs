using System;
using System.Data;
using System.Web.UI.WebControls;
using EnterpriseDB.EDBClient;
using EDBTypes;
using NUnit.Framework;


namespace NUnit
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
            dr2.Read();


            Assert.AreEqual(4, dr2[1]);
            Assert.AreEqual(7.3000000M, dr2[3]);

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
    }
}
