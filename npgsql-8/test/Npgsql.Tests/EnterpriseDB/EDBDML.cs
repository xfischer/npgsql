using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
	/// <summary>
	/// This Class contains functions for unit testing of .Net Driver.
	/// </summary>
	[TestFixture]
    [NonParallelizable]
    public class EDBDMLTest : EPASTestBase
    {
		EDBConnection? con = null;

        [SetUp]
		public void Init()
		{
			con = OpenConnection();
            TestUtil.createTempTable(con, "dml_TestTable1",
										"RecNo int, Name varchar(20)");
			TestUtil.createTempTable(con, "dml_TestTable2",
										"RecNo int, Name varchar(20)");
		}
		
		[Test]
		public void testInsert()
		{
			try 
			{
				string strInsertSql = "Insert INTO dml_TestTable1(RecNo,Name) Values (1234, 'EDB')";
				EDBCommand cmdInsert = new EDBCommand(strInsertSql,con);
				cmdInsert.ExecuteNonQuery();
			}
			catch(EDBException e)
			{
				throw new Exception("\nInsertion Incomplete!\n" + e.ToString());
			}
		}
		
		[Test]
		public void testUpdate()
		{
			try 
			{
				string strUpdateSql = "Update dml_TestTable1 SET Name = 'not-EDB' where RecNo = 1234";
				EDBCommand cmdUpdate = new EDBCommand(strUpdateSql,con);
				cmdUpdate.ExecuteNonQuery();
			}
			catch(EDBException e)
			{
				throw new Exception("\nUpdation Incomplete!\n" + e.ToString());
			}
		}
		[Test]
		public void testDelete()
		{
			try 
			{
				string strDeleteSql = "Delete FROM dml_TestTable1 WHERE RecNo=1234";
				EDBCommand cmdDelete = new EDBCommand(strDeleteSql,con);
				cmdDelete.ExecuteNonQuery();
			}
			catch(EDBException e)
			{
				throw new Exception("\nDeletion Incomplete!\n" + e.ToString());
			}
		}
		[TearDown] 
		public void Dispose()
		{
			TestUtil.dropTable(con,"dml_TestTable1");
			TestUtil.dropTable(con,"dml_TestTable2");
			TestUtil.closeDB(con);
		}
	}
}
