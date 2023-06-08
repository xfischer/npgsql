using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2584: Regression Tests for Basic Statement in SPL

//Port JDBC tests to .NET from enhancements\spl\BasicStatementTest.java
namespace EnterpriseDB.EDBClient.Tests.SPL
{
    [TestFixture]
    public class EDBBasicStatementTest : TestBase
    {
        EDBConnection? conn = null;

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP PROCEDURE dept_salary_rpt");
            Execute("DROP PROCEDURE emp_delete");
            Execute("DROP PROCEDURE emp_insert");
            Execute("DROP PROCEDURE divide_it");
            Execute("DROP PROCEDURE return_into");
            Execute("DROP PROCEDURE return_into_from_delete");
            Execute("DROP PROCEDURE select_into_query");
            Execute("DROP PROCEDURE select_into_exception_query");
            Execute("DROP PROCEDURE emp_comp_update");
            Execute("DROP PROCEDURE status_query");

            Execute("DROP TABLE emp1 CASCADE");

            Execute("CREATE TABLE emp1(empno NUMBER(8),  ename VARCHAR2(10),job VARCHAR2(9), "
                    + "mgr NUMBER(8), hiredate DATE, sal NUMBER(10,2), comm NUMBER(10,2), deptno NUMBER(4))");

            var add1001 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                    + " VALUES(1001,'SMITH','Sales',200,to_date('01-11-07','DD-MM-YY'),120000,0,11)";
            Execute(add1001);

            var add3001 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                    + " VALUES(3001,'WARD','Sales',200,to_date('01-11-07','DD-MM-YY'),100000,0,31)";
            Execute(add3001);

            var add4001 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                    + " VALUES(4001,'JONES','Sales',200,to_date('01-11-07','DD-MM-YY'),120000,0,41)";
            Execute(add4001);

            var add4002 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                    + " VALUES(4002,'MARTIN','Sales',200,to_date('01-11-07','DD-MM-YY'),100000,0,41)";
            Execute(add4002);

            var add5001 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                    + " VALUES(5001,'BLAKE','Sales',200,to_date('01-11-07','DD-MM-YY'),120000,0,51)";
            Execute(add5001);

            //The assignment statement sets a variable or a formal parameter of
            //mode OUT or IN OUT specified on the left side of the assignment
            //:= to the evaluated expression specified on the right side of the assignment.
            var deptSaleyProcedure = "CREATE OR REPLACE PROCEDURE dept_salary_rpt (\n"
                                      + "  p_deptno         IN  NUMBER, \n"
                                      + "  todays_date      OUT DATE, \n"
                                      + "  rpt_title        OUT VARCHAR2(60),\n"
                                      + "  base_sal         OUT INTEGER, \n"
                                      + "  base_comm_rate   OUT NUMBER, "
                                      + "  base_annual      OUT NUMBER \n"
                                      + ")\n"
                                      + "IS\n"
                                      + "BEGIN\n"
                                      + "    todays_date := SYSDATE;\n"
                                      + "    rpt_title := 'Report For Department # ' || p_deptno || ' on '\n"
                                      + " || todays_date;\n"
                                      + "    base_sal := 35525;\n"
                                      + "    base_comm_rate := 1.33;\n"
                                      + "    base_annual := ROUND(base_sal * base_comm_rate, 2);\n"
                                      + "END;";
            Execute(deptSaleyProcedure);

            //You can use an expression in the SPL language wherever an expression
            //is allowed in the SQL DELETE command. Thus, you can use SPL variables
            //and parameters to supply values to the delete operation.
            var empDeleteProcedure = "CREATE OR REPLACE PROCEDURE emp_delete (\n"
                                      + "   p_empno  IN  emp1.empno%TYPE, \n"
                                      + "   msg      OUT VARCHAR2(60)"
                                      + ")\n"
                                      + "IS\n"
                                      + "BEGIN\n"
                                      + "    DELETE FROM emp1 WHERE empno = p_empno;\n"
                                      + "\n"
                                      + "    IF SQL%FOUND THEN\n"
                                      + "        msg := 'Deleted Employee # : ' || p_empno;\n"
                                      + "    ELSE\n"
                                      + "        msg='Employee # ' || p_empno || ' not found';\n"
                                      + "    END IF;\n"
                                      + "END;";
            Execute(empDeleteProcedure);

            //You can use an expression in the SPL language wherever an expression is allowed
            //the SQL INSERT command. Thus, you can use SPL variables and parameters
            //to supply values to the insert operation.
            var empInsertProcedure = "CREATE OR REPLACE PROCEDURE emp_insert (\n"
                                      + "    p_empno         IN emp1.empno%TYPE,\n"
                                      + "    p_ename         IN emp1.ename%TYPE,\n"
                                      + "    p_job           IN emp1.job%TYPE,\n"
                                      + "    p_mgr           IN emp1.mgr%TYPE,\n"
                                      + "    p_hiredate      IN emp1.hiredate%TYPE,\n"
                                      + "    p_sal           IN emp1.sal%TYPE,\n"
                                      + "    p_comm          IN emp1.comm%TYPE,\n"
                                      + "    p_deptno        IN emp1.deptno%TYPE\n"
                                      + ")\n"
                                      + "IS\n"
                                      + "BEGIN\n"
                                      + "    INSERT INTO emp1 VALUES (\n"
                                      + "        p_empno,\n"
                                      + "        p_ename,\n"
                                      + "        p_job,\n"
                                      + "        p_mgr,\n"
                                      + "        p_hiredate,\n"
                                      + "        p_sal,\n"
                                      + "        p_comm,\n"
                                      + "        p_deptno);\n"
                                      + "\n"
                                      + "EXCEPTION\n"
                                      + "    WHEN OTHERS THEN\n"
                                      + "        DBMS_OUTPUT.PUT_LINE('OTHERS exception on INSERT of employee # '\n"
                                      + "            || p_empno);\n"
                                      + "        DBMS_OUTPUT.PUT_LINE('SQLCODE : ' || SQLCODE);\n"
                                      + "        DBMS_OUTPUT.PUT_LINE('SQLERRM : ' || SQLERRM);\n"
                                      + "END;";
            Execute(empInsertProcedure);

            //The simplest statement is the NULL statement.
            //This statement is an executable statement that does nothing.
            var nullStatementProcedure = "CREATE OR REPLACE PROCEDURE divide_it (\n"
                                           + "    p_numerator     IN  NUMBER,\n"
                                           + "    p_denominator   IN  NUMBER,\n"
                                           + "    p_result        OUT NUMBER\n"
                                           + ")\n"
                                           + "IS\n"
                                           + "BEGIN\n"
                                           + "    IF p_denominator = 0 THEN\n"
                                           + "        NULL;\n"
                                           + "    ELSE\n"
                                           + "        p_result := p_numerator / p_denominator;\n"
                                           + "    END IF;\n"
                                            + "END;";
            Execute(nullStatementProcedure);

            //You can append the INSERT, UPDATE, and DELETE commands with the optional RETURNING INTO
            //clause. This clause allows the SPL program to capture the newly added, modified,
            //or deleted values from the results of an INSERT, UPDATE, or DELETE command, respectively.
            var returnIntoProcedure = "CREATE OR REPLACE PROCEDURE return_into (\n"
                                       + "    p_empno         IN  emp1.empno%TYPE,\n"
                                       + "    p_sal           IN  emp1.sal%TYPE,\n"
                                       + "    p_comm          IN  emp1.comm%TYPE,\n"
                                         + "    v_empno         OUT emp1.empno%TYPE,\n"
                                       + "    v_ename         OUT emp1.ename%TYPE,\n"
                                       + "    v_job           OUT emp1.job%TYPE,\n"
                                       + "    v_sal           OUT emp1.sal%TYPE,\n"
                                        + "    v_comm          OUT emp1.comm%TYPE,\n"
                                       + "    v_deptno        OUT emp1.deptno%TYPE,\n"
                                       + "    v_msg           OUT VARCHAR2(60) "
                                       + ")\n"
                                       + "IS\n"
                                       + "BEGIN\n"
                                       + "    UPDATE emp1 SET sal = p_sal, comm = p_comm WHERE empno = p_empno\n"
                                       + "    RETURNING\n"
                                       + "        empno,\n"
                                       + "        ename,\n"
                                       + "        job,\n"
                                       + "        sal,\n"
                                       + "        comm,\n"
                                       + "        deptno\n"
                                       + "    INTO\n"
                                       + "        v_empno,\n"
                                       + "        v_ename,\n"
                                       + "        v_job,\n"
                                       + "        v_sal,\n"
                                       + "        v_comm,\n"
                                       + "        v_deptno;\n"
                                       + "\n"
                                       + "    IF SQL%FOUND THEN\n"
                                       + "        v_msg := 'Updated Employee # : ' || v_empno;\n"
                                       + "    ELSE\n"
                                       + "        v_msg := 'Employee # ' || p_empno || ' not found';\n"
                                       + "    END IF;\n"
                                       + "END;";
            Execute(returnIntoProcedure);

            //The following example modifies the emp_delete procedure, adding the RETURNING INTO clause
            //using record types:
            var returnIntoFromDeleteProcedure = "CREATE OR REPLACE PROCEDURE return_into_from_delete (\n"
                                                 + "    p_empno         IN    emp1.empno%TYPE, \n"
                                                 + "    r_emp           OUT   emp1%ROWTYPE, "
                                                 + "    msg             OUT   VARCHAR2(60) "
                                                 + ")\n"
                                                 + "IS\n"
                                                 + "BEGIN\n"
                                                 + "    DELETE FROM emp1 WHERE empno = p_empno\n"
                                                 + "    RETURNING\n"
                                                 + "        *\n"
                                                    + "    INTO\n"
                                                 + "        r_emp;\n"
                                                 + "\n"
                                                 + "    IF SQL%FOUND THEN\n"
                                                 + "        msg := 'Deleted Employee # : ' || r_emp.empno;\n"
                                                 + "    ELSE\n"
                                                 + "        msg := 'Employee # ' || p_empno || ' not found';\n"
                                                 + "    END IF;\n"
                                                 + "END;";
            Execute(returnIntoFromDeleteProcedure);

            //The SELECT INTO statement is an SPL variation of the SQL SELECT command. The differences are:
            //    SELECT INTO assigns the results to variables or records where they can then be used
            //      in SPL program statements.
            //    The accessible result set of SELECT INTO is at most one row.
            var selectIntoQueryProcedure = "CREATE OR REPLACE PROCEDURE select_into_query (\n"
                                          + "    p_empno         IN  emp1.empno%TYPE,\n"
                                        + "    r_emp           OUT emp1%ROWTYPE, \n"
                                        + "    v_avgsal        OUT emp1.sal%TYPE, \n"
                                        + "    v_msg           OUT VARCHAR2(60)"
                                        + ")\n"
                                        + "IS\n"
                                        + "BEGIN\n"
                                        + "    SELECT * INTO r_emp\n"
                                        + "        FROM emp1 WHERE empno = p_empno;\n"
                                        + "\n"
                                        + "    SELECT AVG(sal) INTO v_avgsal\n"
                                        + "        FROM emp1 WHERE deptno = r_emp.deptno;\n"
                                        + "EXCEPTION\n"
                                        + "    WHEN NO_DATA_FOUND THEN\n"
                                        + "        v_msg := 'Employee # ' || p_empno || ' not found';\n"
                                        + "END;";
            Execute(selectIntoQueryProcedure);

            //Another conditional clause useful in the EXCEPTION section with SELECT INTO is the TOO_MANY_ROWS
            //exception. If more than one row is selected by the SELECT INTO statement, SPL throws an exception.
            var selectIntoExceptionProcedure = "CREATE OR REPLACE PROCEDURE select_into_exception_query (\n"
                                                + "    p_deptno        IN   emp1.deptno%TYPE,\n"
                                                + "    v_ename         OUT  emp1.ename%TYPE, \n"
                                                + "    v_msg           OUT  VARCHAR2(60)"
                                                + ")\n"
                                                + "IS\n"
                                                + "BEGIN\n"
                                                + "    SELECT ename INTO v_ename FROM emp1 WHERE deptno = p_deptno ORDER BY ename;\n"
                                                + "EXCEPTION\n"
                                                + "    WHEN TOO_MANY_ROWS THEN\n"
                                                + "        v_msg := 'More than one employee found';\n"
                                                + "END;";
            Execute(selectIntoExceptionProcedure);

            //You can use an expression in the SPL language wherever an expression is allowed in
            //the SQL UPDATE command. Thus, you can use SPL variables and parameters to supply
            //values to the update operation.
            var empCompUpdateProcedure = "CREATE OR REPLACE PROCEDURE emp_comp_update (\n"
                                          + "    p_empno         IN emp1.empno%TYPE,\n"
                                          + "    p_sal           IN emp1.sal%TYPE,\n"
                                          + "    p_comm          IN emp1.comm%TYPE, \n"
                                          + "    msg             OUT VARCHAR2(60) "
                                          + ")\n"
                                          + "IS\n"
                                          + "BEGIN\n"
                                          + "    UPDATE emp1 SET sal = p_sal, comm = p_comm WHERE empno = p_empno;\n"
                                          + "\n"
                                          + "    IF SQL%FOUND THEN\n"
                                          + "        msg := 'Updated Employee # : ' || p_empno;\n"
                                          + "    ELSE\n"
                                          + "        msg := 'Employee # ' || p_empno || ' not found';\n"
                                          + "    END IF;\n"
                                          + "END;";
            Execute(empCompUpdateProcedure);

            //You can use several attributes to determine the effect of a command.
            //SQL%FOUND is a Boolean that returns TRUE if at least one row was affected by an INSERT,
            //     UPDATE or DELETE command or a SELECT INTO command retrieved one or more rows.
            //SQL%ROWCOUNT provides the number of rows affected by an INSERT, UPDATE, DELETE, or SELECT INTO command.
            //SQL%NOTFOUND is the opposite of SQL%FOUND. SQL%NOTFOUND returns TRUE if no rows were affected by an
            //     INSERT, UPDATE or DELETE command or a SELECT INTO command retrieved no rows.
            var statusQueryprocedure = "CREATE OR REPLACE PROCEDURE status_query(\n"
                                            + "    p_deptno        IN emp1.deptno%TYPE,\n"
                                            + "    v_count         OUT NUMBER(8),\n"
                                            + "    msg_found       OUT VARCHAR2(60), "
                                         + "    msg_not_found   OUT VARCHAR2(60)"
                                         + ")\n"
                                         + "IS\n"
                                         + "BEGIN\n"
                                         + "    UPDATE emp1 SET  mgr = 300 WHERE deptno = p_deptno;\n"
                                         + "    v_count :=  SQL%ROWCOUNT; \n"
                                         + "     IF SQL%FOUND THEN\n"
                                         + "        msg_found := '# rows updated: ' || SQL%ROWCOUNT; \n"
                                         + "    END IF;"
                                         + "    IF SQL%NOTFOUND THEN\n"
                                         + "        msg_not_found :='No rows were updated';\n"
                                         + "    END IF;"
                                         + "END;";
            Execute(statusQueryprocedure);
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

        private bool checkEmployeeExists(int empno)
        {
            var command = "select count(*) from emp1 where empno=" + empno;
            var selectCommand = new EDBCommand(command, conn);
            var selectResult = selectCommand.ExecuteReader();
            selectResult.Read();
            var count = selectResult.GetInt32(0);
            selectResult.Close();
            return (count == 1);
        }

        [Test]
        public void AssignmentStatementTest()
        {
            //Call a procedure has assignment statement
            var commandText = "dept_salary_rpt(:param1,:param2,:param3,:param4,:param5,:param6)";
            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1001));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Date, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            cstmt.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Numeric, 10, "param5",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            cstmt.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Numeric, 10, "param6",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var date = DateTime.Parse(cstmt.Parameters[1].Value.ToString());
            var title = cstmt.Parameters[2].Value.ToString();
            var baseSal = int.Parse(cstmt.Parameters[3].Value.ToString());
            var comRate = double.Parse(cstmt.Parameters[4].Value.ToString());
            var baseAnnual = double.Parse(cstmt.Parameters[5].Value.ToString());

            var today = DateTime.Now;
            Assert.AreEqual(today.ToShortDateString(), date.ToShortDateString());

            Assert.IsTrue(title.StartsWith("Report For Department # 1001"));

            Assert.AreEqual(baseSal, 35525);

            Assert.AreEqual(comRate, 1.33, 0.01);

            Assert.AreEqual(baseAnnual, 47248.25, 0.01);
        }

        [Test]
        public void DeleteStatementExistsTest()
        {
            //Use delete statement to delete a employee from database
            Assert.IsTrue(checkEmployeeExists(1001));
            var deleteExist = "emp_delete(:param1,:param2)";

            var cstmt = new EDBCommand(deleteExist, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1001));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            Assert.IsFalse(checkEmployeeExists(1001));
            var existMsg = cstmt.Parameters[1].Value.ToString();
            Assert.AreEqual("Deleted Employee # : 1001", existMsg);
        }

        [Test]
        public void DeleteStatementNotExistsTest()
        {
            // Delete non exist employee will return not found message
            Assert.IsFalse(checkEmployeeExists(1002));
            var deleteNotExist = "emp_delete(:param1,:param2)";

            var cstmt = new EDBCommand(deleteNotExist, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1002));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var notExistMsg = cstmt.Parameters[1].Value.ToString();
            Assert.AreEqual("Employee # 1002 not found", notExistMsg);
        }

        [Test]
        public void InsertStatementTest()
        {
            //User insert statement to insert a employee into database
            Assert.IsFalse(checkEmployeeExists(2001));
            var commandText = "emp_insert(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 2001));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, "ALLEN"));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, "Sales"));

            cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 200));

            var date = DateTime.Now;// DateOnly.FromDateTime(DateTime.Now);
            cstmt.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Date, 10, "param5",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, date));

            cstmt.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Numeric, 10, "param6",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 12000));

            cstmt.Parameters.Add(new EDBParameter("param7", EDBTypes.EDBDbType.Numeric, 10, "param7",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 0));

            cstmt.Parameters.Add(new EDBParameter("param8", EDBTypes.EDBDbType.Integer, 10, "param8",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 21));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            Assert.IsTrue(checkEmployeeExists(2001));
        }

        [Test]
        public void NullStatementNotNullTest()
        {
            //Call divide_it procedure but NULL statement not executed
            var commandText = "divide_it(:param1,:param2,:param3)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 12));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 0));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var number = double.Parse(cstmt.Parameters[2].Value.ToString());

            Assert.AreEqual(4, number, 0.1);
        }

        [Test]
        public void NullStatementTest()
        {
            //Call divide_it procedure and NULL statement executed
            var commandText = "divide_it(:param1,:param2,:param3)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 4));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 0));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 0));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            Assert.AreEqual(string.Empty, cstmt.Parameters[2].Value.ToString());
        }

        [Test]
        public void ReturningIntoStatementExistsTest()
        {
            //Update employee and use returing into statement to get employee information
            Assert.IsTrue(checkEmployeeExists(3001));
            var commandText = "return_into(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3001));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 150000));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 110000));

            cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 10, "param5",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Varchar, 10, "param6",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param7", EDBTypes.EDBDbType.Numeric, 10, "param7",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param8", EDBTypes.EDBDbType.Numeric, 10, "param8",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param9", EDBTypes.EDBDbType.Integer, 10, "param9",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param10", EDBTypes.EDBDbType.Varchar, 10, "param10",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var empnoExists = int.Parse(cstmt.Parameters[3].Value.ToString());
            var enameExists = cstmt.Parameters[4].Value.ToString();
            var jobExists = cstmt.Parameters[5].Value.ToString();
            var salExists = double.Parse(cstmt.Parameters[6].Value.ToString());
            var commExists = double.Parse(cstmt.Parameters[7].Value.ToString());
            var deptnoExists = int.Parse(cstmt.Parameters[8].Value.ToString());
            var msgExists = cstmt.Parameters[9].Value.ToString();

            Assert.AreEqual(empnoExists, 3001);
            Assert.AreEqual(enameExists, "WARD");
            Assert.AreEqual(jobExists, "Sales");
            Assert.AreEqual(salExists, 150000, 0.01);
            Assert.AreEqual(commExists, 110000, 0.01);
            Assert.AreEqual(deptnoExists, 31);
            Assert.AreEqual("Updated Employee # : 3001", msgExists);
        }

        [Test]
        public void ReturningIntoStatementNotExistsTest()
        {
            //Update not existing employee get not found message
            Assert.IsFalse(checkEmployeeExists(3002));
            var commandText = "return_into(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3002));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 150000));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 110000));

            cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 10, "param5",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Varchar, 10, "param6",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param7", EDBTypes.EDBDbType.Numeric, 10, "param7",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param8", EDBTypes.EDBDbType.Numeric, 10, "param8",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param9", EDBTypes.EDBDbType.Integer, 10, "param9",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param10", EDBTypes.EDBDbType.Varchar, 10, "param10",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var enameNotExists = cstmt.Parameters[4].Value.ToString();
            var msgNotExists = cstmt.Parameters[9].Value.ToString();

            Assert.AreEqual(string.Empty, enameNotExists);
            Assert.AreEqual("Employee # 3002 not found", msgNotExists);
        }

        [Test]
        [Ignore("EC-2633: Could not find a way to map %ROWTYPE. DeriveParameters also fails to find a mapping.")]
        public void ReturningIntoFromDeleteStatementExistsTest()
        {
            //Delete a employee and use return into statement to get information
            Assert.IsTrue(checkEmployeeExists(5001));

            var commandText = "return_into_from_delete";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            EDBCommandBuilder.DeriveParameters(cstmt);

            /* This test is not complete and corresponds to the following JDBC test case.
             * It needs to be completed when there is a solution to %ROWTYPE mapping.
             * 
            String commandExists = "{call  return_into_from_delete(?,?,?)}";
        CallableStatement cstmtExists = con.prepareCall(commandExists);
        cstmtExists.setInt(1, 5001);
        cstmtExists.registerOutParameter(2, Types.STRUCT);
        cstmtExists.registerOutParameter(3, Types.VARCHAR);
        cstmtExists.execute();
        Struct emp = (Struct)cstmtExists.getObject(2);
        Object[] data = emp.getAttributes();
        String name = (String)data[1];
        Assert.assertEquals("BLAKE", name);
        String job = (String)data[2];
        Assert.assertEquals("Sales", job);
        java.sql.Timestamp date = (java.sql.Timestamp)data[4];
        java.sql.Timestamp sqlDate = null;
        try {
            String strDate = "2007-11-01 00:00:00";
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd hh:mm:ss");
        java.util.Date utilDate = sdf.parse(strDate);
        sqlDate = new java.sql.Timestamp(utilDate.getTime());
        }catch(Exception e){
            e.printStackTrace();
        }
        Assert.assertEquals(sqlDate, date);
        BigDecimal sal = (BigDecimal)data[5];
        Assert.assertEquals(120000, sal.doubleValue(), 0.01);
        BigDecimal comm = (BigDecimal)data[6];
        Assert.assertEquals("0.00", comm.toString());
        BigDecimal deptNo = (BigDecimal)data[7];
        Assert.assertEquals(51, deptNo.intValue());
        String msg = cstmtExists.getString(3);
        Assert.assertEquals("Deleted Employee # : 5001", msg);
        Assert.assertFalse(checkEmployeeExists(5001));
            */
        }

        [Test]
        [Ignore("EC-2633: Could not find a way to map %ROWTYPE. DeriveParameters also fails to find a mapping.")]
        public void ReturnIntoFromDeleteStatementNotExistsTest()
        {
            //Delete non existing employee return not found message
            Assert.IsFalse(checkEmployeeExists(5002));

            var commandText = "return_into_from_delete";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            EDBCommandBuilder.DeriveParameters(cstmt);

            /* This test is not complete and corresponds to the following JDBC test case.
             * It needs to be completed when there is a solution to %ROWTYPE mapping.
             * 
        String commandNotExists = "{call return_into_from_delete(?,?,?)}";
        CallableStatement cstmtNotExists = con.prepareCall(commandNotExists);
        cstmtNotExists.setInt(1, 5002);
        cstmtNotExists.registerOutParameter(2, Types.STRUCT);
        cstmtNotExists.registerOutParameter(3, Types.VARCHAR);
        cstmtNotExists.execute();
        String msgNotExists = cstmtNotExists.getString(3);
        Assert.assertEquals("Employee # 5002 not found", msgNotExists);
            */
        }

        [Test]
        [Ignore("EC-2633: Could not find a way to map %ROWTYPE. DeriveParameters also fails to find a mapping.")]
        public void SelectIntoStatementExistsTest()
        {
            /* This test is not complete and corresponds to the following JDBC test case.
             * It needs to be completed when there is a solution to %ROWTYPE mapping.
             * 
            //Update employee and use select into statement to get employee information
            Assert.assertTrue(checkEmployeeExists(4001));
        String commandExists = "{call  select_into_query(?,?,?,?)}";
        CallableStatement cstmtExists = con.prepareCall(commandExists);
        cstmtExists.setInt(1, 4001);
        cstmtExists.registerOutParameter(2, Types.STRUCT);
        cstmtExists.registerOutParameter(3, Types.NUMERIC);
        cstmtExists.registerOutParameter(4, Types.VARCHAR);
        cstmtExists.execute();
        Struct emp = (Struct)cstmtExists.getObject(2);
        Object[] data = emp.getAttributes();
        String name = (String)data[1];
        Assert.assertEquals("JONES", name);
        String job = (String)data[2];
        Assert.assertEquals("Sales", job);
        java.sql.Timestamp date = (java.sql.Timestamp)data[4];
        java.sql.Timestamp sqlDate = null;
        try {
            String strDate = "2007-11-01 00:00:00";
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd hh:mm:ss");
        java.util.Date utilDate = sdf.parse(strDate);
        sqlDate = new java.sql.Timestamp(utilDate.getTime());
        }catch(Exception e){
            e.printStackTrace();
        }
        Assert.assertEquals(sqlDate, date);
        BigDecimal sal = (BigDecimal)data[5];
        Assert.assertEquals(120000, sal.doubleValue(), 0.01);
        BigDecimal comm = (BigDecimal)data[6];
        Assert.assertEquals("0.00", comm.toString());
        BigDecimal deptNo = (BigDecimal)data[7];
        Assert.assertEquals(41, deptNo.intValue());
        BigDecimal avgSalDept41 = cstmtExists.getBigDecimal(3);
        Assert.assertEquals(avgSalDept41.doubleValue(), 110000, 0.01);
        String msg = cstmtExists.getString(4);
        Assert.assertNull(msg);
            */
        }

        [Test]
        [Ignore("EC-2633: Could not find a way to map %ROWTYPE. DeriveParameters also fails to find a mapping.")]
        public void SelectIntoStatementNotExistsTest()
        {
            /* This test is not complete and corresponds to the following JDBC test case.
             * It needs to be completed when there is a solution to %ROWTYPE mapping.
             * 
    //Update non existing employee get not found message
    Assert.assertFalse(checkEmployeeExists(4003));
    String commandNotExists = "{call select_into_query(?,?,?,?)}";
    CallableStatement cstmtNotExists = con.prepareCall(commandNotExists);
    cstmtNotExists.setInt(1, 4003);
    cstmtNotExists.registerOutParameter(2, Types.STRUCT);
    cstmtNotExists.registerOutParameter(3, Types.NUMERIC);
    cstmtNotExists.registerOutParameter(4, Types.VARCHAR);
    cstmtNotExists.execute();
    String msgNotExists = cstmtNotExists.getString(4);
    Assert.assertEquals("Employee # 4003 not found", msgNotExists);
            */
        }

        [Test]
    public void SelectIntoExceptionStatementTest()
        {
            //Call select into statement without exception
            var commandText = "select_into_exception_query(:param1,:param2,:param3)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 31));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var name = cstmt.Parameters[1].Value.ToString();
            var msg = cstmt.Parameters[2].Value.ToString();
            Assert.AreEqual("WARD", name);
            Assert.AreEqual(string.Empty, msg);
    }

        [Test]
    public void SelectIntoExceptionStatementErrorTest()
        {
            //Call select into statement with TOO_MANY_ROWS exception
            var commandText = "select_into_exception_query(:param1,:param2,:param3)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 41));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var name = cstmt.Parameters[1].Value.ToString();
            var msg = cstmt.Parameters[2].Value.ToString();
            Assert.AreEqual("JONES", name);
            Assert.AreEqual("More than one employee found", msg);
    }

        [Test]
    public void UpdateStatementExistsTest()
        {
            //Update employee and get employee information
            Assert.IsTrue(checkEmployeeExists(3001));
            var commandText = "emp_comp_update(:param1,:param2,:param3, :param4)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3001));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 250000));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 210000));

            cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 10, "param4",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var msgExists = cstmt.Parameters[3].Value.ToString();
            Assert.AreEqual("Updated Employee # : 3001", msgExists);

            var command = "SELECT sal, comm FROM emp1 where  empno = 3001";
            var selectCommand = new EDBCommand(command, conn);
            var selectResult = selectCommand.ExecuteReader();
            selectResult.Read();
            var sal = selectResult.GetDouble(0);
            var comm = selectResult.GetDouble(1);
            Assert.AreEqual(250000, sal, 0.01);
            Assert.AreEqual(210000, comm, 0.01);

            selectResult.Close();
    }

        [Test]
    public void UpdateStatementNotExistsTest()
        {
            //Update not existing employee get not found message
            Assert.IsFalse(checkEmployeeExists(3002));

            var commandText = "emp_comp_update(:param1,:param2,:param3, :param4)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3002));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 250000));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 210000));

            cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 10, "param4",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var msgNotExists = cstmt.Parameters[3].Value.ToString();
            Assert.AreEqual("Employee # 3002 not found", msgNotExists);
    }

        [Test]
    public void ResultStatusStatementExistsTest()
        {
            //Update employees and get row counts

            var commandText = "status_query(:param1,:param2,:param3, :param4)";

            var cstmt = new EDBCommand(commandText, conn);
        cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 41));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 10, "param4",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var count = int.Parse(cstmt.Parameters[1].Value.ToString());
            var msgFound = cstmt.Parameters[2].Value.ToString();
            var msgNotFound = cstmt.Parameters[3].Value.ToString();

        Assert.AreEqual(2, count);
        Assert.AreEqual("# rows updated: 2",msgFound);
        Assert.AreEqual(string.Empty, msgNotFound);
    }

        [Test]
    public void ResultStatusStatementNotExistsTest()
        {
            //Update not existing employee get not found message

            var commandText = "status_query(:param1,:param2,:param3, :param4)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 42));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 10, "param4",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var count = int.Parse(cstmt.Parameters[1].Value.ToString());
            var msgFound = cstmt.Parameters[2].Value.ToString();
            var msgNotFound = cstmt.Parameters[3].Value.ToString();

            Assert.AreEqual(0, count);
            Assert.AreEqual(string.Empty, msgFound);
            Assert.AreEqual("No rows were updated", msgNotFound);
    }
}
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
