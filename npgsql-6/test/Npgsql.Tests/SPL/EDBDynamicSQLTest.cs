using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Collections;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2590: Regression Tests for Dynamic SQL in SPL

//These tests are implemented in JDBC as Anonymous block.
//We have implemented them in stored procedures because anonymous blocks do not work in .NET.

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBDynamicSQLTest : TestBase
    {
        EDBConnection? conn = null;

        private static int[] JOB_100_200 = { 100, 200 };
        private static int[] JOB_300_400_500 = { 300, 400, 500 };
        private static string[] JOB_NAMES = { "100 ANALYST", "200 CLERK" };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            TestUtil.dropTable(conn, "job CASCADE");
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

        private List<int> getJobnos()
        {
            var list = new List<int>();

            var command = "select jobno from job order by jobno";

            var seletCommand = new EDBCommand(command, conn);
            EDBDataReader selectResult = seletCommand.ExecuteReader();
            while (selectResult.Read())
            {
                list.Add(selectResult.GetInt32(0));
            }

            selectResult.Close();

            return list;
        }

        [Test]
        public void SimpleDynamicSQLTest()
        {
            //This example shows basic dynamic SQL commands as string literals.
            //Execute("DROP PROCEDURE SimpleDynamicSQL_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE SimpleDynamicSQL_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    v_sql           VARCHAR2(50);\n"
                + "BEGIN\n"
                + "EXECUTE IMMEDIATE 'CREATE TABLE job (jobno NUMBER(3),' ||\n"
                + "        ' jname VARCHAR2(9))';"
                + "    v_sql := 'INSERT INTO job VALUES (100, ''ANALYST'')';\n"
                + "    EXECUTE IMMEDIATE v_sql;\n"
                + "    v_sql := 'INSERT INTO job VALUES (200, ''CLERK'')';\n"
                + "    EXECUTE IMMEDIATE v_sql;\n"
                + "END;";

            //Create SP.
            Execute(sqlStr);

            //Execute SP.
            using (var cstmt = new EDBCommand("SimpleDynamicSQL_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            var list = getJobnos();
            Assert.AreEqual(JOB_100_200.Length, list.Count);
            for (var i = 0; i < list.Count; i++)
                Assert.AreEqual(JOB_100_200[i], list[i]);
        }

        [Test]
        public void UsingClauseDynamicSQLTest()
        {
            Execute("CREATE TABLE job(jobno NUMBER(3), jname VARCHAR2(9))");

            //This example uses the USING clause to pass values to placeholders in
            //the SQL string.
            Execute("DROP PROCEDURE UsingClauseDynamicSQL_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE UsingClauseDynamicSQL_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    v_sql           VARCHAR2(50) := 'INSERT INTO job VALUES ' ||\n"
                + "                        '(:p_jobno, :p_jname)';\n"
                + "    v_jobno         job.jobno%TYPE;\n"
                + "    v_jname         job.jname%TYPE;\n"
                + "BEGIN\n"
                + "    v_jobno := 300;\n"
                + "    v_jname := 'MANAGER';\n"
                + "    EXECUTE IMMEDIATE v_sql USING v_jobno, v_jname;\n"
                + "    v_jobno := 400;\n"
                + "    v_jname := 'SALESMAN';\n"
                + "    EXECUTE IMMEDIATE v_sql USING v_jobno, v_jname;\n"
                + "    v_jobno := 500;\n"
                + "    v_jname := 'PRESIDENT';\n"
                + "    EXECUTE IMMEDIATE v_sql USING v_jobno, v_jname;\n"
                + "END;";

            //Create SP.
            Execute(sqlStr);

            //Execute SP.
            using (var cstmt = new EDBCommand("UsingClauseDynamicSQL_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            var list = getJobnos();
            Assert.AreEqual(JOB_300_400_500.Length, list.Count);
            for (var i = 0; i < list.Count; i++)
                Assert.AreEqual(JOB_300_400_500[i], list[i]);
        }

        [Test]
        public void IntoClauseDynamicSQLTest()
        {
            Execute("CREATE TABLE job(jobno NUMBER(3), jname VARCHAR2(9))");

            Execute("INSERT INTO job VALUES (100, 'ANALYST')");
            Execute("INSERT INTO job VALUES (200, 'CLERK')");

            //This example shows both the INTO and USING clauses. The last execution of
            //the SELECT command returns the results into a record instead of
            //individual variables.
            Execute("DROP PROCEDURE IntoClauseDynamicSQL_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE IntoClauseDynamicSQL_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    v_sql           VARCHAR2(60);\n"
                + "    v_jobno         job.jobno%TYPE;\n"
                + "    v_jname         job.jname%TYPE;\n"
                + "    r_job           job%ROWTYPE;\n"
                + "BEGIN\n"
                + "    v_sql := 'SELECT jobno, jname FROM job WHERE jobno = :p_jobno';\n"
                + "    EXECUTE IMMEDIATE v_sql INTO v_jobno, v_jname USING 100;\n"
                + "    DBMS_OUTPUT.PUT_LINE(v_jobno || ' ' || v_jname);\n"
                + "    EXECUTE IMMEDIATE v_sql INTO v_jobno, v_jname USING 200;\n"
                + "    DBMS_OUTPUT.PUT_LINE(v_jobno || ' ' || v_jname);\n"
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
                using (var cstmt = new EDBCommand("IntoClauseDynamicSQL_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(JOB_NAMES.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(JOB_NAMES[i], notice.MessageText);
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
