using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2577: Regression Tests for Trigger in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBTriggerTest : TestBase
    {
        EDBConnection? conn = null;

        private static string[] BEFORE_STATEMENT_LEVEL_TRIGGER_RESULT = {
            "New employees are about to be added"
            };

        //We don't have user at this time. USER_ADD is placeholder for user to be added
        //later in Setup method. USER_ADD will be replaced by connection user like enterprisedb.
        private static string[] AFTER_STATEMENT_LEVEL_TRIGGER_RESULT = {
            "USER_ADD Added employee(s)",
            "USER_ADD Added employee(s)",
            "USER_ADD Updated employee(s)",
            "USER_ADD Deleted employee(s)"
            };
        private static string today = DateTime.Now.ToString("dd-MM-yyyy");
        private static string[] JOBHIST_RESULT = {
            "9003 " + today + " " + today + " ANALYST 5000.00 null 40 New Hire",
            "9004 " + today + " " + today + " ANALYST 4500.00 null 40 New Hire",
            "9003 " + today + " null ANALYST 5000.00 5500.00 40 Changed commission",
            "9004 " + today + " null ANALYST 4500.00 4950.00 40 Changed commission"
            };
        private static string[] EMP_LOG_RESULT = {
            today + " Added employee # 9003",
            today + " Added employee # 9004",
            today + " Updated employee # 9003",
            today + " Updated employee # 9004",
            today + " Deleted employee # 9003",
            today + " Deleted employee # 9004"
            };
        private static string[] EMP_RESULT = {
            "1234 ASHTON 50",
            "2001 JACK 40"
            };
        private static string[] DEPT_RESULT = {
            "50 IT NEW JERSEY",
            "40 RESEARCH BOSTON"
            };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP Trigger emp_alert_trig;");
            Execute("DROP Trigger emp_audit_trig;");
            Execute("DROP Trigger emp_comm_trig;");
            Execute("DROP Trigger emp_chg_trig;");
            Execute("DROP Trigger empvw_instead_of_trig;");

            Execute("DROP View emp_vw;");

            Execute("DROP TABLE emp1 CASCADE");
            Execute("DROP TABLE dept1 CASCADE");
            Execute("DROP TABLE empauditlog CASCADE");
            Execute("DROP TABLE empchglog CASCADE");
            Execute("DROP TABLE jobhist1 CASCADE");

            //This is required for appending connection user name to trigger results.
            for (var i = 0; i < AFTER_STATEMENT_LEVEL_TRIGGER_RESULT.Length; i++)
                AFTER_STATEMENT_LEVEL_TRIGGER_RESULT[i] = AFTER_STATEMENT_LEVEL_TRIGGER_RESULT[i].Replace("USER_ADD", conn.UserName);

            Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(20), job VARCHAR2(20), hiredate DATE, "
                    + "sal NUMBER(10,2), comm NUMBER(10,2), deptno NUMBER(4))");
            Execute("CREATE TABLE dept1(deptno NUMBER(4),  dname VARCHAR2(20), loc VARCHAR2(20))");
            Execute("CREATE TABLE empauditlog(audit_date  DATE, audit_user VARCHAR2(20), audit_desc VARCHAR2(20))");
            Execute("CREATE TABLE empchglog(chg_date DATE, chg_desc VARCHAR2(30))");
            Execute("CREATE TABLE jobhist1(EMPNO NUMBER(4), STARTDATE DATE, ENDDATE DATE, JOB VARCHAR2(20),"
                    + "SAL  NUMBER(10,2), COMM  NUMBER(10,2), DEPTNO  NUMBER(4), CHGDESC VARCHAR2(30))");

            // The CREATE VIEW statement creates the emp_vw view by joining the two tables.
            var createViewCommand = "CREATE VIEW emp_vw AS SELECT * FROM emp1 e JOIN dept1 d USING(deptno);\n";
            Execute(createViewCommand);

            //This example shows a simple before statement-level trigger that displays
            //a message before an insert operation on the emp1 table:
            var beforeStmtTrig = "CREATE OR REPLACE TRIGGER emp_alert_trig\n"
                                   + "    BEFORE INSERT ON emp1\n"
                                   + "BEGIN\n"
                                   + "    DBMS_OUTPUT.PUT_LINE('New employees are about to be added');\n"
                                   + "END;";
            Execute(beforeStmtTrig);

            //This example shows an after statement-level trigger. When an insert,
            //update, or delete operation occurs on the emp1 table, a row is added to
            //the empauditlog table recording the date, user, and action.
            var afterStmtTrig = "CREATE OR REPLACE TRIGGER emp_audit_trig\n"
                                + "    AFTER INSERT OR UPDATE OR DELETE ON emp1\n"
                                + "DECLARE\n"
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
            Execute(afterStmtTrig);

            //This example shows a before row-level trigger that calculates
            //the commission of every new employee belonging to department 30
            //that's inserted into the emp1 table:
            var beforeRowLevelTrig = "CREATE OR REPLACE TRIGGER emp_comm_trig\n"
                               + "    BEFORE INSERT ON emp1\n"
                               + "    FOR EACH ROW\n"
                               + "BEGIN\n"
                               + "    IF :NEW.deptno = 30 THEN\n"
                               + "        :NEW.comm := :NEW.sal * .4;\n"
                               + "    END IF;\n"
                               + "END;";
            Execute(beforeRowLevelTrig);

            //This example shows an after row-level trigger. When a new employee row
            //is inserted, the trigger adds a row to the jobhist1 table for that employee.
            //When an existing employee is updated, the trigger sets the enddate column of
            //the latest jobhist1 row (assumed to be the one with a null enddate) to the
            //current date and inserts a new jobhist1 row with the employee’s new information.
            //Then, the trigger adds a row to the empchglog table with a description of the action.
            var afterRowLevelTrig = "CREATE OR REPLACE TRIGGER emp_chg_trig\n"
                              + "    AFTER INSERT OR UPDATE OR DELETE ON emp1\n"
                              + "    FOR EACH ROW\n"
                              + "DECLARE\n"
                              + "    v_empno         emp1.empno%TYPE;\n"
                              + "    v_deptno        emp1.deptno%TYPE;\n"
                              + "    v_dname         dept1.dname%TYPE;\n"
                              + "    v_action        VARCHAR2(7);\n"
                              + "    v_chgdesc       jobhist1.chgdesc%TYPE;\n"
                              + "BEGIN\n"
                              + "    IF INSERTING THEN\n"
                              + "        v_action := 'Added';\n"
                              + "        v_empno := :NEW.empno;\n"
                              + "        v_deptno := :NEW.deptno;\n"
                              + "        INSERT INTO jobhist1 VALUES (:NEW.empno, SYSDATE, NULL,\n"
                              + "            :NEW.job, :NEW.sal, :NEW.comm, :NEW.deptno, 'New Hire');\n"
                              + "    ELSIF UPDATING THEN\n"
                              + "        v_action := 'Updated';\n"
                              + "        v_empno := :NEW.empno;\n"
                              + "        v_deptno := :NEW.deptno;\n"
                              + "        v_chgdesc := '';\n"
                              + "        IF NVL(:OLD.ename, '-null-') != NVL(:NEW.ename, '-null-') THEN\n"
                              + "            v_chgdesc := v_chgdesc || 'name, ';\n"
                              + "        END IF;\n"
                              + "        IF NVL(:OLD.job, '-null-') != NVL(:NEW.job, '-null-') THEN\n"
                              + "            v_chgdesc := v_chgdesc || 'job, ';\n"
                              + "        END IF;\n"
                              + "        IF NVL(:OLD.sal, -1) != NVL(:NEW.sal, -1) THEN\n"
                              + "            v_chgdesc := v_chgdesc || 'salary, ';\n"
                              + "        END IF;\n"
                              + "        IF NVL(:OLD.comm, -1) != NVL(:NEW.comm, -1) THEN\n"
                              + "            v_chgdesc := v_chgdesc || 'commission, ';\n"
                              + "        END IF;\n"
                              + "        IF NVL(:OLD.deptno, -1) != NVL(:NEW.deptno, -1) THEN\n"
                              + "            v_chgdesc := v_chgdesc || 'department, ';\n"
                              + "        END IF;\n"
                              + "        v_chgdesc := 'Changed ' || RTRIM(v_chgdesc, ', ');\n"
                              + "        UPDATE jobhist1 SET enddate = SYSDATE WHERE empno = :OLD.empno\n"
                              + "            AND enddate IS NULL;\n"
                              + "        INSERT INTO jobhist1 VALUES (:NEW.empno, SYSDATE, NULL,\n"
                              + "            :NEW.job, :NEW.sal, :NEW.comm, :NEW.deptno, v_chgdesc);\n"
                              + "    ELSIF DELETING THEN\n"
                              + "        v_action := 'Deleted';\n"
                              + "        v_empno := :OLD.empno;\n"
                              + "        v_deptno := :OLD.deptno;\n"
                              + "    END IF;\n"
                              + "    INSERT INTO empchglog VALUES (SYSDATE,\n"
                              + "        v_action || ' employee # ' || v_empno);\n"
                              + "END;";
            Execute(afterRowLevelTrig);

            //This example shows an INSTEAD OF trigger for inserting a new employee row into the emp_vw view.
            //The CREATE VIEW statement creates the emp_vw view by joining the two tables. The trigger adds
            //the corresponding new rows into the emp1 and dept1 tables, respectively, for a specific employee.
            var insteadOfTrig = "CREATE OR REPLACE TRIGGER empvw_instead_of_trig\n"
                             + "    INSTEAD OF INSERT ON emp_vw\n"
                             + "    FOR EACH ROW\n"
                             + "DECLARE\n"
                             + "    v_empno         emp1.empno%TYPE;\n"
                             + "    v_ename         emp1.ename%TYPE;\n"
                             + "    v_deptno        emp1.deptno%TYPE;\n"
                             + "    v_dname         dept1.dname%TYPE;\n"
                             + "    v_loc           dept1.loc%TYPE;\n"
                             + "    v_action        VARCHAR2(7);\n"
                             + "BEGIN\n"
                             + "    v_empno     := :NEW.empno;\n"
                             + "    v_ename     := :New.ename;\n"
                             + "    v_deptno    := :NEW.deptno;\n"
                             + "    v_dname     := :NEW.dname;\n"
                             + "    v_loc       := :NEW.loc;\n"
                             + "     INSERT INTO emp1(empno, ename, deptno) VALUES(v_empno, v_ename, v_deptno);\n"
                             + "     INSERT INTO dept1(deptno, dname, loc) VALUES(v_deptno, v_dname, v_loc);\n"
                             + "END;";
            Execute(insteadOfTrig);

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

        //Got data from resultset and create a list of String
        private static List<string> getResultSetData(EDBDataReader rs)
        {
            var list = new List<string>();
            while (rs.Read())
            {
                var columns = rs.GetColumnSchema().Count;
                var str = new StringBuilder();
                for (var i = 0; i < columns; i++)
                {
                    var obj = rs.GetValue(i);
                    if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                    {
                        str.Append(obj.ToString());
                    }
                    else
                    {
                        str.Append("null");
                    }
                    if (i != columns - 1)
                    {
                        str.Append(" ");
                    }
                }
                list.Add(str.ToString());
            }
            return list;
        }

        //Compare if two list are equal
        private static void compareList(List<string> lista, List<string> listb)
        {
            Assert.AreEqual(lista.Count, listb.Count);

            foreach (var a in lista)
            {
                Assert.IsTrue(listb.Contains(a));
            }

            foreach (var b in listb)
            {
                Assert.IsTrue(lista.Contains(b));
            }
        }

        //Get total employee count
        private int getEmpCount()
        {
            var command = "select count(*) from emp1";
            var selectCommand = new EDBCommand(command, conn);
            var selectResult = selectCommand.ExecuteReader();
            selectResult.Read();
            var count = selectResult.GetInt32(0);
            selectResult.Close();

            return count;
        }

        //Get commission for an employee
        private double getEmpCommission(int empno)
        {
            var command = "select comm from emp1 where empno=" + empno;

            var selectCommand = new EDBCommand(command, conn);
            var selectResult = selectCommand.ExecuteReader();
            selectResult.Read();
            var sal = selectResult.GetDouble(0);
            selectResult.Close();

            return sal;
        }

        [Test]
        public void BeforeStatementLevelTriggerTest()
        {
            var addEmp = new string[]{
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

            Assert.AreEqual(14, getEmpCount());

            //The following INSERT is constructed so that several new rows are inserted upon
            //a single execution of the command.
            //The message New employees are about to be added is displayed once by the firing
            //of the trigger even though the result adds three rows.
            var sqlStr = "INSERT INTO emp1 (empno, ename, deptno) SELECT empno + 1000, ename, 40\n"
                    + "    FROM emp1 WHERE empno BETWEEN 7900 AND 7999;";

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
                    //cstmt.CommandType = CommandType.StoredProcedure;

                    //cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(BEFORE_STATEMENT_LEVEL_TRIGGER_RESULT.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(BEFORE_STATEMENT_LEVEL_TRIGGER_RESULT[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();

            //Assert.assertArrayEquals(BEFORE_STATEMENT_LEVEL_TRIGGER_RESULT, v.toArray());
            Assert.AreEqual(17, getEmpCount());
        }

        [Test]
        public void AfterStatementLevelTriggerTest()
        {
            //In the following sequence of commands, two rows are inserted into the emp1 table using
            //two INSERT commands. One UPDATE command updates the sal and comm columns of both rows.
            //Then, one DELETE command deletes both rows.
            //The contents of the empauditlog table show how many times the trigger was fired:
            //Once each for the two inserts
            //Once for the update (even though two rows were changed)
            //Once for the deletion (even though two rows were deleted)
            Execute("INSERT INTO emp1 VALUES (9001,'SMITH','ANALYST',SYSDATE,NULL,NULL,10);");
            Execute("INSERT INTO emp1 VALUES (9002,'JONES','CLERK',SYSDATE,NULL,NULL,10);");
            Execute("UPDATE emp1 SET sal = 4000.00, comm = 1200.00 WHERE empno IN (9001, 9002);");
            Execute("DELETE FROM emp1 WHERE empno IN (9001, 9002);");

            var list = new System.Collections.Generic.List<string>();
            var command = "SELECT  audit_user, audit_desc FROM empauditlog ORDER BY 1 ASC;";

            var selectCommand = new EDBCommand(command, conn);
            var selectResult = selectCommand.ExecuteReader();
            while (selectResult.Read())
            {
                var user = selectResult.GetString(0);
                var auditDesc = selectResult.GetString(1);
                list.Add(user + " " + auditDesc);
            }
            selectResult.Close();

            Assert.AreEqual(AFTER_STATEMENT_LEVEL_TRIGGER_RESULT.Length, list.Count);
            for (var i = 0; i < list.Count; i++)
                Assert.AreEqual(AFTER_STATEMENT_LEVEL_TRIGGER_RESULT[i], list[i]);
        }

        [Test]
        public void BeforeRowLevelTriggerTest()
        {
            //The listing following the addition of the two employees shows that the trigger computed
            //their commissions and inserted it as part of the new employee rows:
            Execute("INSERT INTO emp1 VALUES (9005,'ROBERS','SALESMAN',SYSDATE,3000.00,NULL,30);");
            Execute("INSERT INTO emp1 VALUES (9006,'ALLEN','SALESMAN',SYSDATE,4500.00,NULL,30);");
            Assert.AreEqual(1200.00, getEmpCommission(9005), 0.01);
            Assert.AreEqual(1800.00, getEmpCommission(9006), 0.01);
        }

        [Test]
        public void AfterRowLevelTriggerTest()
        {
            //In the first sequence of the following commands, two employees are added using
            //two separate INSERT commands. Then both are updated using a single UPDATE command.
            //The contents of the jobhist1 table show the action of the trigger for each affected
            //row: two new-hire entries for the two new employees and two changed commission
            //records for the updated commissions on the two employees. The empchglog table also
            //shows the trigger was fired a total of four times, once for each action on the two rows.
            Execute("INSERT INTO emp1 VALUES (9003,'PETERS','ANALYST',SYSDATE,5000.00,NULL,40);");
            Execute("INSERT INTO emp1 VALUES (9004,'AIKENS','ANALYST',SYSDATE,4500.00,NULL,40);");
            Execute("UPDATE emp1 SET comm = sal * 1.1 WHERE empno IN (9003, 9004);");
            Execute("DELETE FROM emp1 WHERE empno IN (9003, 9004);");
            var histCommand = "SELECT empno, to_char(STARTDATE, 'DD-MM-YYYY') AS \"startdate\","
                    + " to_char(ENDDATE, 'DD-MM-YYYY') AS \"enddate\", JOB, sal, comm,DEPTNO, CHGDESC FROM jobhist1 "
                    + " WHERE empno IN (9003, 9004);";

            var selectCommand1 = new EDBCommand(histCommand, conn);
            var histRs = selectCommand1.ExecuteReader();

            var histList = getResultSetData(histRs);
            var jobHist = JOBHIST_RESULT.ToList<string>();
            compareList(histList, jobHist);
            histRs.Close();

            var logCommand = "select to_char(CHG_DATE, 'DD-MM-YYYY') AS \"CHG_DATE\", CHG_DESC from empchglog;";

            var selectCommand2 = new EDBCommand(logCommand, conn);
            var logRs = selectCommand2.ExecuteReader();

            var logList = getResultSetData(logRs);
            var empLogs = EMP_LOG_RESULT.ToList<string>();
            compareList(empLogs, logList);
            logRs.Close();
        }

        [Test]
        public void InsteadOfTriggerTest()
        {
            //insert into emp_vw will insert rows in emp and dept table
            Execute("INSERT INTO emp_vw (empno, ename, deptno, dname, loc ) VALUES(1234, 'ASHTON', 50, 'IT', 'NEW JERSEY');");
            Execute("INSERT INTO emp_vw (empno, ename, deptno, dname, loc ) VALUES(2001, 'JACK', 40, 'RESEARCH', 'BOSTON');");

            var empCommand = "SELECT EMPNO, ENAME, DEPTNO from emp1;";

            var selectCommand1 = new EDBCommand(empCommand, conn);
            var empRs = selectCommand1.ExecuteReader();

            var empList = getResultSetData(empRs);
            var emps = EMP_RESULT.ToList<string>();
            compareList(empList, emps);
            empRs.Close();

            var deptCommand = "SELECT * FROM dept1;";

            var selectCommand2 = new EDBCommand(deptCommand, conn);
            var deptRs = selectCommand2.ExecuteReader();

            var deptList = getResultSetData(deptRs);
            var depts = DEPT_RESULT.ToList<string>();
            compareList(deptList, depts);
            deptRs.Close();
        }
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
