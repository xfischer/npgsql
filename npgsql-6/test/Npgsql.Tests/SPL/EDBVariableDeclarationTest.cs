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

//EC-2586: Regression Tests for Variable Declarations IN SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBVariableDeclarationTest : TestBase
    {
        EDBConnection? conn = null;

        private static string[] emp01 = { "1001", "SMITH", "Sales", "01-NOV-07 00:00:00",
            "120000.00", "20", "110000.00" };
        private static string ENAME = "SMITH";
        private static string JOB = "Sales";
        private double SALARY = 120000;
        private int DEPTNO = 20;
        private static int AVG_SAL_DEPT20 = 110000;
        private static string DATE = "2007-11-01 00:00:00.0";

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP TABLE emp1 CASCADE");

            Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(10),job VARCHAR2(9), "
                    + " hiredate DATE, sal   NUMBER(10,2) ,deptno NUMBER(5))");
            var add01 = "INSERT INTO emp1(empno,ename,job, hiredate, sal, deptno) "
                    + " VALUES(1001,'SMITH','Sales',to_date('01-11-07','DD-MM-YY'),120000,20)";
            Execute(add01);
            var add02 = "INSERT INTO emp1(empno,ename,job, hiredate, sal, deptno) "
                    + " VALUES(1002,'ALLEN','Sales',to_date('01-11-07','DD-MM-YY'),100000,20)";
            Execute(add02);
            var add03 = "INSERT INTO emp1(empno,ename,job, hiredate, sal, deptno) "
                    + " VALUES(1003,'WARD','Engineer',to_date('01-11-07','DD-MM-YY'),90000,21)";
            Execute(add03);
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
        public void SimpleVariableDecTest()
        {
            Execute("DROP PROCEDURE dept_salary_rpt");

            //This procedure shows some variable declarations that use defaults
            //consisting of string and numeric expressions.
            var deptSalPro = "CREATE OR REPLACE PROCEDURE dept_salary_rpt (\n"
                          + "    p_deptno  NUMBER\n"
                          + ")\n"
                          + "IS\n"
                          + "    todays_date     DATE := SYSDATE;\n"
                          + "    rpt_title       VARCHAR2(60) := 'Report For Department # ' || p_deptno\n"
                          + "                                    || ' on ' || todays_date;\n"
                          + "    base_sal        INTEGER := 35525;\n"
                          + "    base_comm_rate  NUMBER := 1.33333;\n"
                          + "    base_annual     NUMBER := ROUND(base_sal * base_comm_rate, 2);\n"
                          + "BEGIN\n"
                          + "    DBMS_OUTPUT.PUT_LINE(rpt_title);\n"
                          + "    DBMS_OUTPUT.PUT_LINE('Base Annual Salary: ' || base_annual);\n"
                          + "END;";
            Execute(deptSalPro);

            var sqlStr = "dept_salary_rpt(:param1)";

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

                    cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
                        ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();

                }
                mre.WaitOne(5000);

                Assert.AreEqual(2, notices.Count);

                var titleStart = "Report For Department # 20 on";
                var notice1 = (PostgresNotice?)notices[0];
                Assert.IsTrue(notice1.MessageText.StartsWith(titleStart));

                var notice2 = (PostgresNotice?)notices[1];
                Assert.AreEqual(notice2.MessageText, "Base Annual Salary: 47366.55");
            }
            finally
            {
                conn.Notice -= action;
            }
        }

        [Test]
        public void TypeVariableDecTest()
        {
            Execute("DROP PROCEDURE emp_sal_query");

            //Specify a qualified column name in dot notation or the name of a previously declared
            //variable as a prefix to %TYPE. The data type of the column or variable prefixed to %TYPE
            //is assigned to the variable being declared.
            var sqlStr = "CREATE OR REPLACE PROCEDURE emp_sal_query (\n"
                          + "    p_empno         IN emp1.empno%TYPE\n"
                          + ")\n"
                          + "IS\n"
                          + "    v_ename         emp1.ename%TYPE;\n"
                          + "    v_job           emp1.job%TYPE;\n"
                          + "    v_hiredate      emp1.hiredate%TYPE;\n"
                          + "    v_sal           emp1.sal%TYPE;\n"
                          + "    v_deptno        emp1.deptno%TYPE;\n"
                          + "    v_avgsal        v_sal%TYPE;\n" + "BEGIN\n"
                          + "    SELECT ename, job, hiredate, sal, deptno\n"
                          + "        INTO v_ename, v_job, v_hiredate, v_sal, v_deptno\n"
                          + "        FROM emp1 WHERE empno = p_empno;\n"
                          + "    DBMS_OUTPUT.PUT_LINE('Employee # : ' || p_empno);\n"
                          + "    DBMS_OUTPUT.PUT_LINE('Name       : ' || v_ename);\n"
                          + "    DBMS_OUTPUT.PUT_LINE('Job        : ' || v_job);\n"
                          + "    DBMS_OUTPUT.PUT_LINE('Hire Date  : ' || v_hiredate);\n"
                          + "    DBMS_OUTPUT.PUT_LINE('Salary     : ' || v_sal);\n"
                          + "    DBMS_OUTPUT.PUT_LINE('Dept #     : ' || v_deptno);\n" + "\n"
                          + "    SELECT AVG(sal) INTO v_avgsal\n"
                          + "        FROM emp1 WHERE deptno = v_deptno;\n"
                          + "    IF v_sal > v_avgsal THEN\n"
                          + "        DBMS_OUTPUT.PUT_LINE('Employee''s salary is more than the '\n"
                          + "            || 'department average of: ' || v_avgsal);\n"
                          + "    ELSE\n"
                          + "        DBMS_OUTPUT.PUT_LINE('Employee''s salary does not exceed the '\n"
                          + "            || 'department average of: ' || v_avgsal);\n"
                          + "    END IF;\n"
                          + "END;";
            Execute(sqlStr);
            var execStr = "emp_sal_query(:param1)";

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
                using (var cstmt = new EDBCommand(execStr, conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
                        ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1001));

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();

                }
                mre.WaitOne(5000);

                Assert.AreEqual(7, notices.Count);

                //Date value is "01-NOV-07 00:00:00" in .NET
                //While it is "2007-11-01 00:00:00" in JDBC

                for (var i = 0; i < emp01.Length; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    var value = notice.MessageText;
                    var arr = value.Split(":", 2);
                    var field = arr[1].Trim();
                    Assert.AreEqual(emp01[i], field);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
        }

        [Test]
        [Ignore("EC-2634")]
        public void TypeVariableOutputTest()
        {
            Execute("DROP PACKAGE BODY pkgTypeTest;");
            Execute("DROP PACKAGE pkgTypeTest;");

            //Use %TYPE in variable declarations as output parameter
            var createTypePkg = "CREATE OR REPLACE PACKAGE pkgTypeTest Is \n"
                             + "    Procedure emp_sal_query(p_empno IN emp1.empno%TYPE, "
                             + "       v_ename   OUT emp1.ename%TYPE, v_job OUT emp1.job%TYPE, \n"
                             + "       v_hiredate  OUT emp1.hiredate%TYPE, v_sal OUT emp1.sal%TYPE, \n"
                             + "       v_deptno  OUT   emp1.deptno%TYPE);\n"
                             + " End pkgTypeTest; ";
            Execute(createTypePkg);
            var createTypeBody = " CREATE OR REPLACE PACKAGE BODY pkgTypeTest \n" + "  Is \n"
                                  + "   Procedure emp_sal_query(p_empno IN emp1.empno%TYPE, "
                                  + "     v_ename OUT emp1.ename%TYPE, v_job OUT emp1.job%TYPE, \n"
                                  + "     v_hiredate OUT emp1.hiredate%TYPE, v_sal OUT emp1.sal%TYPE, \n"
                                  + "     v_deptno OUT emp1.deptno%TYPE) \n"
                                  + "  IS\n"
                                  + "    BEGIN\n"
                                  + "    SELECT ename, job, hiredate, sal, deptno\n"
                                  + "        INTO v_ename, v_job, v_hiredate, v_sal, v_deptno\n"
                                  + "        FROM emp1 WHERE empno = p_empno;\n"
                                  + "  End emp_sal_query; \n"
                                  + " End pkgTypeTest;";
            Execute(createTypeBody);

            var commandText = "pkgTypeTest.emp_sal_query(:param1,:param2,:param3,:param4,:param5,:param6)";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 7369));

                cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 7369));

                cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
                    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 7369));

                cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Date, 10, "param4",
                    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 7369));

                cstmt.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Numeric, 10, "param5",
                    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 7369));

                cstmt.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Numeric, 10, "param6",
                    ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 7369));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

                var name = cstmt.Parameters[1].Value.ToString();
                Assert.AreEqual(ENAME, name);
                var job = cstmt.Parameters[2].Value.ToString();
                Assert.AreEqual(JOB, job);
                var date = cstmt.Parameters[3].Value.ToString();
                Assert.AreEqual(date, DATE);
                var sal = double.Parse(cstmt.Parameters[4].Value.ToString());
                Assert.AreEqual(SALARY, sal, 0.01);
                var deptno = double.Parse(cstmt.Parameters[5].Value.ToString());
                Assert.AreEqual(DEPTNO, deptno, 0.01);
            }
        }

        [Test]
        [Ignore("EC-2633 && EC-2634")]
        public void RowTypeVariableOutputTest()
        {
            Execute("DROP PACKAGE BODY pkgRowTypeTest;");
            Execute("DROP PACKAGE pkgRowTypeTest;");

            //You can use the %ROWTYPE attribute to declare a record. The %ROWTYPE attribute is prefixed
            //by a table name. Each column in the named table defines an identically named field in
            //the record with the same data type as the column.
            var createRowTypePkg = "CREATE OR REPLACE PACKAGE pkgRowTypeTest Is \n"
                                + "    Procedure emp_sal_query(p_empno IN emp1.empno%TYPE, "
                                + "        r_emp  OUT emp1%ROWTYPE, v_avgsal  OUT  emp1.sal%TYPE);\n"
                                + " End pkgRowTypeTest; ";
            Execute(createRowTypePkg);
            var createRowTypeBody = " CREATE OR REPLACE PACKAGE BODY pkgRowTypeTest \n"
                    + "  Is \n"
                    + "  PROCEDURE emp_sal_query (p_empno IN emp1.empno%TYPE, "
                    + "      r_emp  OUT emp1%ROWTYPE, v_avgsal  OUT  emp1.sal%TYPE)\n"
                    + "    IS\n" + "    BEGIN\n"
                    + "    SELECT ename, job, hiredate, sal, deptno\n"
                    + "        INTO r_emp.ename, r_emp.job, r_emp.hiredate, r_emp.sal, r_emp.deptno\n"
                    + "        FROM emp1 WHERE empno = p_empno;\n"
                    + "    SELECT AVG(sal) INTO v_avgsal\n"
                    + "        FROM emp1 WHERE deptno = r_emp.deptno;\n" + "  End emp_sal_query; \n"
                    + " End pkgRowTypeTest;";
            Execute(createRowTypeBody);

            var commandText = "pkgRowTypeTest.emp_sal_query";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.DeriveParameters();

                cstmt.Parameters[0].Value = 1001;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

            }
        }

        [Test]
        [Ignore("EC-2633 && EC-2634")]
        public void RecordTypeVariableOutputTest()
        {
            Execute("DROP PACKAGE BODY pkgRecordTypeTest;");
            Execute("DROP PACKAGE pkgRecordTypeTest;");
            //You can use the TYPE IS RECORD statement to create the definition of a
            //record type. A record type is a definition of a record made up of one or
            //more identifiers and their corresponding data types. You can't use a
            //record type by itself to manipulate data.
            var createRowTypePkg = "CREATE OR REPLACE PACKAGE pkgRecordTypeTest Is \n"
                                + "     TYPE emp_typ IS RECORD (\n"
                                + "        ename       emp1.ename%TYPE,\n"
                                + "        job         emp1.job%TYPE,\n"
                                + "        hiredate    emp1.hiredate%TYPE,\n"
                                + "        sal         emp1.sal%TYPE,\n"
                                + "        deptno      emp1.deptno%TYPE\n"
                                + "    );"
                                + "    Procedure emp_sal_query(p_empno IN emp1.empno%TYPE, "
                                + "        r_emp  OUT emp_typ, v_avgsal  OUT  emp1.sal%TYPE);\n"
                                + " End pkgRecordTypeTest; ";
            Execute(createRowTypePkg);
            var createRowTypeBody = " CREATE OR REPLACE PACKAGE BODY pkgRecordTypeTest \n"
                                 + "  Is \n"
                                 + "  PROCEDURE emp_sal_query (p_empno IN emp1.empno%TYPE, \n"
                                 + "      r_emp  OUT emp_typ, v_avgsal  OUT  emp1.sal%TYPE)\n"
                                 + "    IS\n"
                                 + "    BEGIN\n"
                                 + "  SELECT ename, job, hiredate, sal, deptno\n"
                                 + "        INTO r_emp.ename, r_emp.job, r_emp.hiredate, r_emp.sal, r_emp.deptno\n"
                                 + "        FROM emp1 WHERE empno = p_empno;\n"
                                 + "  SELECT AVG(sal) INTO v_avgsal\n"
                                 + "        FROM emp1 WHERE deptno = r_emp.deptno;\n"
                                 + "  End emp_sal_query; \n"
                                 + " End pkgRecordTypeTest;";
            Execute(createRowTypeBody);

            var commandText = "pkgRecordTypeTest.emp_sal_query";

            using (var cstmt = new EDBCommand(commandText, conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.DeriveParameters();

                cstmt.Parameters[0].Value = 1001;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();

            }

        }
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
