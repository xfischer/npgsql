using System;
using NUnit.Framework;
using System.Data;
using System.Globalization;
using EDBTypes;
using EnterpriseDB.EDBClient;
using System.Net;

namespace ADO
{
	/// <summary>
	/// Summary description for ADOConnectionTest.
	/// </summary>
	
	[TestFixture]
	public class ADORecordSet
	{
		private ADODB.Connection Conn=null;
		private string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			
		[SetUp]
		protected void SetUp()
		{ 
			
		}	

		[TearDown]
		protected void TearDown()
		{
			
		}

		[Test]
		public void ADORecordSetTestOne()
		{
			string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Connection object
			ADODB.Connection Conn=new ADODB.Connection();
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			Conn.Open(DBConnection,"buildfarm","edb",-1);  
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       
			Assert.AreEqual("SMITH",rs.Fields[0].Value);
			rs.Close();
			Conn.Close();  
		}

		[Test]
		public void ADORecordSetReferenceTest()
		{
			string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADODB Connection object
			ADODB.Connection Conn=new ADODB.Connection();
			//create ADODB Recordset object
			ADODB.Recordset rs= new ADODB.Recordset();
			Conn.Open(DBConnection,"buildfarm","edb",-1);  
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADODB.CursorTypeEnum.adOpenStatic,ADODB.LockTypeEnum.adLockBatchOptimistic,1);       

			//reference the ename field by column index
			string ENameByIndex=rs.Fields[0].Value.ToString();
			
			//reference the ename field by column name
			string ENameByName=rs.Fields["ename"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("SMITH",ENameByName);
			
			//reference the job field by column index
			string JobByIndex=rs.Fields[1].Value.ToString();
			//reference the job field by column name
			string JobByName=rs.Fields["job"].Value.ToString();
			Assert.AreEqual(ENameByIndex,ENameByName);
			Assert.AreEqual("CLERK",JobByIndex);
		
			
			rs.Close();
			Conn.Close();  
		}
	}
}
