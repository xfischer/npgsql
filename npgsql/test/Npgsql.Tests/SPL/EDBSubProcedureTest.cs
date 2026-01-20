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

//EC-2588: Regression Tests for Subprocedure

namespace EnterpriseDB.EDBClient.Tests.SPL;

[NonParallelizable]
internal class EDBSubProcedureTest : EPASTestBase
{
    EDBConnection? conn = null;

    private static string[] enames = new string[] { "SMITH", "ALLEN", "WARD", "JONES",
        "MARTIN", "BLAKE", "CLARK", "SCOTT", "KING", "TURNER" };
    private static int[] empnos = new int[] { 7369, 7499, 7521, 7566, 7654, 7698, 7782,
         7788, 7839, 7844 };
    private static int EMP_TOTAL = empnos.Length;

    [SetUp]
    public void Init()
    {
        conn = OpenConnection();

        Execute("DROP trigger dept_audit_trig on dept1;");
        TestUtil.dropTable(conn, "dept1 CASCADE");
        TestUtil.dropTable(conn, "emp1 CASCADE");

        Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(10))");
        for (var i = 0; i < EMP_TOTAL; i++)
        {
            var addCommand = "INSERT INTO emp1(empno,ename) VALUES(:empno, :ename)";
            using (var cstmt = new EDBCommand(addCommand, conn))
            {
                cstmt.CommandType = CommandType.Text;
                cstmt.Parameters.Add(new EDBParameter("empno", EDBTypes.EDBDbType.Numeric, 10, "empno",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, empnos[i]));

                cstmt.Parameters.Add(new EDBParameter("ename", EDBTypes.EDBDbType.Varchar, 10, "ename",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, enames[i]));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
        }

        Execute("CREATE TABLE dept1(deptno NUMBER(4), dname VARCHAR2(14), loc  VARCHAR2(13))");
        //The following example is a subprocedure within a trigger.
        var trigerStr = "CREATE OR REPLACE TRIGGER dept_audit_trig\n"
                         + "    AFTER INSERT OR UPDATE OR DELETE ON dept1\n"
                         + "DECLARE\n"
                         + "    v_action        VARCHAR2(24);\n"
                         + "    PROCEDURE display_action (\n"
                         + "        p_action    IN  VARCHAR2\n"
                         + "    )\n"
                         + "    IS\n"
                         + "    BEGIN\n"
                         + "        DBMS_OUTPUT.PUT_LINE('User ' || USER || ' ' || p_action ||\n"
                         + "            ' dept on ' || TO_CHAR(SYSDATE,'YYYY-MM-DD'));\n"
                         + "    END display_action;\n"
                         + "BEGIN\n"
                         + "    IF INSERTING THEN\n"
                         + "        v_action := 'added';\n"
                         + "    ELSIF UPDATING THEN\n"
                         + "        v_action := 'updated';\n"
                         + "    ELSIF DELETING THEN\n"
                         + "        v_action := 'deleted';\n"
                         + "    END IF;\n"
                         + "    display_action(v_action);\n"
                         + "END;";
        Execute(trigerStr);
    }

    [TearDown]
    public void Dispose()
    {
        TestUtil.closeDB(conn);
        conn?.Dispose();
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

    private string getMessgeFromTrigerTest(string sqlStr)
    {
        var msg = "";
        //Invoking this trigger produces with insert, update or delete statement,
        //get output message
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
            using (var cstmt = new EDBCommand(sqlStr, conn))
            {
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.That(notices.Count, Is.EqualTo(1));
            var notice = (PostgresNotice?)notices[0];
            msg = notice.MessageText;
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
        return msg;
    }

    [Test]
    public void SimpleSubProcedureTest()
    {
        //This test is implemented in JDBC as Anonymous block.
        //We have implemented it in a stored procedure because anonymous blocks do not work in .NET.

        Execute("DROP PROCEDURE SimpleSubProcedure_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE SimpleSubProcedure_SP()\n"
                  + " IS\n"
                  + " DECLARE\n"
                  + "    PROCEDURE list_emp\n"
                  + "    IS\n"
                  + "        v_empno     NUMBER(4);\n"
                  + "        v_ename     VARCHAR2(10);\n"
                  + "        CURSOR emp_cur IS\n"
                  + "            SELECT empno, ename FROM emp1 ORDER BY empno;\n"
                  + "    BEGIN\n"
                  + "        OPEN emp_cur;\n"
                  + "        LOOP\n"
                  + "            FETCH emp_cur INTO v_empno, v_ename;\n"
                    + "            EXIT WHEN emp_cur%NOTFOUND;\n"
                  + "            DBMS_OUTPUT.PUT_LINE(v_empno || ':' || v_ename);\n"
                    + "        END LOOP;\n"
                  + "        CLOSE emp_cur;\n"
                    + "    END;\n"
                  + "BEGIN\n"
                    + "    list_emp;\n"
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
            using (var cstmt = new EDBCommand("SimpleSubProcedure_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.That(notices.Count, Is.EqualTo(EMP_TOTAL));
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                var value = notice.MessageText;
                var arr = value.Split(":");
                var empno = arr[0].Trim();
                var ename = arr[1].Trim();
                Assert.That(empnos[i].ToString(), Is.EqualTo(empno));
                Assert.That(enames[i], Is.EqualTo(ename));
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }

    [Test]
    public void AddDeptTrigerTest()
    {
        //Invoking this trigger produces with insert statement
        var sqlStr = "INSERT INTO dept1 VALUES (50,'HR','DENVER');";
        var msg = getMessgeFromTrigerTest(sqlStr);
        var expMsg = "User " + conn.UserName + " added dept on";
        Assert.That(msg.StartsWith(expMsg));
    }

    [Test]
    public void UpdateDeptTrigerTest()
    {
        //Invoking this trigger produces with update statement
        var sqlStr = "update dept1 set loc='Boston' where deptno=50;";
        var msg = getMessgeFromTrigerTest(sqlStr);
        var expMsg = "User " + conn.UserName + " updated dept on";
        Assert.That(msg.StartsWith(expMsg));
    }

    [Test]
    public void DeleteDeptTrigerTest()
    {
        //Invoking this trigger produces with delete statement
        var sqlStr = "delete from dept1 where deptno=50;";
        var msg = getMessgeFromTrigerTest(sqlStr);
        var expMsg = "User " + conn.UserName + " deleted dept on";
        Assert.That(msg.StartsWith(expMsg));
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

