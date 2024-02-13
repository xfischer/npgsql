using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Threading;
using System.Collections;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2578: Regression Tests in REF CURSOR and Cursor Variables in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    [NonParallelizable]
    internal class EDBRefCursorTest : EPASTestBase
    {
        EDBConnection? conn = null;

        private static string[] CLOSING_CUSOR_RESULT = {
            "7369 SMITH",
            "7566 JONES",
            "7788 SCOTT",
            "7876 ADAMS",
            "7902 FORD"
            };
        private static string[] CURSOR_FUNCTION_RESULT = {
            "7499 ALLEN",
            "7521 WARD",
            "7654 MARTIN",
            "7844 TURNER"
            };
        private static string[] MODULARIZING_CURSOR_OPERATIONS_RESULT = {
            "ALL EMPLOYEES",
            "EMPNO    ENAME",
            "-----    -------",
            "7369     SMITH",
            "7499     ALLEN",
            "7521     WARD",
            "7566     JONES",
            "7654     MARTIN",
            "7698     BLAKE",
            "7782     CLARK",
            "7788     SCOTT",
            "7839     KING",
            "7844     TURNER",
            "7876     ADAMS",
            "7900     JAMES",
            "7902     FORD",
            "7934     MILLER",
            "****************",
            "EMPLOYEES IN DEPT #10",
            "EMPNO    ENAME",
            "-----    -------",
            "7782     CLARK",
            "7839     KING",
            "7934     MILLER",
            "****************",
            "DEPARTMENTS",
            "DEPT   DNAME",
            "----   ---------",
            "10     ACCOUNTING",
            "20     RESEARCH",
            "30     SALES",
            "40     OPERATIONS",
            "*****************"
            };
        private static string[] DYNAMIC_QUERIES_RESULT = {
            "7499 ALLEN",
            "7698 BLAKE",
            "7844 TURNER"
            };
        private static string[] DYNAMIC_QUERIES_FROM_STRING_RESULT = {
            "7566 JONES",
            "7788 SCOTT",
            "7902 FORD"
            };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP PROCEDURE emp_by_dept;");
            Execute("DROP FUNCTION emp_by_job;");
            Execute("DROP PROCEDURE open_all_emp;");
            Execute("DROP PROCEDURE open_emp_by_dept;");
            Execute("DROP FUNCTION open_dept;");
            Execute("DROP PROCEDURE fetch_emp;");
            Execute("DROP PROCEDURE fetch_dept;");
            Execute("DROP PROCEDURE close_refcur;");
            Execute("DROP PROCEDURE dept_query;");
            Execute("DROP PROCEDURE dept_query_with_parameters;");
            Execute("DROP PROCEDURE dept_query_from_string;");

            TestUtil.dropTable(conn, "dept1 CASCADE");
            TestUtil.dropTable(conn, "emp1 CASCADE");

            Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(20), job VARCHAR2(20), sal NUMBER(10,2), deptno NUMBER(4))");
            Execute("CREATE TABLE dept1(deptno NUMBER(4), dname VARCHAR2(14))");

            var addEmp = new string[]{
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7369,'SMITH','SOFTWARE ENGINEER',800,20)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7499,'ALLEN','SALESMAN',1600,30)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7521,'WARD','SALESMAN',1250,30)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7566,'JONES','SOFTWARE ENGINEER',2975,20)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7654,'MARTIN','SALESMAN',1250,30)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7698,'BLAKE','CLERK',2850,30)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7782,'CLARK','CLERK',2450,10)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7788,'SCOTT','SOFTWARE ENGINEER',3000,20)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7839,'KING','CLERK',5000,10)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7844,'TURNER','SALESMAN',1500,30)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7876,'ADAMS','SOFTWARE ENGINEER',1100,20)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7900,'JAMES','CLERK',950,30)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7902,'FORD','SOFTWARE ENGINEER',3000,20)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(7934,'MILLER','CLERK',1300,10)" };
            for (var i = 0; i < addEmp.Length; i++)
            {
                Execute(addEmp[i]);
            }
            var addDept = new string[]{
                "INSERT INTO dept1(deptno, dname) values (10,'ACCOUNTING')",
                "INSERT INTO dept1(deptno, dname) values (20,'RESEARCH')",
                "INSERT INTO dept1(deptno, dname) values (30,'SALES')",
                "INSERT INTO dept1(deptno, dname) values (40,'OPERATIONS')",
                };
            for (var i = 0; i < addDept.Length; i++)
            {
                Execute(addDept[i]);

            }

            //This example includes the CLOSE statement.
            var empByDept = "CREATE OR REPLACE PROCEDURE emp_by_dept (\n"
                             + "    p_deptno        emp1.deptno%TYPE\n"
                             + ")\n"
                             + "IS\n"
                             + "    emp_refcur      SYS_REFCURSOR;\n"
                             + "    v_empno         emp1.empno%TYPE;\n"
                             + "    v_ename         emp1.ename%TYPE;\n"
                             + "BEGIN\n"
                             + "    OPEN emp_refcur FOR SELECT empno, ename FROM emp1 WHERE deptno = p_deptno order by empno;\n"
                             + "    LOOP\n"
                             + "        FETCH emp_refcur INTO v_empno, v_ename;\n"
                             + "        EXIT WHEN emp_refcur%NOTFOUND;\n"
                             + "        DBMS_OUTPUT.PUT_LINE(v_empno || ' ' || v_ename);\n"
                             + "    END LOOP;\n"
                             + "    CLOSE emp_refcur;\n"
                             + "END;";
            Execute(empByDept);
            //This example opens the cursor variable with a query that selects employees
            //with a given job. The cursor variable is specified in this function’s RETURN
            //statement, which makes the result set available to the caller of the function.
            var empByJob = "CREATE OR REPLACE FUNCTION emp_by_job (p_job VARCHAR2)\n"
                            + "RETURN SYS_REFCURSOR\n"
                            + "IS\n"
                            + "    emp_refcur      SYS_REFCURSOR;\n"
                            + "BEGIN\n"
                            + "    OPEN emp_refcur FOR SELECT empno, ename FROM emp1 WHERE job = p_job order by empno;\n"
                            + "    RETURN emp_refcur;\n"
                            + "END;";
            Execute(empByJob);
            //The following procedure opens the given cursor variable with
            //a SELECT command that retrieves all rows.
            var openAllEmp = "CREATE OR REPLACE PROCEDURE open_all_emp (\n"
                              + "    p_emp_refcur    IN OUT SYS_REFCURSOR\n"
                              + ")\n"
                              + "IS\n"
                              + "BEGIN\n"
                              + "    OPEN p_emp_refcur FOR SELECT empno, ename FROM emp1  order by empno;\n"
                              + "END;";
            Execute(openAllEmp);
            //This variation opens the given cursor variable with a SELECT command
            //that retrieves all rows of a given department.
            var openEmpByDept = "CREATE OR REPLACE PROCEDURE open_emp_by_dept (\n"
                                 + "    p_emp_refcur    IN OUT SYS_REFCURSOR,\n"
                                 + "    p_deptno        emp1.deptno%TYPE\n"
                                 + ")\n"
                                 + "IS\n"
                                 + "BEGIN\n"
                                 + "    OPEN p_emp_refcur FOR SELECT empno, ename FROM emp1\n"
                                 + "        WHERE deptno = p_deptno  order by empno;\n"
                                 + "END;";
            Execute(openEmpByDept);
            //This variation opens the given cursor variable with a SELECT
            //command that retrieves all rows but from a different table.
            //The function’s return value is the opened cursor variable.
            var openDept = "CREATE OR REPLACE FUNCTION open_dept (\n"
                            + "    p_dept_refcur    IN OUT SYS_REFCURSOR\n"
                            + ") RETURN SYS_REFCURSOR\n"
                            + "IS\n"
                            + "    v_dept_refcur    SYS_REFCURSOR;\n"
                            + "BEGIN\n"
                            + "    v_dept_refcur := p_dept_refcur;\n"
                            + "    OPEN v_dept_refcur FOR SELECT deptno, dname FROM dept1 order by deptno;\n"
                            + "    RETURN v_dept_refcur;\n"
                            + "END;";
            Execute(openDept);
            //This procedure fetches and displays a cursor variable result
            //set consisting of employee number and name:
            var fetctEmp = "CREATE OR REPLACE PROCEDURE fetch_emp (\n"
                            + "    p_emp_refcur    IN OUT SYS_REFCURSOR\n"
                            + ")\n"
                            + "IS\n"
                            + "    v_empno         emp1.empno%TYPE;\n"
                            + "    v_ename         emp1.ename%TYPE;\n"
                            + "BEGIN\n"
                            + "    DBMS_OUTPUT.PUT_LINE('EMPNO    ENAME');\n"
                            + "    DBMS_OUTPUT.PUT_LINE('-----    -------');\n"
                            + "    LOOP\n"
                            + "        FETCH p_emp_refcur INTO v_empno, v_ename;\n"
                            + "        EXIT WHEN p_emp_refcur%NOTFOUND;\n"
                            + "        DBMS_OUTPUT.PUT_LINE(v_empno || '     ' || v_ename);\n"
                            + "    END LOOP;\n"
                            + "END;";
            Execute(fetctEmp);
            //This procedure fetches and displays a cursor variable result
            //set consisting of department number and name
            var fetchDept = "CREATE OR REPLACE PROCEDURE fetch_dept (\n"
                             + "    p_dept_refcur   IN SYS_REFCURSOR\n"
                             + ")\n"
                             + "IS\n"
                             + "    v_deptno        dept1.deptno%TYPE;\n"
                             + "    v_dname         dept1.dname%TYPE;\n"
                             + "BEGIN\n"
                             + "    DBMS_OUTPUT.PUT_LINE('DEPT   DNAME');\n"
                             + "    DBMS_OUTPUT.PUT_LINE('----   ---------');\n"
                             + "    LOOP\n"
                             + "        FETCH p_dept_refcur INTO v_deptno, v_dname;\n"
                             + "        EXIT WHEN p_dept_refcur%NOTFOUND;\n"
                             + "        DBMS_OUTPUT.PUT_LINE(v_deptno || '     ' || v_dname);\n"
                             + "    END LOOP;\n"
                             + "END;";
            Execute(fetchDept);
            //This procedure closes the given cursor variable.
            var closeCursor = "CREATE OR REPLACE PROCEDURE close_refcur (\n"
                               + "    p_refcur   IN OUT SYS_REFCURSOR\n"
                               + ")\n"
                               + "IS\n"
                               + "BEGIN\n"
                               + "    CLOSE p_refcur;\n"
                               + "END;";
            Execute(closeCursor);
            //This example shows a dynamic query using a string literal.
            var deptQuery = "CREATE OR REPLACE PROCEDURE dept_query("
                             + " emp_refcur  OUT SYS_REFCURSOR\n"
                             + ")\n"
                             + "IS\n"
                             + "    v_empno         emp1.empno%TYPE;\n"
                             + "    v_ename         emp1.ename%TYPE;\n"
                             + "BEGIN\n"
                             + "    OPEN emp_refcur FOR 'SELECT empno, ename FROM emp1 WHERE deptno = 30' ||\n"
                             + "        ' AND sal >= 1500 order by empno';\n"
                             + "END;";
            Execute(deptQuery);
            //This example query uses bind arguments to pass the query parameters.
            var deptQueryWithParameters = "CREATE OR REPLACE PROCEDURE dept_query_with_parameters (\n"
                                           + "    p_deptno        emp1.deptno%TYPE,\n"
                                           + "    p_sal           emp1.sal%TYPE\n"
                                           + ")\n"
                                           + "IS\n"
                                           + "    emp_refcur      SYS_REFCURSOR;\n"
                                           + "    v_empno         emp1.empno%TYPE;\n"
                                           + "    v_ename         emp1.ename%TYPE;\n"
                                           + "BEGIN\n"
                                           + "    OPEN emp_refcur FOR 'SELECT empno, ename FROM emp1 WHERE deptno = :dept1'\n"
                                           + "        || ' AND sal >= :sal order by empno' USING p_deptno, p_sal;\n"
                                           + "    LOOP\n"
                                           + "        FETCH emp_refcur INTO v_empno, v_ename;\n"
                                           + "        EXIT WHEN emp_refcur%NOTFOUND;\n"
                                           + "        DBMS_OUTPUT.PUT_LINE(v_empno || ' ' || v_ename);\n"
                                           + "    END LOOP;\n"
                                           + "    CLOSE emp_refcur;\n"
                                           + "END;";
            Execute(deptQueryWithParameters);
            //Finally, a string variable is used to pass the SELECT, providing the most flexibility.
            var deptQueryFromString = "CREATE OR REPLACE PROCEDURE dept_query_from_string (\n"
                                       + "    p_deptno        emp1.deptno%TYPE,\n"
                                       + "    p_sal           emp1.sal%TYPE\n"
                                       + ")\n"
                                       + "IS\n"
                                       + "    emp_refcur      SYS_REFCURSOR;\n"
                                       + "    v_empno         emp1.empno%TYPE;\n"
                                       + "    v_ename         emp1.ename%TYPE;\n"
                                       + "    p_query_string  VARCHAR2(100);\n"
                                       + "BEGIN\n"
                                       + "    p_query_string := 'SELECT empno, ename FROM emp1 WHERE ' ||\n"
                                       + "        'deptno = :dept1 AND sal >= :sal order by empno';\n"
                                       + "    OPEN emp_refcur FOR p_query_string USING p_deptno, p_sal;\n"
                                       + "    LOOP\n"
                                       + "        FETCH emp_refcur INTO v_empno, v_ename;\n"
                                       + "        EXIT WHEN emp_refcur%NOTFOUND;\n"
                                       + "        DBMS_OUTPUT.PUT_LINE(v_empno || ' ' || v_ename);\n"
                                       + "    END LOOP;\n"
                                       + "    CLOSE emp_refcur;\n"
                                       + "END;";
            Execute(deptQueryFromString);
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

        [Test, Timeout(10000)]
        public void ClosingCursorVariableTest()
        {
            var sqlStr = "emp_by_dept(20)";

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
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(CLOSING_CUSOR_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(CLOSING_CUSOR_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test, Timeout(10000)]
        public void ReturningRefCursorFromFunctionTest()
        {
            //conn.setAutoCommit(false);                    //JDBC does this.
            EDBTransaction tran = conn.BeginTransaction();   //.NET does this.
            var command = "emp_by_job(:param1)";

            var cstmt = new EDBCommand(command, conn);
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.Transaction = tran;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, "SALESMAN"));

            cstmt.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Refcursor, 10, "ret",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var cursorName = cstmt.Parameters[1].Value.ToString();
            cstmt.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            cstmt.CommandType = CommandType.Text;
            EDBDataReader rst = cstmt.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst);

            var index = 0;
            while (rst.Read())
            {
                var empno = int.Parse(rst.GetValue(0).ToString());
                var name = rst.GetValue(1).ToString();
                var result = empno.ToString() + " " + name;
                Assert.AreEqual(CURSOR_FUNCTION_RESULT[index], result);
                index++;
            }
            rst.Close();
            tran.Commit();

        }

        [Test]
        [Ignore("EC-2634: 42601: missing \";\" at end of SQL statement")]
    public void ModularizingCursorOperationsTest()
        {
            var sqlStr = "DECLARE\n"
                     + "    gen_refcur      SYS_REFCURSOR;\n"
                     + "BEGIN\n"
                     + "    DBMS_OUTPUT.PUT_LINE('ALL EMPLOYEES');\n"
                     + "    open_all_emp(gen_refcur);\n"
                     + "    fetch_emp(gen_refcur);\n"
                     + "    DBMS_OUTPUT.PUT_LINE('****************');\n"
                     + "\n"
                     + "    DBMS_OUTPUT.PUT_LINE('EMPLOYEES IN DEPT #10');\n"
                     + "    open_emp_by_dept(gen_refcur, 10);\n"
                     + "    fetch_emp(gen_refcur);\n"
                     + "    DBMS_OUTPUT.PUT_LINE('****************');\n"
                     + "\n"
                     + "    DBMS_OUTPUT.PUT_LINE('DEPARTMENTS');\n"
                     + "    fetch_dept(open_dept(gen_refcur));\n"
                     + "    DBMS_OUTPUT.PUT_LINE('*****************');\n"
                     + "\n"
                     + "    close_refcur(gen_refcur);\n"
                     + "END;";

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
                    try
                    {
                        cstmt.CommandType = CommandType.Text;
                        cstmt.ExecuteNonQuery();
                    }
                    catch(Exception ex)
                    {
                        Assert.Fail(ex.ToString());
                    }
                }
                mre.WaitOne(5000);
                Assert.AreEqual(MODULARIZING_CURSOR_OPERATIONS_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(MODULARIZING_CURSOR_OPERATIONS_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
    }

    [Test, Timeout(10000)]
    public void DynamicQueriesTest()
        {
            //conn.setAutoCommit(false);                    //JDBC does this.
            EDBTransaction tran = conn.BeginTransaction();   //.NET does this.
            var command = "dept_query(:param1)";

            var cstmt = new EDBCommand(command, conn);
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.Transaction = tran;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Refcursor, 10, "param1",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, "SALESMAN"));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var cursorName = cstmt.Parameters[0].Value.ToString();
            cstmt.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            cstmt.CommandType = CommandType.Text;
            EDBDataReader rst = cstmt.ExecuteReader(CommandBehavior.SequentialAccess);

            Assert.IsNotNull(rst);

            var index = 0;
            while (rst.Read())
            {
                var empno = int.Parse(rst.GetValue(0).ToString());
                var name = rst.GetValue(1).ToString();
                var result = empno.ToString() + " " + name;
                Assert.AreEqual(DYNAMIC_QUERIES_RESULT[index], result);
                index++;
            }
            rst.Close();
            tran.Commit();
    }

        [Test, Timeout(10000)]
    public void DynamicQueriesWithParametersTest()
        {
            var sqlStr = "dept_query_with_parameters(30, 1500)";

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
                    try
                    {
                        cstmt.CommandType = CommandType.StoredProcedure;
                        cstmt.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail(ex.ToString());
                    }
                }
                mre.WaitOne(5000);
                Assert.AreEqual(DYNAMIC_QUERIES_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(DYNAMIC_QUERIES_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
    }

        [Test, Timeout(10000)]
    public void DynamicQueriesFromStringTest()
        {
            var sqlStr = "dept_query_from_string(20, 1500)";

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
                    try
                    {
                        cstmt.CommandType = CommandType.StoredProcedure;
                        cstmt.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail(ex.ToString());
                    }
                }
                mre.WaitOne(5000);
                Assert.AreEqual(DYNAMIC_QUERIES_FROM_STRING_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(DYNAMIC_QUERIES_FROM_STRING_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
    }
}
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
