using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using NUnit;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
	/// <summary>
	/// it creates a new table importing the schema of an existing table
	/// </summary>
	[TestFixture]
    [NonParallelizable]
	public class EDBImportExistingSchema : EPASTestBase
    {
		
		EDBConnection? con=null;

        [SetUp]
        public void SetUp()
        {
            con = OpenConnection();
        }

        [Test]
		public void CreateTable()
		{
			try
            {

                //create the table
                var create="CREATE TABLE NewTable AS Select * From emp";
                var Command = new EDBCommand("",con);
				Command.CommandText=create;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				//test the existence of the new table

				var Select="SELECT * FROM NewTable";
				Command=new EDBCommand("",con);
				Command.CommandText=Select;
				Command.CommandType=CommandType.Text;

				var Reader=Command.ExecuteReader();

				Assert.IsTrue(Reader.Read(),"No data returned from Select");

                Reader.Close();
				var DropTable="Drop TABLE NewTable";
				Command=new EDBCommand("",con);
				Command.CommandText=DropTable;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();


			}
			catch(EDBException exp)
            {
                Console.WriteLine(exp.Message);
                throw new Exception(exp.ToString());
			}

		}

		[Test]
			
		public void CreateViewOnImportedSchema()
		{
			try
            {

                var createTable="CREATE TABLE TableForView AS Select * From dept";
                var Command = new EDBCommand("",con);
				Command.CommandText=createTable;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				var CreateView="CREATE OR REPLACE VIEW ImportedSchemaView AS Select * From TableForView";
				Command=new EDBCommand("",con);
				Command.CommandText=CreateView;
				Command.ExecuteNonQuery();

				var Select="SELECT * FROM ImportedSchemaView";
				Command=new EDBCommand("",con);
				Command.CommandText=Select;
				Command.CommandType=CommandType.Text;

				var Reader=Command.ExecuteReader();

				//Assert.IsTrue(Reader.Read(),"No data returned from Select");

                Reader.Close();
				
				var DropView ="Drop View ImportedSchemaView";
				Command=new EDBCommand("",con);
				Command.CommandText=DropView;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				var Drop="Drop TABLE TableForView";
				Command=new EDBCommand("",con);
				Command.CommandText=Drop;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();
				
			}
			catch (EDBException exp)
			{
                Console.WriteLine(exp.Message);
				throw new Exception(exp.ToString());
			}
		}

		[TearDown]
		public void Dispose()
		{
			TestUtil.closeDB(con);

		}

		[Test]

		public void ExecuteSelectOnImportedSchema()
		{
			try
			{

                //create the table
                var create="CREATE TABLE NewTable AS Select * From emp";
                var Command = new EDBCommand("",con);
				Command.CommandText=create;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				//test the existence of the new table

				var Select="SELECT * FROM NewTable";
				Command=new EDBCommand("",con);
				Command.CommandText=Select;
				Command.CommandType=CommandType.Text;

				var Reader=Command.ExecuteReader();

				Assert.IsTrue(Reader.Read(),"No data returned from Select");
                Reader.Close();

				var DropTable="Drop TABLE NewTable";
				Command=new EDBCommand("",con);
				Command.CommandText=DropTable;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();


			}
			catch(EDBException exp)
            {
                Console.WriteLine(exp.Message);
                throw new Exception(exp.ToString());
			}
		}

		[Test]

		public void ExecuteDeleteOnImportedSchema()
		{
			try 
			{
				var create="CREATE TABLE DeleteTable AS Select * From emp";
				var Command=new EDBCommand("",con);
				Command.CommandText=create;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				//test the existence of the new table

				var Select="Delete  FROM DeleteTable where empno=10";
				Command=new EDBCommand("",con);
				Command.CommandText=Select;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				var DropTable="Drop TABLE DeleteTable";
				Command=new EDBCommand("",con);
				Command.CommandText=DropTable;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();


			}
			catch(EDBException exp)
			{
                 throw new Exception("\n Couldn't complete delete operation!!\n"+exp.ToString());
			}

		}

		[Test]

		public void ExecuteInsertOnImportedSchema()
		{
			try 
			{
                TestUtil.dropTable(con, "InsertTable");
                var create="CREATE TABLE InsertTable AS Select * From dept";
				var Command=new EDBCommand("",con);
				Command.CommandText=create;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				//test the existence of the new table

				var Select="INSERT INTO InsertTable VALUES(80,'Documentation','Hamburg')";
				Command=new EDBCommand("",con);
				Command.CommandText=Select;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				var DropTable="Drop TABLE InsertTable";
				Command=new EDBCommand("",con);
				Command.CommandText=DropTable;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();


			}
			catch(EDBException exp)
			{
				throw new Exception("\n Couldn't complete Insert operation!!\n"+exp.ToString());
			}

		}

		[Test]

		public void ExecuteUpdateOnImportedSchema()
		{
			try 
			{
				var create="CREATE TABLE UpdateTable AS Select * From dept";
				var Command=new EDBCommand("",con);
				Command.CommandText=create;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				//test the existence of the new table

				var Select="UPDATE UpdateTable SET loc='ISBD' WHERE deptno=20";
				Command=new EDBCommand("",con);
				Command.CommandText=Select;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				var DropTable="Drop TABLE UpdateTable";
				Command=new EDBCommand("",con);
				Command.CommandText=DropTable;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();


			}
			catch(EDBException exp)
			{
				throw new Exception("\n Couldn't complete Update operation!!\n"+exp.ToString());
			}

		}
			
		/*[Test]
		public void CreateMaterializedViewOnImportedSchema()
		{
			try
			{
				string createTable="CREATE TABLE TableForMatView AS Select * From dept";
				EDBCommand Command=new EDBCommand("",con);
				Command.CommandText=createTable;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				string CreateView="CREATE MATERIALIZED VIEW MatView REFRESH force AS select * from emp";
				Command=new EDBCommand("",con);
				Command.CommandText=CreateView;
				Command.ExecuteNonQuery();

				string Select="SELECT * FROM MatView";
				Command=new EDBCommand("",con);
				Command.CommandText=Select;
				Command.CommandType=CommandType.Text;

				EDBDataReader Reader=Command.ExecuteReader();

				Assert.IsTrue(Reader.Read(),"No data returned from Select");

				
				
				string DropView ="Drop View MatView";
				Command=new EDBCommand("",con);
				Command.CommandText=DropView;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();

				string Drop="Drop TABLE TableForMatView";
				Command=new EDBCommand("",con);
				Command.CommandText=Drop;
				Command.CommandType=CommandType.Text;
				Command.ExecuteNonQuery();


				
			}
			catch (EDBException exp)
			{
				throw new Exception(exp.ToString());
			}
		}*/
			
		
	}
}
