using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using NUnit.Framework.Constraints;

#pragma warning disable IDE0007 // Use implicit type
namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

#pragma warning disable CS8602
/// <summary>
/// Tests around EC-1001
/// </summary>
/// 
[TestFixture]
[NonParallelizable]
public class EDB_EC_1001_Tests : EPASTestBase
{
    [Test]
    public void CreateTriggerTest()
    {
        using var con = OpenConnection();
#nullable disable
        TestUtil.MinimumPgVersion(con, "12.0.0");
#nullable restore           
        string createTable = "CREATE TABLE CMP_TRIG_TBL (id number(4) primary key, description varchar2(200));";
        EDBCommand? cmd = new EDBCommand()
        {
            CommandText = createTable,
            CommandType = System.Data.CommandType.Text,
            Connection = con
        };
        var result1 = cmd.ExecuteNonQuery();
        Console.WriteLine(result1);

        string createTrigger = "CREATE OR REPLACE TRIGGER CMP_TRIG FOR INSERT OR UPDATE OR DELETE ON CMP_TRIG_TBL\n"
          + "COMPOUND TRIGGER\n"
          + "V_ID  NUMBER;\n"
          + "\n"
          + "BEFORE STATEMENT\n"
          + "IS BEGIN \n"
          + "\n"
          + "IF INSERTING THEN DBMS_OUTPUT.put_line ('INSERT - Before Statement Trigger');\n"
          + "END IF;\n"
          + "\n"
          + "IF UPDATING THEN DBMS_OUTPUT.put_line ('UPDATE - Before Statement Trigger');\n"
          + "END IF;\n"
          + "\n"
          + "IF DELETING THEN DBMS_OUTPUT.put_line ('DELETE - Before Statement Trigger');\n"
          + "END IF;\n"
          + "END BEFORE STATEMENT;\n"
          + "\n"
          + "AFTER STATEMENT\n"
          + "IS\n"
          + "BEGIN\n"
          + "IF INSERTING THEN DBMS_OUTPUT.put_line ('INSERT - After Statement Trigger');\n"
          + "END IF;\n"
          + "\n"
          + "IF UPDATING THEN DBMS_OUTPUT.put_line ('UPDATE - After Statement Trigger');\n"
          + "END IF;\n"
          + "\n"
          + "IF DELETING THEN DBMS_OUTPUT.put_line ('DELETE - After Statement Trigger');\n"
          + "END IF;\n"
          + "END AFTER STATEMENT;\n"
          + "\n"
          + "BEFORE EACH ROW IS BEGIN IF INSERTING THEN DBMS_OUTPUT.put_line ('INSERT - Before Each Row Trigger'); \n"
          + "END IF;\n"
          + "\n"
          + "IF UPDATING THEN DBMS_OUTPUT.put_line ('UPDATE - Before Each Row Trigger'); \n"
          + "END IF;\n"
          + "\n"
          + "IF DELETING THEN DBMS_OUTPUT.put_line ('DELETE - Before Each Row Trigger');\n"
          + "END IF;\n"
          + "END BEFORE EACH ROW;\n"
          + "\n"
          + "AFTER EACH ROW IS BEGIN IF INSERTING THEN DBMS_OUTPUT.put_line ('INSERT - After Each Row Trigger');\n"
          + "END IF;\n"
          + "\n"
          + "IF UPDATING THEN DBMS_OUTPUT.put_line ('UPDATE - After Each Row Trigger');\n"
          + "END IF;\n"
          + "\n"
          + "IF DELETING THEN DBMS_OUTPUT.put_line ('DELETE - After Each Row Trigger');\n"
          + "END IF; \n"
          + "END AFTER EACH ROW;\n"
          + "\n"
          + "END CMP_TRIG;";
        EDBCommand? cmd2 = new EDBCommand()
        {
            CommandText = createTrigger,
            CommandType = System.Data.CommandType.Text,
            Connection = con
        };
        var result2 = cmd2.ExecuteNonQuery();
        Console.WriteLine(result1);

        //Drop table.
        TestUtil.dropTable(con, "CMP_TRIG_TBL");
    }
}

/// <summary>
/// Tests around EC-1113
/// </summary>
/// 
[TestFixture]
public class EDB_EC_1113_Tests : EPASTestBase
{
    /// <summary>
    /// This class is for matching database type in test EC-1113.
    /// </summary>
    public class TestType
    {

        public string? Type1;
        public string? Type2;
    }

    [OneTimeSetUp]
    public void Init()
    {
        using var con = OpenConnection();

        Cleanup(con);

        //Table
        string tableString = "CREATE TABLE account(\n" +
                             "username VARCHAR(250),\n" +
                             "email VARCHAR(250)\n" +
                             ");";
        using (EDBCommand? createTableCommand = new EDBCommand("", con))
        {
            createTableCommand.CommandText = tableString;
            createTableCommand.ExecuteNonQuery();
        }

        //Table
        string typeString = "CREATE OR REPLACE TYPE TEST_TYPE AS OBJECT\n" +
                            "(\n" +
                            "TYPE1   varchar2(250),\n" +
                            "TYPE2   varchar2(250)\n" +
                            ");";
        using (EDBCommand? createTypeCommand = new EDBCommand("", con))
        {
            createTypeCommand.CommandText = typeString;
            createTypeCommand.ExecuteNonQuery();
        }

        //Procedure
        string procCreate = "CREATE OR REPLACE PROCEDURE TEST_PROC_TYPE_ARRAY\n" +
                            "(\n" +
                            " ARG1 IN     NUMBER,\n" +
                            "ARG2 IN     VARCHAR2,\n" +
                            "ARG3 IN     TEST_TYPE[])\n" +
                            "IS\n" +
                            "BEGIN\n" +
                            "insert into account values('t1', 't2');\n" +
                            "END;";
        CreateDropProcedure(procCreate);
    }

    [OneTimeTearDown]
    public void Dispose()
    {
        using var con = OpenConnection();

        Cleanup(con);

        TestUtil.closeDB(con);
        con?.Dispose();
    }

    private void Cleanup(EDBConnection con)
    {
        try
        {
            //Drop Procedure
            CreateDropProcedure("DROP PROCEDURE IF EXISTS TEST_PROC_TYPE_ARRAY");

            //Drop table.
            TestUtil.dropTable(con, "account");

            //Drop type.
            using (EDBCommand? dropTypeCommand = new EDBCommand("", con))
            {
                dropTypeCommand.CommandText = "DROP TYPE IF EXISTS TEST_TYPE";
                dropTypeCommand.ExecuteNonQuery();
            }
        }
        finally
        {
            // swallow exception
        }
    }

    private void CreateDropProcedure(string procString)
    {
        using var con = OpenConnection();
        //Create Procedure
        using var createProcCommand = new EDBCommand("", con);
        createProcCommand.CommandType = CommandType.Text;

        createProcCommand.CommandText = procString;
        int count = createProcCommand.ExecuteNonQuery();
    }

    [Test]
    public async Task CustomTypeArrayAsInParamTest()
    {
        var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
        dataSourceBuilder.MapComposite<TestType>("public.test_type");
        await using var dataSource = dataSourceBuilder.Build();

        await using var connection = await dataSource.OpenConnectionAsync();

        using var Command = new EDBCommand("TEST_PROC_TYPE_ARRAY", connection);

        Command.CommandType = CommandType.StoredProcedure;
        EDBCommandBuilder.DeriveParameters(Command);

        Command.Parameters[0].Value = 20;
        Command.Parameters[1].Value = "Testing3";

        List<TestType> myTests = new List<TestType>()
            {
            new TestType()
            {
                Type1 = "Test2",
                Type2 = "Test3"
            },
            new TestType()
            {
                Type1 = "Test4",
                Type2 = "Test5"
            }
            };
        Command.Parameters[2].Value = myTests.ToArray();

        Assert.DoesNotThrowAsync(async () =>
        {
            await Command.PrepareAsync();
            await Command.ExecuteNonQueryAsync();
        });
    }

    [Test]
    public async Task CustomTypeArrayAsInParamTest_ManualWiring()
    {
        var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
        dataSourceBuilder.MapComposite<TestType>("public.test_type");
        await using var dataSource = dataSourceBuilder.Build();

        await using var connection = await dataSource.OpenConnectionAsync();

        using var Command = new EDBCommand("TEST_PROC_TYPE_ARRAY", connection);
        Command.CommandType = CommandType.StoredProcedure;

        Command.Parameters.AddWithValue(EDBTypes.EDBDbType.Numeric, 20);
        Command.Parameters.AddWithValue(EDBTypes.EDBDbType.Varchar, "Testing3");

        List<TestType> myTests = new List<TestType>()
            {
            new ()
            {
                Type1 = "Test2",
                Type2 = "Test3"
            },
            new TestType()
            {
                Type1 = "Test4",
                Type2 = "Test5"
            }
            };

        Command.Parameters.AddWithValue(myTests.ToArray());

        Assert.DoesNotThrowAsync(async () =>
        {
            await Command.PrepareAsync();
            await Command.ExecuteNonQueryAsync();
        }
        );
    }
}

/// <summary>
/// Tests around EC-1134
/// </summary>
/// 
[TestFixture]
public class EDB_EC_1134_Tests : EPASTestBase
{
    [SetUp]
    public void Init()
    {
        using var con = OpenConnection();

        //Table
        string tableString = "create table testclob (datasource clob);";
        using (var createTableCommand = new EDBCommand("", con))
        {
            createTableCommand.CommandText = tableString;
            createTableCommand.ExecuteNonQuery();
        }

        //Insert into table.
        using (var insertCommand = new EDBCommand("", con))
        {
            insertCommand.CommandText = "insert into testclob (datasource) values (lpad('a', 8200, 'a'));";
            int count = insertCommand.ExecuteNonQuery();
        }
    }

    [TearDown]
    public void Dispose()
    {
        using var con = OpenConnection();

        //Drop table.
        TestUtil.dropTable(con, "testclob");
    }

    [Test]
    public void ExecuteReaderClobTest()
    {
        Assert.DoesNotThrow(() =>
        {
            using var con = OpenConnection();
            using var trans = con.BeginTransaction();
            using EDBCommand cmd = new EDBCommand()
            {
                CommandText = "SELECT datasource FROM testclob ",
                CommandType = System.Data.CommandType.Text,
                Connection = con,
                Transaction = trans
            };
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
            }
            reader.Close();
            trans.Commit(); //throws mentioned exception
        });
    }
}

/// <summary>
/// Tests around EC-1084
/// </summary>
/// 
[TestFixture]
public class EDB_EC_1084_Tests : EPASTestBase
{
    [OneTimeSetUp]
    public void Init()
    {
        using var con = OpenConnection();

        //Drop table.
        TestUtil.dropTable(con, "ca.epas_test_table2");

        //Drop Schema
        CreateDropSchema(con, "DROP SCHEMA ca CASCADE", false);

        //Create Schema.
        CreateDropSchema(con, "CREATE SCHEMA ca", true);

        //Create table and insert data.
        CreateTable(con, "CREATE TABLE ca.epas_test_table2" +
                    "(" +
                        "id numeric not null," +
                        "title character varying(30)" +
                    ")" +
                    "WITH(" +
                        "OIDS = FALSE" +
                    ")");
        InsertIntoTable(con, "insert into ca.epas_test_table2 (id, title) values ( 1, 'row 1')");
    }

    private static void CreateDropSchema(EDBConnection con, string schameString, bool create)
    {
        //Create Table.
        try
        {
            using var createTableCommand = new EDBCommand("", con);
            createTableCommand.CommandText = schameString;
            int count = createTableCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            if (create)
                Assert.Fail(ex.Message);
        }
    }

    private static void CreateTable(EDBConnection con, string tableString)
    {
        //Create Table.
        try
        {
            using var createTableCommand = new EDBCommand("", con);
            createTableCommand.CommandText = tableString;
            int count = createTableCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    private static void InsertIntoTable(EDBConnection con, string insertString)
    {
        //Insert into table.
        using var insertCommand = new EDBCommand("", con);
        insertCommand.CommandText = insertString;
        int count = insertCommand.ExecuteNonQuery();

        Assert.That(count, Is.Not.EqualTo(0), "Data was not inserted.");
    }

    private static void CreateDropProcedure(EDBConnection con, string procString, bool create)
    {
        //Create Procedure
        using var createProcCommand = new EDBCommand("", con);
        createProcCommand.CommandType = CommandType.Text;

        createProcCommand.CommandText = procString;
        int count = createProcCommand.ExecuteNonQuery();

        if (create)
            Assert.That(count, Is.Not.EqualTo(0), "Procedure was not created");
    }

    private void DoSupressTest(string pkg, string pkgBody, string pkgNameCall, string pkgNameDelete, bool shouldThrow, string shouldThrowThis)
    {
        var con = OpenConnection();
        try
        {

            //Create package and body.
            if (pkg != null)
                CreateDropProcedure(con, pkg, true);

            CreateDropProcedure(con, pkgBody, true);

            bool isThrown = true;
            string exceptionMessage = "";

            string storedProcName = pkgNameCall;
            using (var command = new EDBCommand(storedProcName, con))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                try
                {
                    command.Prepare();
                    command.ExecuteNonQuery();

                    isThrown = false;

                }
                catch (Exception e)
                {
                    isThrown = true;
                    exceptionMessage = e.Message;
                }
            }

            if (con is null || (con.Connector == null))
                con = OpenConnection();

            if (pkgNameDelete != null)
            {
                //Drop package body.
                CreateDropProcedure(con, "DROP PACKAGE BODY " + pkgNameDelete, false);

                //Drop package.
                CreateDropProcedure(con, "DROP PACKAGE " + pkgNameDelete, false);
            }
            else
                CreateDropProcedure(con, "DROP PROCEDURE " + pkgNameCall, false);

            Assert.That(isThrown, Is.EqualTo(shouldThrow), "Test result is not as expected.");
            if (shouldThrow)
                Assert.That(exceptionMessage.StartsWith(shouldThrowThis), Is.True, "Exception message is not as expected");  
        }
        finally
        {
            con.Dispose();
        }


    }

    [Test]
    public void TestQueryRetrunsNoRows()
    {
        //Note: Procedure select from table but the query returns nothing.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test2\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test2;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test2\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select title into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id = 10;\n" +
                                "end;\n" +
                            "END epas_test2;";

        string pkgNameCall = "ca.epas_test2.supresseserrors";
        string pkgNameDelete = "ca.epas_test2";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "P0002: query returned no rows");
    }

    [Test]
    public void TestColumnDoesNotExit()
    {
        //Note: Procedure select column title2 which does not exist.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test3\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test3;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test3\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select title2 into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id = 10;\n" +
                                "end;\n" +
                            "END epas_test3;";

        string pkgNameCall = "ca.epas_test3.supresseserrors";
        string pkgNameDelete = "ca.epas_test3";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "42703: column \"title2\" does not exist");
    }

    [Test]
    public void TestTableDoesNotExit()
    {
        //Note: Procedure selects from ca.epas_test_table3 which does not exist.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test4\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test4;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test4\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select title into captureVar\n" +
                                    "from ca.epas_test_table3\n" +
                                    "where id = 10;\n" +
                                "end;\n" +
                            "END epas_test4;";

        string pkgNameCall = "ca.epas_test4.supresseserrors";
        string pkgNameDelete = "ca.epas_test4";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "42P01: relation \"ca.epas_test_table3\" does not exist");
    }

    [Test]
    public void TestColumnInWhereDoesNotExit()
    {
        //Note: Procedure selects from ca.epas_test_table3 which does not exist.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test5\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test5;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test5\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select title into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id2 = 10;\n" +
                                "end;\n" +
                            "END epas_test5;";

        string pkgNameCall = "ca.epas_test5.supresseserrors";
        string pkgNameDelete = "ca.epas_test5";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "42703: column \"id2\" does not exist");
    }

    [Test]
    public void TestSelectCharacterIntoNumeric()
    {
        //Note: Procedure selects character varying into numeric from ca.epas_test_table.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test6\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test6;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test6\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar numeric;\n" +
                                "begin\n" +
                                    "select title into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id = 1;\n" +
                                "end;\n" +
                            "END epas_test6;";

        string pkgNameCall = "ca.epas_test6.supresseserrors";
        string pkgNameDelete = "ca.epas_test6";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "22P02: invalid input syntax for type numeric: \"row 1\"");
    }

    [Test]
    public void TestSelectMultipleIntoSingleVar()
    {
        //Note: Procedure selects * into single variable.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test7\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test7;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test7\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select * into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id = 1;\n" +
                                "end;\n" +
                            "END epas_test7;";

        string pkgNameCall = "ca.epas_test7.supresseserrors";
        string pkgNameDelete = "ca.epas_test7";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "22005: wrong number of values in the INTO list of a SELECT statement");
    }

    [Test]
    public void TestPackageDoesNotExist()
    {
        //Note: Package epas_test81 does not exist.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test8\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test8;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test8\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select title into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id = 1;\n" +
                                "end;\n" +
                            "END epas_test8;";

        string pkgNameCall = "ca.epas_test81.supresseserrors";
        string pkgNameDelete = "ca.epas_test8";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "3F000: package \"epas_test81\" does not exist");
    }

    [Test]
    public void TestProcedureDoesNotExist()
    {
        //Note: Procedure supresseserrors2 does not exist.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test9\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test9;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test9\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select title into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id = 1;\n" +
                                "end;\n" +
                            "END epas_test9;";

        string pkgNameCall = "ca.epas_test9.supresseserrors2";
        string pkgNameDelete = "ca.epas_test9";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "42883: procedure ca.epas_test9.supresseserrors2() does not exist");
    }

    [Test]
    public void TestSchemaDoesNotExist()
    {
        //Note: Schema ca2 does not exist.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test10\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test10;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test10\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select title into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id = 1;\n" +
                                "end;\n" +
                            "END epas_test10;";

        string pkgNameCall = "ca2.epas_test10.supresseserrors";
        string pkgNameDelete = "ca.epas_test10";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "3F000: package \"epas_test10\" does not exist");
    }

    [Test]
    public void TestSuccessCase()
    {
        //Note: Schema ca2 does not exist.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE ca.epas_test11\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test11;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY ca.epas_test11\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select title into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id = 1;\n" +
                                "end;\n" +
                            "END epas_test11;";

        string pkgNameCall = "ca.epas_test11.supresseserrors";
        string pkgNameDelete = "ca.epas_test11";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, false, "");
    }

    [Test]
    public void TestQueryRetrunsNoRowsPublicSchema()
    {
        //Note: Procedure select from table but the query returns nothing.

        //Create package and body.
        string pkg = "CREATE OR REPLACE PACKAGE epas_test12\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors();\n" +
                            "END epas_test12;";

        string pkgBody = "CREATE OR REPLACE PACKAGE BODY epas_test12\n" +
                            "IS\n" +
                                "PROCEDURE supresseserrors() IS\n" +
                                    "captureVar varchar2(30);\n" +
                                "begin\n" +
                                    "select title into captureVar\n" +
                                    "from ca.epas_test_table2\n" +
                                    "where id = 10;\n" +
                                "end;\n" +
                            "END epas_test12;";

        string pkgNameCall = "epas_test12.supresseserrors";
        string pkgNameDelete = "epas_test12";

        DoSupressTest(pkg, pkgBody, pkgNameCall, pkgNameDelete, true, "P0002: query returned no rows");
    }

    [Test]
    public void TestProcedureOutsidePkg()
    {
        //Note: Procedure select from table but the query returns nothing.

        string pkgBody = "CREATE PROCEDURE supresseserrors() IS\n" +
                         "captureVar varchar2(30);\n" +
                         "begin\n" +
                            "select title into captureVar\n" +
                            "from ca.epas_test_table2\n" +
                            "where id = 10;\n" +
                         "end;";

        string pkgNameCall = "supresseserrors";

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        DoSupressTest(null, pkgBody, pkgNameCall, null, true, "P0002: query returned no rows");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
#pragma warning restore CS8602

