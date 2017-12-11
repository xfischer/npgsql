#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using NUnit.Framework;
using System.Data;
using System.Globalization;
using EDBTypes;
using EnterpriseDB.EDBClient;
using System.Net;
using System.Text;

namespace DOTNET
{

    public enum EnumTest : short
    {
        Value1 = 0,
        Value2 = 1
    };

    [TestFixture, Ignore("Hangs on someon these tests")]
    public class CommandTests
    {
        private EDBConnection	_conn = null;

        [SetUp]
        protected void SetUp()
        { 
			string connectionString = System.Configuration.ConfigurationSettings.AppSettings["connectionString"];
			_conn = new EDBConnection(connectionString);


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

        private EDBConnection OpenConnection()
        {
            string connectionString = System.Configuration.ConfigurationSettings.AppSettings["connectionString"];
            var conn = new EDBConnection(connectionString);
            conn.Open();
            return conn;
        }

        [Test]
        public void ParametersGetName()
        {
            EDBCommand command = new EDBCommand();

            // Add parameters.
            command.Parameters.Add(new EDBParameter(":Parameter1", DbType.Boolean));
            command.Parameters.Add(new EDBParameter(":Parameter2", DbType.Int32));
            command.Parameters.Add(new EDBParameter(":Parameter3", DbType.DateTime));
            command.Parameters.Add(new EDBParameter("Parameter4", DbType.DateTime));

            var idbPrmtr = command.Parameters["Parameter1"];
            Assert.IsNotNull(idbPrmtr);
            command.Parameters[0].Value = 1;

            // Get by indexers.

            Assert.AreEqual(":Parameter1", command.Parameters[":Parameter1"].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[":Parameter2"].ParameterName);
            Assert.AreEqual(":Parameter3", command.Parameters[":Parameter3"].ParameterName);
            //Assert.AreEqual(":Parameter4", command.Parameters["Parameter4"].ParameterName); //Should this work?

            Assert.AreEqual(":Parameter1", command.Parameters[0].ParameterName);
            Assert.AreEqual(":Parameter2", command.Parameters[1].ParameterName);
            Assert.AreEqual(":Parameter3", command.Parameters[2].ParameterName);
            Assert.AreEqual("Parameter4", command.Parameters[3].ParameterName);
        }

        [Test]
        public void ParameterNameWithSpace()
        {
            var command = new EDBCommand();

            // Add parameters.
            command.Parameters.Add(new EDBParameter(":Parameter1 ", DbType.Boolean));

            Assert.AreEqual(":Parameter1", command.Parameters[0].ParameterName);
        }

        [Test]
        public void EmptyQuery()
        {
            _conn.Open();

            EDBCommand command = new EDBCommand("select 1", _conn);
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

            Assert.AreEqual(6, result);        

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

            EDBCommand command = new EDBCommand("", _conn);
			command.CommandText = "funcC";

			command.CommandType = CommandType.StoredProcedure;

            EDBDataReader result = command.ExecuteReader();

			Assert.True(result.Read());
            Assert.AreEqual(1, result.FieldCount);
			Assert.AreEqual(5, result.GetInt32(0));
        }


        [Test]
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

            Int64 result = (Int64) command.ExecuteScalar();
			
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
            dr.Close();
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
            dr.Close();
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

        [Test, Description("Makes sure that calling Prepare() twice on a command deallocates the first prepared statement")]
        public void DoublePrepare()
        {
            using (var conn = OpenConnection())
            {
                conn.ExecuteNonQuery("CREATE TEMP TABLE data (name TEXT, int INTEGER)");
                using (var cmd = new EDBCommand("INSERT INTO data (name) VALUES (:p0)", conn))
                {
                    cmd.Parameters.Add(new EDBParameter("p0", EDBDbType.Text));
                    cmd.Parameters["p0"].Value = "test";
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "INSERT INTO data (int) VALUES (:p0)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new EDBParameter("p0", EDBDbType.Integer));
                    cmd.Parameters["p0"].Value = 8;
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
                Assert.That(conn.ExecuteScalar("SELECT COUNT(*) FROM pg_prepared_statements"), Is.EqualTo(0), "Prepared statements are being leaked");
            }
        }

        [Test, IssueLink("https://github.com/npgsql/npgsql/issues/1207")]
        public void DoublePrepare2()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT 1", conn))
            {
                cmd.Prepare();
                cmd.Prepare();
            }
        }

        [Test]
        public void StringEscapeSyntax()
        {
            using (var conn = OpenConnection())
            {

                //the next command will fail on earlier postgres versions, but that is not a bug in itself.
                try
                {
                    conn.ExecuteNonQuery("set standard_conforming_strings=off;set escape_string_warning=off");
                }
                catch
                {
                }
                string cmdTxt = "select :par";
                var command = new EDBCommand(cmdTxt, conn);
                var arrCommand = new EDBCommand(cmdTxt, conn);
                string testStrPar = "This string has a single quote: ', a double quote: \", and a backslash: \\";
                string[,] testArrPar = new string[,] {{testStrPar, ""}, {testStrPar, testStrPar}};
                command.Parameters.AddWithValue(":par", testStrPar);
                using (var rdr = command.ExecuteReader())
                {
                    rdr.Read();
                    Assert.AreEqual(rdr.GetString(0), testStrPar);
                }
                arrCommand.Parameters.AddWithValue(":par", testArrPar);
                using (var rdr = arrCommand.ExecuteReader())
                {
                    rdr.Read();
                    Assert.AreEqual(((string[,]) rdr.GetValue(0))[0, 0], testStrPar);
                }

                try //the next command will fail on earlier postgres versions, but that is not a bug in itself.
                {
                    conn.ExecuteNonQuery("set standard_conforming_strings=on;set escape_string_warning=on");
                }
                catch
                {
                }
                using (var rdr = command.ExecuteReader())
                {
                    rdr.Read();
                    Assert.AreEqual(rdr.GetString(0), testStrPar);
                }
                using (var rdr = arrCommand.ExecuteReader())
                {
                    rdr.Read();
                    Assert.AreEqual(((string[,]) rdr.GetValue(0))[0, 0], testStrPar);
                }
            }
        }

        [Test]
        public void ParameterAndOperatorUnclear()
        {
            using (var conn = OpenConnection())
            {
                //Without parenthesis the meaning of [, . and potentially other characters is
                //a syntax error. See comment in EDBCommand.GetClearCommandText() on "usually-redundant parenthesis".
                using (var command = new EDBCommand("select :arr[2]", conn))
                {
                    command.Parameters.AddWithValue(":arr", new int[] {5, 4, 3, 2, 1});
                    using (var rdr = command.ExecuteReader())
                    {
                        rdr.Read();
                        Assert.AreEqual(rdr.GetInt32(0), 4);
                    }
                }
            }
        }

        [Test]
        public void StatementMappedOutputParameters()
        {
            using (var conn = OpenConnection())
            {
                var command = new EDBCommand("select 3, 4 as param1, 5 as param2, 6;", conn);

                var p = new EDBParameter("param2", EDBDbType.Integer);
                p.Direction = ParameterDirection.Output;
                p.Value = -1;
                command.Parameters.Add(p);

                p = new EDBParameter("param1", EDBDbType.Integer);
                p.Direction = ParameterDirection.Output;
                p.Value = -1;
                command.Parameters.Add(p);

                p = new EDBParameter("p", EDBDbType.Integer);
                p.Direction = ParameterDirection.Output;
                p.Value = -1;
                command.Parameters.Add(p);

                command.ExecuteNonQuery();

                Assert.AreEqual(4, command.Parameters["param1"].Value);
                Assert.AreEqual(5, command.Parameters["param2"].Value);
                //Assert.AreEqual(-1, command.Parameters["p"].Value); //Which is better, not filling this or filling this with an unmapped value?
            }
        }

        [Test]
        public void CaseSensitiveParameterNames()
        {
            using (var conn = OpenConnection())
            {
                using (var command = new EDBCommand("select :p1", conn))
                {
                    command.Parameters.Add(new EDBParameter("P1", EDBDbType.Integer)).Value = 5;
                    var result = command.ExecuteScalar();
                    Assert.AreEqual(5, result);
                }
            }
        }

        [Test]
        public void TestBug1006158OutputParameters()
        {
            using (var conn = OpenConnection())
            {
                const string createFunction =
                    @"CREATE OR REPLACE FUNCTION pg_temp.more_params(OUT a integer, OUT b boolean) AS
            $BODY$DECLARE
                BEGIN
                    a := 3;
                    b := true;
                END;$BODY$
              LANGUAGE 'plpgsql' VOLATILE;";

                var command = new EDBCommand(createFunction, conn);
                command.ExecuteNonQuery();

                command = new EDBCommand("pg_temp.more_params", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("a", DbType.Int32));
                command.Parameters[0].Direction = ParameterDirection.Output;
                command.Parameters.Add(new EDBParameter("b", DbType.Boolean));
                command.Parameters[1].Direction = ParameterDirection.Output;

                var result = command.ExecuteScalar();

                Assert.AreEqual(3, command.Parameters[0].Value);
                Assert.AreEqual(true, command.Parameters[1].Value);
            }
        }

        [Test]
        public void TestErrorInPreparedStatementCausesReleaseConnectionToThrowException()
        {
            using (var conn = OpenConnection())
            {
                // This is caused by having an error with the prepared statement and later, Npgsql is trying to release the plan as it was successful created.
                var cmd = new EDBCommand("sele", conn);
                Assert.That(() => cmd.Prepare(), Throws.Exception.TypeOf<PostgresException>());
            }
        }

#if NET451
        [Test]
        public void Bug1010788UpdateRowSource()
        {
            using (var conn = OpenConnection())
            {
                conn.ExecuteNonQuery("CREATE TEMP TABLE data (id SERIAL PRIMARY KEY, name TEXT)");
                var command = new EDBCommand("SELECT * FROM data", conn);
                Assert.AreEqual(UpdateRowSource.Both, command.UpdatedRowSource);

                var cmdBuilder = new EDBCommandBuilder();
                var da = new EDBDataAdapter(command);
                cmdBuilder.DataAdapter = da;
                Assert.IsNotNull(da.SelectCommand);
                Assert.IsNotNull(cmdBuilder.DataAdapter);

                EDBCommand updateCommand = cmdBuilder.GetUpdateCommand();
                Assert.AreEqual(UpdateRowSource.None, updateCommand.UpdatedRowSource);
            }
        }
#endif

        [Test]
        public void TableDirect()
        {
            using (var conn = OpenConnection())
            {
                conn.ExecuteNonQuery("CREATE TEMP TABLE data (name TEXT)");
                conn.ExecuteNonQuery(@"INSERT INTO data (name) VALUES ('foo')");
                using (var cmd = new EDBCommand("data", conn) { CommandType = CommandType.TableDirect })
                using (var rdr = cmd.ExecuteReader())
                {
                    Assert.That(rdr.Read(), Is.True);
                    Assert.That(rdr["name"], Is.EqualTo("foo"));
                }
            }
        }

        [Test]
        public void InputAndOutputParameters()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "Select :a + 2 as b, :c - 1 as c";
                var b = new EDBParameter { ParameterName = "b", Direction = ParameterDirection.Output };
                cmd.Parameters.Add(b);
                cmd.Parameters.Add(new EDBParameter("a", 3));
                var c = new EDBParameter { ParameterName = "c", Direction = ParameterDirection.InputOutput, Value = 4 };
                cmd.Parameters.Add(c);
                using (cmd.ExecuteReader())
                {
                    Assert.AreEqual(5, b.Value);
                    Assert.AreEqual(3, c.Value);
                }
            }
        }

        [Test]
        public void SendUnknown([Values(PrepareOrNot.NotPrepared, PrepareOrNot.Prepared)] PrepareOrNot prepare)
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p::TIMESTAMP", conn))
            {
                cmd.CommandText = "SELECT @p::TIMESTAMP";
                cmd.Parameters.Add(new EDBParameter("p", EDBDbType.Unknown) { Value = "2008-1-1" });
                if (prepare == PrepareOrNot.Prepared)
                    cmd.Prepare();
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    Assert.That(reader.GetValue(0), Is.EqualTo(new DateTime(2008, 1, 1)));
                }
            }
        }

        [Test, Description("Checks that prepares requires all params to have explicitly set types (EDBDbType or DbType)")]
        public void PrepareRequiresParamTypesSet()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT @p", conn))
            {
                var p = new EDBParameter("p", 8);
                cmd.Parameters.Add(p);
                Assert.That(() => cmd.Prepare(), Throws.InvalidOperationException);
            }
        }

        [Test, IssueLink("https://github.com/npgsql/npgsql/issues/503")]
        public void InvalidUTF8()
        {
            const string badString = "SELECT 'abc\uD801\uD802d'";
            using (var conn = OpenConnection())
            {
                Assert.That(() => conn.ExecuteScalar(badString), Throws.Exception.TypeOf<EncoderFallbackException>());
            }
        }

        [Test]
        [IssueLink("https://github.com/npgsql/npgsql/issues/393")]
        [IssueLink("https://github.com/npgsql/npgsql/issues/299")]
        public void DisposePreparedAfterCommandClose()
        {
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "select 1";
                cmd.Prepare();
                conn.Close();
            }
        }

        [Test, IssueLink("https://github.com/npgsql/npgsql/issues/395")]
        public void PreparedAcrossCloseOpen()
        {
            using (var conn1 = OpenConnection())
            using (var cmd = new EDBCommand("SELECT 1", conn1))
            {
                cmd.Prepare();
                Assert.That(cmd.IsPrepared, Is.True);
                conn1.Close();
                conn1.Open();
                Assert.That(cmd.IsPrepared, Is.False);
                Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1)); // Execute unprepared
                cmd.Prepare();
                Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
            }
        }

        [Test, IssueLink("https://github.com/npgsql/npgsql/issues/395")]
        public void UseAcrossConnectionChange([Values(PrepareOrNot.Prepared, PrepareOrNot.NotPrepared)] PrepareOrNot prepare)
        {
            using (var conn1 = OpenConnection())
            using (var conn2 = OpenConnection())
            using (var cmd = new EDBCommand("SELECT 1", conn1))
            {
                if (prepare == PrepareOrNot.Prepared)
                    cmd.Prepare();
                cmd.Connection = conn2;
                Assert.That(cmd.IsPrepared, Is.False);
                if (prepare == PrepareOrNot.Prepared)
                    cmd.Prepare();
                Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
            }
        }

        [Test, Description("CreateCommand before connection open")]
        [IssueLink("https://github.com/npgsql/npgsql/issues/565")]
        public void CreateCommandBeforeConnectionOpen()
        {
            using (var conn = new EDBConnection(TestUtil.defaultConnectionString)) {
                var cmd = new EDBCommand("SELECT 1", conn);
                conn.Open();
                Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
            }
        }

        [Test]
        public void BadConnection()
        {
            var cmd = new EDBCommand("SELECT 1");
            Assert.That(() => cmd.ExecuteScalar(), Throws.Exception.TypeOf<InvalidOperationException>());

            using (var conn = new EDBConnection(TestUtil.defaultConnectionString))
            {
                cmd = new EDBCommand("SELECT 1", conn);
                Assert.That(() => cmd.ExecuteScalar(), Throws.Exception.TypeOf<InvalidOperationException>());
            }
        }

        [Test, Description("This scenario used to be supported in 3.0, but isn't supported starting 3.1")]
        [IssueLink("https://github.com/npgsql/npgsql/issues/416")]
        public void PreparedDisposeWithOpenReader()
        {
            using (var conn = OpenConnection())
            {
                var cmd1 = new EDBCommand("SELECT 1", conn);
                var cmd2 = new EDBCommand("SELECT 1", conn);
                cmd1.Prepare();
                cmd2.Prepare();
                var reader = cmd2.ExecuteReader();
                reader.Read();
                Assert.That(() => cmd1.Dispose(), Throws.Exception.TypeOf<InvalidOperationException>());
                reader.Close();
                cmd1.Dispose();
                cmd2.Dispose();
                Assert.That(conn.ExecuteScalar("SELECT COUNT(*) FROM pg_prepared_statements"), Is.EqualTo(0));
            }
        }

        [Test, IssueLink("https://github.com/npgsql/npgsql/issues/831")]
        [Timeout(10000)]
        public void ManyParameters()
        {
            using (var conn = OpenConnection())
            using (var cmd = new EDBCommand("SELECT 1", conn))
            {
                for (var i = 0; i < conn.Settings.WriteBufferSize; i++)
                    cmd.Parameters.Add(new EDBParameter("p" + i, 8));
                cmd.ExecuteNonQuery();
            }
        }

        [Test, Description("Bypasses PostgreSQL's int16 limitation on the number of parameters")]
        [IssueLink("https://github.com/npgsql/npgsql/issues/831")]
        [IssueLink("https://github.com/npgsql/npgsql/issues/858")]
        [IssueLink("https://github.com/npgsql/npgsql/issues/1199")]
        public void TooManyParameters()
        {
            using (var conn = OpenConnection())
            {
                using (var cmd = new EDBCommand { Connection = conn })
                {
                    var sb = new StringBuilder("SELECT ");
                    for (var i = 0; i < 65536; i++)
                    {
                        var paramName = "p" + i;
                        cmd.Parameters.Add(new EDBParameter(paramName, 8));
                        if (i > 0)
                            sb.Append(", ");
                        sb.Append('@');
                        sb.Append(paramName);
                    }
                    cmd.CommandText = sb.ToString();
                    Assert.That(() => cmd.ExecuteNonQuery(), Throws.Exception
                        .InstanceOf<Exception>()
                        .With.Message.EqualTo("A statement cannot have more than 65535 parameters")
                        );
                }

                // An individual statement cannot have more than 65535 parameters, but a command can
                // (across multiple statements).
                // Create a command with 1000 statements which have 70 params each
                using (var cmd = new EDBCommand { Connection = conn })
                {
                    var paramIndex = 0;
                    var sb = new StringBuilder();
                    for (var statementIndex = 0; statementIndex < 1000; statementIndex++)
                    {
                        if (statementIndex > 0)
                            sb.Append("; ");
                        sb.Append("SELECT ");
                        var startIndex = paramIndex;
                        var endIndex = paramIndex + 70;
                        for (; paramIndex < endIndex; paramIndex++)
                        {
                            var paramName = "p" + paramIndex;
                            cmd.Parameters.Add(new EDBParameter(paramName, 8));
                            if (paramIndex > startIndex)
                                sb.Append(", ");
                            sb.Append('@');
                            sb.Append(paramName);
                        }
                    }

                    cmd.CommandText = sb.ToString();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [Test, IssueLink("https://github.com/npgsql/npgsql/issues/1037")]
        public void Statements()
        {
            // See also ReaderTests.Statements()
            using (var conn = OpenConnection())
            {
                conn.ExecuteNonQuery("CREATE TEMP TABLE data (name TEXT) WITH OIDS");
                using (var cmd = new EDBCommand(
                    "INSERT INTO data (name) VALUES ('a');" +
                    "UPDATE data SET name='b' WHERE name='doesnt_exist'",
                    conn)
                )
                {
                    cmd.ExecuteNonQuery();

                    Assert.That(cmd.Statements, Has.Count.EqualTo(2));
                    Assert.That(cmd.Statements, Has.Count.EqualTo(2));
                    Assert.That(cmd.Statements[0].SQL, Is.EqualTo("INSERT INTO data (name) VALUES ('a')"));
                    Assert.That(cmd.Statements[0].StatementType, Is.EqualTo(EnterpriseDB.EDBClient.StatementType.Insert));
                    Assert.That(cmd.Statements[0].Rows, Is.EqualTo(1));
                    Assert.That(cmd.Statements[0].OID, Is.Not.EqualTo(0));
                    Assert.That(cmd.Statements[1].SQL,
                        Is.EqualTo("UPDATE data SET name='b' WHERE name='doesnt_exist'"));
                    Assert.That(cmd.Statements[1].StatementType, Is.EqualTo(EnterpriseDB.EDBClient.StatementType.Update));
                    Assert.That(cmd.Statements[1].Rows, Is.EqualTo(0));
                    Assert.That(cmd.Statements[1].OID, Is.EqualTo(0));
                }
            }
        }


        /*[Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ListenNotifySupport()
        {

            _conn.Open();

            EDBCommand command = new EDBCommand("listen notifytest;", _conn);
            command.ExecuteNonQuery();

            _conn.Notification += new NotificationEventHandler(NotificationSupportHelper);


            command = new EDBCommand("notify notifytest;", _conn);
            command.ExecuteNonQuery();



        }*/

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

            //   DateTime d = command.ExecuteScalar();
            TimeSpan tm = (TimeSpan)command.ExecuteScalar();

            Console.WriteLine(tm.ToString());


            Assert.AreEqual("10:03:45.3450000", tm.ToString());

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


            EDBCommand command = new EDBCommand("insert into tableb(field_numeric) values (:a)", _conn);
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


            EDBCommand command = new EDBCommand("insert into tabled(field_float4) values (:a)", _conn);
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


            EDBCommand command = new EDBCommand("insert into tabled(field_float4) values (:a)", _conn);
            command.Parameters.Add(new EDBParameter(":a", EDBDbType.Double));

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

            Assert.AreEqual(6, result);

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

            Assert.AreEqual(6, result);

        }

        [Test]
        public void AnsiStringSupport()
        {
            try
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
            catch (EDBException ex)
            {
                Console.Error.WriteLine(ex.StackTrace);
            }

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


            Assert.AreEqual(7, result);


        }

        [Test]
        public void ConnectionStringWithInvalidParameters()
        {
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;User Id=EDB_tests;Password=j");

            EDBCommand command = new EDBCommand("select * from tablea", conn);

            Assert.Throws<System.Net.Sockets.SocketException>(() => command.Connection.Open());

        }

        [Test]
        public void InvalidConnectionString()
        {
            EDBConnection conn = new EDBConnection("Server=127.0.0.1;User Id=EDB_tests;Password=j");

            EDBCommand command = new EDBCommand("select * from tablea", conn);
            Assert.Throws<System.Net.Sockets.SocketException>(() => command.Connection.Open());

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
            /*ZK: changed exepcted after Npgsql 3.0.5 merge*/
			Assert.AreEqual("10.1.2.3",Reader.GetValue(0).ToString());
			//Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Read();
			Assert.AreEqual("10.0.0.0/8",Reader.GetValue(0).ToString());
			Reader.Read();
            /*ZK: changed exepcted after Npgsql 3.0.5 merge*/
		    Assert.AreEqual("10.0.0.0",Reader.GetValue(0).ToString());

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
				
			command.CommandText="DROP Table NWK_BROADCAST";
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
		
					command.CommandText="drop table NETADD_MASKLEN;";
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
		
			command.CommandText="drop table NET_TEXT;";
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
		
			command.CommandText="drop table tbl_setmasklen;";
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
			Assert.AreEqual("192.168.1.0/24",Reader.GetValue(0).ToString());
			Reader.Read();
            /*ZK: changed exepcted after Npgsql 3.0.5 merge*/
		    Assert.AreEqual("10.1.2.3",Reader.GetValue(0).ToString());
			Console.WriteLine(Reader.GetValue(0).ToString());
			Reader.Read();
			Console.WriteLine(Reader.GetValue(0).ToString());
			Assert.AreEqual("10.0.0.0/8",Reader.GetValue(0).ToString());
			Reader.Read();
			Console.WriteLine(Reader.GetValue(0).ToString());
            /*ZK: changed exepcted after Npgsql 3.0.5 merge*/
		
            Assert.AreEqual("10.0.0.0",Reader.GetValue(0).ToString());

			Reader.Close();
		
			command.CommandText="drop table tbl_network;";
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
				
			command.CommandText="drop table tbl_net;";
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
				
			command.CommandText="drop table tbl_mac;";
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
				
			command.CommandText="drop table tbl_macadd;";
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
				
			command.CommandText="drop table tbl_inet_arr";
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
				
			command.CommandText="drop table tbl_cidr_arr";
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
				
			command.CommandText="drop table inhf;";
			command.ExecuteNonQuery();
			command.CommandText="drop table inhx;";
			command.ExecuteNonQuery();
			_conn.Close();
		}

		[Test]
		public void TestDoubleInheritance()
		{
			
			_conn.Open();
			//EDBTransaction tran=_conn.BeginTransaction();

			EDBCommand command = new EDBCommand("create table p1(ff1 int);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("create table p2(f1 text);", _conn);
			command.ExecuteNonQuery();
			//tran.Commit();
			command = new EDBCommand("create table c1(f3 int) inherits(p1,p2);", _conn);
			command.ExecuteNonQuery();
			command.CommandText=" insert into p2 values ('hello');";
			command.ExecuteNonQuery();
			command.CommandText=" insert into c1(ff1,f1,f3) values(56789, 'hi', 42);";
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
				
			command.CommandText="drop table c1;";
			command.ExecuteNonQuery();
			command.CommandText="drop table p2;";
			command.ExecuteNonQuery();
			command.CommandText="drop table p1;";
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
			command = new EDBCommand("insert into foo values(1,1);insert into foo values(3,3);", _conn);
			command.ExecuteNonQuery();
			//tran.Commit();
			command = new EDBCommand("create temp table bar(f1 int, f2 int);", _conn);
			command.ExecuteNonQuery();
			command.CommandText=" insert into bar values(1,1);";
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

			EDBCommand command = new EDBCommand("create table base (i varchar);", _conn);
			command.ExecuteNonQuery();
			command = new EDBCommand("create table derived() inherits (base);", _conn);
			command.ExecuteNonQuery();
			//tran.Commit();
			command = new EDBCommand("insert into derived (i) values ('abc');", _conn);
			command.ExecuteNonQuery();
            /*ZK: refer to http://www.npgsql.org/doc/faq.html for details*/
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
				
			command.CommandText="drop table derived;";
			command.ExecuteNonQuery();
			command.CommandText="drop table base;";
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
				
			
			command.CommandText="drop table on_hand;";
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
				
			
			command.CommandText="drop table on_hand;";
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
				
			
			command.CommandText="drop table on_hand;";
			command.ExecuteNonQuery();
			command.CommandText="drop table on_hand2;";
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
				
			
			command.CommandText="drop table on_hand;";
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
				
			
			command.CommandText="drop table on_hand;";
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

		[Test]
		public void QuoteHandling1()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="select empno from emp where ename= :Name";
			com.Parameters.Add(new EDBParameter("Name",EDBTypes.EDBDbType.Varchar));
			com.Parameters[0].Value="SMITH";
			EDBDataReader Reader=com.ExecuteReader();

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetDecimal(0));
			}
			Reader.Close();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling2()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			com.CommandText="select id from Quote where b= :No";
			com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Varchar));
			com.Parameters[0].Value="t";
			EDBDataReader Reader=com.ExecuteReader();

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling3()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="select empno from emp where ename= :Name";
			com.Parameters.Add(new EDBParameter("Name",EDBTypes.EDBDbType.Varchar,4));
			com.Parameters[0].Value="SMITH";
			EDBDataReader Reader=com.ExecuteReader();

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling4()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			com.CommandText="select id from Quote where b= :No";
			com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Varchar,1));
			com.Parameters[0].Value="t";
			EDBDataReader Reader=com.ExecuteReader();

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling5()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="select empno from emp where ename= :Name";
			com.Parameters.Add(new EDBParameter("Name",EDBTypes.EDBDbType.Char,5));
			com.Parameters[0].Value="SMITH";
			EDBDataReader Reader=com.ExecuteReader();

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetDecimal(0));
				
			}
			Reader.Close();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling6()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			EDBDataReader Reader=null;
			com.CommandText="select id from Quote where b= :No";
			
			
			com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Char,1));
			com.Parameters[0].Value="t";
			Reader=com.ExecuteReader();
			

			
			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}


		[Test]
		public void QuoteHandling7()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			EDBDataReader Reader=null;
			com.CommandText="select id from Quote where b= :No";
			try
			{
				com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Char));
				com.Parameters[0].Value="t";
				Reader=com.ExecuteReader();

				
			}

			catch(EDBException exp)
			{
				//Console.WriteLine(exp.Message);
				com.CommandText="drop table Quote";
				com.ExecuteNonQuery();
				_conn.Close();
				return;
			}

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling8()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			EDBDataReader Reader=null;
			com.CommandText="select id from Quote where b= :No";
			try
			{
				com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Varchar));
				com.Parameters[0].Value="t";
				Reader=com.ExecuteReader();
				
			}

			catch(EDBException exp)
			{
				//Console.WriteLine(exp.Message);
				com.CommandText="drop table Quote";
				com.ExecuteNonQuery();
				_conn.Close();
				return;
			}

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling9()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			EDBDataReader Reader=null;
			com.CommandText="select id from Quote where b= :No";
			try
			{
				com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Char,-1));
				com.Parameters[0].Value="t";
				Reader=com.ExecuteReader();
				

				//Assert.Fail("should fail for negative value of size");
			}

			catch(EDBException exp)
			{
				//Console.WriteLine(exp.Message);
				com.CommandText="drop table Quote";
				com.ExecuteNonQuery();
				_conn.Close();
				return;
			}

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling10()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="select empno from emp where ename= :Name";
			com.Parameters.Add(new EDBParameter("Name",EDBTypes.EDBDbType.Char,-4));
			com.Parameters[0].Value="SMITH";
			EDBDataReader Reader=null;
			try
			{
				Reader=com.ExecuteReader();
				
			}
				
			catch(EDBException exp)
			{
				_conn.Close();
				return;
			}
			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			_conn.Close();
		}


		[Test]
		public void QuoteHandling11()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="select empno from emp where ename= :Name";
			com.Parameters.Add(new EDBParameter("Name",EDBTypes.EDBDbType.Varchar,-4));
			com.Parameters[0].Value="SMITH";
			EDBDataReader Reader=com.ExecuteReader();

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetDecimal(0));
			}
			Reader.Close();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling12()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			com.CommandText="select id from Quote where b= :No";
			com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Varchar,-1));
			com.Parameters[0].Value="t";
			EDBDataReader Reader=com.ExecuteReader();

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}

		
		[Test]
		public void QuoteHandling13()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="select empno from emp where ename= :Name";
			com.Parameters.Add(new EDBParameter("Name",EDBTypes.EDBDbType.Varchar,-4));
			com.Parameters[0].Value="SMITH";
			EDBDataReader Reader=null;
			try
			{
				Reader=com.ExecuteReader();
				
			}

			catch(Exception exp)
			{
				return;
			}


			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetDecimal(0));
			}
			Reader.Close();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling14()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			com.CommandText="select id from Quote where b= :No";
			com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Varchar,-1));
			com.Parameters[0].Value="t";
			EDBDataReader Reader=null;
			try
			{
				Reader=com.ExecuteReader();
			
			}

			catch(Exception exp)
			{
				com.CommandText="drop table Quote";
				com.ExecuteNonQuery();

				return;
			}

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}


		[Test]
		public void QuoteHandling15()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			EDBDataReader Reader=null;
			com.CommandText="select id from Quote where b= :No";
			try
			{
				com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Char,0));
				com.Parameters[0].Value="t";
				Reader=com.ExecuteReader();
				

				//Assert.Fail("should fail for negative value of size");
			}

			catch(EDBException exp)
			{
				//Console.WriteLine(exp.Message);
				com.CommandText="drop table Quote";
				com.ExecuteNonQuery();
				_conn.Close();
				return;
			}

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling16()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="select empno from emp where ename= :Name";
			com.Parameters.Add(new EDBParameter("Name",EDBTypes.EDBDbType.Char,4));
			com.Parameters[0].Value="SMITH";
			EDBDataReader Reader=null;
			try
			{
				Reader=com.ExecuteReader();
				
			}
				
			catch(EDBException exp)
			{
				_conn.Close();
				return;
			}
			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			_conn.Close();
		}


		[Test]
		public void QuoteHandling17()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="select empno from emp where ename= :Name";
			com.Parameters.Add(new EDBParameter("Name",EDBTypes.EDBDbType.Varchar,4));
			com.Parameters[0].Value="SMITH";
			EDBDataReader Reader=com.ExecuteReader();

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetDecimal(0));
			}
			Reader.Close();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling18()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			com.CommandText="select id from Quote where b= :No";
			com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Varchar,0));
			com.Parameters[0].Value="t";
			EDBDataReader Reader=com.ExecuteReader();

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}

		
		[Test]
		public void QuoteHandling19()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="select empno from emp where ename= :Name";
			com.Parameters.Add(new EDBParameter("Name",EDBTypes.EDBDbType.Varchar,4));
			com.Parameters[0].Value="SMITH";
			EDBDataReader Reader=null;
			try
			{
				Reader=com.ExecuteReader();
				
			}

			catch(Exception exp)
			{
				return;
			}


			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			_conn.Close();
		}

		[Test]
		public void QuoteHandling20()
		{
			_conn.Open();

			EDBCommand com=new EDBCommand("",_conn);
			com.CommandText="create table Quote(id int4, b char)";
			com.ExecuteNonQuery();
			com.CommandText="insert into Quote values(1, 't')";
			com.ExecuteNonQuery();
			com.CommandText="select id from Quote where b= :No";
			com.Parameters.Add(new EDBParameter("No",EDBTypes.EDBDbType.Varchar,0));
			com.Parameters[0].Value="t";
			EDBDataReader Reader=null;
			try
			{
				Reader=com.ExecuteReader();
			
			}

			catch(Exception exp)
			{
				com.CommandText="drop table Quote";
				com.ExecuteNonQuery();

				return;
			}

			while(Reader.Read())
			{
				Console.WriteLine( Reader.GetInt32(0));
			}
			Reader.Close();
			com.CommandText="drop table Quote";
			com.ExecuteNonQuery();
			_conn.Close();
		}



    }
}
