using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Collections;
using System.Threading;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//ED-2590: Regression Tests for Varray

namespace EnterpriseDB.EDBClient.Tests.SPL;

[NonParallelizable]
internal class EDBVarrayTest : EPASTestBase
{
    EDBConnection? conn = null;

    private static string[] deptNames = new string[] { "ACCOUNTING", "OPERATIONS", "RESEARCH", "SALES" };
    private static string[] deptLocs = new string[] { "NEW YORK", "BOSTON", "DALLAS", "CHICAGO" };
    private static int DEPT_TOTAL = deptNames.Length;

    [SetUp]
    public void Init()
    {
        conn = OpenConnection();

        TestUtil.dropTable(conn, "dept1 CASCADE");

        Execute("CREATE TABLE dept1(dname VARCHAR2(14),  loc  VARCHAR2(13))");
        for (var i = 0; i < DEPT_TOTAL; i++)
        {
            var addCommand = "INSERT INTO dept1(dname,loc) VALUES(:dname,:loc)";
            using (var cstmt = new EDBCommand(addCommand, conn))
            {
                cstmt.CommandType = CommandType.Text;
                cstmt.Parameters.Add(new EDBParameter("dname", EDBTypes.EDBDbType.Varchar, 10, "dname",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, deptNames[i]));

                cstmt.Parameters.Add(new EDBParameter("loc", EDBTypes.EDBDbType.Varchar, 10, "loc",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, deptLocs[i]));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
        }
    }

    [TearDown]
    public void Dispose()
    {
        TestUtil.closeDB(conn);
    }

    private int Execute(string query)
    {
        try
        {
            using (var com = new EDBCommand(query, conn))
            {
                com.CommandType = CommandType.Text;
                return com.ExecuteNonQuery();
            }
        }
        catch
        {
        }

        return 0;
    }

    [Test]
    public void VarrayTest()
    {
        Execute("DROP PROCEDURE Varray_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE Varray_SP()\n"
            + " IS\n"
            + " DECLARE\n"
            + "    TYPE dname_varray_typ IS VARRAY(4) OF VARCHAR2(14);\n"
            + "    dname_varray    dname_varray_typ;\n"
            + "    CURSOR dept_cur IS SELECT dname FROM dept1 ORDER BY dname;\n"
            + "    i               INTEGER := 0;\n"
            + "BEGIN\n"
            + "    dname_varray := dname_varray_typ(NULL, NULL, NULL, NULL);\n"
            + "    FOR r_dept IN dept_cur LOOP\n"
            + "        i := i + 1;\n"
            + "        dname_varray(i) := r_dept.dname;\n"
            + "    END LOOP;\n"
            + "    FOR j IN 1..i LOOP\n"
            + "        DBMS_OUTPUT.PUT_LINE(dname_varray(j));\n"
            + "    END LOOP;\n"
            + "END;";

        Execute(sqlStr);

        var mre = new ManualResetEvent(false);
        var notices = new ArrayList();
        NoticeEventHandler action = (sender, args) =>
        {
            notices.Add(args.Notice);
            mre.Set();
        };
        conn.Notice += action;
        try
        {
            using (var cstmt = new EDBCommand("Varray_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.AreEqual(DEPT_TOTAL, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(deptNames[i], notice.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
