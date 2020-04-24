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
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient;
using NUnit.Framework;
using System.Data;
using System.IO;
using System.Globalization;


using System.Net;
using System.Net.Sockets;
using EDBTypes;
using System.Resources;
using System.Threading;
using System.Reflection;
using System.Text;
using NUnit.Framework.Constraints;

namespace EnterpriseDB.EDBClient.Tests
{
#pragma warning disable CS0618
#pragma warning disable CS8602
    #region EDB Command Tests
    public enum EnumTest : short
    {
        Value1 = 0,
        Value2 = 1
    };

    [TestFixture]
    public class EDBCommandTests : TestBase
    {
        private EDBConnection?	_conn = null;

        #region Setup / Tear Down
        [SetUp]
        protected void SetUp()
        { 
			_conn = new EDBConnection(ConnectionString);


            //TestUtil.ExecuteSql(_conn, "CREATE TABLE tablea(field_serial serial NOT NULL,field_text text,field_int4 integer,field_int8 bigint,field_bool boolean)");
            //TestUtil.ExecuteSql();
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

        #endregion

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
        public void ExecuteScalar2()
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

            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_text) values (:p0)", _conn);
            
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Text));
            
            command.Parameters["p0"].Value = @"\test";

            object result = command.ExecuteNonQuery();

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
//            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_text) values (:p0)", _conn);
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

            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_int4) values (:p0)", _conn);
            
            command.Parameters.Add(new EDBParameter("p0", 5));
            
            Assert.AreEqual(command.Parameters[0].EDBDbType, EDBDbType.Integer);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.Int32);

            object result = command.ExecuteNonQuery();

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

            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_int4) values (:p0)", _conn);
            
            command.Parameters.Add(new EDBParameter("p0", 5));
            
            Assert.AreEqual(command.Parameters[0].EDBDbType, EDBDbType.Integer);
            Assert.AreEqual(command.Parameters[0].DbType, DbType.Int32);
            
            
            object result = command.ExecuteNonQuery();

            Assert.AreEqual(1, result);
            
            
            EDBCommand command2 = new EDBCommand("select field_int4 from tablea where field_serial = (select max(field_serial) from tablea)", _conn);
            

            result = command2.ExecuteScalar();
                      
            
            new EDBCommand("delete from tablea where field_serial = (select max(field_serial) from tablea)", _conn).ExecuteNonQuery();
            
            Assert.AreEqual(5, result);
            
            
            
            //reader.FieldCount

        }
        
        
        

        [Test, Ignore("MERGE_NEED_TO_EXPLORE")]
        public void FunctionCallReturnSingleValue()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("", _conn);
			command.CommandText = "funcC";

			command.CommandType = CommandType.StoredProcedure;

            EDBDataReader result = command.ExecuteReader();

			Assert.True(result.Read());
            Assert.AreEqual(1, result.FieldCount);
			Assert.AreEqual(5, result.GetInt32(0));
        }


        [Test, Ignore("MERGE_NEED_TO_EXPLORE")]
        public void FunctionCallReturnSingleValueWithPrepare()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("funcC", _conn);
            command.CommandType = CommandType.StoredProcedure;  

            Object result = command.ExecuteScalar();

            Assert.AreEqual(5, result);

        }

        //[Test]
        public void FunctionCallWithParametersReturnSingleValue()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("public.funcC(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;

			//command.Parameters.Add(new EDBParameter("a", DbType.Int32));

			command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer, 10, "", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
			command.Prepare();

			command.Parameters[0].Value = 4;

            Int64 result = (long) command.ExecuteScalar();
			
            Assert.AreEqual(1, result);

        }

       // [Test]
        public void FunctionCallWithParametersReturnSingleValueEDBDbType()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("funcC(:a)", _conn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Integer));
            command.Prepare();
            command.Parameters[0].Value = 4;
            
            Int64 result = (long) command.ExecuteScalar();

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

            EDBCommand command = new EDBCommand("select * from funcb()", _conn);
            command.CommandType = CommandType.Text;

            EDBDataReader dr = command.ExecuteReader();
			Assert.AreEqual(5, dr.FieldCount);
			for (int i = 0; i < 5; i++)
			{
				Assert.True(dr.Read());
			}
			Assert.False(dr.Read());

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

            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_text) values (:p0);", _conn);
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Text));
            command.Parameters["p0"].Value = "test";
            

            command.Prepare();

            
            EDBDataReader dr = command.ExecuteReader();
            dr.Close();
            new EDBCommand("delete from tablea where field_serial = (select max(field_serial) from tablea);", _conn).ExecuteNonQuery();

        }
        
        [Test]
        public void PreparedStatementInsertNullValue()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_int4) values (:p0);", _conn);
            command.Parameters.Add(new EDBParameter("p0", EDBDbType.Integer));
            command.Parameters["p0"].Value = DBNull.Value;
            

            command.Prepare();

            
            EDBDataReader dr = command.ExecuteReader();
            dr.Close();
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

        private void NotificationSupportHelper(object sender, EDBNotificationEventArgs args)
        {
            throw new InvalidOperationException();
        }
		
		[Test]
        public void ByteSupport()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tableb(field_int2) values (:a)", _conn);

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


            EDBCommand command = new EDBCommand("INSERT INTO tableb(field_int2) values (:a)", _conn);

            command.Parameters.Add(new EDBParameter("a", EDBDbType.Integer));

            command.Parameters[0].Value = 0;
            

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

            command.CommandText = "INSERT INTO tableb(field_timestamp) values (:a);delete from tableb where field_serial > 4;";
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

            command.CommandText = "INSERT INTO tableb(field_timestamp) values (:a);delete from tableb where field_serial > 4;";
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

            //   DateTime d = command.ExecuteScalar();
            TimeSpan tm = (TimeSpan)command.ExecuteScalar();

            Console.WriteLine(tm.ToString());


            Assert.AreEqual("10:03:45.3450000", tm.ToString());

        }

        [Test]
        public void NumericSupport()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tableb(field_numeric) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter("a", DbType.Decimal));

            command.Parameters[0].Value = 7.4M;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tableb where field_numeric = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);

            dr.Close();
            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();

            Assert.AreEqual(7.4000000M, result);

        }

        [Test]
        public void NumericSupportEDBDbType()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tableb(field_numeric) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter("a", EDBDbType.Numeric));

            command.Parameters[0].Value = 7.4M;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tableb where field_numeric = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Decimal result = dr.GetDecimal(3);
            dr.Close();

            command.CommandText = "delete from tableb where field_serial = (select max(field_serial) from tableb) and field_serial != 3;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();

            
            Assert.AreEqual(7.4000000M, result);




        }


        [Test]
        public void InsertSingleValue()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tabled(field_float4) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter(":a", DbType.Single));

            command.Parameters[0].Value = 7.4F;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float4 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Single result = dr.GetFloat(1);
            dr.Close();

            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4F, result);

        }


        [Test]
        public void InsertSingleValueEDBDbType()
        {
            _conn.Open();


            EDBCommand command = new EDBCommand("INSERT INTO tabled(field_float4) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Real));

            command.Parameters[0].Value = 7.4F;

            Int32 rowsAdded = command.ExecuteNonQuery();

            Assert.AreEqual(1, rowsAdded);

            command.CommandText = "select * from tabled where field_float4 = :a";


            EDBDataReader dr = command.ExecuteReader();
            dr.Read();

            Single result = dr.GetFloat(1);

            dr.Close();
            command.CommandText = "delete from tabled where field_serial > 2;";
            command.Parameters.Clear();
            command.ExecuteNonQuery();


            Assert.AreEqual(7.4F, result);

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

            Assert.AreEqual(-4.3000000M, (decimal)result);
            //Assert.AreEqual(11, result.Precision);
            //Assert.AreEqual(7, result.Scale);

        }

        [Test]
        public void MultipleQueriesFirstResultsetEmpty()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("INSERT INTO tablea(field_text) values ('a'); select count(*) from tablea;", _conn);

            Object result = command.ExecuteScalar();


            command.CommandText = "delete from tablea where field_serial > 5";
            command.ExecuteNonQuery();

            command.CommandText = "select * from tablea where field_serial = 0";
            command.ExecuteScalar();


            Assert.AreEqual(6, result);


        }

        [Test]
        public void ConnectionStringWithInvalidParameters()
        {
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;User Id=EDB_tests;Password=j");

            EDBCommand command = new EDBCommand("select * from tablea", conn);

            Assert.Throws<EDBException>(() => command.Connection.Open());

        }

        [Test]
        public void InvalidConnectionString()
        {
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;User Id=EDB_tests;Password=j");

            EDBCommand command = new EDBCommand("select * from tablea", conn);
            Assert.Throws<EDBException>(() => command.Connection.Open());

       //     Assert("Either password must be specified or IntegratedSecurity must be on",);

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

            string sql = @"select * from tablea where
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

            _conn.Open();

            EDBCommand command = new EDBCommand("select field_path from tablee where field_serial = 5", _conn);

            EDBPath path = (EDBPath) command.ExecuteScalar();

            Assert.AreEqual(true, path.Open);
            Assert.AreEqual(2, path.Count);
            Assert.AreEqual(4, path[0].X);
            Assert.AreEqual(3, path[0].Y);
            Assert.AreEqual(5, path[1].X);
            Assert.AreEqual(4, path[1].Y);


        }



        [Test]
        public void TestPolygonSupport()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("select field_polygon from tablee where field_serial = 6", _conn);

            EDBPolygon polygon = (EDBPolygon) command.ExecuteScalar();

            Assert.AreEqual(2, polygon.Count);
            Assert.AreEqual(4, polygon[0].X);
            Assert.AreEqual(3, polygon[0].Y);
            Assert.AreEqual(5, polygon[1].X);
            Assert.AreEqual(4, polygon[1].Y);


        }


        [Test]
        public void TestCircleSupport()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("select field_circle from tablee where field_serial = 7", _conn);

            EDBCircle circle = (EDBCircle) command.ExecuteScalar();

            Assert.AreEqual(4, circle.Center.X);
            Assert.AreEqual(3, circle.Center.Y);
            Assert.AreEqual(5, circle.Radius);



        }

		[Test]
		public void TestInet()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE INET_TBL ( i inet);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO INET_TBL (i) VALUES ('10.90.1.226/32');";
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

			Assert.AreEqual("10.90.1.226",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("254.168.1.226",Reader.GetValue(0).ToString());
			Reader.Close();
				
			command.CommandText="DROP TABLE INET_TBL";
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
            
			Assert.AreEqual("(192.168.1.0, 24)",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("(182.90.6.0, 26)",Reader.GetValue(0).ToString());
			Reader.Close();
				
			command.CommandText="DROP TABLE CIDR_TBL";
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
            
			Assert.AreEqual("(192.168.1.0, 24)",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("(10.1.2.3, 32)",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("(10.0.0.0, 8)",Reader.GetValue(0).ToString());
			Reader.Read();
		    Assert.AreEqual("(10.0.0.0, 32)",Reader.GetValue(0).ToString());

			Reader.Close();
		
			command.CommandText="DROP TABLE NETADD_TBL";
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
				
			command.CommandText="DROP TABLE NW_HOST";
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
				
			command.CommandText="DROP TABLE NW_FAMILY";
			command.ExecuteNonQuery();
			_conn.Close();
		}




		//[Test]
		public void TestNetworkFuncBroadcast()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE NWK_BROADCAST (c cidr);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NWK_BROADCAST (c) VALUES ('10.90.1.145/32');";
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
				
			command.CommandText="DROP TABLE NWK_BROADCAST";
			command.ExecuteNonQuery();
			_conn.Close();
		}


	//	[Test]
				public void TestNetworkFuncMasklen()
				{
			
					_conn.Open();

					EDBCommand command = new EDBCommand("CREATE TABLE NETADD_MASKLEN (c cidr, i inet);", _conn);
					command.ExecuteNonQuery();
					command.CommandText="INSERT INTO NETADD_MASKLEN (c, i) VALUES ('192.168.1', '192.168.1.255/24');";
					command.ExecuteNonQuery();
					command.CommandText="INSERT INTO NETADD_MASKLEN (c, i) VALUES ('10.1.2.3', '10.1.2.3/32');";
					command.ExecuteNonQuery();

					command.CommandText="INSERT INTO NETADD_MASKLEN (c, i) VALUES ('10', '10.1.2.3/8');";
					command.ExecuteNonQuery();		

					command.CommandText="INSERT INTO NETADD_MASKLEN (c, i) VALUES ('10.0.0.0', '10.1.2.3/8');";
					command.ExecuteNonQuery();	

					command.CommandText="SELECT masklen(c) from NETADD_MASKLEN;";
		
					EDBDataReader Reader=command.ExecuteReader();

		
			
					try
					{
						Reader.Read();
					}
					catch(EDBException exp)
					{
						throw new Exception(exp.ToString());
					}

					Console.WriteLine(Reader.GetValue(0).ToString());
					Assert.AreEqual("24",Reader.GetValue(0).ToString());
					Reader.Read();
					Assert.AreEqual("32",Reader.GetValue(0).ToString());
					Console.WriteLine(Reader.GetValue(0).ToString());
					Reader.Read();
					Console.WriteLine(Reader.GetValue(0).ToString());
					Assert.AreEqual("8",Reader.GetValue(0).ToString());
					Reader.Read();
					Console.WriteLine(Reader.GetValue(0).ToString());
					Assert.AreEqual("32",Reader.GetValue(0).ToString());

					Reader.Close();
		
					command.CommandText="DROP TABLE NETADD_MASKLEN;";
					command.ExecuteNonQuery();
					_conn.Close();
				}
		

		[Test]
		public void TestNetworkFuncText()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE NET_TEXT (c cidr, i inet);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NET_TEXT (c, i) VALUES ('192.168.1', '192.168.1.255/24');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO NET_TEXT(c, i) VALUES ('10.1.2.3', '10.1.2.3/32');";
			command.ExecuteNonQuery();

			command.CommandText="INSERT INTO NET_TEXT (c, i) VALUES ('10', '10.1.2.3/8');";
			command.ExecuteNonQuery();		

			command.CommandText="INSERT INTO NET_TEXT ( c, i) VALUES ('10.0.0.0', '10.1.2.3/8');";
			command.ExecuteNonQuery();	

			command.CommandText="SELECT text(c) from NET_TEXT;";
		
			EDBDataReader Reader=command.ExecuteReader();

		
			
			try
			{
				Reader.Read();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("192.168.1.0/24",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("10.1.2.3/32",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Read();
			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("10.0.0.0/8",Reader.GetValue(0).ToString());
			Reader.Read();
			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("10.0.0.0/32",Reader.GetValue(0).ToString());

			Reader.Close();
		
			command.CommandText="DROP TABLE NET_TEXT;";
			command.ExecuteNonQuery();
			_conn.Close();
		}


		//[Test]
		public void TestNetworkFuncsetmask()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE tbl_setmasklen (c cidr, i inet);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO tbl_setmasklen (c, i) VALUES ('192.168.1', '192.168.1.255/24');;";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO tbl_setmasklen(c, i) VALUES ('10.1.2.3', '10.1.2.3/32');";
			command.ExecuteNonQuery();

			command.CommandText="INSERT INTO tbl_setmasklen (c, i) VALUES ('10', '10.1.2.3/8');";
			command.ExecuteNonQuery();		

			command.CommandText="INSERT INTO tbl_setmasklen ( c, i) VALUES ('10.0.0.0', '10.1.2.3/8');";
			command.ExecuteNonQuery();	

			command.CommandText="SELECT set_masklen(inet(text(i)), 24) from tbl_setmasklen;";
		
			EDBDataReader Reader=command.ExecuteReader();

		
			
			try
			{
				Reader.Read();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("192.168.1.255/24",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("10.1.2.3/24",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Read();
			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("10.1.2.3/24",Reader.GetValue(0).ToString());
			Reader.Read();
			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("10.1.2.3/24",Reader.GetValue(0).ToString());

			Reader.Close();
		
			command.CommandText="DROP TABLE tbl_setmasklen;";
			command.ExecuteNonQuery();
			_conn.Close();
		}


		[Test]
		public void TestNetworkFunc()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE tbl_network (c cidr, i inet);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO tbl_network (c, i) VALUES ('192.168.1', '192.168.1.255/24');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO tbl_network(c, i) VALUES ('10.1.2.3', '10.1.2.3/32');";
			command.ExecuteNonQuery();

			command.CommandText="INSERT INTO tbl_network (c, i) VALUES ('10', '182.1.2.3/8');";
			command.ExecuteNonQuery();		

			command.CommandText="INSERT INTO tbl_network ( c, i) VALUES ('10.0.0.0', '10.1.2.19/24');";
			command.ExecuteNonQuery();	

			command.CommandText="SELECT network(c),network (i) from tbl_network;";
		
			EDBDataReader Reader=command.ExecuteReader();

		
			
			try
			{
				Reader.Read();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("(192.168.1.0, 24)",Reader.GetValue(0).ToString());
			Reader.Read();
		    Assert.AreEqual("(10.1.2.3, 32)",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Read();
			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("(10.0.0.0, 8)",Reader.GetValue(0).ToString());
			Reader.Read();
			Console.WriteLine(Reader.GetValue(0).ToString());
		
            Assert.AreEqual("(10.0.0.0, 32)",Reader.GetValue(0).ToString());

			Reader.Close();
		
			command.CommandText="DROP TABLE tbl_network;";
			command.ExecuteNonQuery();
			_conn.Close();
		}



		[Test]
		public void TestNetworkInputVar()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE tbl_net(c cidr, i inet);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO tbl_net (c, i) VALUES ('10:23::8000/113', '10:23::ffff');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO tbl_net(c, i) VALUES ('::ffff:1.2.3.4', '::4.3.2.1/24');";
			command.ExecuteNonQuery();

			command.CommandText="SELECT text(c),text(i) from tbl_net;";
		
			EDBDataReader Reader=command.ExecuteReader();
            
			try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("10:23::8000/113",Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("::ffff:1.2.3.4/128",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Close();
				
			command.CommandText="DROP TABLE tbl_net;";
			command.ExecuteNonQuery();
			_conn.Close();
		}


		[Test]
		public void TestMacAddress()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE tbl_mac(mac macaddr);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO tbl_mac VALUES ('08002b:010203');";
			command.ExecuteNonQuery();
		
			command.CommandText="SELECT * from tbl_mac;";
		
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
            Assert.AreEqual("08002B010203", Reader.GetValue(0).ToString());
			
			Reader.Close();
				
			command.CommandText="DROP TABLE tbl_mac;";
			command.ExecuteNonQuery();
			_conn.Close();
		}

		

		[Test]
		public void TestHarwareAddress()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE tbl_macadd(m macaddr);", _conn);
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO tbl_macadd VALUES ('0800.2b01.0203');";
			command.ExecuteNonQuery();
			command.CommandText="INSERT INTO tbl_macadd VALUES ('06-20-1a-23-02-21');";
			command.ExecuteNonQuery();
		
			command.CommandText="SELECT * from tbl_macadd;";
		
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
                
			Console.WriteLine(Reader.GetValue(0).ToString());
            Assert.AreEqual("08002B010203", Reader.GetValue(0).ToString());
			Reader.Read();
			Console.WriteLine(Reader.GetValue(0).ToString());
            Assert.AreEqual("06201A230221", Reader.GetValue(0).ToString());
			Reader.Close();
				
			command.CommandText="DROP TABLE tbl_macadd;";
			command.ExecuteNonQuery();
			_conn.Close();
		}


	//Checkme	[Test]
		public void TestArrayInet()
		{
			
			_conn.Open();

            EDBInet[] a = {new EDBInet("10.90.1.226/24"),new EDBInet("192.168.1.255/25"),new EDBInet("9.1.2.3/8")};
			EDBCommand command = new EDBCommand("CREATE TABLE tbl_inet_arr ( i inet[]);", _conn);
			command.ExecuteNonQuery();
			command.CommandText=" INSERT INTO tbl_inet_arr (i)  VALUES ( '{10.90.1.226/24, 192.168.1.255/25,9.1.2.3/8}');";
			command.ExecuteNonQuery();
		
			command.CommandText="SELECT * from tbl_inet_arr;";
		
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual(a,(EDBInet[]) Reader.GetValue(0));
			
			Reader.Close();
				
			command.CommandText="DROP TABLE tbl_inet_arr";
			command.ExecuteNonQuery();
			_conn.Close();
		}


		//checkme [Test]
		public void TestArraycidr()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE tbl_cidr_arr ( c cidr[]);", _conn);
			command.ExecuteNonQuery();
			command.CommandText=" INSERT INTO tbl_cidr_arr (c)  VALUES ( '{192.168.1.0/26, 10.1.2.3,20.2.3.164}');";
			command.ExecuteNonQuery();
		
			command.CommandText="SELECT * from tbl_cidr_arr;";
		
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				Reader.Read();;
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("{192.168.1.0/26,10.1.2.3/32,20.2.3.164/32}",Reader.GetValue(0).ToString());
			
			Reader.Close();
				
			command.CommandText="DROP TABLE tbl_cidr_arr";
			command.ExecuteNonQuery();
			_conn.Close();
		}


		[Test]
		public void TestInheritance()
		{
			
			_conn.Open();

			EDBCommand command = new EDBCommand("CREATE TABLE inhx (xx text DEFAULT 'text');", _conn);
			command.ExecuteNonQuery();
		    command = new EDBCommand("CREATE TABLE inhf (LIKE inhx INCLUDING DEFAULTS);", _conn);
			command.ExecuteNonQuery();
			command.CommandText=" INSERT INTO inhf DEFAULT VALUES;";
			command.ExecuteNonQuery();
		
			command.CommandText="SELECT * FROM inhf;";
		
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				Reader.Read();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("text",Reader.GetValue(0).ToString());
			
			Reader.Close();
				
			command.CommandText="DROP TABLE inhf;";
			command.ExecuteNonQuery();
			command.CommandText="DROP TABLE inhx;";
			command.ExecuteNonQuery();
			_conn.Close();
		}

		[Test]
		public void TestDoubleInheritance()
		{
			
			_conn.Open();
			//EDBTransaction tran=_conn.BeginTransaction();

			EDBCommand command = new EDBCommand("CREATE TABLE p1(ff1 int);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("CREATE TABLE p2(f1 text);", _conn);
			command.ExecuteNonQuery();
			//tran.Commit();
			command = new EDBCommand("CREATE TABLE c1(f3 int) inherits(p1,p2);", _conn);
			command.ExecuteNonQuery();
			command.CommandText=" INSERT INTO p2 values ('hello');";
			command.ExecuteNonQuery();
			command.CommandText=" INSERT INTO c1(ff1,f1,f3) values(56789, 'hi', 42);";
			command.ExecuteNonQuery();
		
			command.CommandText="SELECT * FROM c1;";
		
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				Reader.Read();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("56789",Reader.GetValue(0).ToString());
			
			Reader.Close();
				
			command.CommandText="DROP TABLE c1;";
			command.ExecuteNonQuery();
			command.CommandText="DROP TABLE p2;";
			command.ExecuteNonQuery();
			command.CommandText="DROP TABLE p1;";
			command.ExecuteNonQuery();
			//tran.Rollback();
			
			_conn.Close();
		}

		[Test]
		public void TestInheritanceUpdate()
		{
			
			_conn.Open();
			//EDBTransaction tran=_conn.BeginTransaction();

			EDBCommand command = new EDBCommand("create temp table foo(f1 int, f2 int);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("INSERT INTO foo values(1,1);INSERT INTO foo values(3,3);", _conn);
			command.ExecuteNonQuery();
			//tran.Commit();
			command = new EDBCommand("create temp table bar(f1 int, f2 int);", _conn);
			command.ExecuteNonQuery();
			command.CommandText=" INSERT INTO bar values(1,1);";
			command.ExecuteNonQuery();
			command.CommandText=" update bar set f2 = f2 + 100 where f1 in (select f1 from foo);";
			command.ExecuteNonQuery();
		
			command.CommandText="select * from bar";
		
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				
					Reader.Read();
				
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(1).ToString());

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Assert.AreEqual("101",Reader.GetValue(1).ToString());
			Reader.Close();
				
			command.CommandText="DROP TABLE foo;";
			command.ExecuteNonQuery();
			command.CommandText="DROP TABLE bar;";
			command.ExecuteNonQuery();
			
			//tran.Rollback();
		
			_conn.Close();
		}


		[Test]
		public void TestInheritancetest2()
		{
			
			_conn.Open();
			//EDBTransaction tran=_conn.BeginTransaction();

			EDBCommand command = new EDBCommand("CREATE TABLE base (i varchar);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("CREATE TABLE derived() inherits (base);", _conn);
			command.ExecuteNonQuery();
			//tran.Commit();
			command = new EDBCommand("INSERT INTO derived (i) values ('abc');", _conn);
			command.ExecuteNonQuery();
            /*ZK: refer to http://www.EDB.org/doc/faq.html for details*/
			command.CommandText="select derived::TEXT from derived ;";
		    
			EDBDataReader Reader=command.ExecuteReader();

			

			try
			{
				
				Reader.Read();
				
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

			Console.WriteLine(Reader.GetValue(0).ToString());
			//Console.WriteLine(Reader.GetValue(1).ToString());

			Assert.AreEqual("(abc)",Reader.GetValue(0).ToString());
			/*Assert.AreEqual("101",Reader.GetValue(1).ToString());*/
			Reader.Close();
				
			command.CommandText="DROP TABLE derived;";
			command.ExecuteNonQuery();
			command.CommandText="DROP TABLE base;";
			command.ExecuteNonQuery();
			
			//tran.Rollback();
		
			_conn.Close();
		}


	//	[Test]
		public void CompositeTypeTestGeneric()
		{
			
			_conn.Open();
			

			EDBCommand command = new EDBCommand("CREATE TYPE inventory_item AS ( name  text,supplier_id     integer,  price numeric);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("CREATE TABLE on_hand (item  inventory_item,count     integer);", _conn);
			command.ExecuteNonQuery();
			
			command = new EDBCommand("INSERT INTO on_hand VALUES (ROW('fuzzy dice', 42, 1.99), 1000);", _conn);
			command.ExecuteNonQuery();
			
			command.CommandText="select * from on_hand;";
		
			EDBDataReader Reader=command.ExecuteReader();



			try
			{
				Reader.Read();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}


            Console.WriteLine(Reader[0].ToString());
			Assert.AreEqual("(\"fuzzy dice\",42,1.99)",Reader.GetValue(0).ToString());
			
			Reader.Close();
				
			
			command.CommandText="DROP TABLE on_hand;";
			command.ExecuteNonQuery();
			command.CommandText="drop TYPE inventory_item;";
			command.ExecuteNonQuery();
			
			
		
			_conn.Close();
		}
		
		[Test]
		public void CompositeTypeTestIndividualValue()
		{
			
			_conn.Open();
			

			EDBCommand command = new EDBCommand("CREATE TYPE inventory_item AS ( name  text,supplier_id     integer,  price numeric);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("CREATE TABLE on_hand (item  inventory_item,count     integer);", _conn);
			command.ExecuteNonQuery();
			
			command = new EDBCommand("INSERT INTO on_hand VALUES (ROW('fuzzy dice', 42, 1.99), 1000);", _conn);
			command.ExecuteNonQuery();
			
			command.CommandText="select (item).name from on_hand where (item).price=1.99;";
		
			EDBDataReader Reader=command.ExecuteReader();



			try
			{
				
				Reader.Read();
				
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		

			Assert.AreEqual("fuzzy dice",Reader.GetValue(0).ToString());
			
			Reader.Close();
				
			
			command.CommandText="DROP TABLE on_hand;";
			command.ExecuteNonQuery();
			command.CommandText="drop TYPE inventory_item;";
			command.ExecuteNonQuery();
			
			
		
			_conn.Close();
		}


		[Test]
		public void CompositeTypeTestMultiTable()
		{
			
			_conn.Open();
			

			EDBCommand command = new EDBCommand("CREATE TYPE inventory_item AS ( name  text,supplier_id     integer,  price numeric);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("CREATE TABLE on_hand (item  inventory_item,count     integer);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("CREATE TABLE on_hand2 (item  inventory_item,count     integer);", _conn);
			command.ExecuteNonQuery();
			
			
			command = new EDBCommand("INSERT INTO on_hand VALUES (ROW('fuzzy dice', 42, 1.99), 1000);", _conn);
			command.ExecuteNonQuery();
			
			command = new EDBCommand("INSERT INTO on_hand2 VALUES (ROW('fuzzy dice', 42, 1.99), 1000);", _conn);
			command.ExecuteNonQuery();
			

			command.CommandText="select (on_hand.item).name  from on_hand where (on_hand.item).price=( select (on_hand2.item).price from on_hand2 where (on_hand.item).name='fuzzy dice')";
		
			EDBDataReader Reader=command.ExecuteReader();

				

			try
			{
				
				Reader.Read();
				
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		

			Assert.AreEqual("fuzzy dice",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Close();
				
			
			command.CommandText="DROP TABLE on_hand;";
			command.ExecuteNonQuery();
			command.CommandText="DROP TABLE on_hand2;";
			command.ExecuteNonQuery();
			command.CommandText="drop TYPE inventory_item;";
			command.ExecuteNonQuery();
			
			
		
			_conn.Close();
		}

		[Test]
		public void CompositeTypeTestUpdate()
		{
			
			_conn.Open();
			

			EDBCommand command = new EDBCommand("CREATE TYPE inventory_item AS ( name  text,supplier_id     integer,  price numeric);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("CREATE TABLE on_hand (item  inventory_item,count     integer);", _conn);
			command.ExecuteNonQuery();
			
			command = new EDBCommand("INSERT INTO on_hand VALUES (ROW('fuzzy dice', 42, 1.99), 1000);", _conn);
			command.ExecuteNonQuery();
			
			command = new EDBCommand("UPDATE on_hand SET item = ROW('New Name', 50, 10.99) WHERE count=1000;", _conn);
			command.ExecuteNonQuery();
			
			command.CommandText="select (item).name from on_hand where count=1000;";
		
			EDBDataReader Reader=command.ExecuteReader();


		
						

			try
			{
				
				Reader.Read();
				
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		

			Assert.AreEqual("New Name",Reader.GetValue(0).ToString());
			
			Reader.Close();
				
			
			command.CommandText="DROP TABLE on_hand;";
			command.ExecuteNonQuery();
			command.CommandText="drop TYPE inventory_item;";
			command.ExecuteNonQuery();
			
			
		
			_conn.Close();
		}


		[Test]
		public void CompositeTypeTestDelete()
		{
			
			_conn.Open();
			

			EDBCommand command = new EDBCommand("CREATE TYPE inventory_item AS ( name  text,supplier_id     integer,  price numeric);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("CREATE TABLE on_hand (item  inventory_item,count     integer);", _conn);
			command.ExecuteNonQuery();
			
			command = new EDBCommand("INSERT INTO on_hand VALUES (ROW('fuzzy dice', 42, 1.99), 1000);", _conn);
			command.ExecuteNonQuery();
			
			command = new EDBCommand("delete from on_hand WHERE count=1000;", _conn);
			command.ExecuteNonQuery();
			
			command.CommandText="select * from on_hand where count=1000;";
		
			EDBDataReader Reader=command.ExecuteReader();
			
			
			Assert.IsFalse(Reader.HasRows);
			Reader.Close();
				
			
			command.CommandText="DROP TABLE on_hand;";
			command.ExecuteNonQuery();
			command.CommandText="drop TYPE inventory_item;";
			command.ExecuteNonQuery();
			
			
		
			_conn.Close();
		}


		[Test]
		public void SingleExecuteNonQuerryCreateTable()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE TABLE TAB1(A INT4)",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand(" DROP TABLE TAB1",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteScalarCreateTable()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE TABLE TAB1(A INT4)",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand(" DROP TABLE TAB1",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteReaderCreateTable()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE TABLE TAB1read(A INT4)",_conn);
			EDBDataReader dr = command.ExecuteReader();
            dr.Close();
			command=new EDBCommand(" DROP TABLE TAB1read",_conn);
			command.ExecuteReader();


			_conn.Close();
		}

		[Test]
		public void MultipleExecuteNonQuerryCreateTable()
		{
			
			_conn.Open();
			
            
			EDBCommand command=new EDBCommand("CREATE TABLE TAB1(A INT4)",_conn);
			command.ExecuteNonQuery();

            EDBCommand command1 = new EDBCommand("CREATE TABLE TAB2(A INT4)", _conn);
			command1.ExecuteNonQuery();
		

			command=new EDBCommand("DROP TABLE TAB1",_conn);
			command.ExecuteNonQuery();

            EDBCommand command2 = new EDBCommand("DROP TABLE TAB2", _conn);
            command2.ExecuteNonQuery();
		
			_conn.Close();
		}


        // Redundant cases are removed [Test]
		public void MultipleExecuteScalarCreateTable()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE TABLE TAB1(A INT4);CREATE TABLE TAB2(A INT4)",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand(" DROP TABLE TAB1;DROP TABLE TAB2",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteReaderCreateTable()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE TABLE TAB1(A INT4);CREATE TABLE TAB2(A INT4)",_conn);
			command.ExecuteReader();
			
			command=new EDBCommand("DROP TABLE TAB1;DROP TABLE TAB2",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteNonQuerryCreateView()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE VIEW vista AS SELECT text 'Hello World' AS hello;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand(" DROP VIEW vista",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteScalarCreateView()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE VIEW vista AS SELECT text 'Hello World' AS hello;",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP VIEW vista",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteReaderCreateView()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE VIEW vista AS SELECT text 'Hello World' AS hello;",_conn);
			EDBDataReader dr= command.ExecuteReader();
            dr.Close();
			command=new EDBCommand("DROP VIEW vista",_conn);
			command.ExecuteReader();


			_conn.Close();
		}

		[Test]
		public void MultipleExecuteNonQuerryCreateView()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE VIEW vista AS SELECT text 'Hello World' AS hello;",_conn);
			command.ExecuteNonQuery();


            EDBCommand command1 = new EDBCommand("CREATE VIEW vistb AS SELECT text 'Hi Man' AS hi;", _conn);
            command1.ExecuteNonQuery();
			

			command=new EDBCommand(" DROP VIEW vista",_conn);
			command.ExecuteNonQuery();

            EDBCommand command2 = new EDBCommand("DROP VIEW vistb", _conn);
            command2.ExecuteNonQuery();
			

			_conn.Close();
		}


	// redundant 	[Test]
		public void MultipleExecuteScalarCreateView()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE VIEW vistb AS SELECT text 'Hi Man' AS hi;",_conn);
			command.ExecuteScalar();


            command = new EDBCommand("CREATE VIEW vista AS SELECT text 'Hello World' AS hello;", _conn);
            command.ExecuteScalar();

            command = new EDBCommand("DROP VIEW vista;DROP VIEW vistb", _conn);
            command.ExecuteScalar();

			command=new EDBCommand("DROP VIEW vista;DROP VIEW vistb",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


		// [Test]
		public void MultipleExecuteReaderCreateView()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE VIEW vistb AS SELECT text 'Hi Man' AS hi;",_conn);
			command.ExecuteReader();
            command = new EDBCommand("CREATE VIEW vista AS SELECT text 'Hello World' AS hello;", _conn);
            command.ExecuteReader();
            command = new EDBCommand("DROP VIEW vista;DROP VIEW vistb", _conn);
            command.ExecuteReader();
			command=new EDBCommand("DROP VIEW vista;DROP VIEW vistb",_conn);
			command.ExecuteReader();


			_conn.Close();
		}

		[Test]
		public void SingleExecuteNonQuerryCreateSequence()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE SEQUENCE seq1 START 10;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand(" DROP SEQUENCE seq1",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteScalarCreateSequence()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE SEQUENCE seq1 START 10;",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP SEQUENCE seq1",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteReaderCreateSequence()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE SEQUENCE seq1 START 10;",_conn);
			EDBDataReader dr= command.ExecuteReader();
            dr.Close();
			command=new EDBCommand("DROP SEQUENCE seq1",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


		[Test]
		public void MultipleExecuteNonQuerryCreateSequence()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE SEQUENCE seq22 START 1;",_conn);
			command.ExecuteNonQuery();

            command = new EDBCommand("CREATE SEQUENCE seq11 START 10;", _conn);
			command.ExecuteNonQuery();
            command = new EDBCommand("DROP SEQUENCE seq22", _conn);
            command.ExecuteNonQuery();
            command = new EDBCommand(" DROP SEQUENCE seq11;", _conn);
            command.ExecuteNonQuery();


			_conn.Close();
		}


		[Test]
		public void MultipleExecuteScalarCreateSequence()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE SEQUENCE seq_2 START 1;",_conn);
			command.ExecuteScalar();

            command = new EDBCommand("CREATE SEQUENCE seq_1 START 10;", _conn);
            command.ExecuteScalar();

            command = new EDBCommand("DROP SEQUENCE seq_2", _conn);
            command.ExecuteScalar();

			command=new EDBCommand("DROP SEQUENCE seq_1",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


		[Test]
		public void MultipleExecuteReaderCreateSequence()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE SEQUENCE seq_22 START 1;",_conn);
			EDBDataReader dr= command.ExecuteReader();
            dr.Close();  
          
            command = new EDBCommand("CREATE SEQUENCE seq_11 START 10;", _conn);
			dr = command.ExecuteReader();
            dr.Close();

            command = new EDBCommand("DROP SEQUENCE seq_11;", _conn);
            dr = command.ExecuteReader();
            dr.Close();
            command = new EDBCommand("DROP SEQUENCE seq_22;", _conn);
            dr = command.ExecuteReader();
            dr.Close();

			_conn.Close();
		}


		[Test]
		public void SingleExecuteNonQuerryProcedure()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE PROCEDURE P1 AS BEGIN NULL; END;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP PROCEDURE p1",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteScalarProcedure()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE PROCEDURE P1 AS BEGIN NULL;END;",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP PROCEDURE p1",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteReaderProcedure()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE PROCEDURE P1 AS BEGIN NULL;END;",_conn);
			EDBDataReader dr = command.ExecuteReader();
            dr.Close();
			command=new EDBCommand("DROP PROCEDURE p1",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteNonQuerryProcedure()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE PROCEDURE P1 IS BEGIN NULL;END;CREATE PROCEDURE P2 AS BEGIN NULL;END;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand(" DROP PROCEDURE p1;DROP PROCEDURE p2",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteScalarProcedure()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE PROCEDURE P1 IS BEGIN NULL;\rEND;CREATE PROCEDURE P2 AS \rBEGIN\rNULL;\rEND;",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP PROCEDURE p1;DROP PROCEDURE p2",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // Redundant cases are removed [Test]
		public void MultipleExecuteReaderProcedure()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE PROCEDURE P1 IS \rBEGIN\rNULL;\rEND;CREATE PROCEDURE P2 AS \rBEGIN\rNULL;\rEND;",_conn);
			command.ExecuteReader();
			
			command=new EDBCommand("DROP PROCEDURE p1;DROP PROCEDURE p2",_conn);
			command.ExecuteReader();


			_conn.Close();
		}

		[Test]
		public void SingleExecuteNonQuerrySPLFunc()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1 RETURN VOID AS BEGIN NULL;END;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP FUNCTION p1",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteScalarSPLFunc()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1 RETURN VOID AS BEGIN NULL;END;",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP FUNCTION p1",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteReaderSPLFunc()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1 RETURN VOID AS BEGIN NULL;END;",_conn);
			EDBDataReader dr = command.ExecuteReader();
            dr.Close();
			command=new EDBCommand("DROP FUNCTION p1",_conn);
			dr = command.ExecuteReader();
            dr.Close();

			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteNonQuerrySPLFunc()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1 RETURN VOID AS \rBEGIN\rNULL;\rEND;CREATE FUNCTION P2 RETURN VOID AS \rBEGIN\rNULL;\rEND;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP FUNCTION p1;DROP FUNCTION p2",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


        // Redundant cases are removed[Test]
		public void MultipleExecuteScalarSPLFunc()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1 RETURN VOID AS \rBEGIN\rNULL;\rEND;CREATE FUNCTION P2 RETURN VOID AS \rBEGIN\rNULL;\rEND;",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP FUNCTION p1;DROP FUNCTION p2",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteReaderSPLFunc()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1 RETURN VOID AS BEGIN NULL;\rEND;CREATE FUNCTION P2 RETURN VOID AS \rBEGIN\rNULL;\rEND;",_conn);
			command.ExecuteReader();
			
			command=new EDBCommand("DROP FUNCTION p1;DROP FUNCTION p2",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteNonQuerryPgFuncInQuotes()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS' BEGIN NULL; END;' language 'plpgsql';",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP FUNCTION p1",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteScalarPgFuncInQuotes()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS' BEGIN NULL; END;' language 'plpgsql';",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP FUNCTION p1",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


		[Test]
		public void SingleExecuteReaderPgFuncInQuotes()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS' \rBEGIN\rNULL;\rEND;' language 'plpgsql';",_conn);
			EDBDataReader dr= command.ExecuteReader();
            dr.Close();
			command=new EDBCommand("DROP FUNCTION p1",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


        // Redundant cases are removed [Test]
		public void MultipleExecuteNonQuerryPgFuncInQuotes()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS' BEGIN NULL;END;' language 'plpgsql';CREATE FUNCTION P2() RETURNS VOID AS' \rBEGIN\rNULL;\rEND;' language 'plpgsql';",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP FUNCTION p1;DROP FUNCTION p2",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


        // Redundant cases are removed [Test]
		public void MultipleExecuteScalarPgFuncInQuotes()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS' \rBEGIN\rNULL;\rEND;' language 'plpgsql';CREATE FUNCTION P2() RETURNS VOID AS' \rBEGIN\rNULL;\rEND;' language 'plpgsql';",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP FUNCTION p1;DROP FUNCTION p2",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteReaderPgFuncInQuotes()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS' \rBEGIN\rNULL;\rEND;' language 'plpgsql';CREATE FUNCTION P2() RETURNS VOID AS' \rBEGIN\rNULL;\rEND;' language 'plpgsql';",_conn);
			EDBDataReader dr = command.ExecuteReader();
            dr.Close();
			command=new EDBCommand("DROP FUNCTION p1;DROP FUNCTION p2",_conn);
			command.ExecuteReader();


			_conn.Close();
		}

        // Redundant cases are removed		[Test]
		public void SingleExecuteNonQuerryPgFuncInDollars()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS $$ \rBEGIN\rNULL;\rEND; $$ language 'plpgsql';",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP FUNCTION p1",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void SingleExecuteScalarPgFuncInDollars()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS $$ \rBEGIN\rNULL;\rEND; $$ language 'plpgsql';",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP FUNCTION p1",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void SingleExecuteReaderPgFuncInDollars()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS $$ \rBEGIN\rNULL;\rEND; $$ language 'plpgsql';",_conn);
			EDBDataReader dr= command.ExecuteReader();
            dr.Close();
			command=new EDBCommand("DROP FUNCTION p1",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteNonQuerryPgFuncInDollars()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS $$ \rBEGIN\rNULL;\rEND;$$ language 'plpgsql';CREATE FUNCTION P2() RETURNS VOID AS $$ \rBEGIN\rNULL;\rEND;$$ language 'plpgsql';",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP FUNCTION p1;DROP FUNCTION p2",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteScalarPgFuncInDollars()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS $$ \rBEGIN\rNULL;\rEND;$$ language 'plpgsql';CREATE FUNCTION P2() RETURNS VOID AS $$ \rBEGIN\rNULL;\rEND;$$ language 'plpgsql';",_conn);
			command.ExecuteScalar();
			
			command=new EDBCommand("DROP FUNCTION p1;DROP FUNCTION p2",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteReaderPgFuncInDollars()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE FUNCTION P1() RETURNS VOID AS $$ \rBEGIN\rNULL;\rEND;$$ language 'plpgsql';CREATE FUNCTION P2() RETURNS VOID AS $$ \rBEGIN\rNULL;\rEND;$$ language 'plpgsql';",_conn);
			EDBDataReader dr = command.ExecuteReader();
            dr.Close();
			command=new EDBCommand("DROP FUNCTION p1;DROP FUNCTION p2",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


        // Redundant cases are removed [Test]
		public void SingleExecuteNonQuerryPackageIs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteNonQuery();

			 command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void SingleExecuteScalarPackageIs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteScalar();

			 command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);

			command.ExecuteScalar();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void SingleExecuteReaderPackageIs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteReader();

			 command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);

			command.ExecuteReader();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteNonQuerryPackageIs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST IS A INT4:=23;FUNCTION TESTFUNC1 RETURN INT4; FUNCTION TESTFUNC2 RETURN INT4;  END PKG_TEST;",_conn);
			command.ExecuteNonQuery();

            command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST IS FUNCTION TESTFUNC1 RETURN INT4 AS BEGIN DBMS_OUTPUT.PUT_LINE('HI MAN'); RETURN 34; END; FUNCTION TESTFUNC2 RETURN INT4 AS BEGIN DBMS_OUTPUT.PUT_LINE('HI MAN'); RETURN A; END; END;", _conn);
            command.ExecuteNonQuery();

            command = new EDBCommand(" DROP PACKAGE PKG_TEST;", _conn);
            command.ExecuteNonQuery();
			
           
			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteScalarPackageIs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST IS A INT4:=23; FUNCTION TESTFUNC1 RETURN INT4; FUNCTION TESTFUNC2 RETURN INT4; END PKG_TEST;",_conn);
			command.ExecuteScalar();

            command = new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST IS FUNCTION TESTFUNC1 RETURN INT4 AS BEGIN DBMS_OUTPUT.PUT_LINE('HI MAN'); RETURN 34; END;FUNCTION TESTFUNC2 RETURN INT4 AS BEGIN DBMS_OUTPUT.PUT_LINE('HI MAN'); RETURN A; END; END;", _conn);
            command.ExecuteScalar();

            command = new EDBCommand("DROP PACKAGE PKG_TEST;", _conn);
            command.ExecuteScalar();

			_conn.Close();
		}


	// Redundant cases are removed 	[Test]
		public void MultipleExecuteReaderPackageIs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST;",_conn);
			command.ExecuteReader();
			
			

			_conn.Close();
		}



        // Redundant cases are removed	[Test]
		public void SingleExecuteNonQuerryPackageIsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteNonQuery();

			command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void SingleExecuteScalarPackageIsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteScalar();

			command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);

			command.ExecuteScalar();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void SingleExecuteReaderPackageIsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteReader();

			command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);

			command.ExecuteReader();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteNonQuerryPackageIsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteNonQuery();
			
			


			_conn.Close();
		}


        //ZK Redundant cases are removed 	[Test]
		public void MultipleExecuteScalarPackageIsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST;",_conn);
			command.ExecuteScalar();
			
			

			_conn.Close();
		}


        //ZK Redundant cases are removed	[Test]
		public void MultipleExecuteReaderPackageIsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	IS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	IS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteReader();
			
			

			_conn.Close();
		}

        // Redundant cases are removed [Test]
		public void SingleExecuteNonQuerryPackageAs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteNonQuery();

			command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void SingleExecuteScalarPackageAs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteScalar();

			command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);

			command.ExecuteScalar();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // ZK Redundant cases are removed	[Test]
		public void SingleExecuteReaderPackageAs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteReader();

			command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);

			command.ExecuteReader();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteNonQuerryPackageAs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST;",_conn);
			command.ExecuteNonQuery();
			
			


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteScalarPackageAs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST;",_conn);
			command.ExecuteScalar();
			
			

			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteReaderPackageAs()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST;",_conn);
			command.ExecuteReader();
			
			

			_conn.Close();
		}



        // Redundant cases are removed	[Test]
		public void SingleExecuteNonQuerryPackageAsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteNonQuery();

			command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);
			command.ExecuteNonQuery();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteNonQuery();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void SingleExecuteScalarPackageAsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteScalar();

			command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);

			command.ExecuteScalar();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteScalar();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void SingleExecuteReaderPackageAsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;",_conn);
			command.ExecuteReader();

			command=new EDBCommand("CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"END;",_conn);

			command.ExecuteReader();
			
			command=new EDBCommand("DROP PACKAGE PKG_TEST",_conn);
			command.ExecuteReader();


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteNonQuerryPackageAsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST;",_conn);
			command.ExecuteNonQuery();
			
			


			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteScalarPackageAsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST;",_conn);
			command.ExecuteScalar();
			
			

			_conn.Close();
		}


        // Redundant cases are removed	[Test]
		public void MultipleExecuteReaderPackageAsOnNewLine()
		{
			
			_conn.Open();
			

			EDBCommand command=new EDBCommand("CREATE OR REPLACE PACKAGE PKG_TEST " +
				"\r	AS" +
				"\r	A INT4:=23;" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4;" +
				"\rEND PKG_TEST;CREATE OR REPLACE PACKAGE BODY PKG_TEST " +
				"\r	AS" +
				"\r	FUNCTION TESTFUNC1 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN 34;" +
				"\r	END;" +
				"\r	FUNCTION TESTFUNC2 RETURN INT4 AS" +
				"\r	BEGIN" +
				"\r		DBMS_OUTPUT.PUT_LINE('HI MAN');" +
				"\r		RETURN A;" +
				"\r	END;" +
				"\rEND;DROP PACKAGE PKG_TEST;",_conn);
			command.ExecuteReader();
			
			

			_conn.Close();
		}



    }
    #endregion
#pragma warning restore CS0618
#pragma warning restore CS8602
}
