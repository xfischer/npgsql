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

//EC-2581: Regression Tests for Transaction Control in SPL

//These tests are implemented in JDBC as Anonymous block.
//We have implemented them in stored procedures because anonymous blocks do not work in .NET.

namespace EnterpriseDB.EDBClient.Tests.SPL;

[NonParallelizable]
internal class EDBTransactionControlCommitRollbackTest : EPASTestBase
{
    EDBConnection? conn = null;

    private static string[] INSERT_DEPT_RESULT = {
        "SQLERRM: value too long for type character varying(14)",
        "SQLCODE: -6502"
        };
    private static string[] INSERT_EMP_RESULT = {
        "Add employee: 9601",
        "Add employee: 9602"
        };
    private static string[] INSERT_EMP_FAILED_RESULT = {
        "Add employee: 9603",
        "SQLERRM: insert or update on table \"emp1\" violates foreign "
        + "key constraint \"emp1_deptno_fkey\"",
        "An error occurred - roll back inserts" };
    private static int DEPT_COUNT_COMMIT = 6;
    private static int DEPT_COUNT_ROLLBACK = 4;
    private static int EMP_COUNT_SUCCESS = 2;
    private static int EMP_COUNT_ROLLBACK = 0;

    [SetUp]
    public void Init()
    {
        conn = OpenConnection();

        Execute("DROP PROCEDURE emp_insert;");
        TestUtil.dropTable(conn, "dept1 CASCADE");
        TestUtil.dropTable(conn, "emp1 CASCADE");

        Execute("CREATE TABLE dept1(deptno NUMBER(8) UNIQUE,dname VARCHAR2(14),"
                                         + " loc  VARCHAR2(13))");
        Execute("CREATE TABLE emp1(empno NUMBER(8),  ename VARCHAR2(10),job VARCHAR2(9), "
                        + "mgr NUMBER(8), hiredate DATE, sal NUMBER(10,2), comm NUMBER(10,2), "
                        + "deptno NUMBER(8) references dept1(deptno))");
        Execute("SET edb_stmt_level_tx TO on;");
        Execute("insert into dept1 (deptno, dname, loc) values "
                + "(10, 'ACCOUNTING','NEW YORK');");
        Execute("insert into dept1 (deptno, dname, loc) values "
                + "(20, 'RESEARCH','DALLAS');");
        Execute("insert into dept1 (deptno, dname, loc) values "
                + "(30, 'SALES','CHICAGO');");
        Execute("insert into dept1 (deptno, dname, loc) values "
                + "(40, 'OPERATIONS','BOSTON');");
        //The following stored procedure is created. It inserts a new employee.
        var insertSql = "CREATE OR REPLACE PROCEDURE emp_insert (\n"
                         + "    p_empno         IN emp1.empno%TYPE,\n"
                         + "    p_ename         IN emp1.ename%TYPE,\n"
                         + "    p_job           IN emp1.job%TYPE,\n"
                         + "    p_mgr           IN emp1.mgr%TYPE,\n"
                         + "    p_hiredate      IN emp1.hiredate%TYPE,\n"
                         + "    p_sal           IN emp1.sal%TYPE,\n"
                         + "    p_comm          IN emp1.comm%TYPE,\n"
                         + "    p_deptno        IN emp1.deptno%TYPE\n"
                         + ")\n"
                         + "IS\n"
                         + "BEGIN\n"
                         + "    INSERT INTO emp1 VALUES (\n"
                         + "        p_empno,\n"
                         + "        p_ename,\n"
                         + "        p_job,\n"
                         + "        p_mgr,\n"
                         + "        p_hiredate,\n"
                         + "        p_sal,\n"
                         + "        p_comm,\n"
                         + "        p_deptno);\n"
                         + "DBMS_OUTPUT.PUT_LINE('Add employee: ' || p_empno);"
                         + "END;";
        Execute(insertSql);
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

    private int getDeptCount()
    {
        var command = "select count(*) from dept1";

        var selectCommand = new EDBCommand(command, conn);
        var selectResult = selectCommand.ExecuteReader();
        selectResult.Read();
        var count = selectResult.GetInt32(0);
        selectResult.Close();

        return count;
    }

    private int getEmpCount()
    {
        var command = "select count(*) from emp1";
        var selectCommand = new EDBCommand(command, conn);
        var selectResult = selectCommand.ExecuteReader();
        selectResult.Read();
        var count = selectResult.GetInt32(0);
        selectResult.Close();

        return count;
    }

    [Test]
    public void InsertDeptCommitTest()
    {
        //In this example, the third INSERT command in the anonymous block results
        //in an error. The effect of the first two INSERT commands is retained as
        //shown by the first SELECT command. Even after issuing a ROLLBACK command,
        //the two rows remain in the table.
        Execute("DROP PROCEDURE InsertDeptCommit_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE InsertDeptCommit_SP()\n"
                  + " IS\n"
                  + " BEGIN\n"
        //var sqlStr = "BEGIN\n"
                  + "    INSERT INTO dept1 VALUES (50, 'FINANCE', 'DALLAS');\n"
                  + "    INSERT INTO dept1 VALUES (60, 'MARKETING', 'CHICAGO');\n"
                  + "    COMMIT;\n"
                  + "    INSERT INTO dept1 VALUES (70, 'HUMAN RESOURCES', 'CHICAGO');\n"
                  + "EXCEPTION\n"
                  + "    WHEN OTHERS THEN\n"
                  + "        DBMS_OUTPUT.PUT_LINE('SQLERRM: ' || SQLERRM);\n"
                  + "        DBMS_OUTPUT.PUT_LINE('SQLCODE: ' || SQLCODE);\n"
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
            using (var cstmt = new EDBCommand("InsertDeptCommit_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.AreEqual(INSERT_DEPT_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(INSERT_DEPT_RESULT[i], notice.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();

        var count = getDeptCount();
        Assert.AreEqual(DEPT_COUNT_COMMIT, count);
    }

    [Test]
    public void InsertDeptRollbackTest()
    {
        //In this example, the exception section contains a ROLLBACK command. Even though
        //the first two INSERT commands execute successfully, the third causes an exception
        //that results in the rollback of all the INSERT commands in the anonymous block.
        Execute("DROP PROCEDURE InsertDeptRollback_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE InsertDeptRollback_SP()\n"
                  + " IS\n"
                  + " BEGIN\n"
                  + "    INSERT INTO dept1 VALUES (50, 'FINANCE', 'DALLAS');\n"
                  + "    INSERT INTO dept1 VALUES (60, 'MARKETING', 'CHICAGO');\n"
                  + "    INSERT INTO dept1 VALUES (70, 'HUMAN RESOURCES', 'CHICAGO');\n"
                  + "EXCEPTION\n"
                  + "    WHEN OTHERS THEN\n"
                  + "        ROLLBACK;\n"
                  + "        DBMS_OUTPUT.PUT_LINE('SQLERRM: ' || SQLERRM);\n"
                  + "        DBMS_OUTPUT.PUT_LINE('SQLCODE: ' || SQLCODE);\n"
                  + "END;\n";

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
            using (var cstmt = new EDBCommand("InsertDeptRollback_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.AreEqual(INSERT_DEPT_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(INSERT_DEPT_RESULT[i], notice.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();

        var count = getDeptCount();
        Assert.AreEqual(DEPT_COUNT_ROLLBACK, count);
    }

    [Test]
    public void InsertEmpTest()
    {
        //Then the following anonymous block runs. The COMMIT command is used after all calls
        //to the emp_insert procedure and the ROLLBACK command in the exception section.
        Execute("DROP PROCEDURE InsertEmp_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE InsertEmp_SP()\n"
                  + " IS\n"
                  + " BEGIN\n"
                  + "    emp_insert(9601,'FARRELL','ANALYST',7902,'03-MAR-08',5000,NULL,40);\n"
                  + "    emp_insert(9602,'TYLER','ANALYST',7900,'25-JAN-08',4800,NULL,40);\n"
                  + "    COMMIT;\n"
                  + "EXCEPTION\n"
                  + "    WHEN OTHERS THEN\n"
                  + "        DBMS_OUTPUT.PUT_LINE('SQLERRM: ' || SQLERRM);\n"
                  + "        DBMS_OUTPUT.PUT_LINE('An error occurred - roll back inserts');\n"
                  + "        ROLLBACK;\n"
                  + "END;\n";

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
            using (var cstmt = new EDBCommand("InsertEmp_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.AreEqual(INSERT_EMP_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(INSERT_EMP_RESULT[i], notice.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();

        var count = getEmpCount();
        Assert.AreEqual(EMP_COUNT_SUCCESS, count);
    }

    [Test]
    public void InsertEmpRollBackTest()
    {
        //The ROLLBACK command in the exception section successfully undoes
        //the insert of employee Harrison.
        Execute("DROP PROCEDURE InsertEmpRollBack_SP;");

        var sqlStr = "CREATE OR REPLACE PROCEDURE InsertEmpRollBack_SP()\n"
                  + " IS\n"
                  + " BEGIN\n"
                  + "    emp_insert(9603,'HARRISON','SALESMAN',7902,'13-DEC-07',5000,3000,20);\n"
                  + "    emp_insert(9604,'JARVIS','SALESMAN',7902,'05-MAY-08',4800,4100,11);\n"
                  + "    COMMIT;\n"
                  + "EXCEPTION\n"
                  + "    WHEN OTHERS THEN\n"
                  + "        DBMS_OUTPUT.PUT_LINE('SQLERRM: ' || SQLERRM);\n"
                  + "        DBMS_OUTPUT.PUT_LINE('An error occurred - roll back inserts');\n"
                  + "        ROLLBACK;\n"
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
            using (var cstmt = new EDBCommand("InsertEmpRollBack_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.AreEqual(INSERT_EMP_FAILED_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(INSERT_EMP_FAILED_RESULT[i], notice.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();

        var count = getEmpCount();
        Assert.AreEqual(EMP_COUNT_ROLLBACK, count);
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
