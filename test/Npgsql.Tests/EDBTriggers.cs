using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;

namespace EnterpriseDB.EDBClient.Tests
{
	/// <summary>
	/// Summary description for Triggers.
	/// </summary>
	
	[TestFixture]
	public class EDBTriggers : TestBase
    {
		EDBConnection? con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = OpenConnection();
			

		}

		[TearDown] 
		public void Dispose()
		{
			
			
			TestUtil.closeDB(con);
		}
			
		
		[Test]
		public void TriggerBeforeStatementLevel()
		{
			string Sourcestr = "create table SourceTable2( cint int);";
			string Destinationstr = "create table DestinationTable2( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger2 BEFORE INSERT ON SourceTable2 BEGIN insert into DestinationTable2 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable2";
			string DropDestinationstr = "drop table DestinationTable2;";
			string DropTrigger = "drop trigger TestTrigger2;";
	
			string InsertSql ="insert into SourceTable2 values (5);";
			string SelectSql ="select * from DestinationTable2";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}
		

		[Test]
		public void TriggerBefore_ExecuteBeforeStatementLevel()
		{
			string Sourcestr = "create table SourceTable0( cint int not null primary key);";
			string Destinationstr = "create table DestinationTable0( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger0 BEFORE INSERT ON SourceTable0 BEGIN insert into DestinationTable0 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable0";
			string DropDestinationstr = "drop table DestinationTable0;";
			string DropTrigger = "drop trigger TestTrigger0;";
	
			string InsertSql ="insert into SourceTable0 values (5);";
			string InsertSql1 ="insert into SourceTable0 values (5);";

			string SelectSql ="select * from DestinationTable0";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			
			try
			{
				Command.CommandText=InsertSql1;
				Command.ExecuteNonQuery();
				Assert.Fail("should not insert duplicate values");
			}

			catch(EDBException )
			{
				Console.WriteLine("raising exception");;
			}

			finally
			{
				
				Command.CommandText=SelectSql;
				EDBDataReader Reader=Command.ExecuteReader();
				Console.WriteLine("values read");
				Reader.Read();

				Assert.AreEqual("1",Reader.GetValue(0).ToString());
                Reader.Close();
				Command.CommandText=DropTrigger;
				Command.ExecuteNonQuery();
				Command.CommandText=DropSourcestr;
				Command.ExecuteNonQuery();
				Console.WriteLine("source table dropped");
				Command.CommandText=DropDestinationstr;
				Command.ExecuteNonQuery();
				Console.WriteLine("destination table dropped");
			}


		}
			

			
		[Test]
		public void TriggerAfterStatementLevel()
		{
			string Sourcestr = "create table SourceTable1( cint int);";
			string Destinationstr = "create table DestinationTable1( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger1 AFTER INSERT ON SourceTable1 BEGIN insert into DestinationTable1 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable1";
			string DropDestinationstr = "drop table DestinationTable1;";
			string DropTrigger = "drop trigger TestTrigger1;";
	
			string InsertSql ="insert into SourceTable1 values (5);";
			string SelectSql ="select * from DestinationTable1";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}
			
		[Test]
		public void TriggerAfter_ExecuteAfterStatementLevel()
		{
			string Sourcestr = "create table SourceTable3( cint int not null primary key);";
			string Destinationstr = "create table DestinationTable3( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger3 AFTER INSERT ON SourceTable3 BEGIN insert into DestinationTable3 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable3";
			string DropDestinationstr = "drop table DestinationTable3;";
			string DropTrigger = "drop trigger TestTrigger3;";
	
			string InsertSql ="insert into SourceTable3 values (5);";
			string InsertSql1 ="insert into SourceTable3 values (5);";

			string SelectSql ="select * from DestinationTable3";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			
			try
			{
				Command.CommandText=InsertSql1;
				Command.ExecuteNonQuery();
				Assert.Fail("should not insert duplicate values");
			}

			catch(EDBException )
			{
				Console.WriteLine("raising exception");;
			}

			finally
			{
				
				Command.CommandText=SelectSql;
				EDBDataReader Reader=Command.ExecuteReader();
				Console.WriteLine("values read");
				Reader.Read();

				Assert.AreEqual("1",Reader.GetValue(0).ToString());
                Reader.Close();
				Command.CommandText=DropTrigger;
				Command.ExecuteNonQuery();
				Command.CommandText=DropSourcestr;
				Command.ExecuteNonQuery();
				Console.WriteLine("source table dropped");
				Command.CommandText=DropDestinationstr;
				Command.ExecuteNonQuery();
				Console.WriteLine("destination table dropped");
			}


		}
			
		
		[Test]
		public void TriggerBeforeInsertRowLevel()
		{
			string Sourcestr = "create table SourceTable4( cint int);";
			string Destinationstr = "create table DestinationTable4( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger4 BEFORE INSERT ON SourceTable4 FOR EACH ROW BEGIN insert into DestinationTable4 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable4";
			string DropDestinationstr = "drop table DestinationTable4;";
			string DropTrigger = "drop trigger TestTrigger4;";
	
			string InsertSql ="insert into SourceTable4 values (5);";
			string SelectSql ="select * from DestinationTable4";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			
			while(Reader.Read())
			{
				Assert.AreEqual("1",Reader.GetValue(0).ToString());
				Console.WriteLine("values read");
			}
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}
		
		

		[Test]
		public void TriggerAfterInsertRowLevel()
		{
			string Sourcestr = "create table SourceTable5( cint int);";
			string Destinationstr = "create table DestinationTable5( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger5 AFTER INSERT ON SourceTable5 FOR EACH ROW BEGIN insert into DestinationTable5 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable5";
			string DropDestinationstr = "drop table DestinationTable5;";
			string DropTrigger = "drop trigger TestTrigger5;";
	
			string InsertSql ="insert into SourceTable5 values (5);";
			string SelectSql ="select * from DestinationTable5";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			
			while(Reader.Read())
			{
				Assert.AreEqual("1",Reader.GetValue(0).ToString());
				Console.WriteLine("values read");
			}
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}
		
		[Test]
		public void TriggerBeforeDeleteStatementLevel()
		{
			string Sourcestr = "create table SourceTable6( cint int);";
			string Destinationstr = "create table DestinationTable6( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger6 BEFORE DELETE ON SourceTable6 BEGIN insert into DestinationTable6 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable6";
			string DropDestinationstr = "drop table DestinationTable6;";
			string DropTrigger = "drop trigger TestTrigger6;";
	
			string InsertSql ="insert into SourceTable6 values (5);";
			string DeleteSql ="Delete from SourceTable6";
			
			string SelectSql ="select * from DestinationTable6";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}
			
		[Test]
		public void TriggerBeforeDeleteRowLevel()
		{
			string Sourcestr = "create table SourceTable7( cint int,dint int);";
			string Destinationstr = "create table DestinationTable7( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger7 BEFORE DELETE ON SourceTable7 FOR EACH ROW BEGIN insert into DestinationTable7 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable7";
			string DropDestinationstr = "drop table DestinationTable7;";
			string DropTrigger = "drop trigger TestTrigger7;";
	
			string InsertSql1 ="insert into SourceTable7 values (5,10);";
			string InsertSql2 ="insert into SourceTable7 values (20,30);";
			string InsertSql3 ="insert into SourceTable7 values (20,60);";
			string DeleteSql ="Delete from SourceTable7 where cint=20";
			
			string SelectSql ="select * from DestinationTable7";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql1;
			Command.ExecuteNonQuery();
			Command.CommandText=InsertSql2;
			Command.ExecuteNonQuery();
			Command.CommandText=InsertSql3;
			Command.ExecuteNonQuery();
			
			Console.WriteLine("value inserted"); 
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			while(Reader.Read())
			{
				Assert.AreEqual("1",Reader.GetValue(0).ToString());
				Console.WriteLine("values read");
			}
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}



		[Test]
		public void TriggerAfterDeleteStatementLevel()
		{
			string Sourcestr = "create table SourceTable8( cint int);";
			string Destinationstr = "create table DestinationTable8( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger8 AFTER DELETE ON SourceTable8 BEGIN insert into DestinationTable8 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable8";
			string DropDestinationstr = "drop table DestinationTable8;";
			string DropTrigger = "drop trigger TestTrigger8;";
	
			string InsertSql ="insert into SourceTable8 values (5);";
			string DeleteSql ="Delete from SourceTable8";
			
			string SelectSql ="select * from DestinationTable8";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}
			

		[Test]
		public void TriggerAfterDeleteRowLevel()
		{
			string Sourcestr = "create table SourceTable9( cint int,dint int);";
			string Destinationstr = "create table DestinationTable9( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger9 AFTER DELETE ON SourceTable9 FOR EACH ROW BEGIN insert into DestinationTable9 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable9";
			string DropDestinationstr = "drop table DestinationTable9;";
			string DropTrigger = "drop trigger TestTrigger9;";
	
			string InsertSql1 ="insert into SourceTable9 values (5,10);";
			string InsertSql2 ="insert into SourceTable9 values (20,30);";
			string InsertSql3 ="insert into SourceTable9 values (20,60);";
			string DeleteSql ="Delete from SourceTable9 where cint=20";
			
			string SelectSql ="select * from DestinationTable9";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql1;
			Command.ExecuteNonQuery();
			Command.CommandText=InsertSql2;
			Command.ExecuteNonQuery();
			Command.CommandText=InsertSql3;
			Command.ExecuteNonQuery();
			
			Console.WriteLine("value inserted"); 
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			while(Reader.Read())
			{
				Assert.AreEqual("1",Reader.GetValue(0).ToString());
				Console.WriteLine("values read");
			}
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}



		[Test]
		public void TriggerBeforeUpdateStatementLevel()
		{
			string Sourcestr = "create table SourceTable10( cint int,dint int);";
			string Destinationstr = "create table DestinationTable10( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger10 BEFORE UPDATE ON SourceTable10 BEGIN insert into DestinationTable10 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable10";
			string DropDestinationstr = "drop table DestinationTable10;";
			string DropTrigger = "drop trigger TestTrigger10;";
	
			string InsertSql ="insert into SourceTable10 values (5);";
			string DeleteSql ="UPDATE SourceTable10 SET dint=100 where cint=20";
			
			string SelectSql ="select * from DestinationTable10";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}
		
	
		[Test]
		public void TriggerAfterUpdateStatementLevel()
		{
			string Sourcestr = "create table SourceTable11( cint int,dint int);";
			string Destinationstr = "create table DestinationTable11( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger11 AFTER UPDATE ON SourceTable11 BEGIN insert into DestinationTable11 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable11";
			string DropDestinationstr = "drop table DestinationTable11;";
			string DropTrigger = "drop trigger TestTrigger11;";
	
			string InsertSql ="insert into SourceTable11 values (5);";
			string DeleteSql ="UPDATE SourceTable11 SET dint=100 where cint=20";
			
			string SelectSql ="select * from DestinationTable11";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}


		[Test]
		public void TriggerBeforeUpdateRowLevel()
		{
			string Sourcestr = "create table SourceTable10( cint int,dint int);";
			string Destinationstr = "create table DestinationTable10( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger10 BEFORE UPDATE ON SourceTable10 FOR EACH ROW BEGIN insert into DestinationTable10 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable10";
			string DropDestinationstr = "drop table DestinationTable10;";
			string DropTrigger = "drop trigger TestTrigger10;";
	
			string InsertSql ="insert into SourceTable10 values (5,10);";
			string DeleteSql ="UPDATE SourceTable10 SET dint=100 where cint=5";
			
			string SelectSql ="select * from DestinationTable10";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted");
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value updated");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Assert.IsFalse(Reader.Read());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}

		[Test]
		public void TriggerAfterUpdateRowLevel()
		{
			string Sourcestr = "create table SourceTable10( cint int,dint int);";
			string Destinationstr = "create table DestinationTable10( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger10 AFTER UPDATE ON SourceTable10 FOR EACH ROW BEGIN insert into DestinationTable10 values (1); END;";
	
			string DropSourcestr = "drop table SourceTable10";
			string DropDestinationstr = "drop table DestinationTable10;";
			string DropTrigger = "drop trigger TestTrigger10;";
	
			string InsertSql ="insert into SourceTable10 values (5,10);";
			string DeleteSql ="UPDATE SourceTable10 SET dint=100 where cint=5";
			
			string SelectSql ="select * from DestinationTable10";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted");
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value updated");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Assert.IsFalse(Reader.Read());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}

		[Test]
		public void TriggerWithTriggerVariables_NEW_OLD()
		{
			string Sourcestr = "create table t4( cint int,dint int);";
			string Destinationstr = "create table d1( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger16 BEFORE  UPDATE  ON t4 FOR EACH ROW DECLARE v_action        int; BEGIN IF :NEW.dint=100 THEN v_action := OLD.dint; ELSIF :NEW.dint=20 THEN v_action := 1; END IF; INSERT INTO d1 VALUES (v_action); END;";
	
			string DropSourcestr = "drop table t4";
			string DropDestinationstr = "drop table d1;";
			string DropTrigger = "drop trigger TestTrigger16;";
	
			string InsertSql ="insert into t4 values (5,10);";
			string DeleteSql ="UPDATE t4 SET dint=100 where cint=5";
			
			string SelectSql ="select * from t4";


		EDBCommand Command=new EDBCommand("",con);
		Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted");
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value updated");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");

			/*while(Reader.Read())
				Console.WriteLine(Reader.GetValue(0).ToString());*/
			Reader.Read();

			Assert.AreEqual("5",Reader.GetValue(0).ToString());
			Reader.Read();

			Assert.AreEqual("5",Reader.GetValue(0).ToString());
			Assert.IsFalse(Reader.Read());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}


		[Test]
		public void TriggerWithTriggerVariables_Inserting_Updating_Deleting_StatementLevel()
		{
			string Sourcestr = "create table t4( cint int,dint int);";
			string Destinationstr = "create table d1( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger14 AFTER INSERT OR UPDATE OR DELETE ON t4 DECLARE v_action  int; BEGIN IF INSERTING THEN v_action := 1; ELSIF UPDATING THEN v_action := 2; ELSIF DELETING THEN v_action := 3; END IF; INSERT INTO d1 VALUES (v_action); END;";
	
			string DropSourcestr = "drop table t4";
			string DropDestinationstr = "drop table d1;";
			string DropTrigger = "drop trigger TestTrigger14;";
	
			string InsertSql ="insert into t4 values (5,10);";
			string DeleteSql ="UPDATE t4 SET dint=100 where cint=5";
			
			string SelectSql ="select * from d1";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
		
			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value updated");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Reader.Read();

			Assert.AreEqual("2",Reader.GetValue(0).ToString());
			Assert.IsFalse(Reader.Read());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}

		[Test]
		public void TriggerWithTriggerVariables_Inserting_Updating_Deleting_RowLevel()
		{
			string Sourcestr = "create table t4( cint int,dint int);";
			string Destinationstr = "create table d1( cint int);";
			string CreateTrigger ="CREATE OR REPLACE TRIGGER TestTrigger14 AFTER INSERT OR UPDATE OR DELETE ON t4 FOR EACH ROW DECLARE v_action  int; BEGIN IF INSERTING THEN v_action := 1; ELSIF UPDATING THEN v_action := 2; ELSIF DELETING THEN v_action := 3; END IF; INSERT INTO d1 VALUES (v_action); END;";
	
			string DropSourcestr = "drop table t4";
			string DropDestinationstr = "drop table d1;";
			string DropTrigger = "drop trigger TestTrigger14;";
	
			string InsertSql ="insert into t4 values (5,10);";
			string DeleteSql ="UPDATE t4 SET dint=100 where cint=5";
			
			string SelectSql ="select * from d1";


			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=Sourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table created");
			Command.CommandText=Destinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("Desti table created");
			Command.CommandText=CreateTrigger;
			Command.ExecuteNonQuery();
			Console.WriteLine("trigger created");
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
		
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 
			Command.CommandText=InsertSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value inserted"); 

			Command.CommandText=DeleteSql;
			Command.ExecuteNonQuery();
			Console.WriteLine("value updated");
			Command.CommandText=SelectSql;
			EDBDataReader Reader=Command.ExecuteReader();
			Console.WriteLine("values read");
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Reader.Read();

			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Reader.Read();

			Assert.AreEqual("2",Reader.GetValue(0).ToString());

			Reader.Read();

			Assert.AreEqual("2",Reader.GetValue(0).ToString());
			Reader.Read();

			Assert.AreEqual("2",Reader.GetValue(0).ToString());
			Assert.IsFalse(Reader.Read());
            Reader.Close();
			Command.CommandText=DropTrigger;
			Command.ExecuteNonQuery();
			Command.CommandText=DropSourcestr;
			Command.ExecuteNonQuery();
			Console.WriteLine("source table dropped");
			Command.CommandText=DropDestinationstr;
			Command.ExecuteNonQuery();
			Console.WriteLine("destination table dropped");
			


		}



		////////Rules cases
		///
		[Test]
		public void RulesOnInsertDoInstead( )
		{

			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="create table rtest_t1 (a int4, b int4);";
			Command.ExecuteNonQuery();
			Command.CommandText="create view rtest_v1 as select * from rtest_t1;";
			Command.ExecuteNonQuery();
			Command.CommandText="create rule rtest_v1_ins as on insert to rtest_v1 do instead insert into rtest_t1 values (new.a, new.b);";
			Command.ExecuteNonQuery();
			Command.CommandText="insert into rtest_v1 values (1, 11); insert into rtest_v1 values(2,12);";
			Command.ExecuteNonQuery();
			
			Command.CommandText="select * from rtest_v1;";
			EDBDataReader Reader=	Command.ExecuteReader();

			/*while(Reader.Read())
			{
				Console.WriteLine(Reader.GetValue(0).ToString());
				Console.WriteLine(Reader.GetValue(1).ToString());

			}*/
			Reader.Read();
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Assert.AreEqual("11",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("2",Reader.GetValue(0).ToString());
			Assert.AreEqual("12",Reader.GetValue(1).ToString());
	
			Console.WriteLine("2nd read");
            Reader.Close();
			Command.CommandText="select * from rtest_t1;";
			Reader=	Command.ExecuteReader();

			/*while(Reader.Read())
			{
				Console.WriteLine(Reader.GetValue(0).ToString());
				Console.WriteLine(Reader.GetValue(1).ToString());

			}*/
			Reader.Read();
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Assert.AreEqual("11",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("2",Reader.GetValue(0).ToString());
			Assert.AreEqual("12",Reader.GetValue(1).ToString());

            Reader.Close();
			Command.CommandText="DROP VIEW rtest_v1;DROP TABLE rtest_t1";
			Command.ExecuteNonQuery();
		}

		[Test]
		public void RulesOnUpdateDoInstead( )
		{

			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="create table rtest_t1 (a int4, b int4);";
			Command.ExecuteNonQuery();
			Command.CommandText="create view rtest_v1 as select * from rtest_t1;";
			Command.ExecuteNonQuery();
			Command.CommandText="insert into rtest_t1 values (10, 20); insert into rtest_t1 values(30,40); insert into rtest_t1 values(100,200);";
			Command.ExecuteNonQuery();
			Command.CommandText="create rule rtest_v1_upd as on update to rtest_v1 do instead update rtest_t1 set a = new.a, b = new.b where a = old.a;";
			Command.ExecuteNonQuery();

			Command.CommandText="update rtest_v1 set b = 142 where a = 10;";
			Command.ExecuteNonQuery();
			
			Command.CommandText="select * from rtest_t1 order by a;";
			EDBDataReader Reader=	Command.ExecuteReader();

			/*	while(Reader.Read())
				{
					Console.WriteLine(Reader.GetValue(0).ToString());
					Console.WriteLine(Reader.GetValue(1).ToString());

				}*/
			Reader.Read();
			Assert.AreEqual("10",Reader.GetValue(0).ToString());
			Assert.AreEqual("142",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("30",Reader.GetValue(0).ToString());
			Assert.AreEqual("40",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("100",Reader.GetValue(0).ToString());
			Assert.AreEqual("200",Reader.GetValue(1).ToString());

            Reader.Close();
			
			
			Command.CommandText="DROP VIEW rtest_v1;DROP TABLE rtest_t1";
			Command.ExecuteNonQuery();
		}


		
		[Test]
		public void RulesOnDeleteDoInstead( )
		{

			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="create table rtest_t1 (a int4, b int4);";
			Command.ExecuteNonQuery();
			Command.CommandText="create view rtest_v1 as select * from rtest_t1;";
			Command.ExecuteNonQuery();
			Command.CommandText="insert into rtest_t1 values(1, 2); insert into rtest_t1 values(3,4); insert into rtest_t1 values(5,6);";
			Command.ExecuteNonQuery();
			Command.CommandText="create rule rtest_v1_del as on delete to rtest_v1 do instead delete from rtest_t1 where a = old.a;";
			Command.ExecuteNonQuery();

			Command.CommandText="delete from rtest_v1 where a = 3;";
			Command.ExecuteNonQuery();
			
			Command.CommandText="select * from rtest_t1 order by a ;";
			EDBDataReader Reader=	Command.ExecuteReader();

			/*while(Reader.Read())
				{
					Console.WriteLine(Reader.GetValue(0).ToString());
					Console.WriteLine(Reader.GetValue(1).ToString());

				}*/
			Reader.Read();
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Assert.AreEqual("2",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("5",Reader.GetValue(0).ToString());
			Assert.AreEqual("6",Reader.GetValue(1).ToString());



            Reader.Close();
			
			Command.CommandText="DROP VIEW rtest_v1;DROP TABLE rtest_t1";
			Command.ExecuteNonQuery();
		}


	//	[Test]
		public void RulesMultiTabsUpdateDoAlso( )
		{

			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="create table rtest_system (sysname text, sysdesc text);create table rtest_interface (sysname text, ifname text);create table rtest_admin (pname text, sysname text);";
			Command.ExecuteNonQuery();
			Command.CommandText="create rule rtest_sys_upd as on update to rtest_system do also ( update rtest_interface set sysname = new.sysname    where sysname = old.sysname;  update rtest_admin set sysname = new.sysname  where sysname = old.sysname );";
			Command.ExecuteNonQuery();
			Command.CommandText="insert into rtest_system values ('orion', 'Linux Jan Wieck'); "+
				" insert into rtest_system values ('notjw', 'WinNT Jan Wieck (notebook)');"+
				" insert into rtest_system values ('neptun', 'Fileserver'); "+
				" insert into rtest_interface values ('orion', 'eth0');     "+
				" insert into rtest_interface values ('orion', 'eth1');     "+
				" insert into rtest_interface values ('notjw', 'eth0');     "+
				" insert into rtest_interface values ('neptun', 'eth0');    "+
				" insert into rtest_admin values ('jw', 'orion');	     "+	
				" insert into rtest_admin values ('jw', 'notjw');	     "+	
				" insert into rtest_admin values ('bm', 'neptun');";
			Command.ExecuteNonQuery();
			Command.CommandText="update rtest_system set sysname = 'pluto' where sysname = 'neptun';";
			Command.ExecuteNonQuery();

			
			
			Command.CommandText="select * from rtest_interface order by sysname;";
			EDBDataReader Reader=	Command.ExecuteReader();

			/*	while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());

					}*/
			Reader.Read();
			Assert.AreEqual("notjw",Reader.GetValue(0).ToString());
			Assert.AreEqual("eth0",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("orion",Reader.GetValue(0).ToString());
			Assert.AreEqual("eth0",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("orion",Reader.GetValue(0).ToString());
			Assert.AreEqual("eth1",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("pluto",Reader.GetValue(0).ToString());
			Assert.AreEqual("eth0",Reader.GetValue(1).ToString());
            Reader.Close();
			
			Command.CommandText="select * from rtest_admin order by sysname;";
			Reader=	Command.ExecuteReader();
			
			Console.WriteLine("Next one");
			/*	while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());

					}
			*/
	
			Reader.Read();
			Assert.AreEqual("jw",Reader.GetValue(0).ToString());
			Assert.AreEqual("notjw",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("jw",Reader.GetValue(0).ToString());
			Assert.AreEqual("orion",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("bm",Reader.GetValue(0).ToString());
			Assert.AreEqual("pluto",Reader.GetValue(1).ToString());
            Reader.Close();

			Command.CommandText="DROP TABLE rtest_system;DROP TABLE rtest_interface;DROP TABLE rtest_admin";
			Command.ExecuteNonQuery();
		}

		
		//[Test]
		public void RulesMultiTabsOnInsertDoInsteadWithJoins( )
		{

			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText="create table rtest_t1 (a int4, b int4); "+
				" create table rtest_t2 (a int4, b int4); "+
				" create table rtest_t3 (a int4, b int4);";
			Command.ExecuteNonQuery();
			Command.CommandText=" insert into rtest_t2 values (1, 21); "+
				" insert into rtest_t2 values (2, 22); "+
				" insert into rtest_t2 values (3, 23); "+
				" insert into rtest_t3 values (1, 31); "+
				" insert into rtest_t3 values (2, 32); "+
				" insert into rtest_t3 values (3, 33); "+
				" insert into rtest_t3 values (4, 34); "+
				" insert into rtest_t3 values (5, 35); ";
			Command.ExecuteNonQuery();
			Command.CommandText="create view rtest_v1 as select * from rtest_t1;";
			Command.ExecuteNonQuery();
			Command.CommandText="create rule rtest_v1_ins as on insert to rtest_v1 do instead "+
				"		 insert into rtest_t1 values (new.a, new.b);";
			Command.ExecuteNonQuery();

			
			
			Command.CommandText="insert into rtest_v1 select rtest_t2.a, rtest_t3.b "+
				" from rtest_t2, rtest_t3 "+
				" where rtest_t2.a = rtest_t3.a;";
			Command.ExecuteNonQuery();


			
			Command.CommandText="select * from rtest_v1;";
			EDBDataReader Reader=	Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());

					}*/
			Reader.Read();
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Assert.AreEqual("31",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("2",Reader.GetValue(0).ToString());
			Assert.AreEqual("32",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("3",Reader.GetValue(0).ToString());
			Assert.AreEqual("33",Reader.GetValue(1).ToString());

            Reader.Close();

			Command.CommandText="select * from rtest_t1;";
			Reader=	Command.ExecuteReader();
			
			Console.WriteLine("Next one");
			/*	while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());

					}
			*/
	
			Reader.Read();
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Assert.AreEqual("31",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("2",Reader.GetValue(0).ToString());
			Assert.AreEqual("32",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("3",Reader.GetValue(0).ToString());
			Assert.AreEqual("33",Reader.GetValue(1).ToString());
            Reader.Close();

			Command.CommandText="DROP VIEW rtest_v1;DROP TABLE rtest_t1;DROP TABLE rtest_t2;DROP TABLE rtest_t3";
			Command.ExecuteNonQuery();
		}


		[Test]
		public void RulesInsteadNothing( )
		{

			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=" create table rtest_nothn1 (a int4, b text);";
			Command.ExecuteNonQuery();
			Command.CommandText=" create rule rtest_nothn_r1 as on insert to rtest_nothn1 "+
				" where new.a >= 10 and new.a < 20 do instead nothing; ";
			Command.ExecuteNonQuery();


			Command.CommandText="insert into rtest_nothn1 values (1, 'want this'); "+
				" insert into rtest_nothn1 values (2, 'want this'); "+
				" insert into rtest_nothn1 values (10, 'don''t want this'); "+
				" insert into rtest_nothn1 values (19, 'don''t want this');"+
				" insert into rtest_nothn1 values (20, 'want this');";
			Command.ExecuteNonQuery();


		

			
			Command.CommandText="select * from rtest_nothn1;";
			EDBDataReader Reader=	Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());

					}*/

			Reader.Read();
			Assert.AreEqual("1",Reader.GetValue(0).ToString());
			Assert.AreEqual("want this",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("2",Reader.GetValue(0).ToString());
			Assert.AreEqual("want this",Reader.GetValue(1).ToString());
			Reader.Read();
			Assert.AreEqual("20",Reader.GetValue(0).ToString());
			Assert.AreEqual("want this",Reader.GetValue(1).ToString());
            Reader.Close();
		
			Command.CommandText="DROP TABLE rtest_nothn1";
			Command.ExecuteNonQuery();
		}
		


		[Test]
		public void RuleInsertView( )
		{

			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=" create table GeoPath4( cval int4,eval int4);";
			Command.ExecuteNonQuery();
			Command.CommandText=" insert into GeoPath4 values (5,10) ";
			Command.ExecuteNonQuery();


			Command.CommandText="create VIEW  V1 as select * from GeoPath4;";
			Command.ExecuteNonQuery();

			
			Command.CommandText="create table GeoPathLog4( dval int4);";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE RULE log_Geo4 AS ON INSERT TO V1  DO INSTEAD INSERT INTO GeoPathLog4 VALUES ( NEW.eval )";
			Command.ExecuteNonQuery();


		
			Command.CommandText="insert into V1 values (50,100)";
			Command.ExecuteNonQuery();


			
		

			Command.CommandText="select * from V1";
			EDBDataReader Reader=	Command.ExecuteReader();

			/*while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						Console.WriteLine(Reader.GetValue(1).ToString());

					}*/

			Reader.Read();
			Assert.AreEqual("5",Reader.GetValue(0).ToString());
			Assert.AreEqual("10",Reader.GetValue(1).ToString());
			Reader.Close();
			
			Command.CommandText="DROP VIEW V1; DROP TABLE GeoPathLog4; DROP TABLE GeoPath4";
			Command.ExecuteNonQuery();
		}
		


		[Test]
		public void RuleDeleteView( )
		{

			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=" create table GeoPath5( cval int4,eval int4);";
			Command.ExecuteNonQuery();
			Command.CommandText=" insert into GeoPath5 values (5,10) ";
			Command.ExecuteNonQuery();


			Command.CommandText="create VIEW  V2 as select * from GeoPath5;";
			Command.ExecuteNonQuery();

			
			Command.CommandText="create table GeoPathLog5( dval int4);";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE RULE log_Geo5 AS ON DELETE TO V2  DO INSTEAD INSERT INTO GeoPathLog5 VALUES ( OLD.eval )";
			Command.ExecuteNonQuery();


		
			Command.CommandText="Delete from V2 where cval=5";
			Command.ExecuteNonQuery();


			
	

			Command.CommandText="select * from GeoPathLog5";
			EDBDataReader Reader=	Command.ExecuteReader();
	/*
			while(Reader.Read())
					{
						Console.WriteLine(Reader.GetValue(0).ToString());
						//Console.WriteLine(Reader.GetValue(1).ToString());

					}
*/
			Reader.Read();
			
			Assert.AreEqual("10",Reader.GetValue(0).ToString());
			Reader.Close();
			
			Command.CommandText="DROP VIEW V2;DROP TABLE GeoPathLog5;DROP TABLE GeoPath5";
			Command.ExecuteNonQuery();
		}
		

		[Test]
		public void RuleUpdateView( )
		{

			EDBCommand Command=new EDBCommand("",con);
			Command.CommandText=" create table GeoPath5( cval int4,eval int4);";
			Command.ExecuteNonQuery();
			Command.CommandText=" insert into GeoPath5 values (5,10) ";
			Command.ExecuteNonQuery();


			Command.CommandText="create VIEW  V2 as select * from GeoPath5;";
			Command.ExecuteNonQuery();

			
			Command.CommandText="create table GeoPathLog5( dval int4);";
			Command.ExecuteNonQuery();

			Command.CommandText="CREATE RULE log_Geo5 AS ON UPDATE TO V2  DO INSTEAD INSERT INTO GeoPathLog5 VALUES ( OLD.eval )";
			Command.ExecuteNonQuery();


		
			Command.CommandText="Update V2 set eval=100 where cval=5";
			Command.ExecuteNonQuery();


			
	

			Command.CommandText="select * from GeoPathLog5";
			EDBDataReader Reader=	Command.ExecuteReader();
		/*	
					while(Reader.Read())
							{
								Console.WriteLine(Reader.GetValue(0).ToString());
								//Console.WriteLine(Reader.GetValue(1).ToString());

							}
		*/
			Reader.Read();
			
			Assert.AreEqual("10",Reader.GetValue(0).ToString());
			Reader.Close();
			
			Command.CommandText="DROP VIEW V2;DROP TABLE GeoPathLog5;DROP TABLE GeoPath5";
			Command.ExecuteNonQuery();
		}

	}
}



