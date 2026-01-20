using System;
using NUnit.Framework;
using System.Data;
using System.Threading;

//EC-2573: Regression Tests for Exception in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL;

[NonParallelizable]
internal class EDBControlStructuresRaiseAppErrTest : EPASTestBase
{
    EDBConnection? conn = null;

    //This is not normal Setup method. We call it explicitly
    public void Init(string table, string proc)
    {
        conn = OpenConnection();

        Execute("DROP PROCEDURE " + proc);
        Execute("DROP TABLE " + table + " CASCADE");

        Execute("CREATE TABLE " + table + "(empno NUMBER(8),  ename VARCHAR2(10),job VARCHAR2(9), "
                            + "mgr NUMBER(8), hiredate DATE, sal NUMBER(10,2))");

        Execute("INSERT INTO " + table + "(empno,ename,job,mgr, hiredate, sal) "
            + "VALUES(7369,'SMITH','Sales',200,to_date('01-11-07','DD-MM-YY'),800)");
        Execute("INSERT INTO " + table + "(empno,ename,job,mgr, hiredate, sal) "
            + "VALUES(7499,null,'Sales',200,to_date('01-11-07','DD-MM-YY'),1600)");
        Execute("INSERT INTO " + table + "(empno,ename,job,mgr, hiredate, sal) "
            + "VALUES(7521,'WARD',null,200,to_date('01-11-07','DD-MM-YY'),1250)");
        Execute("INSERT INTO " + table + "(empno,ename,job,mgr, hiredate, sal) "
            + "VALUES(7566,'JONES','Sales',null,to_date('01-11-07','DD-MM-YY'),2975)");
        Execute("INSERT INTO " + table + "(empno,ename,job,mgr, hiredate, sal) "
            + "VALUES(7654,'MARTIN','Sales',200,null,1250)");

        var purPro = "CREATE OR REPLACE PROCEDURE " + proc + " (\n"
            + "    p_empno         NUMBER\n"
            + ")\n"
            + "IS\n"
            + "    v_ename         " + table + ".ename%TYPE;\n"
            + "    v_job           " + table + ".job%TYPE;\n"
            + "    v_mgr           " + table + ".mgr%TYPE;\n"
            + "    v_hiredate      " + table + ".hiredate%TYPE;\n"
            + "BEGIN\n"
            + "    SELECT ename, job, mgr, hiredate\n"
            + "        INTO v_ename, v_job, v_mgr, v_hiredate FROM " + table + "\n"
            + "        WHERE empno = p_empno;\n"
            + "    IF v_ename IS NULL THEN\n"
            + "        RAISE_APPLICATION_ERROR(-20010, 'No name for ' || p_empno);\n"
            + "    END IF;\n"
            + "    IF v_job IS NULL THEN\n"
            + "        RAISE_APPLICATION_ERROR(-20020, 'No job for ' || p_empno);\n"
            + "    END IF;\n"
            + "    IF v_mgr IS NULL THEN\n"
            + "        RAISE_APPLICATION_ERROR(-20030, 'No manager for ' || p_empno);\n"
            + "    END IF;\n"
            + "    IF v_hiredate IS NULL THEN\n"
            + "        RAISE_APPLICATION_ERROR(-20040, 'No hire date for ' || p_empno);\n"
            + "    END IF;\n"
            + "    DBMS_OUTPUT.PUT_LINE('Employee ' || p_empno ||\n"
            + "        ' validated without errors');\n"
            + "EXCEPTION\n"
            + "    WHEN OTHERS THEN\n"
            + "        DBMS_OUTPUT.PUT_LINE('SQLCODE: ' || SQLCODE);\n"
            + "        DBMS_OUTPUT.PUT_LINE('SQLERRM: ' || SQLERRM);\n"
            + "END;";
        Execute(purPro);
    }

    [TearDown]
    public void Dispose()
    {
        TestUtil.closeDB(conn);
        conn?.Dispose();
    }

    private void Execute(string query)
    {
        try
        {
            using var com = new EDBCommand(query, conn);
            com.CommandType = CommandType.Text;
            com.ExecuteNonQuery();
        }
        catch
        {
            // Ignore
        }
    }

    [Test]
    public void VerifyEmpTest()
    {
        Init("emp1", "verify_emp1");
        //Verify employee with correct information
        var mre = new ManualResetEvent(false);
        PostgresNotice? notice = null;
        void action(object sender, EDBNoticeEventArgs args)
        {
            Assert.That(args.Notice, Is.Not.Null);
            notice = args.Notice;
            mre.Set();
        }
        conn!.Notice += action;
        try
        {
            using (var cstmt = new EDBCommand("verify_emp1(:param1)", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 7369));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.That(notice, Is.Not.Null);
            Assert.That(notice!.MessageText, Is.EqualTo("Employee 7369 validated without errors"));
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }
    [Test]
    public void VerifyEmpNoNameTest()
    {
        Init("emp2", "verify_emp2");
        //Verify employee with no name error
        var mre = new ManualResetEvent(false);
        PostgresNotice? notice = null;
        void action(object sender, EDBNoticeEventArgs args)
        {
            Assert.That(args.Notice, Is.Not.Null);
            notice = args.Notice;
            mre.Set();
        }
        conn!.Notice += action;
        try
        {
            using (var cstmt = new EDBCommand("verify_emp2(:param1)", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 7499));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
            mre.WaitOne(5000);
            Assert.That(notice, Is.Not.Null);
            Assert.That(notice!.MessageText, Is.EqualTo("SQLERRM: EDB-20010: No name for 7499"));
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }
}
