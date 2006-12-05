using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;

namespace NUnit
{
	/// <summary>
	/// This Class contains functions for unit testing of .Net Driver.D:\shared\EDBNunit\
	/// </summary>
	[TestFixture] 
	public class ConnectionTest
	{
		EDBConnection con = null;

		[SetUp]
		public void Init()
		{			
			con = TestUtil.openDB();
			Console.WriteLine(con.ConnectionString.ToString());
		}

		[Test]
		public void testConnecting()
		{
			try 
			{
				con = TestUtil.openDB();
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}

		[Test]
		public void testConnectingWithoutPooling()
		{
			try 
			{
				con = TestUtil.openDBwithoutPooling();
			}
			catch(EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}
		[Test]
		public void Open()
		{
			try
			{
				con.Open();
				//Assert.AreEqual("ConnectionOpen", ConnectionState.Open, con.State);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}


		}

		[Test]
		public void ChangeDatabase()
		{

			con.ChangeDatabase("template1");

			EDBCommand command = new EDBCommand("select current_database()", con);

			string result = (string)command.ExecuteScalar();
			Console.WriteLine(result);
			Assert.AreEqual("template1", result);

		}

		[Test]		
		public void NestedTransaction()
		{
			//con.Open();

			/*EDBTransaction t = null;
			try
			{
				t = con.BeginTransaction();

				t = con.BeginTransaction();
			}
			catch(EDBException e)
			{
				// Catch exception so we call rollback the transaction initiated.
				// This way, the connection pool doesn't get a connection with a transaction
				// started.
				t.Rollback();
				//throw e;
			}*/

		}

		[Test]
		public void SequencialTransaction()
		{
			/*con.Open();

			EDBTransaction t = con.BeginTransaction();

			t.Rollback();

			t = con.BeginTransaction();

			t.Rollback();*/
			


		}

		[TearDown] 
		public void Dispose()
		{
			TestUtil.closeDB(con);
		}

		//Haroon
		[Test]
		public void TestEDBCommandStatement() 
		{
			
			EDBCommand Command=new EDBCommand("",con);	
			Assert.IsNotNull(Command);
			Command.Dispose();
		
			//Ask for Updateable ResultSets
		}

		
		[Test]
		public void TestIsClosed()
		{
		 EDBConnection Con = TestUtil.openDB();

		// Should not say closed
			Console.WriteLine(Con.State.ToString());

		Assert.AreEqual("OPEN",Con.State.ToString().ToUpper());

		TestUtil.closeDB(Con);
			Console.WriteLine(Con.State.ToString());

		// Should now say closed
		Assert.AreEqual("CLOSED",Con.State.ToString().ToUpper());
		}
		
		[Test]
		public void TestDoubleClose()
		{
			EDBConnection Con = TestUtil.openDB();
			Con.Close();
			Con.Close();
			
		}

	}
}
