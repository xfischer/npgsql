using System;
using NUnit.Framework;
using System.Data;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

[TestFixture]
[NonParallelizable]

public class EDBDBFeaturesTest : EPASTestBase
{    
        
    [Test]
    public void TestSynonyms()
    {
        using var con = OpenConnection();

        var sql = "CREATE OR REPLACE PUBLIC SYNONYM employee FOR emp;";
        var select = "SELECT * FROM employee";

        var cmd = new EDBCommand(sql, con);
        cmd.ExecuteNonQuery();

        var selectCmd = new EDBCommand(select, con);
        var reader = selectCmd.ExecuteReader();

        var fc = reader.FieldCount;
        if (fc <= 0)
            Assert.Fail();
        reader.Close();
    }

    [Test]
    public void TestSelectINTO()
    {
        using var con = OpenConnection();

        TestUtil.dropTable(con, "empTemp");
        var sql = "SELECT ENAME,EMPNO,DEPTNO INTO empTemp FROM emp WHERE deptno  IN (10,20)";
        var selectSql = "select * from empTemp";

        var cmd = new EDBCommand(sql, con);

        cmd.ExecuteNonQuery();

        var selectCmd = new EDBCommand(selectSql, con);
        var reader = selectCmd.ExecuteReader();

        var fc = reader.FieldCount;
        Console.WriteLine(fc);
        reader.Close();
        if (fc < 3)
            Assert.Fail();

        var deleteSql = "drop table empTemp";
        cmd.CommandText = deleteSql;
        cmd.ExecuteNonQuery();
    }

    [Test]
    public void TestDefaultwithOneParameter()
    {
        Assert.DoesNotThrow(() =>
        {
            using var con = OpenConnection();

            var defaultProcedure = "create or replace procedure defaultProc(y varchar, x integer default 20 + 1) is " +
                                        " begin " +
                                        " dbms_output.put_line('value of y = ' || y ); " +
                                        " dbms_output.put_line('value of x = ' || x); " +
                                            " declare " +
                                            " res integer := x + 10/2; " +
                                            " begin " +
                                                " dbms_output.put_line('res = ' || res); " +
                                            " end; " +
                                        "end;";
            var cmd = new EDBCommand(defaultProcedure, con);
            cmd.ExecuteNonQuery();

            var cmdProc = new EDBCommand("defaultProc(:y)", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmdProc.Parameters.Add(new EDBParameter("y", EDBTypes.EDBDbType.Varchar, 10, "y", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, "test"));
            cmdProc.Prepare();
            cmdProc.Parameters[0].Value = "hello";
            cmdProc.ExecuteNonQuery();
        });
    }

    [Test]
    public void UserDefinedType()
    {
        Assert.DoesNotThrow(() =>
        {
            using var con = OpenConnection();

            var sql = "CREATE OR REPLACE TYPE PERSONOBJ AS OBJECT (\n"
              + "  first_name  VARCHAR2(50),\n"
              + "  last_name   VARCHAR2(50),\n"
              + "  date_of_birth  DATE,\n"
              + "  MEMBER FUNCTION getAge RETURN NUMBER\n"
              + ");";
            var cmd = new EDBCommand(sql, con);
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            sql = "DROP TYPE PERSONOBJ;";
            cmd = new EDBCommand(sql, con);
            cmd.ExecuteNonQuery();
        });
    }
}
