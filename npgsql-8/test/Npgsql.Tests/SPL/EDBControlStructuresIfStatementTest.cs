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

//Port JDBC tests to .NET from enhancements\spl\ControlStructuresIfStatementTest.java
namespace EnterpriseDB.EDBClient.Tests.SPL
{
    [TestFixture]
    [NonParallelizable]
    public class EDBControlStructuresIfStatementTest : EPASTestBase
    {
        EDBConnection? conn = null;

        private static string[] IF_THEN_RESULT = {
            "7499 $300.00",
            "7521 $500.00",
            "7654 $1400.00"
        };

        private static string[] IF_THEN_ELSE_RESULT = {
            "7369 Non-commission",
            "7499 $300.00",
            "7521 $500.00",
            "7566 Non-commission",
            "7654 $1400.00",
            "7698 Non-commission",
            "7782 Non-commission",
            "7788 Non-commission",
            "7839 Non-commission",
            "7844 Non-commission",
            "7876 Non-commission",
            "7900 Non-commission",
            "7902 Non-commission",
            "7934 Non-commission"
    };

        private static string[] IF_THEN_ELSE_IF_RESULT = {
            "Average Yearly Compensation: $53,528.57",
            "7369 $19,200.00 Below Average",
            "7499 $45,600.00 Below Average",
            "7521 $42,000.00 Below Average",
            "7566 $71,400.00 Exceeds Average",
            "7654 $63,600.00 Exceeds Average",
            "7698 $68,400.00 Exceeds Average",
            "7782 $58,800.00 Exceeds Average",
            "7788 $72,000.00 Exceeds Average",
            "7839 $120,000.00 Exceeds Average",
            "7844 $36,000.00 Below Average",
            "7876 $26,400.00 Below Average",
            "7900 $22,800.00 Below Average",
            "7902 $72,000.00 Exceeds Average",
            "7934 $31,200.00 Below Average"
    };

        private static string[] IF_THEN_ELSIF_ELSE_RESULT = {
            "Less than 25,000 : 2",
            "25,000 - 49,9999 : 5",
            "50,000 - 74,9999 : 6",
            "75,000 - 99,9999 : 0",
            "100,000 and over : 1"
    };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP TABLE emp1");

            Execute("CREATE TABLE emp1(empno NUMBER(8),  ename VARCHAR2(10), "
                            + "sal NUMBER(10,2), comm NUMBER(10,2))");

            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7369,'SMITH',800,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7499,'ALLEN',1600,300)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7521,'WARD',1250,500)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7566,'JONES',2975,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7654,'MARTIN',1250,1400)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7698,'BLAKE',2850,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7782,'CLARK',2450,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7788,'SCOTT',3000,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7839,'KING',5000,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7844,'TURNER',1500,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7876,'ADAMS',1100,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7900,'JAMES',950,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7902,'FORD',3000,0)");
            Execute("INSERT INTO emp1(empno,ename,sal,comm) VALUES(7934,'MILLER',1300,0)");
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
        //        [Ignore("EC-2638, 42601: syntax error at or near \"v_empno\"")]
        public void IfThenStatementTest()
        {
            //In the following example an IF-THEN statement is used to test and
            //display employees who have a commission.
            Execute("DROP PROCEDURE IfThenStatement_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE IfThenStatement_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                                 + "    v_empno         emp1.empno%TYPE;\n"
                     + "    v_comm          emp1.comm%TYPE;\n"
                     + "    CURSOR emp_cursor IS SELECT empno, comm FROM emp1 order by empno;\n"
                     + "BEGIN\n"
                     + "    OPEN emp_cursor;\n"
                     + "    LOOP\n"
                     + "        FETCH emp_cursor INTO v_empno, v_comm;\n"
                     + "        EXIT WHEN emp_cursor%NOTFOUND;\n"
                     //+ "--\n"
                     //+ "--  Test whether or not the employee gets a commission\n"
                     //+ "--\n"
                     + "        IF v_comm IS NOT NULL AND v_comm > 0 THEN\n"
                     + "            DBMS_OUTPUT.PUT_LINE(v_empno || ' ' ||\n"
                     + "            TO_CHAR(v_comm,'$FM99999.00'));\n"
                     + "        END IF;\n"
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
                using (var cstmt = new EDBCommand("IfThenStatement_SP()", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(IF_THEN_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(IF_THEN_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void IfThenElseStatementTest()
        {
            //An IF-THEN-ELSE statement is used to display the text Non-commission if
            //the employee does not get a commission.
            Execute("DROP PROCEDURE IfThenElseStatement_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE IfThenElseStatement_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                                  + "    v_empno         emp1.empno%TYPE;\n"
                      + "    v_comm          emp1.comm%TYPE;\n"
                      + "    CURSOR emp_cursor IS SELECT empno, comm FROM emp1 order by empno;\n"
                      + "BEGIN\n"
                      + "    OPEN emp_cursor;\n"
                      + "    LOOP\n"
                      + "        FETCH emp_cursor INTO v_empno, v_comm;\n"
                      + "        EXIT WHEN emp_cursor%NOTFOUND;\n"
                      + "--\n"
                      + "--  Test whether or not the employee gets a commission\n"
                      + "--\n"
                      + "        IF v_comm IS NOT NULL AND v_comm > 0 THEN\n"
                      + "            DBMS_OUTPUT.PUT_LINE(v_empno || ' ' ||\n"
                      + "            TO_CHAR(v_comm,'$FM99999.00'));\n"
                      + "        ELSE\n"
                      + "            DBMS_OUTPUT.PUT_LINE(v_empno || ' ' || 'Non-commission');\n"
                      + "        END IF;\n"
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
                using (var cstmt = new EDBCommand("IfThenElseStatement_SP()", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(IF_THEN_ELSE_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(IF_THEN_ELSE_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void IfThenElseIfStatementTest()
        {
            //In the following example the outer IF-THEN-ELSE statement tests whether
            //or not an employee has a commission. The inner IF-THEN-ELSE statements
            //then test whether the employee’s total compensation exceeds or is less
            //than the company average.
            Execute("DROP PROCEDURE IfThenElseIfStatement_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE IfThenElseIfStatement_SP()\n"
                    + " IS\n"
                    + " DECLARE\n"
                    + "    v_empno         emp1.empno%TYPE;\n"
                    + "    v_sal           emp1.sal%TYPE;\n"
                    + "    v_comm          emp1.comm%TYPE;\n"
                    + "    v_avg           NUMBER(7,2);\n"
                    + "    CURSOR emp_cursor IS SELECT empno, sal, comm FROM emp1;\n"
                    + "BEGIN\n"
                    + "--\n"
                    + "--  Calculate the average yearly compensation in the company\n"
                    + "--\n"
                    + "    SELECT AVG((sal + NVL(comm,0)) * 24) INTO v_avg FROM emp1;\n"
                    + "    DBMS_OUTPUT.PUT_LINE('Average Yearly Compensation: ' ||\n"
                    + "        TO_CHAR(v_avg,'$FM999,999.00'));\n"
                    + "    OPEN emp_cursor;\n"
                    + "    LOOP\n"
                    + "        FETCH emp_cursor INTO v_empno, v_sal, v_comm;\n"
                    + "        EXIT WHEN emp_cursor%NOTFOUND;\n"
                    + "--\n"
                    + "--  Test whether or not the employee gets a commission\n"
                    + "--\n"
                    + "        IF v_comm IS NOT NULL AND v_comm > 0 THEN\n"
                    + "--\n"
                    + "--  Test if the employee's compensation with commission exceeds the average\n"
                    + "--\n"
                    + "            IF (v_sal + v_comm) * 24 > v_avg THEN\n"
                    + "                DBMS_OUTPUT.PUT_LINE(v_empno || ' ' ||\n"
                    + "                    TO_CHAR((v_sal + v_comm) * 24,'$FM999,999.00') || ' Exceeds Average');\n"
                    + "            ELSE\n"
                    + "                DBMS_OUTPUT.PUT_LINE(v_empno || ' ' ||\n"
                    + "                    TO_CHAR((v_sal + v_comm) * 24,'$FM999,999.00') || ' Below Average');\n"
                    + "            END IF;\n"
                    + "        ELSE\n"
                    + "--\n"
                    + "--  Test if the employee's compensation without commission exceeds the\n"
                    + "-- average\n"
                    + "--\n"
                    + "            IF v_sal * 24 > v_avg THEN\n"
                    + "                DBMS_OUTPUT.PUT_LINE(v_empno || ' ' ||\n"
                    + "                    TO_CHAR(v_sal * 24,'$FM999,999.00') || ' Exceeds Average');\n"
                    + "            ELSE\n"
                    + "                DBMS_OUTPUT.PUT_LINE(v_empno || ' ' ||\n"
                    + "                    TO_CHAR(v_sal * 24,'$FM999,999.00') || ' Below Average');\n"
                    + "            END IF;\n"
                    + "        END IF;\n"
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
                using (var cstmt = new EDBCommand("IfThenElseIfStatement_SP()", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(IF_THEN_ELSE_IF_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(IF_THEN_ELSE_IF_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void IfThenElsifElseStatementTest()
        {
            //The following example uses an IF-THEN-ELSIF-ELSE statement
            //to count the number of employees by compensation ranges of $25,000.
            Execute("DROP PROCEDURE IfThenElsifElseStatement_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE IfThenElsifElseStatement_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                            + "    v_empno         emp1.empno%TYPE;\n"
                + "    v_comp          NUMBER(8,2);\n"
                + "    v_lt_25K        SMALLINT := 0;\n"
                + "    v_25K_50K       SMALLINT := 0;\n"
                + "    v_50K_75K       SMALLINT := 0;\n"
                + "    v_75K_100K      SMALLINT := 0;\n"
                + "    v_ge_100K       SMALLINT := 0;\n"
                + "    CURSOR emp_cursor IS SELECT empno, (sal + NVL(comm,0)) * 24 FROM emp1;\n"
                + "BEGIN\n"
                + "    OPEN emp_cursor;\n"
                + "    LOOP\n"
                + "        FETCH emp_cursor INTO v_empno, v_comp;\n"
                + "        EXIT WHEN emp_cursor%NOTFOUND;\n"
                + "        IF v_comp < 25000 THEN\n"
                + "            v_lt_25K := v_lt_25K + 1;\n"
                + "        ELSIF v_comp < 50000 THEN\n"
                + "            v_25K_50K := v_25K_50K + 1;\n"
                + "        ELSIF v_comp < 75000 THEN\n"
                + "            v_50K_75K := v_50K_75K + 1;\n"
                + "        ELSIF v_comp < 100000 THEN\n"
                + "            v_75K_100K := v_75K_100K + 1;\n"
                + "        ELSE\n"
                + "            v_ge_100K := v_ge_100K + 1;\n"
                + "        END IF;\n"
                + "    END LOOP;\n"
                + "    CLOSE emp_cursor;\n"
                + "    DBMS_OUTPUT.PUT_LINE('Less than 25,000 : ' || v_lt_25K);\n"
                + "    DBMS_OUTPUT.PUT_LINE('25,000 - 49,9999 : ' || v_25K_50K);\n"
                + "    DBMS_OUTPUT.PUT_LINE('50,000 - 74,9999 : ' || v_50K_75K);\n"
                + "    DBMS_OUTPUT.PUT_LINE('75,000 - 99,9999 : ' || v_75K_100K);\n"
                + "    DBMS_OUTPUT.PUT_LINE('100,000 and over : ' || v_ge_100K);\n"
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
                using (var cstmt = new EDBCommand("IfThenElsifElseStatement_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(IF_THEN_ELSIF_ELSE_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(IF_THEN_ELSIF_ELSE_RESULT[i], notice.MessageText);
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

