using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections;
using NUnit;

//Haroon
namespace DOTNET
{
	/// <summary>
	/// Summary description for MiscTest.
	/// </summary>
	
	[TestFixture]
	public class MiscTest : TestBase
    {
		EDBConnection con = null;

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
			catch(Exception exp)
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
	
			catch(Exception exp)
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
			catch(Exception exp)
			{
			}
			
		}
		
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
			catch(Exception exp)
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
			catch(Exception exp)
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
			catch(Exception exp)
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
			catch(Exception exp)
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

			/*	while(Reader.Read())
				{
					Console.WriteLine(Reader.GetValue(0).ToString());
				}*/
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

				/*	while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
					}*/
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
				catch(EDBException exp)
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

				/*	while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
					}*/
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

				catch(EDBException exp)
				{
				Assert.Fail("Could not create Hash index");
				}
				
				/*	while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
					}*/
				Command.CommandText="DROP TABLE tb_hash;";
				Command.ExecuteNonQuery();

			

			
		}
	

		// Following cases verify Arrays w.r.t various datatypes
		


		[Test]
		public void ArraysInt2()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (i int2[10],j int2[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{0,1,2,3,4,5,6,7,8,9}','{40,50,60,70,81,90,32765}');";
			Command.ExecuteNonQuery();
            Int16[] a = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Int16[] b = { 40, 50, 60, 70, 81, 90, 32765 };
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

				/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Int16[])Reader.GetValue(0));
            Assert.AreEqual(b, (Int16[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

			

			
		}

		
		[Test]
		public void ArraysInt4()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (i int4[10],j int4[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{0,1,2,3,4,5,6,7,8,9}','{-2147483648,100,433,544,2147483647}');";
			Command.ExecuteNonQuery();

            Int32[] a = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Int32[] b = { -2147483648, 100, 433, 544, 2147483647 };

			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Int32[])Reader.GetValue(0));
			Assert.AreEqual(b,(Int32[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

			

			
		}

		[Test]
		public void ArraysInt8()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (i int8[10],j int8[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{1000,2000,3000,4000,50000,6000,7000,8000,9000,10000}','{65454545,32769}');";
			Command.ExecuteNonQuery();

            Int64[] a = { 1000, 2000, 3000, 4000, 50000, 6000, 7000, 8000, 9000, 10000 };
            Int64[] b = { 65454545, 32769 };

			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Int64[])Reader.GetValue(0));
            Assert.AreEqual(b, (Int64[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

			

		}

		[Test]
		public void ArraysFloat()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (f1 Float[10],f2 Float[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{2.0,4.21,6.32,3.98,4.00,5.91,6.00,7.66,8.88,9.99}','{43534.234,5534.463}');";
			Command.ExecuteNonQuery();

            Double[] a = { 2, 4.21, 6.32, 3.98, 4, 5.91, 6, 7.66, 8.88, 9.99 };
            Double[] b = { 43534.234, 5534.463 };
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Double[] )Reader.GetValue(0));
            Assert.AreEqual(b, (Double[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

			

			
		}

		[Test]
		public void ArraysFloat4()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (f1 Float4[10],f2 Float4[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{65.2,23.1,56.42,334.5,46.3,532.33,69.64,75.234,8.75,92.1}','{2132.32,987.145}');";
			Command.ExecuteNonQuery();
            Single[] a = { 65.2F, 23.1F, 56.42F, 334.5F, 46.3F, 532.33F, 69.64F, 75.234F, 8.75F, 92.1F };
            Single[] b = { 2132.32F, 987.145F};

			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Single[])Reader.GetValue(0));
            Assert.AreEqual(b, (Single[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

			

			
		}

		[Test]
		public void ArraysFloat8()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (f1 Float8[10],f2 Float8[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{122.33,230.32,1342.24,28766.33,343245.234,462.33,575.323,6787.433,7004.344,865.345,983.433}','{8555.233,654.9785}');";
			Command.ExecuteNonQuery();

            Double[] a = { 122.33, 230.32, 1342.24, 28766.33, 343245.234, 462.33, 575.323, 6787.433, 7004.344, 865.345, 983.433 };
            Double[] b = { 8555.233, 654.9785 };
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Double[])Reader.GetValue(0));
			Assert.AreEqual(b,(Double[])Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysReal()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest1 (r1 Real[10],r2 Real[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest1 VALUES ('{12.3233,13.223,265.323,30.001,4235.9,543.454,543.453,775.235,800.992,9122.12}');";
			Command.ExecuteNonQuery();

            Single[] a = { 12.3233F, 13.223F, 265.323F, 30.001F, 4235.9F, 543.454F, 543.453F, 775.235F, 800.992F, 9122.12F };

			Command.CommandText="SELECT * FROM arrtest1;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(Single[])Reader.GetValue(0));
			//Assert.AreEqual("{8555.233,654.9785}",Reader.GetValue(1));*/
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest1";
			Command.ExecuteNonQuery();

			

			
		}

		
		[Test]
		public void ArraysNumeric()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysNumeric (n1 Numeric[10],n2 numeric[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysNumeric VALUES ('{120.89809,1234.00090,2.2434,3123.0,42342.22,53552.2,652.233,7.09,8.11,9.654}','{132.654,897.2563}');";
			Command.ExecuteNonQuery();

            Decimal[] a = { 120.89809M, 1234.00090M, 2.2434M, 3123.0M, 42342.22M, 53552.2M, 652.233M, 7.09M, 8.11M, 9.654M };
            Decimal[] b = { 132.654M, 897.2563M };
            Command.CommandText = "SELECT * FROM ArraysNumeric;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(Decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (Decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysNumeric";
			Command.ExecuteNonQuery();

			

			
		}


		[Test]
		public void ArraysNumericWithPrecision()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysNumericWithPrecision (n1 Numeric(5,2)[10],n2 Numeric(4,3)[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysNumericWithPrecision VALUES ('{120.89,123.90,22.334,412.40,422.22,552.21,62.22,712.09,18.11,91.65}','{1.234,2.142}');";
			Command.ExecuteNonQuery();

            Decimal[] a = { 120.89M, 123.90M, 22.33M, 412.40M, 422.22M, 552.21M, 62.22M, 712.09M, 18.11M, 91.65M };
            Decimal[] b = { 1.234M, 2.142M };

            Command.CommandText = "SELECT * FROM ArraysNumericWithPrecision;";
			EDBDataReader Reader = Command.ExecuteReader();

//			while(Reader.Read())
//					{
//						Console.WriteLine(Reader.GetValue(0).ToString());
//						Console.WriteLine(Reader.GetValue(1).ToString());
//					}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(Decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (Decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysNumericWithPrecision";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysSmallInt()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysSmallInt (i smallint[10],j smallint[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysSmallInt VALUES ('{-1,-2,-3,-4,0,4,5,6,7,8}','{40,50,60,70,81,90,32765}');";
			Command.ExecuteNonQuery();

            Int16[] a = { -1, -2, -3, -4, 0, 4, 5, 6, 7, 8 };
            Int16[] b = { 40, 50, 60, 70, 81, 90, 32765 };
            Command.CommandText = "SELECT * FROM ArraysSmallInt;";
			EDBDataReader Reader = Command.ExecuteReader();

			//			while(Reader.Read())
			//					{
			//						Console.WriteLine(Reader.GetValue(0).ToString());
			//						Console.WriteLine(Reader.GetValue(1).ToString());
			//					}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(Int16[])Reader.GetValue(0));
            Assert.AreEqual(b, (Int16[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysSmallInt";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysBigInt()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysBigInt (i bigint[10],j bigint[]);";
			Command.ExecuteNonQuery();

			Int64[] a ={-100,-200,-300,-4000,-922337203685477,50000,6000,7000,8000,9000};
            Int64[] b ={ -9223372036854775808, 9223372036854775807};

            Command.CommandText = "INSERT INTO ArraysBigInt VALUES ('{-100,-200,-300,-4000,-922337203685477,50000,6000,7000,8000,9000}','{-9223372036854775808,9223372036854775807}');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysBigInt;";
			EDBDataReader Reader = Command.ExecuteReader();

			//			while(Reader.Read())
			//					{
			//						Console.WriteLine(Reader.GetValue(0).ToString());
			//						Console.WriteLine(Reader.GetValue(1).ToString());
			//					}
			Assert.IsTrue(Reader.Read());
            Assert.AreEqual(a, (Int64[])Reader.GetValue(0));
            Assert.AreEqual(b, (Int64[])Reader.GetValue(1));
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysBigInt";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysDoublePrecision()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysDoublePrecision (d1 double precision[3],d2 double precision[]);";
			Command.ExecuteNonQuery();

            Double[] a = { 122.323423453, 230.32131231322, 123342.2323324 };
            Double[] b = { 555.43534543233, 344654.34534439782 };

            Command.CommandText = "INSERT INTO ArraysDoublePrecision VALUES ('{122.323423453,230.32131231322,123342.2323324}','{555.43534543233,344654.34534439785}');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysDoublePrecision;";
			EDBDataReader Reader = Command.ExecuteReader();

//						while(Reader.Read())
//								{
//									Console.WriteLine(Reader.GetValue(0).ToString());
//									Console.WriteLine(Reader.GetValue(1).ToString());
//								}
			Assert.IsTrue(Reader.Read());
            Assert.AreEqual(a, (Double[])Reader.GetValue(0));
            Assert.AreEqual(b, (Double[])Reader.GetValue(1));
			//Assert.AreEqual("{122.323423453,230.32131231322,123342.2323324}",Reader.GetValue(0));
			//Assert.AreEqual("{555.43534543233,344654.345344398}",Reader.GetValue(1));

			Reader.Close();

            Command.CommandText = "DROP TABLE ArraysDoublePrecision";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysInteger()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysInteger (i integer[],j integer[2]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysInteger VALUES ('{-2147483648,2147483647}','{5,9}');";
			Command.ExecuteNonQuery();

            Int32[] a = { -2147483648, 2147483647 };
            Int32[] b = { 5, 9 };

            Command.CommandText = "SELECT * FROM ArraysInteger;";
			EDBDataReader Reader = Command.ExecuteReader();

			//						while(Reader.Read())
			//								{
			//									Console.WriteLine(Reader.GetValue(0).ToString());
			//									Console.WriteLine(Reader.GetValue(1).ToString());
			//								}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(Int32[])Reader.GetValue(0));
            Assert.AreEqual(b, (Int32[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysInteger";
			Command.ExecuteNonQuery();

		}

		
		[Test]
		public void ArraysNumber()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtestNumber (n1 Number[5],n2 Number[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO arrtestNumber VALUES ('{321.255,654.233,8987,545.23,654.36}','{31.2434,23.1442}');";
			Command.ExecuteNonQuery();
            Decimal[] a = { 321.255M, 654.233M, 8987M, 545.23M, 654.36M };
            Decimal[] b = { 31.2434M, 23.1442M };
            Command.CommandText = "SELECT * FROM arrtestNumber;";
			EDBDataReader Reader = Command.ExecuteReader();

			//						while(Reader.Read())
			//								{
			//									Console.WriteLine(Reader.GetValue(0).ToString());
			//									Console.WriteLine(Reader.GetValue(1).ToString());
			//								}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(Decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (Decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE arrtestNumber";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysDecimal()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysDecimal (n1 Decimal(5,2)[10],n2 Decimal(4,3)[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysDecimal VALUES ('{120.89,123.90,22.334,412.40,422.22,552.21,62.22,712.09,18.11,91.65}','{1.234,2.142}');";
			Command.ExecuteNonQuery();

            Decimal[] a = { 120.89M, 123.90M, 22.33M, 412.40M, 422.22M, 552.21M, 62.22M, 712.09M, 18.11M, 91.65M };
            Decimal[] b = { 1.234M, 2.142M };
            Command.CommandText = "SELECT * FROM ArraysDecimal;";
			EDBDataReader Reader = Command.ExecuteReader();

//									while(Reader.Read())
//											{
//												Console.WriteLine(Reader.GetValue(0).ToString());
//												Console.WriteLine(Reader.GetValue(1).ToString());
//											}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(Decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (Decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysDecimal";
			Command.ExecuteNonQuery();

		}


		/*[Test]
		public void ArraysMoney()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (m1 money[],m2 money[2]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{-21474823123326.4128,2123432474836.247}','{2343245.571,523432.3226}');";
			Command.ExecuteNonQuery();
			
            Decimal[] a = 

			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();
			object[] test={"-$21,474,823,123,326.41","$2,123,432,474,836.25"};
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{\""+test[0].ToString()+"\",\""+test[1].ToString()+"\"}",Reader.GetValue(0).ToString());
			string[] teststr={"$2,343,245.57","$523,432.32"};
			Assert.AreEqual("{\""+teststr[0]+"\",\""+teststr[1]+"\"}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}*/

		[Test]
		public void ArraysSmallMoney()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysSmallMoney (m1 smallmoney[],m2 smallmoney[2]);";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraysSmallMoney VALUES ('{-474836.4128,74836.2417}','{45.1157,15.2636}');";
			Command.ExecuteNonQuery();

            Decimal[] a = { -474836.4128M, 74836.2417M };
            Decimal[] b = { 45.1157M, 15.2636M };
            Command.CommandText = "SELECT * FROM ArraysSmallMoney;";
			EDBDataReader Reader = Command.ExecuteReader();

//															while(Reader.Read())
//																	{
//																		Console.WriteLine(Reader.GetValue(0).ToString());
//																		Console.WriteLine(Reader.GetValue(1).ToString());
//																	}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(Decimal[])Reader.GetValue(0));
            Assert.AreEqual(b, (Decimal[])Reader.GetValue(1));
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysSmallMoney";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysText()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE books( books text[]);;";
			Command.ExecuteNonQuery();

			Command.CommandText="INSERT INTO books VALUES ('{ Lord of the Rings , Suffocles}');";
			Command.ExecuteNonQuery();

            String[] a = {"Lord of the Rings","Suffocles"};

			Command.CommandText="SELECT * FROM  books;";
			EDBDataReader Reader = Command.ExecuteReader();

//																		while(Reader.Read())
//																				{
//																					Console.WriteLine(Reader.GetValue(0).ToString());
//																					Console.WriteLine(Reader.GetValue(1).ToString());
//																				}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(String[])Reader.GetValue(0));
//			Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE  books";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysVarchar()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE favourite_books( books Varchar[3]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO favourite_books VALUES ('{The Hitchhikers Guide to the Galaxy,Harry Potter,Kitten, Squared}');";
			Command.ExecuteNonQuery();

            String[] a = {"The Hitchhikers Guide to the Galaxy","Harry Potter","Kitten","Squared"};

			Command.CommandText="SELECT * FROM  favourite_books;";
			EDBDataReader Reader = Command.ExecuteReader();

//			while(Reader.Read())
//			{
//				Console.WriteLine(Reader.GetValue(0).ToString());
//				Console.WriteLine(Reader.GetValue(1).ToString());
//			}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(String[])Reader.GetValue(0));
					//	Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE  favourite_books";
			Command.ExecuteNonQuery();

		}


		
		[Test]
		public void ArraysTinyText()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE books( books tinytext[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO books VALUES ('{Lord of the Rings,Suffocles}');";
			Command.ExecuteNonQuery();
			
            String[] a = {"Lord of the Rings","Suffocles"};

			Command.CommandText="SELECT * FROM  books;";
			EDBDataReader Reader = Command.ExecuteReader();

			//			while(Reader.Read())
			//			{
			//				Console.WriteLine(Reader.GetValue(0).ToString());
			//				Console.WriteLine(Reader.GetValue(1).ToString());
			//			}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(String[])Reader.GetValue(0));
			//	Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE  books";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysVarchar2()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE favourite_books( books Varchar2[3]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO favourite_books VALUES ('{The Hitchikers Guide to the Galaxy,Harry Potter,Kitten, Squared}');";
			Command.ExecuteNonQuery();

            String[] a = {"The Hitchikers Guide to the Galaxy","Harry Potter","Kitten","Squared"};
			
			Command.CommandText="SELECT * FROM  favourite_books;";
			EDBDataReader Reader = Command.ExecuteReader();

//						while(Reader.Read())
//						{
//							Console.WriteLine(Reader.GetValue(0).ToString());
//							//Console.WriteLine(Reader.GetValue(1).ToString());
//						}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual(a,(String[])Reader.GetValue(0));
			//	Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE  favourite_books";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysCharacter()
		{
            String[] a = {"1st char  ","sec char  " };
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE chartest( ch character(10)[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO chartest VALUES ('{1st char,sec char}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM chartest;";
			EDBDataReader Reader = Command.ExecuteReader();
//
//									while(Reader.Read())
//									{
//										Console.WriteLine(Reader.GetValue(0).ToString());
//										//Console.WriteLine(Reader.GetValue(1).ToString());
//									}
			
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(String[])Reader.GetValue(0));
			
			Reader.Close();
			Command.CommandText="DROP TABLE chartest";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysChar()
		{
            String[] a = { "1st char","sec char" };
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE chartest( ch char(8)[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO chartest VALUES ('{1st char,sec char}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM chartest;";
			EDBDataReader Reader = Command.ExecuteReader();
			
//												while(Reader.Read())
//												{
//													Console.WriteLine(Reader.GetValue(0).ToString());
//													//Console.WriteLine(Reader.GetValue(1).ToString());
//												}
			
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(String[])Reader.GetValue(0));
			
			Reader.Close();
			Command.CommandText="DROP TABLE chartest";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysLong()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE books( books long[]);";
			Command.ExecuteNonQuery();

            String[] a = { "Lord of the War", "Suffocles" };

			Command.CommandText="INSERT INTO books VALUES ('{Lord of the War,Suffocles}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM books;";
			EDBDataReader Reader = Command.ExecuteReader();
			
//															while(Reader.Read())
//															{
//																Console.WriteLine(Reader.GetValue(0).ToString());
//																//Console.WriteLine(Reader.GetValue(1).ToString());
//															}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(String[])Reader.GetValue(0));
			
			Reader.Close();
			Command.CommandText="DROP TABLE books";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysLongText()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE books( books longtext[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO books VALUES ('{Lord of the War,Suffocles,A walk in the cloudsssss }');";
			Command.ExecuteNonQuery();

            String[] a = { "Lord of the War", "Suffocles", "A walk in the cloudsssss" };

			Command.CommandText="SELECT * FROM books;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(String[])Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE books";
			Command.ExecuteNonQuery();

		}

        //ZK CHECKME: Date[] to DateTime[] cast not supported in npgsql
		[Test]
		public void ArraysDate()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (d1 Date[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{040506,101203}');";
			Command.ExecuteNonQuery();

            DateTime[] a = { Convert.ToDateTime("2004-05-06 00:00:00"), Convert.ToDateTime("2010-12-03 00:00:00") };

			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
			//Assert.AreEqual(a,(D)Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysTimestamp()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysTimestamp (t Timestamp[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysTimestamp VALUES ('{1999-01-08 04:05:06,December 11 04:05:06 2006}');";
			Command.ExecuteNonQuery();

            DateTime[] a = { DateTime.Parse("1999-01-08 04:05:06"), DateTime.Parse("2006-12-11 04:05:06") };

            Command.CommandText = "SELECT * FROM ArraysTimestamp;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
		//	Assert.AreEqual(a,(DateTime[])Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysTimestamp;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysDateTime()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysDateTime (t DATETIME[]);";
			Command.ExecuteNonQuery();

			
            DateTime[] a = {Convert.ToDateTime("1999-01-08 04:05:06"),Convert.ToDateTime("2006-12-11 04:05:06")};
            Command.CommandText = "INSERT INTO ArraysDateTime VALUES ('{1999-01-08 04:05:06,December 11 04:05:06 2006}');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysDateTime;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
		//	Assert.AreEqual(a,(DateTime[])Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysDateTime;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysTime()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysTime (t TIME[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysTime VALUES ('{04:05:06,12:10:48 }');";
			Command.ExecuteNonQuery();
			
            

            DateTime[] a = {DateTime.Parse("04:05:06"),DateTime.Parse("12:10:48")};

            Command.CommandText = "SELECT * FROM ArraysTime;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
          //  DateTime[] data = (DateTime[])Reader.GetValue(0);

		//	Assert.AreEqual(a[0].ToShortTimeString(),data[0].ToShortTimeString());
          //  Assert.AreEqual(a[1].ToShortTimeString(), data[1].ToShortTimeString());
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysTime;";
			Command.ExecuteNonQuery();

		}


		public static String BitStreamToString(IEnumerable myList, int myWidth)
		{
			System.IO.StringWriter sw = new System.IO.StringWriter();

			int i = myWidth;
			foreach (Object obj in myList)
			{
				if (i <= 0)
				{
					i = myWidth;
					sw.WriteLine();
				}
				i--;
				sw.Write("{0,8}", obj);
			}
			sw.WriteLine();
			return sw.ToString();
		}

		public static String MakeDebugMessage(BitArray expected, BitArray actual)
		{

			return "Expected:\n" + BitStreamToString((IEnumerable)expected, 8) + "Actual:\n" + BitStreamToString((IEnumerable)actual, 8);

		}

		[Test]
		public void ArraysBoolean()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysBoolean (t boolean[]);";
			Command.ExecuteNonQuery();

            Boolean[] a =  { true, false, true, true, true, false, false, false, false, false, false, false };

            Command.CommandText = "INSERT INTO ArraysBoolean VALUES ('{t,f,t,t,t,f,f,f,f,f,f,f }');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysBoolean;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Boolean[])Reader.GetValue(0), MakeDebugMessage(new BitArray(a), new BitArray((Boolean[])Reader.GetValue(0))));

			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysBoolean;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysBoolTrueFalse()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysBool (t bool[]);";
			Command.ExecuteNonQuery();

            Boolean[] a = { true, false, false, false, true, false, false, true };

			Command.CommandText = "INSERT INTO ArraysBool VALUES ('{true,false,false,false,true,false,false,true }');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysBool;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Boolean[])Reader.GetValue(0), MakeDebugMessage(new BitArray(a), new BitArray((Boolean[])Reader.GetValue(0))));

			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysBool;";
			Command.ExecuteNonQuery();

		}

		
		[Test]
		public void ArraysBoolOneZero()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysBool2 (t bool[]);";
			Command.ExecuteNonQuery();

            Boolean[] a = { false, true, true, false, false, false, true, true, true, true, true, false };

            Command.CommandText = "INSERT INTO ArraysBool2 VALUES ('{0,1,1,0,0,0,1,1,1,1,1,0}');";
			Command.ExecuteNonQuery();

            Command.CommandText = "SELECT * FROM ArraysBool2;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Boolean[])Reader.GetValue(0), MakeDebugMessage(new BitArray(a), new BitArray((Boolean[])Reader.GetValue(0))));
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysBool2;";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysTimestampWithoutTimeZone()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysTimestampWithoutTimeZone (t Timestamp[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysTimestampWithoutTimeZone VALUES ('{1999-01-08 04:05:06 -8:00,2005-11-08 12:02:06 -8:00,February 10 00:04:50 2004 PST}');";
			Command.ExecuteNonQuery();

            DateTime[] a = { DateTime.Parse("1999-01-08 04:05:06"), DateTime.Parse("2005-11-08 12:02:06"), DateTime.Parse("2004-02-10 00:04:50") };

            Command.CommandText = "SELECT * FROM ArraysTimestampWithoutTimeZone;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//	while(Reader.Read())
			//	{
			//		Console.WriteLine(Reader.GetValue(0).ToString());
			//		Console.WriteLine(Reader.GetValue(1).ToString());
			//	}
			Assert.IsTrue(Reader.Read());
		//	Assert.AreEqual(a,(DateTime[])Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysTimestampWithoutTimeZone;";
			Command.ExecuteNonQuery();
            

		}


		[Test]
		public void ArraysBitString()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE IF NOT EXISTS arrtest (t bit(3)[]);";
			Command.ExecuteNonQuery();

			BitArray []a = new BitArray[3] {
				new BitArray(new bool[] { true, false, true }),
				new BitArray(new bool[] { true, true, false }),
				new BitArray(new bool[] { false, true, true }) };

			Command.CommandText="INSERT INTO arrtest VALUES ('{101,110,011}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			Assert.IsTrue(Reader.Read());
			BitArray[] myBools = (BitArray[])Reader.GetValue(0);
			Assert.AreEqual(3, myBools.Length);
			BitArray b0 = (BitArray)myBools.GetValue(0);
			BitArray b1 = (BitArray)myBools.GetValue(1);
			BitArray b2 = (BitArray)myBools.GetValue(2);

			BitStreamToString((IEnumerable)b1, 8);
			Assert.AreEqual(a[0], b0, MakeDebugMessage(a[0],b0));
			Assert.AreEqual(a[1], b1, MakeDebugMessage(a[1], b1));
			Assert.AreEqual(a[2], b2, MakeDebugMessage(a[2], b2));

			Reader.Close();
			Command.CommandText="DROP TABLE arrtest;";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysInterval()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysInterval (t interval[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysInterval VALUES ('{1 12:59:10,2 01:23:34}');";
			Command.ExecuteNonQuery();
//            EDBTypes.EDBInterval[] a = { EDBTypes.EDBInterval.Parse("1 day 12:59:10"), EDBTypes.EDBInterval.Parse("2 days 01:23:34") };

            Command.CommandText = "SELECT * FROM ArraysInterval;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//	while(Reader.Read())
			//	{
			//		Console.WriteLine(Reader.GetValue(0).ToString());
			//		Console.WriteLine(Reader.GetValue(1).ToString());
			//	}
			Assert.IsTrue(Reader.Read());
            //Assert.AreEqual(a,(EDBTypes.EDBInterval[])Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysInterval;";
			Command.ExecuteNonQuery();

		}

		
		[Test]
		public void ArraysInterval2()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraysInterval2 (t interval[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraysInterval2 VALUES ('{-23:00:00,2 01:23:34,1 day -01:00:00,21 days}');";
			Command.ExecuteNonQuery();


            //EDBTypes.EDBInterval[] a = { EDBTypes.EDBInterval.Parse("-23:00:00"), EDBTypes.EDBInterval.Parse("2 days 01:23:34"),
            //EDBTypes.EDBInterval.Parse("1 day -01:00:00"),EDBTypes.EDBInterval.Parse("21 days")};
            Command.CommandText = "SELECT * FROM ArraysInterval2;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//	while(Reader.Read())
			//	{
			//		Console.WriteLine(Reader.GetValue(0).ToString());
			//		Console.WriteLine(Reader.GetValue(1).ToString());
			//	}
			Assert.IsTrue(Reader.Read());
            //Assert.AreEqual(a,(EDBTypes.EDBInterval[])Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraysInterval2;";
			Command.ExecuteNonQuery();

		}


		
		[Test]
		public void ArraySelect()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArraySelect (a int2[],b int, c name[],e float8[],f char(5)[],g varchar(5)[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArraySelect (a,b, c, e, f, g) " +
  			 " VALUES ('{100,200,300,400,500}', 101, '{}',  '{}', '{}', '{}');	";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArraySelect (a, b, c, e, f, g) VALUES ('{11,12,23}',103, '{ foobar}', " +
				" '{ 3.4,  6.7}', '{abc,abcde}', '{xyz,xyzz}');";
			Command.ExecuteNonQuery();

            Int16[] a = { 100, 200, 300, 400, 500 };
            Int16[] c = {  };

            Command.CommandText = "SELECT  * FROM ArraySelect where b = 101;";
			EDBDataReader Reader = Command.ExecuteReader();
			
//				while(Reader.Read())
//				{
//					Console.WriteLine(Reader.GetValue(0).ToString());
//					Console.WriteLine(Reader.GetValue(1).ToString());
//					Console.WriteLine(Reader.GetValue(2).ToString());
//					Console.WriteLine(Reader.GetValue(3).ToString());
//					Console.WriteLine(Reader.GetValue(4).ToString());
//					Console.WriteLine(Reader.GetValue(5).ToString());
//				}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Int16[])Reader.GetValue(0));
            Assert.AreEqual(101, (Int32)Reader.GetValue(1));
						
			Reader.Close();
            Command.CommandText = "DROP TABLE ArraySelect;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArrayUpdate()
		{
			
			EDBCommand Command = new EDBCommand("",con);

            Command.CommandText = "CREATE TABLE ArrayUpdate (a int2[],b int, c name[],e float8[],f char(5)[],g varchar(5)[]);";
			Command.ExecuteNonQuery();



            Command.CommandText = "INSERT INTO ArrayUpdate (a,b, c, e, f, g) " +
				" VALUES ('{100,200,300,400,500}', 101, '{}',  '{}', '{}', '{}');	";
			Command.ExecuteNonQuery();

            Command.CommandText = "UPDATE ArrayUpdate SET e[0] = '1.10'";
			Command.ExecuteNonQuery();

            Command.CommandText = "INSERT INTO ArrayUpdate (a, b, c, e, f, g) VALUES ('{11,12,23}',103, '{ foobar}', " +
				" '{ 3.4,  6.7}', '{abc,abcde}', '{xyz,xyzz}');";
			Command.ExecuteNonQuery();

            Int16[] a = { 100, 200, 300, 400, 500 };
            Command.CommandText = "SELECT a, e[0] ,e[1]  FROM ArrayUpdate where a[2] = 200;";
			EDBDataReader Reader = Command.ExecuteReader();
//			
//							while(Reader.Read())
//							{
//								Console.WriteLine(Reader.GetValue(0).ToString());
//								Console.WriteLine(Reader.GetValue(1).ToString());
//								Console.WriteLine(Reader.GetValue(2).ToString());
//								
//							}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual(a,(Int16[])Reader.GetValue(0));
			Assert.AreEqual("1.1",Reader.GetValue(1).ToString());
			Assert.AreEqual("",Reader.GetValue(2).ToString());
//			//Console.WriteLine(Reader.GetValue(0).ToString());

			
			Reader.Close();
            Command.CommandText = "DROP TABLE ArrayUpdate;";
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
}
