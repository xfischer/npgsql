using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections;
using NUnit;

//Haroon
namespace EnterpriseDB.EDBClient.Tests
{
#pragma warning disable CS8602
    /// <summary>
    /// Summary description for MiscTest.
    /// </summary>

    [TestFixture]
	public class EDBMiscTest : TestBase
    {
		EDBConnection? con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();
			TestUtil.createTempTable(con,"TESTTAB","a VARCHAR, b INT4");
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="INSERT INTO TESTTAB VALUES('V1',1)";
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
				EDBConnection Con=OpenConnection();
			
				EDBCommand Command=new EDBCommand("",Con);
				Command.CommandType=CommandType.Text;
				string Select="select datname from pg_database";
				Command.CommandText=Select;
				EDBDataReader Reader=Command.ExecuteReader();
				
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
			 
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB where b=max(b)";
			
				Command.CommandType=CommandType.Text;
				
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText= "select a from TESTTAB group by a,b having max(b)=b order by a";
			
				Command.CommandType=CommandType.Text;
				EDBDataReader Reader=Command.ExecuteReader();
			
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB group by a,b having b=(select max(b) from testtab);";
			
				EDBDataReader Reader = Command.ExecuteReader();
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
		public void testAggregateSelectMax()
		{
			try
			{
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB where b=(select max(b) from TESTTAB)";
			
				EDBDataReader Reader = Command.ExecuteReader();
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
		public void testAggregateInvalidMin()
		{
			EDBCommand Command = new EDBCommand("",con);
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
		public void testAggregateHavingMin()
		{
			try
			{
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText= "select a from TESTTAB group by a,b having min(b)=b order by a";
				EDBDataReader Reader = Command.ExecuteReader();

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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB group by a,b having b=(select min(b) from testtab);";

				EDBDataReader Reader = Command.ExecuteReader();
				
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB where b=(select min(b) from TESTTAB)";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB where b=avg(b)";
				
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText= "select a from TESTTAB group by a,b having avg(b)=b order by a";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB group by a,b having b=(select avg(b) from testtab);";

				EDBDataReader Reader = Command.ExecuteReader();
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB where b<(select avg(b) from TESTTAB)";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
			EDBCommand Command = new EDBCommand("",con);

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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText= "select a from TESTTAB group by a,b having BIT_AND(b)=b order by a";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB group by a,b having b=(select BIT_AND(b) from testtab);";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB where b>(select BIT_AND(b) from TESTTAB)";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
			
			EDBCommand Command = new EDBCommand("",con);

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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText= "select a from TESTTAB group by a,b having BIT_OR(b)=b order by a";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
			
			EDBCommand Command = new EDBCommand("",con);

			try
			{
				Command.CommandText="select a from TESTTAB group by a,b having b=(select BIT_OR(b) from testtab);";
				EDBDataReader Reader = Command.ExecuteReader();
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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB where b<(select BIT_OR(b) from TESTTAB)";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
			
			EDBCommand Command = new EDBCommand("",con);

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
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB group by a,b having b=count(*)";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
		public void testAggregateHavingSelectCount()
		{
				
			try
			{
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB group by a,b having b=(select count(*) from testtab);";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
		public void testAggregateSelectCount()
		{
			
			try
			{
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB where b<(select count(*) from TESTTAB)";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
		public void testAggregateSelectCountNonNull()
		{

			try
			{
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="INSERT INTO TESTTAB(b) VALUES(3)";
				Command.ExecuteNonQuery();
				Command.CommandText="select a from TESTTAB where b<(select count(a) from TESTTAB)";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
		public void testAggregateInvalidSum()
		{

			EDBCommand Command = new EDBCommand("",con);

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
		public void testAggregateHavingSum()
		{

			try
			{
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText= "select a from TESTTAB group by a,b having b=sum(b) order by a";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
		public void testAggregateHavingSelectSum()
		{

			try
			{
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB group by a,b having b=(select sum(b) from testtab);";
			
				EDBDataReader Reader = Command.ExecuteReader();
				Assert.IsFalse(Reader.Read());
				
				Reader.Close();
			}

			catch(EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}

		[Test]
		public void testAggregateSelectSum()
		{
			
			try
			{
				EDBCommand Command = new EDBCommand("",con);
				Command.CommandText="select a from TESTTAB where b<(select sum(b) from TESTTAB)";
			
				EDBDataReader Reader = Command.ExecuteReader();
				
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
				EDBCommand Command = new EDBCommand("",con);
				
				Command.CommandText="INSERT INTO test_Index VALUES (2000, 3000, 'Ali');";
				Command.ExecuteNonQuery();

				Command.CommandText="CREATE INDEX test_2_mm_idx ON test_Index (major, minor);";
				Command.ExecuteNonQuery();

				Command.CommandText="SELECT name FROM test_Index WHERE major = 2000 AND minor = 3000;";
				EDBDataReader Reader = Command.ExecuteReader();

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
				EDBCommand Command = new EDBCommand("",con);
				
				Command.CommandText="INSERT INTO test_Index VALUES (3000, 4000, 'Usman');";
				Command.ExecuteNonQuery();

				Command.CommandText="CREATE UNIQUE INDEX index2 ON test_Index (major, minor);";
				Command.ExecuteNonQuery();

				Command.CommandText="SELECT name FROM test_Index WHERE major = 3000 AND minor = 4000;";
				EDBDataReader Reader = Command.ExecuteReader();

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
				EDBCommand Command = new EDBCommand("",con);
				
				Command.CommandText="INSERT INTO test_Index VALUES (3000, 4000, 'Kamran');";
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
				EDBCommand Command = new EDBCommand("",con);
				
				Command.CommandText="CREATE TABLE functional_index (name NAME,id int);";
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
				EDBDataReader Reader = Command.ExecuteReader();

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
			
				EDBCommand Command = new EDBCommand("",con);
				
				Command.CommandText="CREATE TABLE tb_hash (major int,minor int,name varchar)";
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

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE PROCEDURE oneOutArgProc_test(a OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("oneOutArgProc_test", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("5", Command.Parameters[0].Value.ToString());
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP PROCEDURE oneOutArgProc_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases     [Test]
        public void OutParamProcSingleInt()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE PROCEDURE oneOutArgProc_test2(a OUT int) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("oneOutArgProc_test2(:param1)", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 1));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("5", Command.Parameters[0].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP PROCEDURE oneOutArgProc_test";
            Command.ExecuteNonQuery();

        }

        [Test]
        public void OutParamProcVarchar()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE PROCEDURE oneOutArgProc_test1(a OUT varchar) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:='HELLO'; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("oneOutArgProc_test1(:param1)", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 1));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("HELLO", Command.Parameters[0].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP PROCEDURE oneOutArgProc_test1";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases     [Test]
        public void OutParamSingleInParamProc()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE PROCEDURE oneOutOneInArgProc_test(a OUT varchar, b IN varchar) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= b; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("oneOutOneInArgProc_test(:param1)", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "HELLO"));
            Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("HELLO", Command.Parameters["param2"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP PROCEDURE oneOutOneInArgProc_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases    [Test]
        public void OutParamTwoVarchar()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE PROCEDURE twoOutArgProc_test(a OUT varchar, b OUT varchar) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 'HELLO1'; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("twoOutArgProc_test", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HELLO"));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("(HELLO,HELLO1)", Command.Parameters["param1"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP PROCEDURE twoOutArgProc_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases    [Test]
        public void OutParamMultipleMixed()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE PROCEDURE allOutMixedArgProc_test(a OUT varchar, b OUT int, c OUT numeric, d OUT long) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 10; \n"
                    + "    c:= 20.55; \n"
                    + "    d:= 'HELLO1'; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("allOutMixedArgProc_test()", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HELLO"));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("(HELLO,10,20.55,HELLO1)", Command.Parameters["param1"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP PROCEDURE allOutMixedArgProc_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases    [Test]
        public void OutParamTwoInOutParamVarchar()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE PROCEDURE twoInOutArgProc_test(a OUT varchar, b INOUT varchar) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 'HELLO1'; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("twoInOutArgProc_test(:param1)", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "a"));
            Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HELLO"));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("(HELLO,HELLO1)", Command.Parameters["param2"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP PROCEDURE twoInOutArgProc_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases    [Test]
        public void OutParamFuncSingleOutNumeric()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE FUNCTION oneOutArgFunction_test(a OUT NUMERIC) RETURN INT\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " return 10;\n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("oneOutArgFunction_test()", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("(5,10)", Command.Parameters["param2"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP FUNCTION oneOutArgFunction_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases    [Test]
        public void OutParamFuncSingleOutInt()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE FUNCTION oneOutArgFunc_test(a OUT int) RETURN INT\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " return 10;\n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("oneOutArgFunc_test(:param2)", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 1));
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("5", Command.Parameters["param2"].Value.ToString());
            Assert.AreEqual("10", Command.Parameters["param1"].Value.ToString());
         
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP FUNCTION oneOutArgFunc_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases   [Test]
        public void OutParamFuncSingleOutVarchar()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE FUNCTION oneOutArgFunc_test(a OUT varchar) RETURN INT\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:='HELLO'; \n"
                    + " return 10;\n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("oneOutArgFunc_test()", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 1));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("(HELLO,10)", Command.Parameters["param2"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP FUNCTION oneOutArgFunc_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases   [Test]
        public void OutParamFuncSingleOutParamSingleInParam()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE FUNCTION oneOutOneInArgFunc_test(a OUT varchar, b IN varchar) RETURN int \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= b; \n"
                    + " return 10;\n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("oneOutOneInArgFunc_test(:param1)", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "HELLO"));
            Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HI"));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("(HELLO,10)", Command.Parameters["param2"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP FUNCTION oneOutOneInArgFunc_test(varchar)";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases    [Test]
        public void OutParamFuncTwoOutParamVarchar()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE FUNCTION twoOutArgFunc_test(a OUT varchar, b OUT varchar) RETURN INT\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 'HELLO1'; \n"
                    + "    return 10; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("twoOutArgFunc_test()", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HI"));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("(HELLO,HELLO1,10)", Command.Parameters["param2"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP FUNCTION twoOutArgFunc_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases    [Test]
        public void OutParamFuncMultipleMixedParam()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE FUNCTION allOutMixedArgFunc_test2(a OUT varchar, b OUT int, c OUT numeric, d OUT long) RETURN varchar\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 10; \n"
                    + "    c:= 20.55; \n"
                    + "    d:= 40; \n"
                    + "    return 'zk'; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("allOutMixedArgFunc_test2(:param1,:param2,:param3,:param4)", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null));
            Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null));
            Command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null));
            Command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Numeric, 10, "param4", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null));
            Command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 0, "param5", ParameterDirection.ReturnValue, false, 0, 0, DataRowVersion.Current,null));
         
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
        //    Assert.AreEqual("(HELLO,10,20.55,HELLO1,10)", Command.Parameters["param2"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP FUNCTION allOutMixedArgFunc_test";
            Command.ExecuteNonQuery();

        }

        //ZK: Redundent cases      [Test]
        public void OutParamFuncTwoInOutParamVarchar()
        {

            EDBCommand Command = new EDBCommand("", con);

            Command.CommandText = "CREATE OR REPLACE FUNCTION twoInOutArgFunc_test(a OUT varchar, b INOUT varchar) RETURN int\n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:= 'HELLO'; \n"
                    + "    b:= 'HELLO1'; \n"
                    + "    return 10; \n"
                    + " END; \n";
            Command.ExecuteNonQuery();

            Command = new EDBCommand("twoInOutArgFunc_test(:param1)", con);
            Command.CommandType = CommandType.StoredProcedure;
            Command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "HELLO"));
            Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HI"));
            Command.Prepare();
            EDBDataReader result = Command.ExecuteReader();
            Assert.AreEqual("(HELLO,HELLO1,10)", Command.Parameters["param2"].Value.ToString());
            result.Close();
            Command = new EDBCommand();
            Command.Connection = con;
            Command.CommandText = "DROP FUNCTION twoInOutArgFunc_test(varchar)";
            Command.ExecuteNonQuery();

        }
    }
#pragma warning restore CS8602
}
