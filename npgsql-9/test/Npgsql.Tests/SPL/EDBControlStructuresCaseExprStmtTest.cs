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

//EC-2583: Regression tests for Case Expression and Case Statement in SPL

//These tests are implemented in JDBC as Anonymous block.
//We have implemented them in stored procedures because anonymous blocks do not work in .NET.

namespace EnterpriseDB.EDBClient.Tests.SPL;

[NonParallelizable]
internal class EDBControlStructuresCaseExprStmtTest : EPASTestBase
{
    EDBConnection? conn = null;

    private static string[] CASE_EXPR_RESULT = {
        "7369 SMITH      20 Research",
        "7499 ALLEN      30 Sales",
        "7521 WARD       30 Sales",
        "7566 JONES      20 Research",
        "7654 MARTIN     30 Sales",
        "7698 BLAKE      30 Sales",
        "7782 CLARK      10 Accounting",
        "7788 SCOTT      20 Research",
        "7839 KING       10 Accounting",
        "7844 TURNER     30 Sales",
        "7876 ADAMS      20 Research",
        "7900 JAMES      30 Sales",
        "7902 FORD       20 Research",
        "7934 MILLER     10 Accounting",
        };
    private static string[] CASE_STMT_RESULT = {
        "7369 SMITH      20 Research     Dallas",
        "7499 ALLEN      30 Sales        Chicago",
        "7521 WARD       30 Sales        Chicago",
        "7566 JONES      20 Research     Dallas",
        "7654 MARTIN     30 Sales        Chicago",
        "7698 BLAKE      30 Sales        Chicago",
        "7782 CLARK      10 Accounting   New York",
        "7788 SCOTT      20 Research     Dallas",
        "7839 KING       10 Accounting   New York",
        "7844 TURNER     30 Sales        Chicago",
        "7876 ADAMS      20 Research     Dallas",
        "7900 JAMES      30 Sales        Chicago",
        "7902 FORD       20 Research     Dallas",
        "7934 MILLER     10 Accounting   New York", };

    [SetUp]
    public void Init()
    {
        conn = OpenConnection();

        Execute("DROP TABLE emp1");

        Execute("CREATE TABLE emp1(empno NUMBER(8),  ename VARCHAR2(20),"
                                     + "sal NUMBER(10,2), deptno NUMBER(8))");
        var add = new string[] {
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7369,'SMITH',800,20)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7499,'ALLEN',1600,30)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7521,'WARD',1250,30)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7566,'JONES',2975,20)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7654,'MARTIN',1250,30)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7698,'BLAKE',2850,30)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7782,'CLARK',2450,10)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7788,'SCOTT',3000,20)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7839,'KING',5000,10)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7844,'TURNER',1500,30)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7876,'ADAMS',1100,20)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7900,'JAMES',950,30)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7902,'FORD',3000,20)",
            "INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7934,'MILLER',1300,10)"
            };
        for (var i = 0; i < add.Length; i++)
        {
            Execute(add[i]);
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
    public void SelectorCaseExprTest()
    {
        //The following example uses a selector CASE expression to assign the department
        //name to a variable based upon the department number.
        Execute("DROP PROCEDURE SelectorCaseExpr_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE SelectorCaseExpr_SP()\n"
            + " IS\n"
            + " DECLARE\n"
            + "    v_empno         emp1.empno%TYPE;\n"
            + "    v_ename         emp1.ename%TYPE;\n"
            + "    v_deptno        emp1.deptno%TYPE;\n"
            + "    v_dname         VARCHAR2(20);\n"
            + "    CURSOR emp_cursor IS SELECT empno, ename, deptno FROM emp1 order by empno;\n"
            + "BEGIN\n"
            + "    OPEN emp_cursor;\n"
            + "    LOOP\n"
            + "        FETCH emp_cursor INTO v_empno, v_ename, v_deptno;\n"
            + "        EXIT WHEN emp_cursor%NOTFOUND;\n"
            + "        v_dname :=\n"
            + "            CASE v_deptno\n"
            + "                WHEN 10 THEN 'Accounting'\n"
            + "                WHEN 20 THEN 'Research'\n"
            + "                WHEN 30 THEN 'Sales'\n"
            + "                WHEN 40 THEN 'Operations'\n"
            + "                ELSE 'unknown'\n"
            + "            END;\n"
            + "        DBMS_OUTPUT.PUT_LINE(v_empno || ' ' || RPAD(v_ename, 10) ||\n"
            + "            ' ' || v_deptno || ' ' || v_dname);\n"
            + "    END LOOP;\n"
            + "    CLOSE emp_cursor;\n"
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
            using (var cstmt = new EDBCommand("SelectorCaseExpr_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.AreEqual(CASE_EXPR_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(CASE_EXPR_RESULT[i], notice.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }

    [Test]
    public void SearchedCaseExprTest()
    {
        //The following example uses a searched CASE expression to assign the department
        //name to a variable based upon the department number.
        Execute("DROP PROCEDURE SearchedCaseExpr_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE SearchedCaseExpr_SP()\n"
            + " IS\n"
            + " DECLARE\n"
            + "    v_empno         emp1.empno%TYPE;\n"
            + "    v_ename         emp1.ename%TYPE;\n"
            + "    v_deptno        emp1.deptno%TYPE;\n"
            + "    v_dname         VARCHAR2(20);\n"
            + "    CURSOR emp_cursor IS SELECT empno, ename, deptno FROM emp1 order by empno;\n"
            + "BEGIN\n"
            + "    OPEN emp_cursor;\n"
            + "    LOOP\n"
            + "        FETCH emp_cursor INTO v_empno, v_ename, v_deptno;\n"
            + "        EXIT WHEN emp_cursor%NOTFOUND;\n"
            + "        v_dname :=\n"
            + "            CASE\n"
            + "                WHEN v_deptno = 10 THEN 'Accounting'\n"
            + "                WHEN v_deptno = 20 THEN 'Research'\n"
            + "                WHEN v_deptno = 30 THEN 'Sales'\n"
            + "                WHEN v_deptno = 40 THEN 'Operations'\n"
            + "                ELSE 'unknown'\n"
            + "            END;\n"
            + "        DBMS_OUTPUT.PUT_LINE(v_empno || ' ' || RPAD(v_ename, 10) ||\n"
            + "            ' ' || v_deptno || ' ' || v_dname);\n"
            + "    END LOOP;\n"
            + "    CLOSE emp_cursor;\n"
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
            using (var cstmt = new EDBCommand("SearchedCaseExpr_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.AreEqual(CASE_EXPR_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(CASE_EXPR_RESULT[i], notice.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }

    [Test]
    public void SelectorCaseStmtTest()
    {
        //The following example uses a selector CASE statement to assign a department name and
        //location to a variable based upon the department number.
        Execute("DROP PROCEDURE SelectorCaseStmt_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE SelectorCaseStmt_SP()\n"
            + " IS\n"
            + " DECLARE\n"
            + "    v_empno         emp1.empno%TYPE;\n"
            + "    v_ename         emp1.ename%TYPE;\n"
            + "    v_deptno        emp1.deptno%TYPE;\n"
            + "    v_dname         VARCHAR2(20);\n"
            + "    v_loc           VARCHAR2(20);\n"
            + "    CURSOR emp_cursor IS SELECT empno, ename, deptno FROM emp1 order by empno;\n"
            + "BEGIN\n"
            + "    OPEN emp_cursor;\n"
            + "    LOOP\n"
            + "        FETCH emp_cursor INTO v_empno, v_ename, v_deptno;\n"
            + "        EXIT WHEN emp_cursor%NOTFOUND;\n" + "        CASE\n"
            + "            WHEN v_deptno = 10 THEN v_dname := 'Accounting';\n"
            + "                                    v_loc   := 'New York';\n"
            + "            WHEN v_deptno = 20 THEN v_dname := 'Research';\n"
            + "                                    v_loc   := 'Dallas';\n"
            + "            WHEN v_deptno = 30 THEN v_dname := 'Sales';\n"
            + "                                    v_loc   := 'Chicago';\n"
            + "            WHEN v_deptno = 40 THEN v_dname := 'Operations';\n"
            + "                                    v_loc   := 'Boston';\n"
            + "            ELSE v_dname := 'unknown';\n"
            + "                                    v_loc   := '';\n"
            + "        END CASE;\n"
            + "        DBMS_OUTPUT.PUT_LINE(v_empno || ' ' || RPAD(v_ename, 10) ||\n"
            + "            ' ' || v_deptno || ' ' || RPAD(v_dname, 12) || ' ' ||\n"
            + "            v_loc);\n"
            + "    END LOOP;\n"
            + "    CLOSE emp_cursor;\n"
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
            using (var cstmt = new EDBCommand("SelectorCaseStmt_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.AreEqual(CASE_STMT_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(CASE_STMT_RESULT[i], notice.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }

    [Test]
    public void SearchedCaseStmtTest()
    {
        //The following example uses a searched CASE statement to assign a department
        //name and location to a variable based upon the department number.
        Execute("DROP PROCEDURE SearchedCaseStmt_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE SearchedCaseStmt_SP()\n"
            + " IS\n"
            + " DECLARE\n"
            + "    v_empno         emp1.empno%TYPE;\n"
            + "    v_ename         emp1.ename%TYPE;\n"
            + "    v_deptno        emp1.deptno%TYPE;\n"
            + "    v_dname         VARCHAR2(20);\n"
            + "    v_loc           VARCHAR2(20);\n"
            + "    CURSOR emp_cursor IS SELECT empno, ename, deptno FROM emp1 order by empno;\n"
            + "BEGIN\n"
            + "    OPEN emp_cursor;\n"
            + "    LOOP\n"
            + "        FETCH emp_cursor INTO v_empno, v_ename, v_deptno;\n"
            + "        EXIT WHEN emp_cursor%NOTFOUND;\n"
            + "        CASE v_deptno\n"
            + "            WHEN 10 THEN v_dname := 'Accounting';\n"
            + "                         v_loc   := 'New York';\n"
            + "            WHEN 20 THEN v_dname := 'Research';\n"
            + "                         v_loc   := 'Dallas';\n"
            + "            WHEN 30 THEN v_dname := 'Sales';\n"
            + "                         v_loc   := 'Chicago';\n"
            + "            WHEN 40 THEN v_dname := 'Operations';\n"
            + "                         v_loc   := 'Boston';\n"
            + "            ELSE v_dname := 'unknown';\n"
            + "                         v_loc   := '';\n"
            + "        END CASE;\n"
            + "        DBMS_OUTPUT.PUT_LINE(v_empno || ' ' || RPAD(v_ename, 10) ||\n"
            + "            ' ' || v_deptno || ' ' || RPAD(v_dname, 12) || ' ' ||\n"
            + "            v_loc);\n"
            + "    END LOOP;\n"
            + "    CLOSE emp_cursor;\n"
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
            using (var cstmt = new EDBCommand("SearchedCaseStmt_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.AreEqual(CASE_STMT_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(CASE_STMT_RESULT[i], notice.MessageText);
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

