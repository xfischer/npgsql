using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Threading;
using System.Collections;
using System.Numerics;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2577: Regression Tests for Trigger in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBCompoundTriggerTest : TestBase
    {
        EDBConnection? conn = null;

        private static string[] FIRST_TRIGGER_RESULT = {
            "Before Statement: 11000",
            "Before Each Row: 12000",
            "After Each Row: 13000",
            "After Statement: 14000"
            };
        private static string[] TRUNCATE_TRIGGER_RESULT = {
            "Before Statement: 11000",
            "After Statement: 12000"
            };
        private static string[] CONDITIONAL_TRIGGER_INSERT_RESULT = {
            "Before Statement",
            "Before Each Row:  1600",
            "After Each Row:  1600",
            "After Statement"
            };
        private static string[] CONDITIONAL_TRIGGER_UPDATE_RESULT = {
            "Before Statement",
            "Before Each Row: 1600 7500",
            "After Each Row: 1600 7500",
            "After Statement"
            };
        private static string[] CONDITIONAL_TRIGGER_DELETE_RESULT = {
            "Before Statement",
            "Before Each Row: 7500 ",
            "After Each Row: 7500 ",
            "After Statement"
            };
        private bool triggerCreated = false;

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP Trigger hr_trigger;");
            Execute("DROP TABLE emp1 CASCADE");

            Execute("CREATE TABLE emp1(EMPNO INT, ENAME TEXT, SAL INT, DEPTNO INT)");
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

        public void createFirstTrigger(bool addEmp)
        {
            if (addEmp)
            {
                //for update or delete operation, need to insert record first
                Execute("INSERT INTO emp1 (EMPNO, ENAME, SAL, DEPTNO) VALUES(1111,'SMITH', 10000, 20);");
            }
            //Create a compound trigger named hr_trigger. The trigger uses each
            //of the four timing points to modify the salary with an INSERT, UPDATE,
            //or DELETE statement. In the global declaration section, the initial
            //salary is declared as 10,000.
            var trigger = "CREATE OR REPLACE TRIGGER hr_trigger\n"
                           + "  FOR INSERT OR UPDATE OR DELETE ON emp1\n"
                           + "    COMPOUND TRIGGER\n"
                           + "  -- Global declaration.\n"
                           + "  var_sal NUMBER := 10000;\n"
                           + "  BEFORE STATEMENT IS\n"
                           + "  BEGIN\n"
                           + "    var_sal := var_sal + 1000;\n"
                           + "    DBMS_OUTPUT.PUT_LINE('Before Statement: ' || var_sal);\n"
                           + "  END BEFORE STATEMENT;\n"
                           + "  BEFORE EACH ROW IS\n"
                           + "  BEGIN\n"
                           + "    var_sal := var_sal + 1000;\n"
                           + "    DBMS_OUTPUT.PUT_LINE('Before Each Row: ' || var_sal);\n"
                           + "  END BEFORE EACH ROW;\n"
                           + "  AFTER EACH ROW IS\n"
                           + "  BEGIN\n"
                           + "    var_sal := var_sal + 1000;\n"
                           + "    DBMS_OUTPUT.PUT_LINE('After Each Row: ' || var_sal);\n"
                           + "  END AFTER EACH ROW;\n"
                           + "  AFTER STATEMENT IS\n"
                           + "  BEGIN\n"
                           + "    var_sal := var_sal + 1000;\n"
                           + "    DBMS_OUTPUT.PUT_LINE('After Statement: ' || var_sal);\n"
                           + "  END AFTER STATEMENT;\n"
                           + "END hr_trigger;\n";
            Execute(trigger);
            triggerCreated = true;
        }

        public void createTruncateTrigger()
        {
            Execute("INSERT INTO emp1 (EMPNO, ENAME, SAL, DEPTNO) VALUES(1111,'SMITH', 10000, 20);");
            Execute("INSERT INTO emp1 (EMPNO, ENAME, SAL, DEPTNO) VALUES(2001,'JACK', 10000, 20);");
            //Create a trigger for truncate statement
            var trigger = "CREATE OR REPLACE TRIGGER hr_trigger\n"
                           + "  FOR TRUNCATE ON emp1\n"
                           + "    COMPOUND TRIGGER\n"
                           + "  -- Global declaration.\n"
                           + "  var_sal NUMBER := 10000;\n"
                           + "  BEFORE STATEMENT IS\n"
                           + "  BEGIN\n"
                           + "    var_sal := var_sal + 1000;\n"
                           + "    DBMS_OUTPUT.PUT_LINE('Before Statement: ' || var_sal);\n"
                           + "  END BEFORE STATEMENT;\n"
                           + "  AFTER STATEMENT IS\n"
                           + "  BEGIN\n"
                           + "    var_sal := var_sal + 1000;\n"
                           + "    DBMS_OUTPUT.PUT_LINE('After Statement: ' || var_sal);\n"
                           + "  END AFTER STATEMENT;\n"
                           + "END hr_trigger;";
            Execute(trigger);
            triggerCreated = true;
        }

        [Test]
        public void FirstTriggerInsertTest()
        {
            TestUtil.MinimumPgVersion(conn, "12.0.0", "EC-2548 WHEN clause not working for compound trigger in EPAS12");

            createFirstTrigger(false);
            //Insert the record into table emp1:
            var insertSql = "INSERT INTO emp1 (EMPNO, ENAME, SAL, DEPTNO) VALUES(1111,'SMITH', 10000, 20);";

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
                Execute(insertSql);

                mre.WaitOne(5000);
                Assert.AreEqual(FIRST_TRIGGER_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(FIRST_TRIGGER_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void FirstTriggerUpdateTest()
        {
            TestUtil.MinimumPgVersion(conn, "12.0.0", "EC-2548 WHEN clause not working for compound trigger in EPAS12");

            createFirstTrigger(true);

            //The UPDATE statement updates the employee salary record.
            var updateSql = "UPDATE emp1 SET SAL = 15000 where EMPNO = 1111;;";

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
                Execute(updateSql);

                mre.WaitOne(5000);
                Assert.AreEqual(FIRST_TRIGGER_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(FIRST_TRIGGER_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void FirstTriggerDeleteTest()
        {
            TestUtil.MinimumPgVersion(conn, "12.0.0", "EC-2548 WHEN clause not working for compound trigger in EPAS12");

            createFirstTrigger(true);
            //The DELETE statement deletes the employee salary record.
            var deleteSql = "DELETE from emp1 where EMPNO = 1111;";

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
                Execute(deleteSql);

                mre.WaitOne(5000);
                Assert.AreEqual(FIRST_TRIGGER_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(FIRST_TRIGGER_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();

        }

        [Test]
        public void TruncateStatmentTest()
        {
            TestUtil.MinimumPgVersion(conn, "12.0.0", "EC-2548 WHEN clause not working for compound trigger in EPAS12");

            createTruncateTrigger();
            //The TRUNCATE statement removes all the records from the emp1 table.
            var sql = "TRUNCATE emp1;";

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
                Execute(sql);

                mre.WaitOne(5000);
                Assert.AreEqual(TRUNCATE_TRIGGER_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(TRUNCATE_TRIGGER_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        public void createConditionalTrigger(bool addEmp, bool updateEmp)
        {
            if (addEmp)
            {
                //insert recored for update and delete statement
                Execute("INSERT INTO emp1 (EMPNO, ENAME, SAL, DEPTNO) VALUES(1111,'SMITH', 1600, 20);");
            }
            if (updateEmp)
            {
                //update record before delete statement
                Execute("UPDATE emp1 SET SAL = 7500 where EMPNO = 1111;");
            }
            //This example creates a compound trigger named hr_trigger on the emp1 table with a WHEN condition.
            //The WHEN condition checks and prints the employee salary when an INSERT, UPDATE, or DELETE
            //statement affects the emp1 table. The database evaluates the WHEN condition for a row-level
            //trigger, and the trigger executes once per row if the WHEN condition evaluates to TRUE.
            //The statement-level trigger executes regardless of the WHEN condition.
            var trigger = "CREATE OR REPLACE TRIGGER hr_trigger\n"
                           + "  FOR INSERT OR UPDATE OR DELETE ON emp1\n"
                           + "  REFERENCING NEW AS new OLD AS old\n"
                           + "  WHEN (old.sal > 5000 OR new.sal < 8000)\n"
                           + "    COMPOUND TRIGGER\n"
                           + "  BEFORE STATEMENT IS\n"
                           + "  BEGIN\n"
                           + "    DBMS_OUTPUT.PUT_LINE('Before Statement');\n"
                           + "  END BEFORE STATEMENT;\n"
                           + "  BEFORE EACH ROW IS\n"
                           + "  BEGIN\n"
                           + "    DBMS_OUTPUT.PUT_LINE('Before Each Row: ' || :OLD.sal ||' ' || :NEW.sal);\n"
                           + "  END BEFORE EACH ROW;\n"
                           + "  AFTER EACH ROW IS\n"
                           + "  BEGIN\n"
                           + "    DBMS_OUTPUT.PUT_LINE('After Each Row: ' || :OLD.sal ||' ' || :NEW.sal);\n"
                           + "  END AFTER EACH ROW;\n"
                           + "  AFTER STATEMENT IS\n"
                           + "  BEGIN\n"
                           + "    DBMS_OUTPUT.PUT_LINE('After Statement');\n"
                           + "  END AFTER STATEMENT;\n"
                           + "END hr_trigger;";
            Execute(trigger);
            triggerCreated = true;
        }

        [Test]
        public void ConditionalTrigerInsertTest()
        {
            //EC-2548 WHEN clause not working for compound trigger in EPAS12
            TestUtil.MinimumPgVersion(conn, "13.0.0", "EC-2548 WHEN clause not working for compound trigger in EPAS12");

            createConditionalTrigger(false, false);
            //Insert the record into table emp1.
            var insertSql = "INSERT INTO emp1 (EMPNO, ENAME, SAL, DEPTNO) VALUES(1111,'SMITH', 1600, 20);";

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
                Execute(insertSql);

                mre.WaitOne(5000);
                Assert.AreEqual(CONDITIONAL_TRIGGER_INSERT_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(CONDITIONAL_TRIGGER_INSERT_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void ConditionalTrigerUpdateTest()
        {
            //EC-2548 WHEN clause not working for compound trigger in EPAS12
            TestUtil.MinimumPgVersion(conn, "13.0.0", "EC-2548 WHEN clause not working for compound trigger in EPAS12");

            createConditionalTrigger(true, false);

            //The UPDATE statement updates the employee salary record.
            var updateSql = "UPDATE emp1 SET SAL = 7500 where EMPNO = 1111;";

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
                Execute(updateSql);

                mre.WaitOne(5000);
                Assert.AreEqual(CONDITIONAL_TRIGGER_UPDATE_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(CONDITIONAL_TRIGGER_UPDATE_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void ConditionalTrigerDeleteTest()
        {
            //EC-2548 WHEN clause not working for compound trigger in EPAS12
            TestUtil.MinimumPgVersion(conn, "13.0.0", "EC-2548 WHEN clause not working for compound trigger in EPAS12");

            createConditionalTrigger(true, true);
            //The DELETE statement deletes the employee salary record.
            var deleteSql = "DELETE from emp1 where EMPNO = 1111;";

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
                Execute(deleteSql);

                mre.WaitOne(5000);
                Assert.AreEqual(CONDITIONAL_TRIGGER_DELETE_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(CONDITIONAL_TRIGGER_DELETE_RESULT[i], notice.MessageText);
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

