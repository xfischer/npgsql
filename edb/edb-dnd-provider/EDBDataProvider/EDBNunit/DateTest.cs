using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;


namespace NUnit
{
	/// <summary>
	/// Summary description for DateTest.
	/// </summary>
	 
	[TestFixture]
	public class DateTest
	{
		EDBConnection con;
		bool testingSetDate = false;


		[SetUp]
		public void Init()
		{			
			con = TestUtil.openDB();
			TestUtil.createTempTable(con, "testdate", "dt date");
		}

		[TearDown] 
		public void Dispose()
		{
			TestUtil.dropTable(con, "testdate");
			TestUtil.closeDB(con);
		}
			
		[Test]
		public void testGetDate() 
		{

			EDBCommand Command = new EDBCommand("",con);
			

			 //Statement stmt = con.createStatement();
			Command.CommandText="insert into testdate values('1950-02-07')";
			int a=Command.ExecuteNonQuery();
			Assert.AreEqual(1,a);
			
			Command.CommandText="insert into testdate values('1970-06-02')";
			 a=Command.ExecuteNonQuery();
			Assert.AreEqual(1,a);

			Command.CommandText="insert into testdate values('1999-08-11')";
			 a=Command.ExecuteNonQuery();
			Assert.AreEqual(1,a);

			
			Command.CommandText="insert into testdate values('2001-02-13')";
			 a=Command.ExecuteNonQuery();
			Assert.AreEqual(1,a);
			
			Command.CommandText="insert into testdate values('1950-04-02')";
			 a=Command.ExecuteNonQuery();
			Assert.AreEqual(1,a);

			Command.CommandText="insert into testdate values('1934-02-28')";
			 a=Command.ExecuteNonQuery();
			Assert.AreEqual(1,a);

			
			Command.CommandText="DELETE FROM " + "testdate";
			 a=Command.ExecuteNonQuery();
			Assert.AreEqual(6,a);

	}

	}
}
