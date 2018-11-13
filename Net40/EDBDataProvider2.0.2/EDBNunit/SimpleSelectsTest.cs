using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;


namespace DOTNET
{
	/// <summary>
	/// A couple of tests for Simple select statements
	/// </summary>
	/// 
	[TestFixture]
	public class SimpleSelectsTest
	{
		EDBConnection con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = TestUtil.openDB();

		}
		[Test]
		public void testSingleRecord()
		{
			/*try 
			{

				EDBCommand cmdSelect = new EDBCommand("SELECT * FROM dept where deptno=10",con); 
				cmdSelect.CommandType = CommandType.Text;
				EDBDataReader drDept = cmdSelect.ExecuteReader(); 
                      
				Assert.IsTrue(drDept.Read(),"No data returned from Select");
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}*/
		}
		[Test]
		public void testWholeTable()
		{
			/*try 
			{
				EDBCommand cmdSelect = new EDBCommand("SELECT * FROM dept",con); 
				cmdSelect.CommandType = CommandType.Text;
				EDBDataReader drDept = cmdSelect.ExecuteReader(); 
                      
				Assert.IsTrue(drDept.Read(),"No data returned from Select");
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}*/
		}
		[Test]
		public void testTwoTables()
		{
			/*try 
			{
				EDBCommand cmdSelect = new EDBCommand("SELECT * FROM dept,emp",con); 
				cmdSelect.CommandType = CommandType.Text;
				EDBDataReader drDept = cmdSelect.ExecuteReader(); 
                      
				Assert.IsTrue(drDept.Read(),"No data returned from Select");
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}*/
		}

		public void testView()
		{
			try 
			{
				EDBCommand cmdSelect = new EDBCommand("SELECT * FROM salesemp",con); 
				cmdSelect.CommandType = CommandType.Text;
				EDBDataReader drDept = cmdSelect.ExecuteReader(); 

				Assert.IsTrue(drDept.Read(),"No data returned from Select");
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}
		//------------------------------------------17AUG06---------------
		public void testCount()
		{
			try 
			{
				EDBCommand cmdSelect = new EDBCommand("SELECT count(*) FROM emp",con); 
				cmdSelect.CommandType = CommandType.Text;
				EDBDataReader drDept = cmdSelect.ExecuteReader(); 

				Assert.IsTrue(drDept.Read(),"14");
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}	
		

		public void testCountWithFiled()
		{
			try 
			{
				EDBCommand cmdSelect = new EDBCommand("SELECT count(empno) FROM emp",con); 
				cmdSelect.CommandType = CommandType.Text;
				EDBDataReader drDept = cmdSelect.ExecuteReader(); 

				Assert.IsTrue(drDept.Read(),"14");
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}
		
//		public void testCount()
//		{
//			try 
//			{
//				EDBCommand cmdSelect = new EDBCommand("SELECT MAX(sal) FROM emp",con); 
//				cmdSelect.CommandType = CommandType.Text;
//				EDBDataReader drDept = cmdSelect.ExecuteReader(); 
//
//				Assert.IsTrue(drDept.Read(),"14");
//			}
//			catch(EDBException e)
//			{
//				throw new Exception(e.ToString());
//			}
//		}


		[TearDown] 
		public void Dispose()
		{
			TestUtil.closeDB(con);
		}
	}
}
