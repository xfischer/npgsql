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
	public class ADOConnection
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
		public void ADOConnectionTest()
		{
			Conn=new ADODB.Connection();
			Conn.Open(DBConnection,"buildfarm","edb",(int)ADODB.ConnectModeEnum.adModeUnknown);
			Assert.AreEqual(1,Conn.State);
			Conn.Close();
			Assert.AreEqual(0,Conn.State);

		}

		[Test]
		public void ADOConnectionPropertiesTest ()
		{
			
			string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			ADODB.Connection Conn=new ADODB.Connection();
			Conn.Open(DBConnection,"buildfarm","edb",(int)ADODB.ConnectModeEnum.adModeUnknown);
			Assert.AreEqual(1,Conn.State);
			// ADO connection's 12th property represents the DBMS Name
			Assert.AreEqual("EnterpriseDB",Conn.Properties[11].Value.ToString());
			// ADO connection's 41st property represents the user Name
			Assert.AreEqual("buildfarm",Conn.Properties[40].Value.ToString());
			
			Conn.Close();
			Assert.AreEqual(0,Conn.State);

		}

		[Test]
		public void ADOConnectionTimeOutTest ()
		{
			
			string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			ADODB.Connection Conn=new ADODB.Connection();
			Conn.ConnectionTimeout=30;
			Conn.Open(DBConnection,"buildfarm","edb",(int)ADODB.ConnectModeEnum.adModeUnknown);
			Assert.AreEqual(30,Conn.ConnectionTimeout);			
			Conn.Close();
			Assert.AreEqual(0,Conn.State);

		}

		[Test]
		public void ADODDLTest()
		{
			string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			
			string SQL = "create table test(a int, b varchar)";
			string DropSql="drop table test";
		
			//create ADODB Connection object
			ADODB.Connection Conn=new ADODB.Connection();
			Conn.Open(DBConnection,"buildfarm","edb",(int)ADODB.ConnectModeEnum.adModeUnknown);
			object RecordsAffected=null;
			Conn.Execute(SQL,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute(DropSql,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Close();
		}
		
		[Test]
		public void ADODMLTest1()
		{
			string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			
			string SQL = "create table test(a int, b varchar)";
			string DropSql="drop table test";
		
			//create ADODB Connection object
			ADODB.Connection Conn=new ADODB.Connection();
			Conn.Open(DBConnection,"buildfarm","edb",(int)ADODB.ConnectModeEnum.adModeUnknown);
			object RecordsAffected=null;
			Conn.Execute(SQL,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,\'a\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,\'a\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,\'a\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,\'a\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(2,\'b\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("delete from test where a=1;",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Assert.AreEqual(4,RecordsAffected);
			Conn.Execute(DropSql,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Close();
		}

		[Test]
		public void ADODMLTest2()
		{
			string DBConnection = "Provider=MSDASQL.1;Persist Security Info=False;Data Source=EnterpriseDB";
			
			string SQL = "create table test(a int, b varchar)";
			string DropSql="drop table test";
		
			//create ADODB Connection object
			ADODB.Connection Conn=new ADODB.Connection();
			Conn.Open(DBConnection,"buildfarm","edb",(int)ADODB.ConnectModeEnum.adModeUnknown);
			object RecordsAffected=null;
			Conn.Execute(SQL,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,\'a\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,\'a\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,\'a\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,\'a\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(2,\'b\')",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("update test set b=\'c\' where a=1;",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Assert.AreEqual(4,RecordsAffected);
			Conn.Execute(DropSql,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Close();
		}


	}
}
