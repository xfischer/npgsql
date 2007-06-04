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
		private ADOCOM.Connection Conn=null;
		private string DBConnection = "Provider=MSDASQL.1;Persist Security Inf o=False;Data Source=EnterpriseDB";
			
		[SetUp]
		protected void SetUp()
		{ 
			Conn=new ADOCOM.Connection();
			Conn.Open(DBConnection,"buildfarm","edb",(int)ADOCOM.ConnectModeEnum.adModeUnknown);
		}	

		[TearDown]
		protected void TearDown()
		{
			//Conn.Close();  			
		}


		[Test]
		public void ADOConnectionTest()
		{
		
			
			Assert.AreEqual(1,Conn.State);
			Conn.Close();
			Assert.AreEqual(0,Conn.State);
		
		}

		[Test]
		public void ADOConnectionPropertiesTest ()
		{
			
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

			Conn.Close();
			Conn.ConnectionTimeout=30;
			Conn.Open(DBConnection,"buildfarm","edb",(int)ADOCOM.ConnectModeEnum.adModeUnknown);
			Assert.AreEqual(30,Conn.ConnectionTimeout);			
			Conn.Close();
			Assert.AreEqual(0,Conn.State);

		}

		[Test]
		public void ADODDLTest()
		{
			
			string SQL = "create table test(a int, b varchar)";
			string DropSql="drop table test";
		
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
			
			string SQL = "create table test(a int, b varchar)";
			string DropSql="drop table test";
		
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
			
			string SQL = "create table test(a int, b varchar)";
			string DropSql="drop table test";
		
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
		
		[Test]
		public void ADODDLTestWithStringVariables()
		{
			string TableName="test";
			string SQL = "create table\t"+TableName+"(a int, b varchar)";
			string DropSql="drop table\t"+TableName;
		
			object RecordsAffected=null;
			Conn.Execute(SQL,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute(DropSql,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Close();
		}
		
		[Test]
		public void ADODMLTestWithStringVariables()
		{
			string TableName="test";
			string SQL = "create table\t"+TableName+"(a int, b varchar)";
			string DropSql="drop table\t"+TableName;
					
			object RecordsAffected=null;
			Conn.Execute(SQL,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			string Input="\'a\'";
			Conn.Execute("insert into test values(1,"+Input+")",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,"+Input+")",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,"+Input+")",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,"+Input+")",out RecordsAffected,-1);
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
		public void ADODMLTestTwoWithStringVariables()
		{
			
			string TableName="test";
			string SQL = "create table\t"+TableName+"(a int, b varchar)";
			string DropSql="drop table\t"+TableName;
					
			object RecordsAffected=null;
			Conn.Execute(SQL,out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			string Input="\'a\'";
			Conn.Execute("insert into test values(1,"+Input+")",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,"+Input+")",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,"+Input+")",out RecordsAffected,-1);
			Assert.AreEqual(0,Conn.Errors.Count);
			Conn.Execute("insert into test values(1,"+Input+")",out RecordsAffected,-1);
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
