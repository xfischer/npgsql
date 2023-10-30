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

//EC-2574: Regression Tests for Working with Collections in SPL

//These tests are implemented in JDBC as Anonymous block.
//We have implemented them in stored procedures because anonymous blocks do not work in .NET.

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBCollectionBulkCollectTest : EPASTestBase
    {
        EDBConnection? conn = null;

        private static string[] EMP_RESULT = {
            "7369   SMITH    CLERK      17-DEC-80     800.00        .00  20",
            "7499   ALLEN    SALESMAN   20-FEB-81   1,600.00     300.00  30",
            "7521   WARD     SALESMAN   22-FEB-81   1,250.00     500.00  30",
            "7566   JONES    MANAGER    02-APR-81   2,975.00        .00  20",
            "7654   MARTIN   SALESMAN   28-SEP-81   1,250.00   1,400.00  30",
            "7698   BLAKE    MANAGER    01-MAY-81   2,850.00        .00  30",
            "7782   CLARK    MANAGER    09-JUN-81   2,450.00        .00  10",
            "7788   SCOTT    ANALYST    19-APR-87   3,000.00        .00  20",
            "7839   KING     PRESIDENT  17-NOV-81   5,000.00        .00  10",
            "7844   TURNER   SALESMAN   08-SEP-81   1,500.00        .00  30",
            "7876   ADAMS    CLERK      23-MAY-87   1,100.00        .00  20",
            "7900   JAMES    CLERK      03-DEC-81     950.00        .00  30",
            "7902   FORD     ANALYST    03-DEC-81   3,000.00        .00  20",
            "7934   MILLER   CLERK      23-JAN-82   1,300.00        .00  10"
    };

        private static string[] INCREASING_SALARY_RESULT = {
            "7369   SMITH      1,200.00",
            "7876   ADAMS      1,650.00",
            "7900   JAMES      1,425.00",
            "7934   MILLER     1,950.00"
    };

        private static string[] DELETE_ROWS_RESULT = {
            "7369   SMITH    CLERK      17-DEC-80   1,200.00        .00  20",
            "7876   ADAMS    CLERK      23-MAY-87   1,650.00        .00  20",
            "7900   JAMES    CLERK      03-DEC-81   1,425.00        .00  30",
            "7934   MILLER   CLERK      23-JAN-82   1,950.00        .00  10"
    };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            TestUtil.dropTable(conn, "emp1 CASCADE");
            TestUtil.dropTable(conn, "dept1 CASCADE");
            TestUtil.dropTable(conn, "clerkemp CASCADE");

            Execute("CREATE TABLE emp1("
                    + "empno NUMBER(4),  ename VARCHAR2(20), job VARCHAR2(20), hiredate DATE, "
                    + "sal NUMBER(10,2), comm NUMBER(10,2), deptno NUMBER(4))");
            Execute("CREATE TABLE dept1(deptno NUMBER(4), dname VARCHAR2(14))");

            string[] addEmp = {
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7369,'SMITH','CLERK',"
                + "to_date('17-12-1980','DD-MM-YYYY'),800,0,20)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7499,'ALLEN','SALESMAN',"
                + "to_date('20-02-1981','DD-MM-YYYY'),1600,300,30)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7521,'WARD','SALESMAN',"
                + "to_date('22-02-1981','DD-MM-YYYY'),1250,500,30)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7566,'JONES','MANAGER',"
                + "to_date('02-04-1981','DD-MM-YYYY'),2975,0,20)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7654,'MARTIN','SALESMAN',"
                + "to_date('28-09-1981','DD-MM-YYYY'),1250,1400,30)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7698,'BLAKE','MANAGER',"
                + "to_date('01-05-1981','DD-MM-YYYY'),2850,0,30)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7782,'CLARK','MANAGER',"
                + "to_date('09-06-1981','DD-MM-YYYY'),2450,0,10)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7788,'SCOTT','ANALYST',"
                + "to_date('19-04-1987','DD-MM-YYYY'),3000,0,20)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7839,'KING','PRESIDENT',"
                + "to_date('17-11-1981','DD-MM-YYYY'),5000,0,10)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7844,'TURNER','SALESMAN',"
                + "to_date('08-09-1981','DD-MM-YYYY'),1500,0,30)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7876,'ADAMS','CLERK',"
                + "to_date('23-05-1987','DD-MM-YYYY'),1100,0,20)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7900,'JAMES','CLERK',"
                + "to_date('03-12-1981','DD-MM-YYYY'),950,0,30)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7902,'FORD','ANALYST',"
                + "to_date('03-12-1981','DD-MM-YYYY'),3000,0,20)",
                "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,deptno) VALUES(7934,'MILLER','CLERK',"
                + "to_date('23-01-1982','DD-MM-YYYY'),1300,0,10)" };
            for (var i = 0; i < addEmp.Length; i++)
            {
                Execute(addEmp[i]);
            }
            Execute("CREATE TABLE clerkemp AS SELECT * FROM emp1 WHERE job = 'CLERK';");
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
        public void SelectBulkCollectSigleFieldTest()
        {
            //This example uses the BULK COLLECT clause where the target collections
            //are associative arrays consisting of a single field:
            Execute("DROP PROCEDURE SelectBulkCollectSigleField_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE SelectBulkCollectSigleField_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE empno_tbl    IS TABLE OF emp1.empno%TYPE    INDEX BY BINARY_INTEGER;\n"
                + "    TYPE ename_tbl    IS TABLE OF emp1.ename%TYPE    INDEX BY BINARY_INTEGER;\n"
                + "    TYPE job_tbl      IS TABLE OF emp1.job%TYPE      INDEX BY BINARY_INTEGER;\n"
                + "    TYPE hiredate_tbl IS TABLE OF emp1.hiredate%TYPE INDEX BY BINARY_INTEGER;\n"
                + "    TYPE sal_tbl      IS TABLE OF emp1.sal%TYPE      INDEX BY BINARY_INTEGER;\n"
                + "    TYPE comm_tbl     IS TABLE OF emp1.comm%TYPE     INDEX BY BINARY_INTEGER;\n"
                + "    TYPE deptno_tbl   IS TABLE OF emp1.deptno%TYPE   INDEX BY BINARY_INTEGER;\n"
                + "    t_empno           EMPNO_TBL;\n"
                + "    t_ename           ENAME_TBL;\n"
                + "    t_job             JOB_TBL;\n"
                + "    t_hiredate        HIREDATE_TBL;\n"
                + "    t_sal             SAL_TBL;\n"
                + "    t_comm            COMM_TBL;\n"
                + "    t_deptno          DEPTNO_TBL;\n"
                + "BEGIN\n"
                + "    SELECT empno, ename, job, hiredate, sal, comm, deptno BULK COLLECT\n"
                + "       INTO t_empno, t_ename, t_job, t_hiredate, t_sal, t_comm, t_deptno\n"
                + "       FROM emp1;\n"
                + "    FOR i IN 1..t_empno.COUNT LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE(t_empno(i) || '   ' ||\n"
                + "            RPAD(t_ename(i),8) || ' ' ||\n"
                + "            RPAD(t_job(i),10) || ' ' ||\n"
                + "            TO_CHAR(t_hiredate(i),'DD-MON-YY') || ' ' ||\n"
                + "            TO_CHAR(t_sal(i),'99,999.99') || ' ' ||\n"
                + "            TO_CHAR(NVL(t_comm(i),0),'99,999.99') || '  ' ||\n"
                + "            t_deptno(i));\n"
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
                using (var cstmt = new EDBCommand("SelectBulkCollectSigleField_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void SelectBulkCollectRowtypeTest()
        {
            //This example produces the same result but uses an associative array
            //on a record type defined with the %ROWTYPE attribute
            Execute("DROP PROCEDURE SelectBulkCollectRowtype_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE SelectBulkCollectRowtype_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE emp_tbl IS TABLE OF emp1%ROWTYPE INDEX BY BINARY_INTEGER;\n"
                + "    t_emp           EMP_TBL;\n"
                + "BEGIN\n"
                + "    SELECT * BULK COLLECT INTO t_emp FROM emp1;\n"
                + "    FOR i IN 1..t_emp.COUNT LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE(t_emp(i).empno || '   ' ||\n"
                + "            RPAD(t_emp(i).ename,8) || ' ' ||\n"
                + "            RPAD(t_emp(i).job,10) || ' ' ||\n"
                + "            TO_CHAR(t_emp(i).hiredate,'DD-MON-YY') || ' ' ||\n"
                + "            TO_CHAR(t_emp(i).sal,'99,999.99') || ' ' ||\n"
                + "            TO_CHAR(NVL(t_emp(i).comm,0),'99,999.99') || '  ' ||\n"
                + "            t_emp(i).deptno);\n"
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
                using (var cstmt = new EDBCommand("SelectBulkCollectRowtype_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void FetchBulkCollectTest()
        {
            //This example uses the FETCH BULK COLLECT statement to retrieve
            //rows into an associative array
            Execute("DROP PROCEDURE FetchBulkCollect_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE FetchBulkCollect_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE emp_tbl IS TABLE OF emp1%ROWTYPE INDEX BY BINARY_INTEGER;\n"
                + "    t_emp           EMP_TBL;\n"
                + "    CURSOR emp_cur IS SELECT * FROM emp1;\n"
                + "BEGIN\n"
                + "    OPEN emp_cur;\n"
                + "    FETCH emp_cur BULK COLLECT INTO t_emp;\n"
                + "    CLOSE emp_cur;\n"
                + "    FOR i IN 1..t_emp.COUNT LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE(t_emp(i).empno || '   ' ||\n"
                + "            RPAD(t_emp(i).ename,8) || ' ' ||\n"
                + "            RPAD(t_emp(i).job,10) || ' ' ||\n"
                + "            TO_CHAR(t_emp(i).hiredate,'DD-MON-YY') || ' ' ||\n"
                + "            TO_CHAR(t_emp(i).sal,'99,999.99') || ' ' ||\n"
                + "            TO_CHAR(NVL(t_emp(i).comm,0),'99,999.99') || '  ' ||\n"
                + "            t_emp(i).deptno);\n"
                + "    END LOOP;\n"
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
                using (var cstmt = new EDBCommand("FetchBulkCollect_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void ReturningBulkCollectIncreasingSalaryTest()
        {
            //This example increases all employee salaries by 1.5, stores the
            //employees’ numbers, names, and new salaries in three associative arrays,
            //and displays the contents of these arrays:
            Execute("DROP PROCEDURE ReturningBulkCollectIncreasingSalary_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE ReturningBulkCollectIncreasingSalary_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE empno_tbl IS TABLE OF emp1.empno%TYPE INDEX BY BINARY_INTEGER;\n"
                + "    TYPE ename_tbl IS TABLE OF emp1.ename%TYPE INDEX BY BINARY_INTEGER;\n"
                + "    TYPE sal_tbl   IS TABLE OF emp1.sal%TYPE   INDEX BY BINARY_INTEGER;\n"
                + "    t_empno         EMPNO_TBL;\n"
                + "    t_ename         ENAME_TBL;\n"
                + "    t_sal           SAL_TBL;\n"
                + "BEGIN\n"
                + "    UPDATE clerkemp SET sal = sal * 1.5 RETURNING empno, ename, sal\n"
                + "        BULK COLLECT INTO t_empno, t_ename, t_sal;\n"
                + "    FOR i IN 1..t_empno.COUNT LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE(t_empno(i) || '   ' || RPAD(t_ename(i),8) ||\n"
                + "            ' ' || TO_CHAR(t_sal(i),'99,999.99'));\n"
                + "    END LOOP;\n"
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
                using (var cstmt = new EDBCommand("ReturningBulkCollectIncreasingSalary_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(INCREASING_SALARY_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(INCREASING_SALARY_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void ReturningBulkCollectRecordTypeTest()
        {
            //This example uses a single collection defined with a record type
            //to store the employees’ numbers, names, and new salaries.
            Execute("DROP PROCEDURE ReturningBulkCollectRecordType_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE ReturningBulkCollectRecordType_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE emp_rec IS RECORD (\n"
                + "        empno       emp1.empno%TYPE,\n"
                + "        ename       emp1.ename%TYPE,\n"
                + "        sal         emp1.sal%TYPE\n"
                + "    );\n"
                + "    TYPE emp_tbl IS TABLE OF emp_rec INDEX BY BINARY_INTEGER;\n"
                + "    t_emp           EMP_TBL;\n"
                + "BEGIN\n"
                + "    UPDATE clerkemp SET sal = sal * 1.5 RETURNING empno, ename, sal\n"
                + "        BULK COLLECT INTO t_emp;\n"
                + "    FOR i IN 1..t_emp.COUNT LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE(t_emp(i).empno || '   ' ||\n"
                + "            RPAD(t_emp(i).ename,8) || ' ' ||\n"
                + "            TO_CHAR(t_emp(i).sal,'99,999.99'));\n"
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
                using (var cstmt = new EDBCommand("ReturningBulkCollectRecordType_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(INCREASING_SALARY_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(INCREASING_SALARY_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void ReturningBulkCollectDeleteRowsTest()
        {
            //This example deletes all rows from the clerkemp table and returns
            //information on the deleted rows into an associative array. It
            //then displays the array.
            Execute("DROP PROCEDURE ReturningBulkCollectDeleteRows_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE ReturningBulkCollectDeleteRows_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE emp_rec IS RECORD (\n"
                + "        empno       emp1.empno%TYPE,\n"
                + "        ename       emp1.ename%TYPE,\n"
                + "        job         emp1.job%TYPE,\n"
                + "        hiredate    emp1.hiredate%TYPE,\n"
                + "        sal         emp1.sal%TYPE,\n"
                + "        comm        emp1.comm%TYPE,\n"
                + "        deptno      emp1.deptno%TYPE\n"
                + "    );\n"
                + "    TYPE emp_tbl IS TABLE OF emp_rec INDEX BY BINARY_INTEGER;\n"
                + "    r_emp           EMP_TBL;\n"
                + "BEGIN\n"
                + "    UPDATE clerkemp SET sal = sal * 1.5;"
                + "    DELETE FROM clerkemp RETURNING empno, ename, job, hiredate, sal,\n"
                + "        comm, deptno BULK COLLECT INTO r_emp;\n"
                + "    FOR i IN 1..r_emp.COUNT LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE(r_emp(i).empno || '   ' ||\n"
                + "            RPAD(r_emp(i).ename,8) || ' ' ||\n"
                + "            RPAD(r_emp(i).job,10) || ' ' ||\n"
                + "            TO_CHAR(r_emp(i).hiredate,'DD-MON-YY') || ' ' ||\n"
                + "            TO_CHAR(r_emp(i).sal,'99,999.99') || ' ' ||\n"
                + "            TO_CHAR(NVL(r_emp(i).comm,0),'99,999.99') || '  ' ||\n"
                + "            r_emp(i).deptno);\n"
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
                using (var cstmt = new EDBCommand("ReturningBulkCollectDeleteRows_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(DELETE_ROWS_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(DELETE_ROWS_RESULT[i], notice.MessageText);
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

