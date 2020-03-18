using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Configuration;


namespace EnterpriseDB.EDBClient.Tests
{
	/// <summary>
	/// Summary description for BaseDataSourceTest.
	/// </summary>
	[TestFixture, /*Ignore("Fix open without pooling ")*/]
	public class EDBBaseDataSourceTest : TestBase
    {
		EDBConnection con = null;
        String conString = ConnectionString;
        [SetUp]
		public void Init()
		{
            con = new EDBConnection(conString);
			con.Open();
			/*TestUtil.createTempTable(con, "poolingtest", "id int4 not null primary key, name varchar(50)");
			EDBCommand Command = new EDBCommand("",con);
			

			Command.CommandText="INSERT INTO poolingtest VALUES (1, 'Test Row 1')";
			Command.ExecuteNonQuery();


			Command.CommandText="INSERT INTO poolingtest VALUES (2, 'Test Row 2')";
			Command.ExecuteNonQuery();
			*/
			TestUtil.closeDB(con);
		}


		[TearDown] 
		public void Dispose()
		{
			con = OpenConnection();
			TestUtil.dropTable(con, "poolingtest");
			TestUtil.closeDB(con);
		}

		[Test]
		public void testUseConnection()
		{
			try
			{
                con = new EDBConnection(conString);
				//con = new EDBConnection("Server={127.0.0.1};Trusted_Connection={Yes};Database={edb};");
				
				con.Open();
				
				TestUtil.createTempTable(con, "poolingtest", "id int4 not null primary key, name varchar(50)");
				EDBCommand Command = new EDBCommand("",con);
			

				Command.CommandText="INSERT INTO poolingtest VALUES (1, 'Test Row 1')";
				Command.ExecuteNonQuery();


				Command.CommandText="INSERT INTO poolingtest VALUES (2, 'Test Row 2')";
				Command.ExecuteNonQuery();
			
				
				
				Command.CommandText="SELECT COUNT(*) FROM poolingtest";

				EDBDataReader Reader=Command.ExecuteReader();

				if(Reader.Read())
				{
					int count=int.Parse( Reader.GetValue(0).ToString());
					//Console.WriteLine(count.ToString());
					if(Reader.Read())
					{
						Assert.Fail("Should only have one row in SELECT COUNT result set");
					}

					if(count!=2)
					{
						Assert.Fail("Count returned " + count + " expecting 2");
					}
				}
				else
				{
					Assert.Fail("Should have one row in SELECT COUNT result set");
				}
				
				Reader.Close();
				con.Close();
				
			}
			catch (EDBException exp)
			{
				Assert.Fail(exp.ToString());
			}
		}
			
		[Test]
		public void testDdlOverConnection()
		{
			try
			{
                con = new EDBConnection(conString);
				con.Open();
				Console.WriteLine(con.ConnectionString);
				
				TestUtil.createTempTable(con, "poolingtest", "id int4 not null primary key, name varchar(50)");
				con.Close();
			}
			catch (EDBException e)
			{
				Assert.Fail(e.ToString());
			}
		}

		[Test]
		public void testNotPooledConnection()
		{
			try
			{
				con = openDBwithoutPooling();
				string name = con.ToString();
				Console.WriteLine("con1=="+con.ToString());
				con.Close();
				con = openDBwithoutPooling();
				string name2 = con.ToString();
				
				Console.WriteLine("con2=="+con.ToString());
				con.Close();
				//Assert.IsTrue(!name.Equals(name2));
				//Assert.IsTrue(!name.Equals(name2));
			}
			catch (EDBException exp)
			{
				Assert.Fail(exp.ToString());
			}
		}
	}
}

