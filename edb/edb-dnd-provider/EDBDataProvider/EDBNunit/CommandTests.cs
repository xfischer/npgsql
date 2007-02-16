using System;
using NUnit.Framework;
using System.Data;
using System.Globalization;
using EDBTypes;
using EnterpriseDB.EDBClient;
using System.Net;

namespace NUnit
{

    public enum EnumTest : short
    {
        Value1 = 0,
        Value2 = 1
    };

    [TestFixture]
    public class CommandTests
    {
        private EDBConnection 	_conn = null;        

        [SetUp]
        protected void SetUp()
        { 
			string connectionString = System.Configuration.ConfigurationSettings.AppSettings["connectionString"];
			_conn = new EDBConnection(connectionString);		
	
		
//			TestUtil.ExecuteSql(_conn, "add_tables.sql");
//			TestUtil.ExecuteSql(_conn, "add_functions.sql");
//			TestUtil.ExecuteSql(_conn, "add_triggers.sql");
//			TestUtil.ExecuteSql(_conn, "add_views.sql");
//			TestUtil.ExecuteSql(_conn, "add_data.sql");		
			
        }	

        [TearDown]
        protected void TearDown()
        {
            if (_conn.State != ConnectionState.Closed)
                _conn.Close();
        }


        [Test]
        public void ParametersGetName()
        {
            EDBCommand command = new EDBCommand();

            // Add parameters.
            command.Parameters.Add(new EDBParameter(":Parameter1", DbType.Boolean));
            command.Parameters.Add(new EDBParameter(":Parameter2", DbType.Int32));
            command.Parameters.Add(new EDBParameter(":Parameter3", DbType.DateTime));


            // Get by indexers.

            Assert.AreEqual(":Parameter1", command.Parameters[":Parameter1"].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[":Parameter2"].ParameterName);
            Assert.AreEqual(":Parameter3", command.Parameters[":Parameter3"].ParameterName);


            Assert.AreEqual(":Parameter1", command.Parameters[0].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[1].ParameterName);
            Assert.AreEqual(":Parameter3", command.Parameters[2].ParameterName);



        }

        [Test]
        public void EmptyQuery()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand(";", _conn);
            command.ExecuteNonQuery();

        }

//        [Test]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void NoNameParameterAdd()
//        {
//            EDBCommand command = new EDBCommand();
//
//            command.Parameters.Add(new EDBParameter());
//        }       

        [Test]
        public void FunctionCallFromSelect()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select * from funcB()", _conn);

            EDBDataReader reader = command.ExecuteReader();            
			Assert.IsNotNull(reader);
            
        }

        [Test]
        public void ExecuteScalar()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select count(*) from tablea", _conn);

            Object result = command.ExecuteScalar();

            Assert.AreEqual(5, result);        

        }
        
        
        [Test]
        public void InsertStringWithBackslashes()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:p0)", _conn);
            
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Text));
            
            command.Parameters["p0"].Value = @"\test";

            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            EDBCommand command2 = new EDBCommand("select field_text from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
            

            result = command2.ExecuteScalar();
            
            
            
            new EDBCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(@"\test", result);
            
            
            
            //reader.FieldCount

        }
        
               
        
//        [Test]
//        public void UseStringParameterWithNoEDBDbType()
//        {
//            _conn.Open();
//
//            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:p0)", _conn);
//            
//            command.Parameters.Add(new EDBParameter("p0","test"));
//            
//            
//            Assert.AreEqual(command.Parameters[0].EDBDbType, EDBDbType.Varchar2);
//            Assert.AreEqual(command.Parameters[0].DbType, DbType.String);
//            
//            Object result = command.ExecuteNonQuery();
//			
//            Assert.AreEqual(1, result);
//            
//            
//            EDBCommand command2 = new EDBCommand("select field_text from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
//            
//
//            result = command2.ExecuteScalar();
//            
//            
//            
//            new EDBCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();            
//			
//            Assert.AreEqual("test", result);
//
//        }
        
        [Test]
        public void UseIntegerParameterWithNoEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tablea(field_int4) values (:p0)", _conn);
            
            command.Parameters.Add(new EDBParameter("p0", 5));
            
            Assert.AreEqual(command.Parameters[0].EDBDbType, EDBDbType.Integer);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.Int32);
            
            
            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            EDBCommand command2 = new EDBCommand("select field_int4 from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
            

            result = command2.ExecuteScalar();
            
            
            
            new EDBCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(5, result);
            
            
            
            //reader.FieldCount

        }
        
        
        [Test]
        public void UseSmallintParameterWithNoEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tablea(field_int4) values (:p0)", _conn);
            
            command.Parameters.Add(new EDBParameter("p0", (Int16)5));
            
            Assert.AreEqual(command.Parameters[0].EDBDbType, EDBDbType.Smallint);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.Int16);
            
            
            Object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            EDBCommand command2 = new EDBCommand("select field_int4 from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
            

            result = command2.ExecuteScalar();
                      
            
            new EDBCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(5, result);
            
            
            
            //reader.FieldCount

        }
        
        
        

        [Test]
        public void FunctionCallReturnSingleValue()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("funcC()", _conn);
            command.CommandType = CommandType.StoredProcedure;

            Object result = command.ExecuteScalar();

            Assert.AreEqual(5, result);
            //reader.FieldCount

        }


        [Test]
        public void FunctionCallReturnSingleValueWithPrepare()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("funcC()", _conn);
            command.CommandType = CommandType.StoredProcedure;  
          			
            Object result = command.ExecuteScalar();
			
            Assert.AreEqual(5, result);           

        }

        [Test]
        public void FunctionCallWithParametersReturnSingleValue()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("funcC(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));

            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();
			
            Assert.AreEqual(1, result);


        }

        [Test]
        public void FunctionCallWithParametersReturnSingleValueEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("funcC(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Integer));

            command.Parameters[0].Value = 4;

            Int64 result = (Int64) command.ExecuteScalar();

            Assert.AreEqual(1, result);

        }




//        [Test]
//        public void FunctionCallWithParametersPrepareReturnSingleValue()
//        {
//            _conn.Open();
//
//            EDBCommand command = new EDBCommand("funcC(:a)", _conn);
//            command.CommandType = CommandType.StoredProcedure;
//
//
//            command.Parameters.Add(new EDBParameter("a", DbType.Int32));
//
//            Assert.AreEqual(1, command.Parameters.Count);
//            command.Prepare();
//
//
//            command.Parameters[0].Value = 4;
//
//            Int64 result = (Int64) command.ExecuteScalar();
//			
//            Assert.AreEqual(1, result);
//
//
//        }

//        [Test]
//        public void FunctionCallWithParametersPrepareReturnSingleValueEDBDbType()
//        {
//            _conn.Open();
//
//            EDBCommand command = new EDBCommand("funcC(:a)", _conn);
//            command.CommandType = CommandType.StoredProcedure;
//
//
//            command.Parameters.Add(new EDBParameter("a", EDBDbType.Integer));
//
//            Assert.AreEqual(1, command.Parameters.Count);
//            command.Prepare();
//
//
//            command.Parameters[0].Value = 4;
//
//            Int64 result = (Int64) command.ExecuteScalar();
//			Console.WriteLine(result.ToString());
//            Assert.AreEqual(1, result);
//
//
//        }


        [Test]
        public void FunctionCallReturnResultSet()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("funcB()", _conn);
            command.CommandType = CommandType.StoredProcedure;

            EDBDataReader dr = command.ExecuteReader();




        }


        [Test]
        public void CursorStatement()
        {

            _conn.Open();

            Int32 i = 0;

            EDBTransaction t = _conn.BeginTransaction();

            EDBCommand command = new EDBCommand("declare te cursor for select * from tablea;", _conn);

            command.ExecuteNonQuery();

            command.CommandText = "fetch forward 3 in te;";

            EDBDataReader dr = command.ExecuteReader();


            while (dr.Read())
            {
                i++;
            }
			Console.WriteLine(i.ToString());
            Assert.AreEqual(3, i);           

            t.Commit();



        }

        [Test]
        public void PreparedStatementNoParameters()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select * from tablea;", _conn);

            command.Prepare();

            
            EDBDataReader dr = command.ExecuteReader();


        }
        
        
        [Test]
        public void PreparedStatementInsert()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:p0);", _conn);
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Text));
            command.Parameters["p0"].Value = "test";
            

            command.Prepare();

            
            EDBDataReader dr = command.ExecuteReader();
            
            new EDBCommand("delete from tablea where field_serial = (select max(field_serial) from tablea);", _conn).ExecuteNonQuery();
            


        }
        
        [Test]
        public void PreparedStatementInsertNullValue()
        {


            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tablea(field_int4) values (:p0);", _conn);
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Integer));
            command.Parameters["p0"].Value = DBNull.Value;
            

            command.Prepare();

            
            EDBDataReader dr = command.ExecuteReader();
            new EDBCommand("delete from tablea where field_serial = (select max(field_serial) from tablea);", _conn).ExecuteNonQuery();
            


        }
        
        

        [Test]
        public void PreparedStatementWithParameters()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select * from tablea where field_int4 = :a and field_int8 = :b;", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));
            command.Parameters.Add(new EDBParameter("b", DbType.Int64));

            Assert.AreEqual(2, command.Parameters.Count);

            Assert.AreEqual(DbType.Int32, command.Parameters[0].DbType);

            command.Prepare();

            command.Parameters[0].Value = 3;
            command.Parameters[1].Value = 5;

            EDBDataReader dr = command.ExecuteReader();




        }

        [Test]
        public void PreparedStatementWithParametersEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select * from tablea where field_int4 = :a and field_int8 = :b;", _conn);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Integer));
            command.Parameters.Add(new EDBParameter("b", EDBDbType.Bigint));

            Assert.AreEqual(2, command.Parameters.Count);

            Assert.AreEqual(DbType.Int32, command.Parameters[0].DbType);

            command.Prepare();

            command.Parameters[0].Value = 3;
            command.Parameters[1].Value = 5;

            EDBDataReader dr = command.ExecuteReader();




        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ListenNotifySupport()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("listen notifytest;", _conn);
            command.ExecuteNonQuery();

            _conn.Notification += new NotificationEventHandler(NotificationSupportHelper);


            command = new EDBCommand("notify notifytest;", _conn);
            command.ExecuteNonQuery();



        }

        private void NotificationSupportHelper(Object sender, EDBNotificationEventArgs args)
        {
            throw new InvalidOperationException();
        }
		
		[Test]
        public void ByteSupport()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Byte));

            command.Parameters[0].Value = 2;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.Parameters.Clear();
            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();
        }
        
        
		[Test]
        public void EnumSupport()
        {
        
            
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Smallint));

            command.Parameters[0].Value = EnumTest.Value1;
            

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.Parameters.Clear();
            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();
        }

        [Test]
        public void DateTimeSupport()
        {
            _conn.Open();
			
            EDBCommand command = new EDBCommand("select field_timestamp from tableb where field_serial = 2;", _conn);			
            DateTime d = (DateTime)command.ExecuteScalar();			 
            Assert.AreEqual("2002-02-02 09:00:23Z", d.ToString("u"));

            DateTimeFormatInfo culture = new DateTimeFormatInfo();
            culture.TimeSeparator = ":";
            DateTime dt = System.DateTime.Parse("2004-06-04 09:48:00", culture);

            command.CommandText = "insert into tableb(field_timestamp) values (:a);delete from tableb where field_serial > 4;";
            command.Parameters.Add(new EDBParameter("a", DbType.DateTime));
            command.Parameters[0].Value = dt;

            command.ExecuteScalar();

        }


        [Test]
        public void DateTimeSupportEDBDbType()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("select field_timestamp from tableb where field_serial = 2;", _conn);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("2002-02-02 09:00:23Z", d.ToString("u"));

            DateTimeFormatInfo culture = new DateTimeFormatInfo();
            culture.TimeSeparator = ":";
            DateTime dt = System.DateTime.Parse("2004-06-04 09:48:00", culture);

            command.CommandText = "insert into tableb(field_timestamp) values (:a);delete from tableb where field_serial > 4;";
            command.Parameters.Add(new EDBParameter("a", EDBDbType.Timestamp));
            command.Parameters[0].Value = dt;

            command.ExecuteScalar();

        }

        [Test]
        public void DateSupport()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select field_date from tablec where field_serial = 1;", _conn);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("2002-03-04", d.ToString("yyyy-MM-dd"));

        }

        [Test]
        public void TimeSupport()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select field_time from tablec where field_serial = 2;", _conn);

            DateTime d = (DateTime)command.ExecuteScalar();


            Assert.AreEqual("10:03:45.345", d.ToString("HH:mm:ss.fff"));

        }

        [Test]
        public void NumericSupport()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tableb(field_numeric) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter("a", DbType.Decimal));

            command.Parameters[0].Value = 7.4M;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tableb where field_numeric = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);


            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4000000M, result);




        }

        [Test]
        public void NumericSupportEDBDbType()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tableb(field_numeric) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter("a", EDBDbType.Numeric));

            command.Parameters[0].Value = 7.4M;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tableb where field_numeric = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);


            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4000000M, result);




        }


        [Test]
        public void InsertSingleValue()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tabled(field_float4) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter(":a", DbType.Single));

            command.Parameters[0].Value = 7.4F;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float4 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Single result = dr.GetFloat(1);


            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4F, result);

        }


        [Test]
        public void InsertSingleValueEDBDbType()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tabled(field_float4) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Float));

            command.Parameters[0].Value = 7.4F;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float4 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Single result = dr.GetFloat(1);


            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4F, result);

        }

        [Test]
        public void InsertDoubleValue()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tabled(field_float8) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter(":a", DbType.Double));

            command.Parameters[0].Value = 7.4D;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float8 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Double result = dr.GetDouble(2);


            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            //command.ExecuteNonQuery();


            Assert.AreEqual(7.4D, result);

        }


        [Test]
        public void InsertDoubleValueEDBDbType()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tabled(field_float8) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Double));

            command.Parameters[0].Value = 7.4D;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float8 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Double result = dr.GetDouble(2);


            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            //command.ExecuteNonQuery();


            Assert.AreEqual(7.4D, result);

        }


        [Test]
        public void NegativeNumericSupport()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("select * from tableb where field_serial = 4", _conn);


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);

            Assert.AreEqual(-4.3000000M, result);

        }


        [Test]
        public void PrecisionScaleNumericSupport()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("select * from tableb where field_serial = 4", _conn);


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);

            Assert.AreEqual(-4.3000000M, (Decimal)result);
            //Assert.AreEqual(11, result.Precision);
            //Assert.AreEqual(7, result.Scale);

        }

        [Test]
        public void InsertNullString()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.String));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_text is null";
            command.Parameters.Clear();

            Int64 result = (Int64)command.ExecuteScalar();

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea) and field_serial != 4;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }

        [Test]
        public void InsertNullStringEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Text));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_text is null";
            command.Parameters.Clear();

            Int64 result = (Int64)command.ExecuteScalar();

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea) and field_serial != 4;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }



        [Test]
        public void InsertNullDateTime()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tableb(field_timestamp) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.DateTime));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_timestamp is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar();

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }


        [Test]
        public void InsertNullDateTimeEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tableb(field_timestamp) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Timestamp));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_timestamp is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar();

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);



        }



        [Test]
        public void InsertNullInt16()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Int16));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_int2 is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);


        }


        [Test]
        public void InsertNullInt16EDBDbType()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Smallint));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_int2 is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();

            Assert.AreEqual(4, result);


        }


        [Test]
        public void InsertNullInt32()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tablea(field_int4) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_int4 is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
            command.ExecuteNonQuery();

            Assert.AreEqual(5, result);

        }


        [Test]
        public void InsertNullNumeric()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tableb(field_numeric) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Decimal));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tableb where field_numeric is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb);";
            command.ExecuteNonQuery();

            Assert.AreEqual(3, result);

        }

        [Test]
        public void InsertNullBoolean()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("insert into tablea(field_bool) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Boolean));

            command.Parameters[0].Value = DBNull.Value;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select count(*) from tablea where field_bool is null";
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
            command.ExecuteNonQuery();

            Assert.AreEqual(5, result);

        }

        [Test]
        public void AnsiStringSupport()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tablea(field_text) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.AnsiString));

            command.Parameters[0].Value = "TesteAnsiString";

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = String.Format("select count(*) from tablea where field_text = '{0}'", command.Parameters[0].Value);
            command.Parameters.Clear();

            Object result = command.ExecuteScalar(); // The missed cast is needed as Server7.2 returns Int32 and Server7.3+ returns Int64

            command.CommandText = "delete from tablea where field_serial = (select max(field_serial) from tablea);";
            command.ExecuteNonQuery();

            Assert.AreEqual(1, result);

        }


        [Test]
        public void MultipleQueriesFirstResultsetEmpty()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("insert into tablea(field_text) values ('a'); select count(*) from tablea;", _conn);

            Object result = command.ExecuteScalar();


            command.CommandText = "delete from tablea where field_serial > 5";
            command.ExecuteNonQuery();

            command.CommandText = "select * from tablea where field_serial = 0";
            command.ExecuteScalar();


            Assert.AreEqual(6, result);


        }

        [Test]
        [ExpectedException(typeof(EDBException))]
        public void ConnectionStringWithInvalidParameters()
        {
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;User Id=EDB_tests;Password=j");

            EDBCommand command = new EDBCommand("select * from tablea", conn);

            command.Connection.Open();
            command.ExecuteReader();
            command.Connection.Close();


        }

        [Test]
        [ExpectedException(typeof(EDBException))]
        public void InvalidConnectionString()
        {
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;User Id=EDB_tests");

            EDBCommand command = new EDBCommand("select * from tablea", conn);

            command.Connection.Open();
            command.ExecuteReader();
            command.Connection.Close();


        }


//        [Test]
//        public void AmbiguousFunctionParameterType()
//        {
//            EDBConnection conn = new EDBConnection("Server=127.0.0.1;User Id=enterprisedb;Password=enterprisedb");
//
//
//            EDBCommand command = new EDBCommand("ambiguousParameterType(:a, :b, :c, :d, :e, :f)", conn);
//            command.CommandType = CommandType.StoredProcedure;
//            EDBParameter p = new EDBParameter("a", DbType.Int16);
//            p.Value = 2;
//            command.Parameters.Add(p);
//            p = new EDBParameter("b", DbType.Int32);
//            p.Value = 2;
//            command.Parameters.Add(p);
//            p = new EDBParameter("c", DbType.Int64);
//            p.Value = 2;
//            command.Parameters.Add(p);
//            p = new EDBParameter("d", DbType.String);
//            p.Value = "a";
//            command.Parameters.Add(p);
//            p = new EDBParameter("e", DbType.String);
//            p.Value = "a";
//            command.Parameters.Add(p);
//            p = new EDBParameter("f", DbType.String);
//            p.Value = "a";
//            command.Parameters.Add(p);
//
//
//            command.Connection.Open();
//            command.Prepare();
//            command.ExecuteScalar();
//            command.Connection.Close();
//
//
//        }


        [Test]
        public void TestParameterReplace()
        {
            _conn.Open();

            String sql = @"select * from tablea where
                         field_serial = :a
                         ";


            EDBCommand command = new EDBCommand(sql, _conn);

            command.Parameters.Add(new EDBParameter("a", DbType.Int32));

            command.Parameters[0].Value = 2;

            Int32 rowsAdded = command.ExecuteNonQuery();

        }

        [Test]
        public void TestPointSupport()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("select field_point from tablee where field_serial = 1", _conn);

            EDBPoint p = (EDBPoint) command.ExecuteScalar();

            Assert.AreEqual(4, p.X);
            Assert.AreEqual(3, p.Y);
        }


        [Test]
        public void TestBoxSupport()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("select field_box from tablee where field_serial = 2", _conn);

            EDBBox box = (EDBBox) command.ExecuteScalar();

            Assert.AreEqual(5, box.UpperRight.X);
            Assert.AreEqual(4, box.UpperRight.Y);
            Assert.AreEqual(4, box.LowerLeft.X);
            Assert.AreEqual(3, box.LowerLeft.Y);


        }

        [Test]
        public void TestLSegSupport()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("select field_lseg from tablee where field_serial = 3", _conn);

            EDBLSeg lseg = (EDBLSeg) command.ExecuteScalar();

            Assert.AreEqual(4, lseg.Start.X);
            Assert.AreEqual(3, lseg.Start.Y);
            Assert.AreEqual(5, lseg.End.X);
            Assert.AreEqual(4, lseg.End.Y);


        }

        [Test]
        public void TestClosedPathSupport()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("select field_path from tablee where field_serial = 4", _conn);

            EDBPath path = (EDBPath) command.ExecuteScalar();

            Assert.AreEqual(false, path.Open);
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(4, path[0].X);
            Assert.AreEqual(3, path[0].Y);
            Assert.AreEqual(5, path[1].X);
            Assert.AreEqual(4, path[1].Y);


        }

        [Test]
        public void TestOpenPathSupport()
        {

           /* _conn.Open();

            EDBCommand command = new EDBCommand("select field_path from tablee where field_serial = 5", _conn);

            EDBPath path = (EDBPath) command.ExecuteScalar();

            Assert.AreEqual(true, path.Open);
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(4, path[0].X);
            Assert.AreEqual(3, path[0].Y);
            Assert.AreEqual(5, path[1].X);
            Assert.AreEqual(4, path[1].Y);*/


        }



        [Test]
        public void TestPolygonSupport()
        {

            /*_conn.Open();

            EDBCommand command = new EDBCommand("select field_polygon from tablee where field_serial = 6", _conn);

            EDBPolygon polygon = (EDBPolygon) command.ExecuteScalar();

            Assert.AreEqual(2, polygon.Count);
            Assert.AreEqual(4, polygon[0].X);
            Assert.AreEqual(3, polygon[0].Y);
            Assert.AreEqual(5, polygon[1].X);
            Assert.AreEqual(4, polygon[1].Y);*/


        }


        [Test]
        public void TestCircleSupport()
        {

           /* _conn.Open();

            EDBCommand command = new EDBCommand("select field_circle from tablee where field_serial = 7", _conn);

            EDBCircle circle = (EDBCircle) command.ExecuteScalar();

            Assert.AreEqual(4, circle.Center.X);
            Assert.AreEqual(3, circle.Center.Y);
            Assert.AreEqual(5, circle.Radius);*/



        }

		[Test]
		public void TestInet()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE INET_TBL ( i inet);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO INET_TBL (i) VALUES ('10.90.1.226/24');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO INET_TBL (i) VALUES ('254.168.1.226');";
			command.ExecuteNonQuery();

			command.CommandText="select * from INET_TBL";
		
			EDBDataReader Reader=command.ExecuteReader();

			try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Assert.AreEqual("10.90.1.226/24",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("254.168.1.226",Reader.GetValue(0).ToString());
			Reader.Close();
				
			command.CommandText="DROP Table INET_TBL";
			command.ExecuteNonQuery();
            _conn.Close();
		}
		

	
		[Test]
		public void TestCidr()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE CIDR_TBL (c cidr);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO CIDR_TBL  VALUES ('192.168.1');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO CIDR_TBL  VALUES ('182.90.6/26');";
			command.ExecuteNonQuery();

			command.CommandText="select * from CIDR_TBL";
		
			EDBDataReader Reader=command.ExecuteReader();

			try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			//Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("192.168.1.0/24",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("182.90.6.0/26",Reader.GetValue(0).ToString());
			//Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Close();
				
			command.CommandText="DROP Table CIDR_TBL";
			command.ExecuteNonQuery();
			_conn.Close();
		}
		



		[Test]
		public void TestNetworkAddress()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE NETADD_TBL (c cidr, i inet);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NETADD_TBL (c, i) VALUES ('192.168.1', '192.168.1.255/24');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NETADD_TBL (c, i) VALUES ('10.1.2.3', '10.1.2.3/32');";
			command.ExecuteNonQuery();

			command.CommandText="INSERT INTO NETADD_TBL (c, i) VALUES ('10', '10.1.2.3/8');";
			command.ExecuteNonQuery();		

			command.CommandText="INSERT INTO NETADD_TBL (c, i) VALUES ('10.0.0.0', '10.1.2.3/8');";
			command.ExecuteNonQuery();	

			command.CommandText="select * from NETADD_TBL";
		
			EDBDataReader Reader=command.ExecuteReader();

		
			
			try
			{
				Reader.Read();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			//Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("192.168.1.0/24",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("10.1.2.3/32",Reader.GetValue(0).ToString());
			//Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("10.0.0.0/8",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("10.0.0.0/32",Reader.GetValue(0).ToString());

			Reader.Close();
		
			command.CommandText="DROP Table NETADD_TBL";
			command.ExecuteNonQuery();
			_conn.Close();
		}
		


		[Test]
		public void TestNetworkFuncHost()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE NW_HOST (i inet);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NW_HOST (i) VALUES ('192.168.1.226');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NW_HOST (i) VALUES ('192.168.1.226');";
			command.ExecuteNonQuery();

			command.CommandText="SELECT host(i) from NW_HOST;";
		
			EDBDataReader Reader=command.ExecuteReader();

			

		try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			//Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("192.168.1.226",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("192.168.1.226",Reader.GetValue(0).ToString());
			//Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Close();
				
			command.CommandText="DROP Table NW_HOST";
			command.ExecuteNonQuery();
			_conn.Close();
		}
		


		[Test]
		public void TestNetworkFuncFamily()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE NW_FAMILY (i inet);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NW_FAMILY (i) VALUES ('10.90.1.145');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NW_FAMILY (i) VALUES ('255.122.11.129');";
			command.ExecuteNonQuery();

			command.CommandText="SELECT family(i) from NW_FAMILY;";
		
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			//Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("4",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("4",Reader.GetValue(0).ToString());
			//Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Close();
				
			command.CommandText="DROP Table NW_FAMILY";
			command.ExecuteNonQuery();
			_conn.Close();
		}




		[Test]
		public void TestNetworkFuncBroadcast()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE NWK_BROADCAST (c cidr);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NWK_BROADCAST (c) VALUES ('10.90.1.145');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NWK_BROADCAST(c) VALUES ('20');";
			command.ExecuteNonQuery();

			command.CommandText="SELECT BROADCAST(c) from NWK_BROADCAST;";
		
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			//Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("10.90.1.145",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("20.255.255.255/8",Reader.GetValue(0).ToString());
			//Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Close();
				
			command.CommandText="DROP Table NWK_BROADCAST";
			command.ExecuteNonQuery();
			_conn.Close();
		}



    }
}
