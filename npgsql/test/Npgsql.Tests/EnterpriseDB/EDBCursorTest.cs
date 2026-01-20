using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

#pragma warning disable CS8604
/// <summary>
/// Testing Procedures with Different combination of parameters
/// </summary>
[TestFixture]
public class EDBCursorTest : EPASTestBase
{
    EDBConnection? con = null;

    [SetUp]
    public void SetUp()
    {
        con = OpenConnection();

        var com = new EDBCommand("", con)
        {
            CommandType = CommandType.Text
        };

        var strSql = "CREATE OR REPLACE PROCEDURE CUR_TEST(v_id OUT NUMERIC)\n"
                        + "IS\n"
                        //							+ "v_id number;\n"
                        + "v_name varchar2(20);\n"
                        + "CURSOR c_name IS SELECT EMPNO,ENAME FROM EMP where EMPNO=7521;\n"
                        + "BEGIN\n"
                        + "OPEN c_name;\n"
                        + "FETCH c_name into v_id,v_name;\n"
                        + "CLOSE c_name;\n"
                        + "DBMS_OUTPUT.PUT_LINE(v_id);\n"
                        + "DBMS_OUTPUT.PUT_LINE(v_name);\n"
                        + "END;\n";
        com.CommandText = strSql;
        com.ExecuteNonQuery();

    }

    [TearDown]
    public void Dispose()
    {
        var com = new EDBCommand("", con)
        {
            CommandType = CommandType.Text,

            CommandText = "DROP PROCEDURE CUR_TEST;"
        };
        com.ExecuteNonQuery();

        TestUtil.closeDB(con);
        con?.Dispose();
    }

    [Test]
    public void TestSelect() //Have to change the dependancy on emp table
    {
        var command = new EDBCommand("CUR_TEST(:v_id)", con)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new EDBParameter("v_id",
            EDBTypes.EDBDbType.Numeric, 10, "v_id",
            ParameterDirection.Output, false, 2, 2,
            System.Data.DataRowVersion.Current, 1));

        command.Prepare();
        command.ExecuteNonQuery();

        Assert.That(int.Parse(command.Parameters[0].Value!.ToString()), Is.EqualTo(7521));
    }
}
#pragma warning restore CS8604

