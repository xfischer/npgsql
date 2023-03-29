using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Threading;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//Port JDBC tests to .NET from enhancements\spl\ControlStructuresIfStatementTest.java
namespace EnterpriseDB.EDBClient.Tests.SPL
{
    [TestFixture]
    public class EDBControlStructuresIfStatementTest : TestBase
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
            Execute("DROP TABLE emp1");
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
        [Ignore("EC-2638, 42601: syntax error at or near \"v_empno\"")]
    public void IfThenStatementTest()
        {
            //In the following example an IF-THEN statement is used to test and
            //display employees who have a commission.
            var sqlStr = "DECLARE\n"
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

            var mre = new ManualResetEvent(false);
            PostgresNotice? notice = null;
            NoticeEventHandler action = (sender, args) =>
            {
                Assert.IsNotNull(args.Notice);
                notice = args.Notice;
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
                //Assert.That(notice, Is.Not.Null, "No notice was emitted");
                Assert.That(notice!.MessageText, Is.EqualTo("number 10"));
                Assert.That(notice.Severity, Is.EqualTo("NOTICE"));
            }
            finally
            {
                conn.Notice -= action;
            }

        //Assert.assertArrayEquals(IF_THEN_RESULT, v.toArray());
    }
}
}

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

