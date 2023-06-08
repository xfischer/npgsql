using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2592: Regression Tests for Associative Arrays

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBAssociativeArrayTest : TestBase
    {
        //EDBConnection? conn = null;

        private static string[] enames = new string[] { "SMITH", "ALLEN", "WARD", "JONES",
            "MARTIN", "BLAKE", "CLARK","SCOTT", "KING", "TURNER" };
        private static int[] empnos = new int[] { 7369, 7499, 7521, 7566, 7654, 7698,
            7782, 7788, 7839, 7844 };
        private static string[] jobTypes = new string[] { "ANALYST", "CLERK", "MANAGER",
            "SALESMAN", "PRESIDENT" };
        private static string[] jobNumbers = new string[] { "100", "200", "300", "400", "500" };
        private static int EMP_TOTAL = empnos.Length;
        private static int JOB_TOTAL = jobTypes.Length;

        [SetUp]
        public async Task Init()
        {
            //conn = OpenConnection();
            var conn = await OpenConnectionAsync();
            TestUtil.dropTable(conn, "emp1 CASCADE");

            await Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(10))");
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
                    await cstmt.ExecuteNonQueryAsync();
                }
            }
        }

        //[TearDown]
        //public async Task Dispose()
        //{
        //    TestUtil.closeDB(conn);
        //}

        private async Task<int> Execute(string query, bool checkResult=false)
        {
            try
            {
                var conn = await OpenConnectionAsync();

                using (var com = new EDBCommand("", conn))
                {
                    com.CommandType = CommandType.Text;

                    com.CommandText = query;
                    return await com.ExecuteNonQueryAsync();
                }
            }
            catch(Exception ex)
            {
                if (checkResult)
                    Assert.Fail(ex.Message);
            }

            return 0;
        }

        //private int Execute2(string query)
        //{
        //    try
        //    {
        //        using (var com = new EDBCommand(query, conn))
        //        {
        //            com.CommandType = CommandType.Text;
        //            return com.ExecuteNonQuery();
        //        }
        //    }
        //    catch
        //    {
        //    }

        //    return 0;
        //}

        [Test]
        public async Task SimpleAssociativeArrayTest()
        {
            var conn = await OpenConnectionAsync();
            //The following example reads the first ten employee names from the emp1 table,
            //stores them in an array, then displays the results from the array.
            await Execute("DROP PROCEDURE SimpleAssociativeArray_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE SimpleAssociativeArray_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                            + " TYPE emp_arr_typ IS TABLE OF VARCHAR2(10) INDEX BY BINARY_INTEGER; \n"
                            + " emp_arr         emp_arr_typ; \n"
                            + " CURSOR emp_cur IS SELECT ename FROM emp1 WHERE ROWNUM <= 10 order by empno; \n"
                            + " i  INTEGER := 0; \n"
                            + " BEGIN \n"
                            + " FOR r_emp IN emp_cur LOOP \n"
                            + "   i := i + 1; \n"
                            + "   emp_arr(i) := r_emp.ename; \n"
                            + " END LOOP; \n "
                            + " FOR j IN 1..10 LOOP \n"
                            + "    DBMS_OUTPUT.PUT_LINE(emp_arr(j)); \n"
                            + " END LOOP; \n"
                            + " END; ";

            await Execute(sqlStr, true);

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
                using (var cstmt = new EDBCommand("SimpleAssociativeArray_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    await cstmt.ExecuteNonQueryAsync();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_TOTAL, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(enames[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public async Task RecordTypeAssociativeArrayTest()
        {
            var conn = await OpenConnectionAsync();
            // use a record type in the array definition
            await Execute("DROP PROCEDURE RecordTypeAssociativeArray_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE RecordTypeAssociativeArray_SP()\n"
                    + " IS\n"
                    + " DECLARE\n"
                    + "    TYPE emp_rec_typ IS RECORD (\n"
                    + "        empno       NUMBER(4),\n"
                    + "        ename       VARCHAR2(10)\n"
                    + "    );\n"
                    + "    TYPE emp_arr_typ IS TABLE OF emp_rec_typ INDEX BY BINARY_INTEGER;\n"
                    + "    emp_arr         emp_arr_typ;\n"
                    + "    CURSOR emp_cur IS SELECT empno, ename FROM emp1 WHERE ROWNUM <= 10 order by empno;\n"
                    + "    i               INTEGER := 0;\n"
                    + " BEGIN\n "
                    + "    FOR r_emp IN emp_cur LOOP\n"
                    + "        i := i + 1;\n"
                    + "        emp_arr(i).empno := r_emp.empno;\n"
                    + "        emp_arr(i).ename := r_emp.ename;\n"
                    + "    END LOOP;\n" + "    FOR j IN 1..10 LOOP\n"
                    + "        DBMS_OUTPUT.PUT_LINE(emp_arr(j).empno || ':' ||\n"
                    + "            emp_arr(j).ename);\n"
                    + "    END LOOP;\n"
                    + " END;";
            await Execute(sqlStr, true);

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
                using (var cstmt = new EDBCommand("RecordTypeAssociativeArray_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    await cstmt.ExecuteNonQueryAsync();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_TOTAL, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    var arr = notice.MessageText.Split(":");
                    var empno = arr[0].Trim();
                    var ename = arr[1].Trim();
                    Assert.AreEqual(empnos[i].ToString(), empno);
                    Assert.AreEqual(enames[i], ename);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public async Task RowTypeAssociativeArrayTest()
        {
            var conn = await OpenConnectionAsync();

            //The emp1%ROWTYPE attribute could be used to define emp_arr_typ
            await Execute("DROP PROCEDURE RowTypeAssociativeArray_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE RowTypeAssociativeArray_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE emp_arr_typ IS TABLE OF emp1%ROWTYPE INDEX BY BINARY_INTEGER;\n"
                + "    emp_arr         emp_arr_typ;\n"
                + "    CURSOR emp_cur IS SELECT empno, ename FROM emp1 WHERE ROWNUM <= 10 order by empno;\n"
                + "    i               INTEGER := 0;\n"
                + " BEGIN\n"
                + "    FOR r_emp IN emp_cur LOOP\n"
                + "        i := i + 1;\n"
                + "        emp_arr(i).empno := r_emp.empno;\n"
                + "        emp_arr(i).ename := r_emp.ename;\n"
                + "    END LOOP;\n"
                + "    FOR j IN 1..10 LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE(emp_arr(j).empno || ':' ||\n"
                + "            emp_arr(j).ename);\n"
                + "    END LOOP;\n"
                + " END;";

            await Execute(sqlStr, true);

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
                using (var cstmt = new EDBCommand("RowTypeAssociativeArray_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    await cstmt.ExecuteNonQueryAsync();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_TOTAL, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    var arr = notice.MessageText.Split(":");
                    var empno = arr[0].Trim();
                    var ename = arr[1].Trim();
                    Assert.AreEqual(empnos[i].ToString(), empno);
                    Assert.AreEqual(enames[i], ename);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public async Task RecordLevelAssignmentAssociativeArrayTestTest()
        {
            var conn = await OpenConnectionAsync();
            //a record level assignment can be made from r_emp to emp_arr.
            await Execute("DROP PROCEDURE RecordLevelAssignmentAssociativeArrayTest_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE RecordLevelAssignmentAssociativeArrayTest_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE emp_rec_typ IS RECORD (\n"
                + "        empno       NUMBER(4),\n"
                + "        ename       VARCHAR2(10)\n"
                + "    );\n"
                + "    TYPE emp_arr_typ IS TABLE OF emp_rec_typ INDEX BY BINARY_INTEGER;\n"
                + "    emp_arr         emp_arr_typ;\n"
                + "    CURSOR emp_cur IS SELECT empno, ename FROM emp1 WHERE ROWNUM <= 10 order by empno;\n"
                + "    i               INTEGER := 0;\n"
                + " BEGIN\n"
                + "    FOR r_emp IN emp_cur LOOP\n"
                + "        i := i + 1;\n"
                + "        emp_arr(i) := r_emp;\n"
                + "    END LOOP;\n"
                + "    FOR j IN 1..10 LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE(emp_arr(j).empno || ':' ||\n"
                + "            emp_arr(j).ename);\n"
                + "    END LOOP;\n"
                + "END;";
            await Execute(sqlStr, true);

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
                using (var cstmt = new EDBCommand("RecordLevelAssignmentAssociativeArrayTest_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    await cstmt.ExecuteNonQueryAsync();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_TOTAL, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    var arr = notice.MessageText.Split(":");
                    var empno = arr[0].Trim();
                    var ename = arr[1].Trim();
                    Assert.AreEqual(empnos[i].ToString(), empno);
                    Assert.AreEqual(enames[i], ename);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public async Task CharacterDataAssociativeArrayTest()
        {
            var conn = await OpenConnectionAsync();
            //The key of an associative array can be character data
            await Execute("DROP PROCEDURE CharacterDataAssociativeArray_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE CharacterDataAssociativeArray_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE job_arr_typ IS TABLE OF NUMBER INDEX BY VARCHAR2(9);\n"
                + "    job_arr         job_arr_typ;\n"
                + " BEGIN\n"
                + "    job_arr('ANALYST')   := 100;\n"
                + "    job_arr('CLERK')     := 200;\n"
                + "    job_arr('MANAGER')   := 300;\n"
                + "    job_arr('SALESMAN')  := 400;\n"
                + "    job_arr('PRESIDENT') := 500;\n"
                + "    DBMS_OUTPUT.PUT_LINE('ANALYST  : ' || job_arr('ANALYST'));\n"
                + "    DBMS_OUTPUT.PUT_LINE('CLERK    : ' || job_arr('CLERK'));\n"
                + "    DBMS_OUTPUT.PUT_LINE('MANAGER  : ' || job_arr('MANAGER'));\n"
                + "    DBMS_OUTPUT.PUT_LINE('SALESMAN : ' || job_arr('SALESMAN'));\n"
                + "    DBMS_OUTPUT.PUT_LINE('PRESIDENT: ' || job_arr('PRESIDENT'));\n"
                + " END;";

            await Execute(sqlStr, true);

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
                using (var cstmt = new EDBCommand("CharacterDataAssociativeArray_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    await cstmt.ExecuteNonQueryAsync();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(JOB_TOTAL, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    var arr = notice.MessageText.Split(":");
                    var jobType = arr[0].Trim();
                    var jobNumber = arr[1].Trim();
                    Assert.AreEqual(jobTypes[i], jobType);
                    Assert.AreEqual(jobNumbers[i], jobNumber);
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
