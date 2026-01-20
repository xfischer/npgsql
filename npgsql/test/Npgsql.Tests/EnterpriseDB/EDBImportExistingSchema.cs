using System;
using NUnit.Framework;
using System.Data;


namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

/// <summary>
/// it creates a new table importing the schema of an existing table
/// </summary>
[TestFixture]
[NonParallelizable]
public class EDBImportExistingSchema : EPASTestBase
{

    EDBConnection? con = null;

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
            var create = "CREATE TABLE NewTable AS Select * From emp";
            var Command = new EDBCommand("", con)
            {
                CommandText = create,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            //test the existence of the new table

            var Select = "SELECT * FROM NewTable";
            Command = new EDBCommand("", con)
            {
                CommandText = Select,
                CommandType = CommandType.Text
            };

            var Reader = Command.ExecuteReader();

            Assert.That(Reader.Read(), "No data returned from Select");

            Reader.Close();
            var DropTable = "Drop TABLE NewTable";
            Command = new EDBCommand("", con)
            {
                CommandText = DropTable,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();


        }
        catch (EDBException exp)
        {
            Console.WriteLine(exp.Message);
            throw new Exception(exp.ToString());
        }

    }

    [Test]

    public void CreateViewOnImportedSchema()
    {
        Assert.DoesNotThrow(() =>
        {
            var createTable = "CREATE TABLE TableForView AS Select * From dept";
            var Command = new EDBCommand("", con)
            {
                CommandText = createTable,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            var CreateView = "CREATE OR REPLACE VIEW ImportedSchemaView AS Select * From TableForView";
            Command = new EDBCommand("", con)
            {
                CommandText = CreateView
            };
            Command.ExecuteNonQuery();

            var Select = "SELECT * FROM ImportedSchemaView";
            Command = new EDBCommand("", con)
            {
                CommandText = Select,
                CommandType = CommandType.Text
            };

            var Reader = Command.ExecuteReader();

            Reader.Close();

            var DropView = "Drop View ImportedSchemaView";
            Command = new EDBCommand("", con)
            {
                CommandText = DropView,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            var Drop = "Drop TABLE TableForView";
            Command = new EDBCommand("", con)
            {
                CommandText = Drop,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

        });
    }

    [TearDown]
    public void Dispose()
    {
        TestUtil.closeDB(con);
        con?.Dispose();
    }

    [Test]

    public void ExecuteSelectOnImportedSchema()
    {
        try
        {

            //create the table
            var create = "CREATE TABLE NewTable AS Select * From emp";
            var Command = new EDBCommand("", con)
            {
                CommandText = create,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            //test the existence of the new table

            var Select = "SELECT * FROM NewTable";
            Command = new EDBCommand("", con)
            {
                CommandText = Select,
                CommandType = CommandType.Text
            };

            var Reader = Command.ExecuteReader();

            Assert.That(Reader.Read(), "No data returned from Select");
            Reader.Close();

            var DropTable = "Drop TABLE NewTable";
            Command = new EDBCommand("", con)
            {
                CommandText = DropTable,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();


        }
        catch (EDBException exp)
        {
            Console.WriteLine(exp.Message);
            throw new Exception(exp.ToString());
        }
    }

    [Test]

    public void ExecuteDeleteOnImportedSchema()
    {
        Assert.DoesNotThrow(() =>
        {
            var create = "CREATE TABLE DeleteTable AS Select * From emp";
            var Command = new EDBCommand("", con)
            {
                CommandText = create,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            //test the existence of the new table

            var Select = "Delete  FROM DeleteTable where empno=10";
            Command = new EDBCommand("", con)
            {
                CommandText = Select,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            var DropTable = "Drop TABLE DeleteTable";
            Command = new EDBCommand("", con)
            {
                CommandText = DropTable,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

        });
    }

    [Test]

    public void ExecuteInsertOnImportedSchema()
    {
        Assert.DoesNotThrow(() =>
        {
            TestUtil.dropTable(con, "InsertTable");
            var create = "CREATE TABLE InsertTable AS Select * From dept";
            var Command = new EDBCommand("", con)
            {
                CommandText = create,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            //test the existence of the new table

            var Select = "INSERT INTO InsertTable VALUES(80,'Documentation','Hamburg')";
            Command = new EDBCommand("", con)
            {
                CommandText = Select,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            var DropTable = "Drop TABLE InsertTable";
            Command = new EDBCommand("", con)
            {
                CommandText = DropTable,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();


        });
    }

    [Test]

    public void ExecuteUpdateOnImportedSchema()
    {
        Assert.DoesNotThrow(() =>
        {
            var create = "CREATE TABLE UpdateTable AS Select * From dept";
            var Command = new EDBCommand("", con)
            {
                CommandText = create,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            //test the existence of the new table

            var Select = "UPDATE UpdateTable SET loc='ISBD' WHERE deptno=20";
            Command = new EDBCommand("", con)
            {
                CommandText = Select,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();

            var DropTable = "Drop TABLE UpdateTable";
            Command = new EDBCommand("", con)
            {
                CommandText = DropTable,
                CommandType = CommandType.Text
            };
            Command.ExecuteNonQuery();


        });
    }
}
