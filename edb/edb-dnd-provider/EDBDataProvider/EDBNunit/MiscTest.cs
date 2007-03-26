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
	

	}
}
