using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections;
using NUnit;

//Haroon
namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

#pragma warning disable CS8602
/// <summary>
/// Summary description for MiscTest.
/// </summary>

[TestFixture]
[NonParallelizable]
public class EDBMiscTest : EPASTestBase
{
		EDBConnection? con = null;

    [SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();
        TestUtil.createTempTable(con,"TESTTAB","a VARCHAR, b INT4");
        var Command = new EDBCommand("", con)
        {
            CommandText = "INSERT INTO TESTTAB VALUES('V1',1)"
        };
        Command.ExecuteNonQuery();
			Command.CommandText="INSERT INTO TESTTAB VALUES('V2',2)";
			Command.ExecuteNonQuery();
			TestUtil.createTempTable(con,"test_Index","major int4, minor INT4, name VARCHAR");

		}

		[TearDown] 
		public void Dispose()
		{
			
        //EDBCommand Command=new EDBCommand("",con);
        //Command.CommandText="DROP Table TESTTAB";
        //Command.CommandType=CommandType.Text;
        //Command.ExecuteNonQuery();

        //Command.CommandText="DROP Table test_Index";
        //Command.CommandType=CommandType.Text;
        //Command.ExecuteNonQuery();
			
			TestUtil.closeDB(con);
		}
			
		[Test]
		public void TestDatabaseSelectNullBug()
		{
			try
			{
				var Con=OpenConnection();

            var Command = new EDBCommand("", Con)
            {
                CommandType = CommandType.Text
            };
            var Select="select datname from pg_database";
				Command.CommandText=Select;
				var Reader=Command.ExecuteReader();
				
				Assert.IsNotNull(Reader);
	
				while(Reader.Read())
				{
					Console.WriteLine(Reader.GetValue(0).ToString());

				}
				
				Reader.Close();
				TestUtil.closeDB(Con);
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}
		
		/// <summary>
		/// Test Aggregate functions in various scenarios
		/// </summary>
		[Test]
		public void TestAggregateInvalidMax()
		{
			try
			{

            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB where b=max(b)",

                CommandType = CommandType.Text
            };

            Command.ExecuteNonQuery();
				
				Assert.Fail("Expected an exception on misuse of aggregate function");
				
			}
			catch(Exception )
			{

			}
		
		}
		[Test]
		public void TestAggregateHavingMax()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having max(b)=b order by a",

                CommandType = CommandType.Text
            };
            var Reader=Command.ExecuteReader();
			
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0).ToString());
				Console.WriteLine(Reader.GetValue(0).ToString());
				Assert.IsTrue(Reader.Read());
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void TestAggregateHavingSelectMax()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having b=(select max(b) from testtab);"
            };

            var Reader = Command.ExecuteReader();
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V2",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}
		
		[Test]
		public void TestAggregateSelectMax()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB where b=(select max(b) from TESTTAB)"
            };

            var Reader = Command.ExecuteReader();
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V2",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				Reader.Close();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
			
		}

		[Test]
		public void TestAggregateInvalidMin()
		{
			var Command = new EDBCommand("",con);
			try
			{
				Command.CommandText="select a from TESTTAB where b=min(b)";
				Command.ExecuteNonQuery();
				Assert.Fail("Expected an exception on misuse of aggregate function");
			}
	
			catch(Exception )
			{
				
			}
		
		}
		[Test]
		public void TestAggregateHavingMin()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having min(b)=b order by a"
            };
            var Reader = Command.ExecuteReader();

				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsTrue(Reader.Read());
				Reader.Close();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}
		
		[Test]
		public void TestAggregateHavingSelectMin()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having b=(select min(b) from testtab);"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				Reader.Close();

			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		}
		
		//17-11-2006
		
		[Test]
		public void TestAggregateSelectMin()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB where b=(select min(b) from TESTTAB)"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		}

		[Test]
		public void TestAggregateInvalidAvg()
		{

			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB where b=avg(b)"
            };

            Command.ExecuteNonQuery();
				Assert.Fail("Expected an exception on misuse of aggregate function");
			}
			catch(Exception )
			{
			}
			
		}

		[Ignore("MERGE_HANG")]
		[Test]
		public void TestAggregateHavingAvg()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having avg(b)=b order by a"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsTrue(Reader.Read());
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void TestAggregateHavingSelectAvg()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having b=(select avg(b) from testtab);"
            };

            var Reader = Command.ExecuteReader();
				Assert.IsFalse(Reader.Read());
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]

		public void TestAggregateSelectAvg()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB where b<(select avg(b) from TESTTAB)"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void TestAggregateInvalidBIT_AND()
		{
			var Command = new EDBCommand("",con);

			try
			{
				Command.CommandText="select a from TESTTAB where b=BIT_AND(b)";
				Command.ExecuteNonQuery();
				Assert.Fail("Expected an exception on misuse of aggregate function");
			}
			catch(Exception )
			{
			}
		
		}

		[Test]

		public void TestAggregateHavingBIT_AND()
		{

			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having BIT_AND(b)=b order by a"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsTrue(Reader.Read());
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		}

		[Test]
		public void TestAggregateHavingSelectBIT_AND()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having b=(select BIT_AND(b) from testtab);"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsFalse(Reader.Read());
				Reader.Close();
			}
			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
			
		}

		//***********************
		[Test]
		public void TestAggregateSelectBIT_AND()
		{

			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB where b>(select BIT_AND(b) from TESTTAB)"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsTrue(Reader.Read());
				Assert.IsFalse(Reader.Read());
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		}

		[Test]
		public void TestAggregateInvalidBIT_OR()
		{
			
			var Command = new EDBCommand("",con);

			try
			{
				Command.CommandText="select a from TESTTAB where b=BIT_OR(b)";
				Command.ExecuteNonQuery();
				Assert.Fail("Expected an exception on misuse of aggregate function");
			}
			catch(Exception )
			{
			}
		
		}

		[Test]
		public void TestAggregateHavingBIT_OR()
		{
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having BIT_OR(b)=b order by a"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsTrue(Reader.Read());
				
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void TestAggregateHavingSelectBIT_OR()
		{
			
			var Command = new EDBCommand("",con);

			try
			{
				Command.CommandText="select a from TESTTAB group by a,b having b=(select BIT_OR(b) from testtab);";
				var Reader = Command.ExecuteReader();
				Assert.IsFalse(Reader.Read());
				Reader.Close();
			}
			catch(Exception exp)
			{
				throw new Exception(exp.ToString());
			}

		}

		[Test]
		public void TestAggregateSelectBIT_OR()
		{

			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB where b<(select BIT_OR(b) from TESTTAB)"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsTrue(Reader.Read());
				Assert.IsFalse(Reader.Read());
				
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		}

		[Test]

		public void TestAggregateInvalidCount()
		{
			
			var Command = new EDBCommand("",con);

			try
			{
				Command.CommandText="select a from TESTTAB where b=count(*)";
				Command.ExecuteNonQuery();
				Assert.Fail("Expected an exception on misuse of aggregate function");
			}
			catch(Exception )
			{
			}

		}

		[Test]
		public void TestAggregateHavingCount()
		{

			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having b=count(*)"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				
				Reader.Close();
				Console.WriteLine(con.Database.ToString()+"afaf");
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		}

		[Test]
		public void TestAggregateHavingSelectCount()
		{
				
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having b=(select count(*) from testtab);"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V2",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void TestAggregateSelectCount()
		{
			
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB where b<(select count(*) from TESTTAB)"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		}

		[Test]
		public void TestAggregateSelectCountNonNull()
		{

			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "INSERT INTO TESTTAB(b) VALUES(3)"
            };
            Command.ExecuteNonQuery();
				Command.CommandText="select a from TESTTAB where b<(select count(a) from TESTTAB)";
			
				var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				
				Reader.Close();

				Command.CommandText="DELETE FROM TESTTAB WHERE a IS NULL";
				Command.ExecuteNonQuery();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		}

		[Test]
		public void TestAggregateInvalidSum()
		{

			var Command = new EDBCommand("",con);

			try
			{
				Command.CommandText="select a from TESTTAB where b=sum(b)";
				Command.ExecuteNonQuery();
				Assert.Fail("Expected an exception on misuse of aggregate function");
			}
			catch(Exception )
			{
			}

		}

		[Test]
		public void TestAggregateHavingSum()
		{

			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having b=sum(b) order by a"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V2",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}

		}

		[Test]
		public void TestAggregateHavingSelectSum()
		{

			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB group by a,b having b=(select sum(b) from testtab);"
            };

            var Reader = Command.ExecuteReader();
				Assert.IsFalse(Reader.Read());
				
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void TestAggregateSelectSum()
		{
			
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "select a from TESTTAB where b<(select sum(b) from TESTTAB)"
            };

            var Reader = Command.ExecuteReader();
				
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V1",Reader.GetValue(0));
				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("V2",Reader.GetValue(0));
				Assert.IsFalse(Reader.Read());
				
				Reader.Close();
				
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Ignore("MERGE_HANG")]
		[Test]
		public void MultiColIndex ()
		{
			
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "INSERT INTO test_Index VALUES (2000, 3000, 'Ali');"
            };
            Command.ExecuteNonQuery();

				Command.CommandText="CREATE INDEX test_2_mm_idx ON test_Index (major, minor);";
				Command.ExecuteNonQuery();

				Command.CommandText="SELECT name FROM test_Index WHERE major = 2000 AND minor = 3000;";
				var Reader = Command.ExecuteReader();

				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("Ali",Reader.GetValue(0));

				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void UniqueIndex ()
		{
			
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "INSERT INTO test_Index VALUES (3000, 4000, 'Usman');"
            };
            Command.ExecuteNonQuery();

				Command.CommandText="CREATE UNIQUE INDEX index2 ON test_Index (major, minor);";
				Command.ExecuteNonQuery();

				Command.CommandText="SELECT name FROM test_Index WHERE major = 3000 AND minor = 4000;";
				var Reader = Command.ExecuteReader();

				Assert.IsTrue(Reader.Read());
				Assert.AreEqual("Usman",Reader.GetValue(0));

				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void ViolUniqueIndex ()
		{
			
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "INSERT INTO test_Index VALUES (3000, 4000, 'Kamran');"
            };
            try
				{
					Command.ExecuteNonQuery();

				}
				catch(EDBException )
				{
					Assert.Fail("Unable to execute... Unique Index violated");;
				}

			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void DdlFunctionalIndex ()
		{
			
			try
			{
            var Command = new EDBCommand("", con)
            {
                CommandText = "CREATE TABLE functional_index (name NAME,id int);"
            };
            Command.ExecuteNonQuery();

				Command.CommandText="CREATE SEQUENCE id INCREMENT BY 5 START WITH 1000 MAXVALUE 1010 MINVALUE 1000 Cache 3;";
				Command.ExecuteNonQuery();

				Command.CommandText="INSERT INTO functional_index VALUES('Ali',id.NextVal);";
				Command.ExecuteNonQuery();

				Command.CommandText="INSERT INTO functional_index VALUES('Ahmed',id.NextVal);";
				Command.ExecuteNonQuery();

				Command.CommandText="CREATE INDEX upper_name_idx ON functional_index(upper(name));";
				Command.ExecuteNonQuery();

				Command.CommandText="SELECT * from functional_index where upper(name) ='Ali';";
				var Reader = Command.ExecuteReader();

				Assert.IsFalse(Reader.Read());

				Reader.Close();
				
				Command.CommandText="DROP TABLE functional_index;Drop sequence id";
				Command.ExecuteNonQuery();

			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void DdlHashIndex ()
		{

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE TABLE tb_hash (major int,minor int,name varchar)"
        };
        Command.ExecuteNonQuery();

				Command.CommandText="CREATE INDEX tb_hash_idx ON tb_hash USING hash(name);";
				try
				{
				Command.ExecuteNonQuery();
				}

				catch(EDBException )
				{
				Assert.Fail("Could not create Hash index");
				}

				Command.CommandText="DROP TABLE tb_hash;";
				Command.ExecuteNonQuery();

		}

/*
 * Following test cases test the OUT Param refactoring FB17344.
 */

 //ZK: Redundent cases   [Test]
    public void OutParamProcSingleNumeric()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE PROCEDURE oneOutArgProc_test(a OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("oneOutArgProc_test", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("5", Command.Parameters[0].Value.ToString());
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP PROCEDURE oneOutArgProc_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases     [Test]
    public void OutParamProcSingleInt()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE PROCEDURE oneOutArgProc_test2(a OUT int) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("oneOutArgProc_test2(:param1)", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 1));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("5", Command.Parameters[0].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP PROCEDURE oneOutArgProc_test"
        };
        Command.ExecuteNonQuery();

    }

    [Test]
    public void OutParamProcVarchar()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE PROCEDURE oneOutArgProc_test1(a OUT varchar) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:='HELLO'; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("oneOutArgProc_test1(:param1)", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 1));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("HELLO", Command.Parameters[0].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP PROCEDURE oneOutArgProc_test1"
        };
        Command.ExecuteNonQuery();

    }

    [Test]
    public void OutParamProcVarcharPostGres()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = @"CREATE OR REPLACE PROCEDURE oneOutArgProc_test1(a OUT varchar)
                                        LANGUAGE plpgsql
                                        AS $$
                                        BEGIN
                                        a:= 'HELLO';
                                    END; $$"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("oneOutArgProc_test1(:param1)", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 1));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("HELLO", Command.Parameters[0].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP PROCEDURE oneOutArgProc_test1"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases     [Test]
    public void OutParamSingleInParamProc()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE PROCEDURE oneOutOneInArgProc_test(a OUT varchar, b IN varchar) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= b; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("oneOutOneInArgProc_test(:param1)", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "HELLO"));
        Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("HELLO", Command.Parameters["param2"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP PROCEDURE oneOutOneInArgProc_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases    [Test]
    public void OutParamTwoVarchar()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE PROCEDURE twoOutArgProc_test(a OUT varchar, b OUT varchar) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 'HELLO1'; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("twoOutArgProc_test", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HELLO"));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("(HELLO,HELLO1)", Command.Parameters["param1"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP PROCEDURE twoOutArgProc_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases    [Test]
    public void OutParamMultipleMixed()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE PROCEDURE allOutMixedArgProc_test(a OUT varchar, b OUT int, c OUT numeric, d OUT long) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 10; \n"
                    + "    c:= 20.55; \n"
                    + "    d:= 'HELLO1'; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("allOutMixedArgProc_test()", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HELLO"));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("(HELLO,10,20.55,HELLO1)", Command.Parameters["param1"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP PROCEDURE allOutMixedArgProc_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases    [Test]
    public void OutParamTwoInOutParamVarchar()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE PROCEDURE twoInOutArgProc_test(a OUT varchar, b INOUT varchar) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 'HELLO1'; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("twoInOutArgProc_test(:param1)", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "a"));
        Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HELLO"));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("(HELLO,HELLO1)", Command.Parameters["param2"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP PROCEDURE twoInOutArgProc_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases    [Test]
    public void OutParamFuncSingleOutNumeric()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE FUNCTION oneOutArgFunction_test(a OUT NUMERIC) RETURN INT\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " return 10;\n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("oneOutArgFunction_test()", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("(5,10)", Command.Parameters["param2"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP FUNCTION oneOutArgFunction_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases    [Test]
    public void OutParamFuncSingleOutInt()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE FUNCTION oneOutArgFunc_test(a OUT int) RETURN INT\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " return 10;\n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("oneOutArgFunc_test(:param2)", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 1));
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("5", Command.Parameters["param2"].Value.ToString());
        Assert.AreEqual("10", Command.Parameters["param1"].Value.ToString());
     
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP FUNCTION oneOutArgFunc_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases   [Test]
    public void OutParamFuncSingleOutVarchar()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE FUNCTION oneOutArgFunc_test(a OUT varchar) RETURN INT\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:='HELLO'; \n"
                    + " return 10;\n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("oneOutArgFunc_test()", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("(HELLO,10)", Command.Parameters["param2"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP FUNCTION oneOutArgFunc_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases   [Test]
    public void OutParamFuncSingleOutParamSingleInParam()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE FUNCTION oneOutOneInArgFunc_test(a OUT varchar, b IN varchar) RETURN int \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= b; \n"
                    + " return 10;\n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("oneOutOneInArgFunc_test(:param1)", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "HELLO"));
        Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HI"));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("(HELLO,10)", Command.Parameters["param2"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP FUNCTION oneOutOneInArgFunc_test(varchar)"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases    [Test]
    public void OutParamFuncTwoOutParamVarchar()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE FUNCTION twoOutArgFunc_test(a OUT varchar, b OUT varchar) RETURN INT\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 'HELLO1'; \n"
                    + "    return 10; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("twoOutArgFunc_test()", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HI"));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("(HELLO,HELLO1,10)", Command.Parameters["param2"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP FUNCTION twoOutArgFunc_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases    [Test]
    public void OutParamFuncMultipleMixedParam()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE FUNCTION allOutMixedArgFunc_test2(a OUT varchar, b OUT int, c OUT numeric, d OUT long) RETURN varchar\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 10; \n"
                    + "    c:= 20.55; \n"
                    + "    d:= 40; \n"
                    + "    return 'zk'; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("allOutMixedArgFunc_test2(:param1,:param2,:param3,:param4)", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
        Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
        Command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
        Command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Numeric, 10, "param4", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null!));
        Command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 0, "param5", ParameterDirection.ReturnValue, false, 0, 0, DataRowVersion.Current,null!));
     
        Command.Prepare();
        var result = Command.ExecuteReader();
    //    Assert.AreEqual("(HELLO,10,20.55,HELLO1,10)", Command.Parameters["param2"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP FUNCTION allOutMixedArgFunc_test"
        };
        Command.ExecuteNonQuery();

    }

    //ZK: Redundent cases      [Test]
    public void OutParamFuncTwoInOutParamVarchar()
    {

        var Command = new EDBCommand("", con)
        {
            CommandText = "CREATE OR REPLACE FUNCTION twoInOutArgFunc_test(a OUT varchar, b INOUT varchar) RETURN int\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 'HELLO1'; \n"
                    + "    return 10; \n"
                    + " END; \n"
        };
        Command.ExecuteNonQuery();

        Command = new EDBCommand("twoInOutArgFunc_test(:param1)", con)
        {
            CommandType = CommandType.StoredProcedure
        };
        Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "HELLO"));
        Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HI"));
        Command.Prepare();
        var result = Command.ExecuteReader();
        Assert.AreEqual("(HELLO,HELLO1,10)", Command.Parameters["param2"].Value.ToString());
        result.Close();
        Command = new EDBCommand
        {
            Connection = con,
            CommandText = "DROP FUNCTION twoInOutArgFunc_test(varchar)"
        };
        Command.ExecuteNonQuery();

    }
}
#pragma warning restore CS8602

