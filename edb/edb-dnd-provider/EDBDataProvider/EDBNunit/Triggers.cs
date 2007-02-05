using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;

namespace NUnit
{
	/// <summary>
	/// Summary description for Triggers.
	/// </summary>
	
	[TestFixture]
	public class Triggers
	{
		EDBConnection con = null;

		[SetUp]
		public void Init()
		{
			//write setup for following test cases
			con = TestUtil.openDB();
			

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

			catch(EDBException exp)
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

			catch(EDBException exp)
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
}



