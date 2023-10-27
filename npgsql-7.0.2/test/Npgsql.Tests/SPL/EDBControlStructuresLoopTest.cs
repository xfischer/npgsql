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

//EC-2583: Regression tests for Case Expression and Case Statement in SPL

//These tests are implemented in JDBC as Anonymous block.
//We have implemented them in stored procedures because anonymous blocks do not work in .NET.

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBControlStructuresLoopTest : TestBase
    {
        EDBConnection? conn = null;

        private static string[] EXIT_WHILE_FOR_RESULT = {
            "Iteration # 1",
            "Iteration # 2",
            "Iteration # 3",
            "Iteration # 4",
            "Iteration # 5",
            "Iteration # 6",
            "Iteration # 7",
            "Iteration # 8",
            "Iteration # 9",
            "Iteration # 10"
            };

        private static string[] CONTINUE_RESULT = {
            "Iteration # 2",
            "Iteration # 4",
            "Iteration # 6",
            "Iteration # 8",
            "Iteration # 10"
            };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();
            TestUtil.EnsureEDBAdvancedServer(conn);
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
        public void ExitTest()
        {
            // The following is a simple example of a loop that iterates ten times
            // and then uses the EXIT statement to terminate.
            Execute("DROP PROCEDURE ExitTest_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE ExitTest_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                                  + "    v_counter       NUMBER(2);\n"
                      + "BEGIN\n"
                      + "    v_counter := 1;\n"
                      + "    LOOP\n"
                      + "        EXIT WHEN v_counter > 10;\n"
                      + "        DBMS_OUTPUT.PUT_LINE('Iteration # ' || v_counter);\n"
                      + "        v_counter := v_counter + 1;\n"
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
                using (var cstmt = new EDBCommand("ExitTest_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EXIT_WHILE_FOR_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EXIT_WHILE_FOR_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
    public void ContinueTest()
        {
            //The following is a variation of the previous example
            //that uses the CONTINUE statement to skip the display of the odd numbers.
            Execute("DROP PROCEDURE ContinueTest_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE ContinueTest_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                                  + "    v_counter       NUMBER(2);\n"
                      + "BEGIN\n"
                      + "    v_counter := 0;\n"
                      + "    LOOP\n"
                      + "        v_counter := v_counter + 1;\n"
                      + "        EXIT WHEN v_counter > 10;\n"
                      + "        CONTINUE WHEN MOD(v_counter,2) = 1;\n"
                      + "        DBMS_OUTPUT.PUT_LINE('Iteration # ' || v_counter);\n"
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
                using (var cstmt = new EDBCommand("ContinueTest_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(CONTINUE_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(CONTINUE_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
    }

        [Test]
    public void WhileTest()
        {
            //The following example contains the same logic as in the previous example
            //except the WHILE statement is used to take the place of the EXIT
            //statement to determine when to exit the loop.
            Execute("DROP PROCEDURE WhileTest_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE WhileTest_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                              + "    v_counter       NUMBER(2);\n"
                + "BEGIN\n"
                + "    v_counter := 1;\n"
                + "    WHILE v_counter <= 10 LOOP\n"
                + "        DBMS_OUTPUT.PUT_LINE('Iteration # ' || v_counter);\n"
                + "        v_counter := v_counter + 1;\n"
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
                using (var cstmt = new EDBCommand("WhileTest_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EXIT_WHILE_FOR_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EXIT_WHILE_FOR_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
    }

        [Test]
    public void ForTest()
        {
            //The following example simplifies the WHILE loop example
            //even further by using a FOR loop that iterates from 1 to 10.
            Execute("DROP PROCEDURE ForTest_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE ForTest_SP()\n"
                      + " IS\n"
                      + "BEGIN\n"
                      + "    FOR i IN 1 .. 10 LOOP\n"
                      + "        DBMS_OUTPUT.PUT_LINE('Iteration # ' || i);\n"
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
                using (var cstmt = new EDBCommand("ForTest_SP", conn))
                {
                    cstmt.CommandType = CommandType.StoredProcedure;

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EXIT_WHILE_FOR_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EXIT_WHILE_FOR_RESULT[i], notice.MessageText);
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

