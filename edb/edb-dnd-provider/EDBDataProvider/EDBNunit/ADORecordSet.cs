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
		private ADOCOM.Connection Conn=null;
		private string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			
		[SetUp]
		protected void SetUp()
		{ 
			Conn=new ADOCOM.Connection();
			Conn.Open(DBConnection,"buildfarm","edb",-1); 
		}	

		[TearDown]
		protected void TearDown()
		{
			Conn.Close();  			
		}

		[Test]
		public void ADORecordSetTestOne()
		{
			
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			
			//create ADOCOM Recordset object
			ADOCOM.Recordset rs= new ADOCOM.Recordset();
			 
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockBatchOptimistic,1);       
			Assert.AreEqual("SMITH",rs.Fields[0].Value);
			rs.Close();

		}

		[Test]
		public void ADORecordSetReferenceTest()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADOCOM Recordset object
			ADOCOM.Recordset rs= new ADOCOM.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockBatchOptimistic,1);       

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
			
		}

		[Test]
		public void ADORecordSetUpdateTest()
		{
			// sql statment
			string SQL = "select ename,job,empno from emp where empno=7369;";
			//create ADOCOM Recordset object
			ADOCOM.Recordset rs= new ADOCOM.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockOptimistic,1);       

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

			rs.Update(1,"Salesman");
			
			rs.Close();

			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockOptimistic,1);  
     
			Assert.AreEqual("Salesman",rs.Fields["job"].Value.ToString());

			rs.Close();

			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockOptimistic,1);  
				
			rs.Update(1,"CLERK");
			rs.Close();
			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockOptimistic,1);  
			
			Assert.AreEqual("CLERK",rs.Fields["job"].Value.ToString());

			rs.Close();

			
		}

		[Test]
		public void ADORecordSetUpdateBatchOptimistic()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADOCOM Recordset object
			ADOCOM.Recordset rs= new ADOCOM.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockBatchOptimistic,1);       

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
			//update should not update field in the data source
			rs.Update(1,"New Job");
			
			
			rs.Close();
			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockBatchOptimistic,1);      
		
			Assert.AreEqual("CLERK",rs.Fields[1].Value.ToString());
			rs.Close();
		}

		[Test]
		public void ADORecordSetUpdateReadOnly()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADOCOM Recordset object
			ADOCOM.Recordset rs= new ADOCOM.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockReadOnly,1);       

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
			//update should not update field in the data source
			
			try
			{
				rs.Update(1,"New Job");
				Assert.Fail("Can't update a readonly recordset");
				rs.Close();
			}

			catch(Exception exp)
			{
				rs.Close();
				
			}
			
			
		}

		[Test]
		public void ADORecordSetUpdatePessimistic()
		{
			// sql statment
			string SQL = "select ename,job from emp where empno=7369;";
			//create ADOCOM Recordset object
			ADOCOM.Recordset rs= new ADOCOM.Recordset();
			   
			//execute the query specifying static sursor, batch optimistic locking
			rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockPessimistic,1);       

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
			//update should not update field in the data source
			
			try
			{
				rs.Update(1,"New Job");
				rs.Close();

				rs.Open(SQL,DBConnection,ADOCOM.CursorTypeEnum.adOpenStatic,ADOCOM.LockTypeEnum.adLockPessimistic,1);  
				rs.Update(1,"CLERK");
				rs.Close();
			}

			catch(Exception exp)
			{
				rs.Close();
				Console.WriteLine(exp.Message);
				Assert.Fail("Could not update");
				
			}
			
			
		}

	}
}
