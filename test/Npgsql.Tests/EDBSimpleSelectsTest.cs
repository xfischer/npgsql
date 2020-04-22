using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;


namespace EnterpriseDB.EDBClient.Tests
{
	/// <summary>
	/// A couple of tests for Simple select statements
	/// </summary>
	/// 
	[TestFixture]
	public class EDBSimpleSelectsTest : TestBase
    {
		EDBConnection? con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();
		}

		[Test]
		public void testSingleRecord()
		{
			EDBCommand cmdSelect = new EDBCommand("SELECT * FROM dept where deptno=10",con); 
			cmdSelect.CommandType = CommandType.Text;
			EDBDataReader drDept = cmdSelect.ExecuteReader(); 

			Assert.IsTrue(drDept.Read(),"No data returned from Select");
		}

		[Test]
		public void testWholeTable()
		{
			EDBCommand cmdSelect = new EDBCommand("SELECT * FROM dept",con); 
			cmdSelect.CommandType = CommandType.Text;
			EDBDataReader drDept = cmdSelect.ExecuteReader(); 

			Assert.IsTrue(drDept.Read(),"No data returned from Select");
		}

		[Test]
		public void testTwoTables()
		{
			EDBCommand cmdSelect = new EDBCommand("SELECT * FROM dept,emp",con); 
			cmdSelect.CommandType = CommandType.Text;
			EDBDataReader drDept = cmdSelect.ExecuteReader(); 

			Assert.IsTrue(drDept.Read(),"No data returned from Select");
		}

		[Test]
		public void testView()
		{
			EDBCommand cmdSelect = new EDBCommand("SELECT * FROM salesemp",con); 
			cmdSelect.CommandType = CommandType.Text;
			EDBDataReader drDept = cmdSelect.ExecuteReader(); 

			Assert.IsTrue(drDept.Read(),"No data returned from Select");
		}

		//------------------------------------------17AUG06---------------
		[Test]
		public void testCount()
		{
			EDBCommand cmdSelect = new EDBCommand("SELECT count(*) FROM emp",con); 
			cmdSelect.CommandType = CommandType.Text;
			EDBDataReader drDept = cmdSelect.ExecuteReader(); 

			Assert.IsTrue(drDept.Read(),"14");
		}
		
		[Test]
		public void testCountWithFiled()
		{
			EDBCommand cmdSelect = new EDBCommand("SELECT count(empno) FROM emp",con); 
			cmdSelect.CommandType = CommandType.Text;
			EDBDataReader drDept = cmdSelect.ExecuteReader(); 

			Assert.IsTrue(drDept.Read(),"14");
		}

		[Test]
		public void testCountMax()
		{
			EDBCommand cmdSelect = new EDBCommand("SELECT MAX(sal) FROM emp", con);
			cmdSelect.CommandType = CommandType.Text;
			EDBDataReader drDept = cmdSelect.ExecuteReader();

			Assert.IsTrue(drDept.Read(), "14");
		}

		[TearDown] 
		public void Dispose()
		{
			TestUtil.closeDB(con);
		}
	}
}
