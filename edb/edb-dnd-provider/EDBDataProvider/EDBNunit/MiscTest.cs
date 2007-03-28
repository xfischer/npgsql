using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;

//Haroon
namespace NUnit
{
	/// <summary>
	/// Summary description for MiscTest.
	/// </summary>
	
	[TestFixture]
	public class MiscTest
	{
		EDBConnection con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = TestUtil.openDB();
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
			
			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="DROP Table TESTTAB";
			Command.CommandType=CommandType.Text;
			Command.ExecuteNonQuery();

			Command.CommandText="DROP Table test_Index";
			Command.CommandType=CommandType.Text;
			Command.ExecuteNonQuery();
			
			TestUtil.closeDB(con);
		}
			
		[Test]
		public void TestDatabaseSelectNullBug()
		{
			try
			{
				EDBConnection Con=TestUtil.openDB();
			
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
				Command.CommandText="select a from TESTTAB group by a,b having max(b)=b";
			
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
				Command.CommandText="select a from TESTTAB group by a,b having min(b)=b";
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
				Command.CommandText="select a from TESTTAB group by a,b having avg(b)=b";
			
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
				Command.CommandText="select a from TESTTAB group by a,b having BIT_AND(b)=b";
			
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
				Command.CommandText="select a from TESTTAB group by a,b having BIT_OR(b)=b";
			
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
				Command.CommandText="select a from TESTTAB group by a,b having b=sum(b)";
			
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
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

				/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{0,1,2,3,4,5,6,7,8,9}",Reader.GetValue(0));
			Assert.AreEqual("{40,50,60,70,81,90,32765}",Reader.GetValue(1));
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
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/

			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{0,1,2,3,4,5,6,7,8,9}",Reader.GetValue(0));
			Assert.AreEqual("{-2147483648,100,433,544,2147483647}",Reader.GetValue(1));
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
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{1000,2000,3000,4000,50000,6000,7000,8000,9000,10000}",Reader.GetValue(0));
			Assert.AreEqual("{65454545,32769}",Reader.GetValue(1));
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
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{2,4.21,6.32,3.98,4,5.91,6,7.66,8.88,9.99}",Reader.GetValue(0));
			Assert.AreEqual("{43534.234,5534.463}",Reader.GetValue(1));
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
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{65.2,23.1,56.42,334.5,46.3,532.33,69.64,75.234,8.75,92.1}",Reader.GetValue(0));
			Assert.AreEqual("{2132.32,987.145}",Reader.GetValue(1));
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
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{122.33,230.32,1342.24,28766.33,343245.234,462.33,575.323,6787.433,7004.344,865.345,983.433}",Reader.GetValue(0));
			Assert.AreEqual("{8555.233,654.9785}",Reader.GetValue(1));
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
			
			Command.CommandText="SELECT * FROM arrtest1;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{12.3233,13.223,265.323,30.001,4235.9,543.454,543.453,775.235,800.992,9122.12}",Reader.GetValue(0));
			//Assert.AreEqual("{8555.233,654.9785}",Reader.GetValue(1));*/
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest1";
			Command.ExecuteNonQuery();

			

			
		}

		
		[Test]
		public void ArraysNumeric()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (n1 Numeric[10],n2 numeric[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{120.89809,1234.00090,2.2434,3123.0,42342.22,53552.2,652.233,7.09,8.11,9.654}','{132.654,897.2563}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());
					}*/
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{120.89809,1234.00090,2.2434,3123.0,42342.22,53552.2,652.233,7.09,8.11,9.654}",Reader.GetValue(0));
			Assert.AreEqual("{132.654,897.2563}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

			

			
		}


		[Test]
		public void ArraysNumericWithPrecision()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (n1 Numeric(5,2)[10],n2 Numeric(4,3)[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{120.89,123.90,22.334,412.40,422.22,552.21,62.22,712.09,18.11,91.65}','{1.234,2.142}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

//			while(Reader.Read())
//					{
//						Console.WriteLine(Reader.GetValue(0).ToString());
//						Console.WriteLine(Reader.GetValue(1).ToString());
//					}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{120.89,123.90,22.33,412.40,422.22,552.21,62.22,712.09,18.11,91.65}",Reader.GetValue(0));
			Assert.AreEqual("{1.234,2.142}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysSmallInt()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (i smallint[10],j smallint[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{-1,-2,-3,-4,0,4,5,6,7,8}','{40,50,60,70,81,90,32765}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			//			while(Reader.Read())
			//					{
			//						Console.WriteLine(Reader.GetValue(0).ToString());
			//						Console.WriteLine(Reader.GetValue(1).ToString());
			//					}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{-1,-2,-3,-4,0,4,5,6,7,8}",Reader.GetValue(0));
			Assert.AreEqual("{40,50,60,70,81,90,32765}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysBigInt()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (i bigint[10],j bigint[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{-100,-200,-300,-4000,-922337203685477,50000,6000,7000,8000,9000}','{-9223372036854775808,9223372036854775807}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			//			while(Reader.Read())
			//					{
			//						Console.WriteLine(Reader.GetValue(0).ToString());
			//						Console.WriteLine(Reader.GetValue(1).ToString());
			//					}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{-100,-200,-300,-4000,-922337203685477,50000,6000,7000,8000,9000}",Reader.GetValue(0));
			Assert.AreEqual("{-9223372036854775808,9223372036854775807}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysDoublePrecision()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (d1 double precision[3],d2 double precision[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{122.323423453,230.32131231322,123342.2323324}','{555.43534543233,344654.34534439785}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

//						while(Reader.Read())
//								{
//									Console.WriteLine(Reader.GetValue(0).ToString());
//									Console.WriteLine(Reader.GetValue(1).ToString());
//								}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{122.323423453,230.32131231322,123342.2323324}",Reader.GetValue(0));
			Assert.AreEqual("{555.43534543233,344654.345344398}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysInteger()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (i integer[],j integer[2]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{-2147483648,2147483647}','{5,9}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			//						while(Reader.Read())
			//								{
			//									Console.WriteLine(Reader.GetValue(0).ToString());
			//									Console.WriteLine(Reader.GetValue(1).ToString());
			//								}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{-2147483648,2147483647}",Reader.GetValue(0));
			Assert.AreEqual("{5,9}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}

		
		[Test]
		public void ArraysNumber()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (n1 Number[5],n2 Number[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{321.255,654.233,8987,545.23,654.36}','{31.2434,23.1442}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

			//						while(Reader.Read())
			//								{
			//									Console.WriteLine(Reader.GetValue(0).ToString());
			//									Console.WriteLine(Reader.GetValue(1).ToString());
			//								}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{321.255,654.233,8987,545.23,654.36}",Reader.GetValue(0));
			Assert.AreEqual("{31.2434,23.1442}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysDecimal()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (n1 Decimal(5,2)[10],n2 Decimal(4,3)[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{120.89,123.90,22.334,412.40,422.22,552.21,62.22,712.09,18.11,91.65}','{1.234,2.142}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

//									while(Reader.Read())
//											{
//												Console.WriteLine(Reader.GetValue(0).ToString());
//												Console.WriteLine(Reader.GetValue(1).ToString());
//											}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{120.89,123.90,22.33,412.40,422.22,552.21,62.22,712.09,18.11,91.65}",Reader.GetValue(0));
			Assert.AreEqual("{1.234,2.142}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysMoney()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (m1 money[],m2 money[2]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{-21474823123326.4128,2123432474836.247}','{2343245.571,523432.3226}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

//												while(Reader.Read())
//														{
//															Console.WriteLine(Reader.GetValue(0).ToString());
//															Console.WriteLine(Reader.GetValue(1).ToString());
//														}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{-21474823123326.4128,2123432474836.2470}",Reader.GetValue(0));
			Assert.AreEqual("{2343245.5710,523432.3226}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysSmallMoney()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (m1 smallmoney[],m2 smallmoney[2]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{-474836.4128,74836.2417}','{45.1157,15.2636}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();

//															while(Reader.Read())
//																	{
//																		Console.WriteLine(Reader.GetValue(0).ToString());
//																		Console.WriteLine(Reader.GetValue(1).ToString());
//																	}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{-474836.4128,74836.2417}",Reader.GetValue(0));
			Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest";
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
			
			Command.CommandText="SELECT * FROM  books;";
			EDBDataReader Reader = Command.ExecuteReader();

//																		while(Reader.Read())
//																				{
//																					Console.WriteLine(Reader.GetValue(0).ToString());
//																					Console.WriteLine(Reader.GetValue(1).ToString());
//																				}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{\"Lord of the Rings\",Suffocles}",Reader.GetValue(0));
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
			
			Command.CommandText="SELECT * FROM  favourite_books;";
			EDBDataReader Reader = Command.ExecuteReader();

//			while(Reader.Read())
//			{
//				Console.WriteLine(Reader.GetValue(0).ToString());
//				Console.WriteLine(Reader.GetValue(1).ToString());
//			}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{\"The Hitchhikers Guide to the Galaxy\",\"Harry Potter\",Kitten,Squared}",Reader.GetValue(0));
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
			
			Command.CommandText="SELECT * FROM  books;";
			EDBDataReader Reader = Command.ExecuteReader();

			//			while(Reader.Read())
			//			{
			//				Console.WriteLine(Reader.GetValue(0).ToString());
			//				Console.WriteLine(Reader.GetValue(1).ToString());
			//			}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{\"Lord of the Rings\",Suffocles}",Reader.GetValue(0));
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
			
			Command.CommandText="SELECT * FROM  favourite_books;";
			EDBDataReader Reader = Command.ExecuteReader();

//						while(Reader.Read())
//						{
//							Console.WriteLine(Reader.GetValue(0).ToString());
//							//Console.WriteLine(Reader.GetValue(1).ToString());
//						}
			Assert.IsTrue(Reader.Read());
			
			Assert.AreEqual("{\"The Hitchikers Guide to the Galaxy\",\"Harry Potter\",Kitten,Squared}",Reader.GetValue(0));
			//	Assert.AreEqual("{45.1157,15.2636}",Reader.GetValue(1));
			Reader.Close();
			Command.CommandText="DROP TABLE  favourite_books";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysCharacter()
		{
			
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
			Assert.AreEqual("{\"1st char  \",\"sec char  \"}",Reader.GetValue(0));
			
			Reader.Close();
			Command.CommandText="DROP TABLE chartest";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysChar()
		{
			
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
			Assert.AreEqual("{\"1st char\",\"sec char\"}",Reader.GetValue(0));
			
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
			Assert.AreEqual("{\"Lord of the War\",Suffocles}",Reader.GetValue(0));
			
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
			
			Command.CommandText="SELECT * FROM books;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{\"Lord of the War\",Suffocles,\"A walk in the cloudsssss\"}",Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE books";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysDate()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (d1 Date[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{040506,101203}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{\"2004-05-06 00:00:00\",\"2010-12-03 00:00:00\"}",Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysTimestamp()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (t Timestamp[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{1999-01-08 04:05:06,December 11 04:05:06 2006}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{\"1999-01-08 04:05:06\",\"2006-12-11 04:05:06\"}",Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysDateTime()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (t DATETIME[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{1999-01-08 04:05:06,December 11 04:05:06 2006}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{\"1999-01-08 04:05:06\",\"2006-12-11 04:05:06\"}",Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysTime()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (t TIME[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{04:05:06,12:10:48 }');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{04:05:06,12:10:48}",Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest;";
			Command.ExecuteNonQuery();

		}


		[Test]
		public void ArraysBoolean()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (t boolean[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{t,f,t,t,t,f,f,f,f,f,f,f }');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{t,f,t,t,t,f,f,f,f,f,f,f}",Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest;";
			Command.ExecuteNonQuery();

		}

		[Test]
		public void ArraysBool()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (t bool[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{true,false,false,false,true,false,false,true }');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//															while(Reader.Read())
			//															{
			//																Console.WriteLine(Reader.GetValue(0).ToString());
			//																//Console.WriteLine(Reader.GetValue(1).ToString());
			//															}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{t,f,f,f,t,f,f,t}",Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest;";
			Command.ExecuteNonQuery();

		}

		
		[Test]
		public void ArraysBool2()
		{
			
			EDBCommand Command = new EDBCommand("",con);
				
			Command.CommandText="CREATE TABLE arrtest (t bool[]);";
			Command.ExecuteNonQuery();

			

			Command.CommandText="INSERT INTO arrtest VALUES ('{0,1,1,0,0,0,1,1,1,1,1,0}');";
			Command.ExecuteNonQuery();
			
			Command.CommandText="SELECT * FROM arrtest;";
			EDBDataReader Reader = Command.ExecuteReader();
			
			//	while(Reader.Read())
			//	{
			//		Console.WriteLine(Reader.GetValue(0).ToString());
			//		Console.WriteLine(Reader.GetValue(1).ToString());
			//	}
			Assert.IsTrue(Reader.Read());
			Assert.AreEqual("{f,t,t,f,f,f,t,t,t,t,t,f}",Reader.GetValue(0));
			//Console.WriteLine(Reader.GetValue(0).ToString());
			
			Reader.Close();
			Command.CommandText="DROP TABLE arrtest;";
			Command.ExecuteNonQuery();

		}


		


	}
}
