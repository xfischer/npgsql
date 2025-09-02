using System;
using NUnit.Framework;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.IO;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{

    [TestFixture]
    [NonParallelizable]
    internal class EDBAS18Tests : EPASTestBase
    {
        private async Task<int> Execute(EDBConnection conn, string query, bool ignoreResult)
        {
            try
            {
                //await using var conn = await OpenConnectionAsync();

                using (var com = new EDBCommand("", conn))
                {
                    com.CommandType = CommandType.Text;

                    com.CommandText = query;
                    return await com.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                //In case of drop statement, the object may not exist.
                //So we do not care about the result.
                if (!ignoreResult)
                    Assert.Fail(ex.Message);
            }

            return 0;
        }

        //Simple select returning single value
        private async Task<object?> ExecuteSimpleReader(EDBConnection conn, string query)
        {
            object? val = null;
            try
            {
                using (var com = new EDBCommand("", conn))
                {
                    com.CommandType = CommandType.Text;

                    com.CommandText = query;
                    EDBDataReader reader = await com.ExecuteReaderAsync();

                    Assert.IsTrue(reader.HasRows);

                    if (await reader.ReadAsync())
                    {
                        val = reader.GetValue(0);
                    }
                    await reader.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            return val;
        }

        private async Task RunQueryAndVerifyResultAsync(EDBConnection conn, string query, string[] expected)
        {
            try
            {
                using (var com = new EDBCommand("", conn))
                {
                    com.CommandType = CommandType.Text;

                    com.CommandText = query;
                    EDBDataReader reader = await com.ExecuteReaderAsync();

                    Assert.IsTrue(reader.HasRows);

                    int i = 0;
                    while (await reader.ReadAsync())
                    {
                        Assert.AreEqual(expected[i], reader.GetString(0));
                        i++;
                    }
                    Assert.AreEqual(expected.Length, i);
                    await reader.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        private static void RunQueryAndVerifyResult(EDBConnection conn, string query, string[] expected)
        {
            try
            {
                using (var com = new EDBCommand("", conn))
                {
                    com.CommandType = CommandType.Text;

                    com.CommandText = query;
                    using (EDBDataReader reader = com.ExecuteReader())
                    {
                        Assert.IsTrue(reader.HasRows);

                        int i = 0;
                        while (reader.Read())
                        {
                            Assert.AreEqual(expected[i], reader.GetString(0));
                            i++;
                        }
                        Assert.AreEqual(expected.Length, i);
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        //Executes a stored procedure with no arguments
        //If there are any messages from dbms_output.put_line, they are retured as a list.
        public async Task<List<string>> ExecuteProcNotice(EDBConnection conn, string sqlStr)
        {
            var messages = new List<string>();

            //await using var conn = await OpenConnectionAsync();

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

                    await cstmt.PrepareAsync();
                    await cstmt.ExecuteNonQueryAsync();
                }
                mre.WaitOne(5000);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    messages.Add(notice!.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();

            return messages;
        }

        private async Task SetUpSchedularTest(EDBConnection conn)
        {
            await Execute(conn, "drop procedure db_922_negative_bymonthday;", true);
            await Execute(conn, "drop procedure db_2762_support_interval_keyword;", true);
            await Execute(conn, "drop procedure db_2284_secondly_implementation;", true);

            var procSql1 = "CREATE OR REPLACE PROCEDURE db_922_negative_bymonthday()\n"
            + "IS\n"
            + "  nrd timestamp;\n"
            + "  rda timestamp := null;\n"
            + "BEGIN\n"
            + "  for i in 1 .. 10 loop\n"
            + "    dbms_scheduler.evaluate_calendar_string('freq = monthly; byhour=14; byminute=30; bymonthday=-20', to_date('15-May-2029 14:30:00', 'dd-Mon-yyyy hh24:mi:ss'), rda, nrd);\n"
            + "    dbms_output.put_line('Next run date : ' || to_char(nrd, 'dd-Mon-yyyy hh24:mi:ss'));\n"
            + "    rda := nrd;\n"
            + "  END loop;\n"
            + "END;\n";

            await Execute(conn, procSql1, false);

            var procSql2 = "CREATE OR REPLACE PROCEDURE db_2762_support_interval_keyword()\n"
            + "IS\n"
            + "  nrd timestamp;\n"
            + "  rda timestamp := null;\n"
            + "BEGIN\n"
            + "  for i in 1 .. 10 loop\n"
            + "    dbms_scheduler.evaluate_calendar_string('FREQ=monthly;interval=2', to_date('31-Jan-2028 06:00:00', 'dd-Mon-yyyy hh24:mi:ss'), rda, nrd);\n"
            + "    dbms_output.put_line('Next run date : ' || to_char(nrd, 'dd-Mon-yyyy hh24:mi:ss'));\n"
            + "    rda := nrd;\n"
            + "  END loop;\n"
            + "END;\n";

            await Execute(conn, procSql2, false);

            var procSql3 = "CREATE OR REPLACE PROCEDURE db_2284_secondly_implementation()\n"
            + "IS\n"
            + "  nrd timestamp;\n"
            + "  rda timestamp := null;\n"
            + "BEGIN\n"
            + "  for i in 1 .. 5 loop\n"
            + "    dbms_scheduler.evaluate_calendar_string('freq = secondly; bydate=20300214,20420921,20350419;interval=2; byminute=3;byhour=2;', to_date('15-May-2029 14:37:00', 'dd-Mon-yyyy hh24:mi:ss'), rda, nrd);\n"
            + "    dbms_output.put_line('Next run date : ' || to_char(nrd, 'dd-Mon-yyyy hh24:mi:ss'));\n"
            + "    rda := nrd;\n"
            + "  END loop;\n"
            + "END;\n";

            await Execute(conn, procSql3, false);

        }

        //--DB-2284 : DBMS SCHEDULER SECONDLY Implementation for Oracle compatibility.
        [Test]
        public async Task DB_2284_SecondlyImplementation_Test()
        {
            await using var conn = await OpenConnectionAsync();

            TestUtil.MinimumPgVersion(conn, "18.0.0");
            await SetUpSchedularTest(conn);

            var procMsg = new string[] {
                    "Next run date : 11-Jun-2029 14:30:00",
                    "Next run date : 12-Jul-2029 14:30:00",
                    "Next run date : 12-Aug-2029 14:30:00",
                    "Next run date : 11-Sep-2029 14:30:00",
                    "Next run date : 12-Oct-2029 14:30:00",
                    "Next run date : 11-Nov-2029 14:30:00",
                    "Next run date : 12-Dec-2029 14:30:00",
                    "Next run date : 12-Jan-2030 14:30:00",
                    "Next run date : 09-Feb-2030 14:30:00",
                    "Next run date : 12-Mar-2030 14:30:00"
            };
            var message1 = await ExecuteProcNotice(conn, "db_922_negative_bymonthday");
            Assert.AreEqual(10, message1.Count);
            for(var i=0; i<10; i++)
                Assert.AreEqual(procMsg[i], message1[i]);
        }

        //--DB-2762 : DBMS SCHEDULER supporting INTERVAL keyword
        [Test]
        public async Task DB_2762_SupportIntervalKeyword_Test()
        {
            await using var conn = await OpenConnectionAsync();

            TestUtil.MinimumPgVersion(conn, "18.0.0");
            await SetUpSchedularTest(conn);

            var procMsg = new string[] {
                    "Next run date : 31-Jan-2028 06:00:00",
                    "Next run date : 31-Mar-2028 06:00:00",
                    "Next run date : 31-May-2028 06:00:00",
                    "Next run date : 31-Jul-2028 06:00:00",
                    "Next run date : 31-Jan-2029 06:00:00",
                    "Next run date : 31-Mar-2029 06:00:00",
                    "Next run date : 31-May-2029 06:00:00",
                    "Next run date : 31-Jul-2029 06:00:00",
                    "Next run date : 31-Jan-2030 06:00:00",
                    "Next run date : 31-Mar-2030 06:00:00"
            };
            var message1 = await ExecuteProcNotice(conn, "db_2762_support_interval_keyword");
            Assert.AreEqual(10, message1.Count);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(procMsg[i], message1[i]);
        }

        //--DB-922 : Support negative values for BYMONTHDAY in DBMS_SCHEDULER
        [Test]
        public async Task DB_922_SupportNegativeByMonthDay_Test()
        {
            await using var conn = await OpenConnectionAsync();

            TestUtil.MinimumPgVersion(conn, "18.0.0");
            await SetUpSchedularTest(conn);

            var procMsg = new string[] {
                    "Next run date : 11-Jun-2029 14:30:00",
                    "Next run date : 12-Jul-2029 14:30:00",
                    "Next run date : 12-Aug-2029 14:30:00",
                    "Next run date : 11-Sep-2029 14:30:00",
                    "Next run date : 12-Oct-2029 14:30:00",
                    "Next run date : 11-Nov-2029 14:30:00",
                    "Next run date : 12-Dec-2029 14:30:00",
                    "Next run date : 12-Jan-2030 14:30:00",
                    "Next run date : 09-Feb-2030 14:30:00",
                    "Next run date : 12-Mar-2030 14:30:00"
            };
            var message1 = await ExecuteProcNotice(conn, "db_922_negative_bymonthday");
            Assert.AreEqual(10, message1.Count);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(procMsg[i], message1[i]);
        }
    }
}
