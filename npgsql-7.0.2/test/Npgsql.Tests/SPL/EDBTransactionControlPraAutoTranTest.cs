using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Collections.Generic;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2581: Regression Tests for Transaction Control in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBTransactionControlPraAutoTranTest : TestBase
    {
        EDBConnection? conn = null;

        private static int[] DEPT_50_60_70 = { 50, 60, 70 };
        private static int[] DEPT_60_70 = { 60, 70 };
        private static int[] DEPT_50 = { 50 };
        private static int[] DEPT_50_60 = { 50, 60 };
        private static int[] DEPT_60 = { 60 };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();
            TestUtil.EnsureEDBAdvancedServer(conn);

            Execute("DROP PROCEDURE insert_dept_70;");
            Execute("DROP PROCEDURE insert_dept_70_rollback;");
            Execute("DROP PROCEDURE insert_dept_70_auto;");
            Execute("DROP trigger emp_audit_trig on emp;");
            Execute("DROP TYPE BODY insert_dept_typ;");
            Execute("DROP TYPE insert_dept_typ;");

            TestUtil.dropTable(conn, "dept1 CASCADE");
            TestUtil.dropTable(conn, "emp1 CASCADE");
            TestUtil.dropTable(conn, "empauditlog CASCADE");

            Execute("CREATE TABLE dept1(deptno NUMBER(8) UNIQUE,dname VARCHAR2(14), loc  VARCHAR2(13))");
            Execute("CREATE TABLE emp1(empno NUMBER(8),  ename VARCHAR2(10),job VARCHAR2(9), "
                    + "mgr NUMBER(8), hiredate DATE, sal NUMBER(10,2), comm NUMBER(10,2), "
                     + "deptno NUMBER(8))");
            Execute("CREATE TABLE empauditlog("
                    + "audit_date DATE, audit_user VARCHAR2(20), audit_desc VARCHAR2(20))");

            var createPro = "CREATE OR REPLACE PROCEDURE insert_dept_70 IS\n"
                    + "BEGIN\n"
                     + "    INSERT INTO dept1 VALUES (70,'MARKETING','LOS ANGELES');\n"
                    + "END;";
            Execute(createPro);

            //This procedure has the ROLLBACK command at the end. However,
            //the PRAGMA ANONYMOUS_TRANSACTION isn't included in this procedure.
            var createProRollback = "CREATE OR REPLACE PROCEDURE insert_dept_70_rollback IS\n"
                    + "BEGIN\n"
                     + "    INSERT INTO dept1 VALUES (70,'MARKETING','LOS ANGELES');\n"
                    + "    ROLLBACK;\n"
                     + "END;";
            Execute(createProRollback);

            //The procedure with the ROLLBACK command at the end also has PRAGMA
            //ANONYMOUS_TRANSACTION included. This isolates the effect of the
            //ROLLBACK command in the procedure.
            var createProAuto = "CREATE OR REPLACE PROCEDURE insert_dept_70_auto IS\n"
                    + "    PRAGMA AUTONOMOUS_TRANSACTION;\n"
                     + "BEGIN\n"
                    + "    INSERT INTO dept1 VALUES (70,'MARKETING','LOS ANGELES');\n"
                     + "    ROLLBACK;\n"
                     + "END;";
            Execute(createProAuto);

            //The trigger attached to the emp1 table that inserts these changes into
            //the empauditlog table is the following. PRAGMA AUTONOMOUS_TRANSACTION
            //is included in the declaration section.
            var createTriger = "CREATE OR REPLACE TRIGGER emp_audit_trig\n"
                    + "    AFTER INSERT OR UPDATE OR DELETE ON emp1\n" + "DECLARE\n"
                    + "    PRAGMA AUTONOMOUS_TRANSACTION;\n"
                    + "    v_action        VARCHAR2(20);\n"
                    + "BEGIN\n"
                    + "    IF INSERTING THEN\n"
                    + "        v_action := 'Added employee(s)';\n"
                    + "    ELSIF UPDATING THEN\n"
                    + "        v_action := 'Updated employee(s)';\n"
                    + "    ELSIF DELETING THEN\n"
                    + "        v_action := 'Deleted employee(s)';\n"
                    + "    END IF;\n"
                    + "    INSERT INTO empauditlog VALUES (SYSDATE, USER,\n"
                    + "        v_action);\n"
                    + "END;";
            Execute(createTriger);

            //The following object type and object type body are created. The member procedure
            //in the object type body contains the PRAGMA AUTONOMOUS_TRANSACTION in the
            //declaration section along with COMMIT at the end of the procedure.
            var createType = "CREATE OR REPLACE TYPE insert_dept_typ AS OBJECT (\n"
                    + "    deptno          NUMBER(2),\n"
                     + "    dname           VARCHAR2(14),\n"
                    + "    loc             VARCHAR2(13),\n"
                     + "    MEMBER PROCEDURE insert_dept\n"
                    + ");\n";
            Execute(createType);

            var createTypeBody = "CREATE OR REPLACE TYPE BODY insert_dept_typ AS\n"
                     + "    MEMBER PROCEDURE insert_dept\n"
                     + "    IS\n"
                     + "        PRAGMA AUTONOMOUS_TRANSACTION;\n"
                     + "    BEGIN\n"
                     + "        INSERT INTO dept1 VALUES (SELF.deptno,SELF.dname,SELF.loc);\n"
                     + "        COMMIT;\n"
                       + "    END;\n"
                     + "END;";
            Execute(createTypeBody);
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

        private List<int> getDeptnos()
        {
            var list = new List<int>();
            var command = "select deptNo from dept1 order by deptNo";

            using (var selectCommand = new EDBCommand(command, conn))
            {
                var selectResult = selectCommand.ExecuteReader();
                while (selectResult.Read())
                    list.Add(selectResult.GetInt32(0));
                selectResult.Close();
            }

            return list;
        }

        private int getEmpCount()
        {
            var count = -1;
            var command = "select count(*) from emp1";

            using (var selectCommand = new EDBCommand(command, conn))
            {
                var selectResult = selectCommand.ExecuteReader();
                selectResult.Read();
                count = selectResult.GetInt32(0);
                selectResult.Close();
            }

            return count;
        }

        private int getEmpAuditLogCount()
        {
            var count = -1;
            var command = "select count(*) from empauditlog";

            using (var selectCommand = new EDBCommand(command, conn))
            {
                var selectResult = selectCommand.ExecuteReader();
                selectResult.Read();
                count = selectResult.GetInt32(0);
                selectResult.Close();
            }

            return count;
        }

        [Test]
        public void Scenario1aTest()
        {
            //Below is comment from JDBC test but it looks like .NET does not support running
            //such queries, at least, we know anonymous blocks are not supported.
            //We have changed the test to run insert separately and run anonymous blcok as
            //Stored procedure.

            //This first set of scenarios shows the insertion of three rows:
            //Starting just after the initial BEGIN command of the transaction
            //From an anonymous block in the starting transactions
            //From a stored procedure executed from the anonymous block
            //After the final commit, all three rows are inserted.

            Execute("DROP PROCEDURE Scenario1a_SP;");
            var insertSql = "INSERT INTO dept1 VALUES (50,'HR','DENVER');";
            Execute(insertSql);

            var sqlStr = "CREATE OR REPLACE PROCEDURE Scenario1a_SP()\n"
                        + " IS\n"
                        + "BEGIN\n"
                        + "    INSERT INTO dept1 VALUES (60,'FINANCE','CHICAGO');\n"
                        + "    insert_dept_70;\n" + "END;\n"
                        + "COMMIT;";

            Execute(sqlStr);

            using (var cstmt = new EDBCommand("Scenario1a_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            var list = getDeptnos();
            Assert.AreEqual(DEPT_50_60_70.Length, list.Count);
            for (var i = 0; i < list.Count; i++)
                Assert.AreEqual(DEPT_50_60_70[i], list[i]);
        }

        [Test]
        public void Scenario1bTest()
        {
            //JDBC test case has the following SQL:
            /*
            String insertSql = "INSERT INTO dept VALUES (50,'HR','DENVER');\n"
                 + "BEGIN\n"
                 + "    INSERT INTO dept VALUES (60,'FINANCE','CHICAGO');\n"
                 + "    insert_dept_70;\n"
                 + "END;\n"
                 + "ROLLBACK;";
            */

            //It looks like .NET does not support running such queries.

            //We have changed the test to EDB Transaction scenario such that
            //Transaction is started.
            //insert is run within this transaction.
            //anonymous blcok is run as Stored procedure.
            //Rollback is performed using EDBTransaction.

            //The next scenario shows that a final ROLLBACK command after all inserts
            //results in the rollback of all three insertions:
            Execute("DROP PROCEDURE Scenario1b_SP;");
            var insertSql = "INSERT INTO dept1 VALUES (50,'HR','DENVER');";

            var sqlStr = "CREATE OR REPLACE PROCEDURE Scenario1b_SP()\n"
                        + " IS\n"
                        + "BEGIN\n"
                        + "    INSERT INTO dept1 VALUES (60,'FINANCE','CHICAGO');\n"
                        + "    insert_dept_70;\n"
                        + "END;\n";

            Execute(sqlStr);

            EDBTransaction trans = conn.BeginTransaction();
            using (var cstmt = new EDBCommand(insertSql, conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.Text;
                cstmt.ExecuteNonQuery();
            }

            using (var cstmt = new EDBCommand("Scenario1b_SP", conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            trans.Rollback();
            var list = getDeptnos();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
    public void Scenario1cTest()
        {
            //The comment and code from JDBC test are as follow:

            //Comment:
            //A ROLLBACK command given at the end of the anonymous block also
            //eliminates all three prior insertions:

            //Code
            /*
            String insertSql = "INSERT INTO dept VALUES (50,'HR','DENVER');\n"
                         + "BEGIN\n"
                         + "    INSERT INTO dept VALUES (60,'FINANCE','CHICAGO');\n"
                         + "    insert_dept_70;\n"
                         + "    ROLLBACK;\n"
                         + "END;\n"
                         + "COMMIT;";
            */

            //We are going to change it to EDB .NET Transaction based approach with Stored Proc.
            //Since we are running ROLLBACK inside SP, we will exclude the INSERT statement from this test.

            //var insertSql = "INSERT INTO dept1 VALUES (50,'HR','DENVER');";
            //Execute(insertSql);

            var sqlStr = "CREATE OR REPLACE PROCEDURE Scenario1c_SP()\n"
                         + " IS\n"
                         + "BEGIN\n"
                         + "    INSERT INTO dept1 VALUES (60,'FINANCE','CHICAGO');\n"
                         + "    insert_dept_70;\n"
                         + "    ROLLBACK;\n"
                         + "END;\n"
                         + "COMMIT;";
            Execute(sqlStr);

            using (var cstmt = new EDBCommand("Scenario1c_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            var list = getDeptnos();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
    public void Scenario2aTest()
        {
            //Below is the comment and code in JDBC test.

            //Comment
            //The PRAGMA AUTONOMOUS_TRANSACTION is given with the anonymous
            //block along with the COMMIT command at the end of the anonymous block.
            //After the ROLLBACK at the end of the transaction, only the first
            //row insertion at the beginning of the transaction is discarded.
            //The other two row insertions in the anonymous block with PRAGMA
            //AUTONOMOUS_TRANSACTION were independently committed.

            //Code
            /*
             String insertSql = "INSERT INTO dept VALUES (50,'HR','DENVER');\n"
                         + "DECLARE\n"
                         + "    PRAGMA AUTONOMOUS_TRANSACTION;\n"
                         + "BEGIN\n"
                         + "    INSERT INTO dept VALUES (60,'FINANCE','CHICAGO');\n"
                         + "    insert_dept_70;\n"
                         + "    COMMIT;\n"
                         + "END;\n"
                         + "ROLLBACK;";
             */

            //We have changed the code to .NET Transaction.

            Execute("DROP PROCEDURE Scenario2a_SP;");
            var insertSql = "INSERT INTO dept1 VALUES (50,'HR','DENVER');";

            var sqlStr = "CREATE OR REPLACE PROCEDURE Scenario2a_SP()\n"
                        + " IS\n"
                        + "DECLARE\n"
                        + "    PRAGMA AUTONOMOUS_TRANSACTION;\n"
                        + "BEGIN\n"
                        + "    INSERT INTO dept1 VALUES (60,'FINANCE','CHICAGO');\n"
                        + "    insert_dept_70;\n"
                        + "    COMMIT;\n"
                        + "END;\n";

            Execute(sqlStr);

            EDBTransaction trans = conn.BeginTransaction();
            using (var cstmt = new EDBCommand(insertSql, conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.Text;
                cstmt.ExecuteNonQuery();
            }

            using (var cstmt = new EDBCommand("Scenario2a_SP", conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            trans.Rollback();
            var list = getDeptnos();
            Assert.AreEqual(DEPT_60_70.Length, list.Count);
            for (var i = 0; i < list.Count; i++)
                Assert.AreEqual(DEPT_60_70[i], list[i]);
        }

        [Test]
    public void Scenario2bTest()
        {
            //Below is the comment and code in JDBC test.

            //Comment
            //The rollback in the procedure removes the two rows inserted in the
            //anonymous block (deptno 60 and 70) before the final COMMIT command
            //in the anonymous block.
            //After the final commit at the end of the transaction, the only row
            //inserted is the first one from the beginning of the transaction.
            //Since the anonymous block is an autonomous transaction, the rollback
            //in the enclosed procedure has no effect on the insertion that occurs
            //before the anonymous block is executed.

            //Code
            /*
            String insertSql = "INSERT INTO dept VALUES (50,'HR','DENVER');\n"
                         + "DECLARE\n"
                         + "    PRAGMA AUTONOMOUS_TRANSACTION;\n"
                         + "BEGIN\n"
                         + "    INSERT INTO dept VALUES (60,'FINANCE','CHICAGO');\n"
                         + "    insert_dept_70_rollback;\n"
                         + "    COMMIT;\n"
                         + "END;\n"
                         + "COMMIT;";
            */

            //We have changed the code to .NET Transaction.

            Execute("DROP PROCEDURE Scenario2b_SP;");
            var insertSql = "INSERT INTO dept1 VALUES (50,'HR','DENVER');";

            var sqlStr = "CREATE OR REPLACE PROCEDURE Scenario2b_SP()\n"
                        + " IS\n"
                        + "DECLARE\n"
                         + "    PRAGMA AUTONOMOUS_TRANSACTION;\n"
                         + "BEGIN\n"
                         + "    INSERT INTO dept1 VALUES (60,'FINANCE','CHICAGO');\n"
                         + "    insert_dept_70_rollback;\n"
                         + "    COMMIT;\n"
                         + "END;\n";

            Execute(sqlStr);

            EDBTransaction trans = conn.BeginTransaction();
            using (var cstmt = new EDBCommand(insertSql, conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.Text;
                cstmt.ExecuteNonQuery();
            }

            using (var cstmt = new EDBCommand("Scenario2b_SP", conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            trans.Commit();
            var list = getDeptnos();
            Assert.AreEqual(DEPT_50.Length, list.Count);
            for (var i = 0; i < list.Count; i++)
                Assert.AreEqual(DEPT_50[i], list[i]);
        }

        [Test]
    public void Scenario2cTest()
        {
            //The rollback in the procedure removes the row inserted by
            //the procedure but not the other row inserted in the anonymous block.
            //After the final commit at the end of the transaction, the row inserted
            //is the first one from the beginning of the transaction as well as
            //the row inserted at the beginning of the anonymous block. The only
            //insertion rolled back is the one in the procedure.

            Execute("DROP PROCEDURE Scenario2c_SP;");
            var insertSql = "INSERT INTO dept1 VALUES (50,'HR','DENVER');";

            var sqlStr = "CREATE OR REPLACE PROCEDURE Scenario2c_SP()\n"
                        + " IS\n"
                        + "DECLARE\n"
                         + "    PRAGMA AUTONOMOUS_TRANSACTION;\n"
                         + "BEGIN\n"
                         + "    INSERT INTO dept1 VALUES (60,'FINANCE','CHICAGO');\n"
                         + "    insert_dept_70_auto;\n"
                         + "    COMMIT;\n"
                         + "END;\n";

            Execute(sqlStr);

            EDBTransaction trans = conn.BeginTransaction();
            using (var cstmt = new EDBCommand(insertSql, conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.Text;
                cstmt.ExecuteNonQuery();
            }

            using (var cstmt = new EDBCommand("Scenario2c_SP", conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            trans.Commit();
            var list = getDeptnos();
            Assert.AreEqual(DEPT_50_60.Length, list.Count);
            for (var i = 0; i < list.Count; i++)
                Assert.AreEqual(DEPT_50_60[i], list[i]);
    }

        [Test]
    public void AutonomousTransactionTriggerTest()
        {
            //The following two inserts are made into the emp table
            //in a transaction.
            //But then the ROLLBACK command is given during this session.
            //The emp table no longer contains the two rows, but the empauditlog
            //table still contains its two entries. The trigger implicitly
            //performed a commit, and PRAGMA AUTONOMOUS_TRANSACTION commits
            //those changes independent from the rollback given in the calling transaction.

            var insertSql = "INSERT INTO emp1 VALUES \n"
                         + "(9001,'SMITH','ANALYST',7782,SYSDATE,NULL,NULL,10);\n"
                         + "INSERT INTO emp1 VALUES \n"
                         + "(9002,'JONES','CLERK',7782,SYSDATE,NULL,NULL,10);";

            EDBTransaction trans = conn.BeginTransaction();
            using (var cstmt = new EDBCommand(insertSql, conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.Text;
                cstmt.ExecuteNonQuery();
            }

            trans.Rollback();
        var empCount = getEmpCount();
        var auditCount = getEmpAuditLogCount();
        Assert.AreEqual(0, empCount);
        Assert.AreEqual(2, auditCount);
    }

        [Test]
    public void AutonomousTransactionObjectTypeMethodTest()
        {
            //In the following anonymous block, an insert is performed into the dept table,
            //followed by invoking the insert_dept method of the object and ending with a
            //ROLLBACK command in the anonymous block.
            //Since insert_dept was declared as an autonomous transaction, its insert of
            //department number 60 remains in the table, but the rollback removes the
            //insertion of department 50:

            Execute("DROP PROCEDURE AutonomousTransactionObjectTypeMethod_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE AutonomousTransactionObjectTypeMethod_SP()\n"
                        + " IS\n"
                        + "DECLARE\n"
                         + "    v_dept          INSERT_DEPT_TYP :=\n"
                         + "                      insert_dept_typ(60,'FINANCE','CHICAGO');\n"
                         + "BEGIN\n"
                         + "    INSERT INTO dept1 VALUES (50,'HR','DENVER');\n"
                         + "    v_dept.insert_dept;\n"
                         + "    ROLLBACK;\n"
                         + "END;";

            Execute(sqlStr);

            EDBTransaction trans = conn.BeginTransaction();

            using (var cstmt = new EDBCommand("AutonomousTransactionObjectTypeMethod_SP", conn))
            {
                cstmt.Transaction = trans;
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            trans.Commit();
            var list = getDeptnos();
            Assert.AreEqual(DEPT_60.Length, list.Count);
            for (var i = 0; i < list.Count; i++)
                Assert.AreEqual(DEPT_60[i], list[i]);
    }
}
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

