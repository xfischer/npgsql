using System;
using NUnit.Framework;
using System.Data;
using EnterpriseDB.EDBClient.Tests.Support;
using System.Runtime.Serialization;
using System.Globalization;


//EC-2584: Regression Tests for Basic Statement in SPL

//Port JDBC tests to .NET from enhancements\spl\BasicStatementTest.java
namespace EnterpriseDB.EDBClient.Tests.SPL;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 
[TestFixture]
public class EDBBasicStatementTest : EPASTestBase
{
    public class Employee
    {
        public int? empno { get; set; }
        public string? ename { get; set; }
        public string? job { get; set; }
        public int? mgr { get; set; }
        public DateTime? hiredate { get; set; }
        public decimal? sal { get; set; }
        public decimal? comm { get; set; }
        public int? deptno { get; set; }
    }

    [OneTimeSetUp]
    public void Init()
    {
        using var conn = OpenConnection();

        Execute("DROP PROCEDURE IF EXISTS dept_salary_rpt", conn);
        Execute("DROP PROCEDURE IF EXISTS emp_delete", conn);
        Execute("DROP PROCEDURE IF EXISTS emp_insert", conn);
        Execute("DROP PROCEDURE IF EXISTS divide_it", conn);
        Execute("DROP PROCEDURE IF EXISTS return_into", conn);
        Execute("DROP PROCEDURE IF EXISTS return_into_from_delete", conn);
        Execute("DROP PROCEDURE IF EXISTS select_into_query", conn);
        Execute("DROP PROCEDURE IF EXISTS select_into_exception_query", conn);
        Execute("DROP PROCEDURE IF EXISTS emp_comp_update", conn);
        Execute("DROP PROCEDURE IF EXISTS status_query", conn);

        Execute("DROP TABLE IF EXISTS emp1 CASCADE", conn);

        Execute("CREATE TABLE emp1(empno NUMBER(8),  ename VARCHAR2(10),job VARCHAR2(9), "
                + "mgr NUMBER(8), hiredate DATE, sal NUMBER(10,2), comm NUMBER(10,2), deptno NUMBER(4))", conn);

        var add1001 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                + " VALUES(1001,'SMITH','Sales',200,to_date('01-11-07','DD-MM-YY'),120000,0,11)";
        Execute(add1001, conn);

        var add3001 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                + " VALUES(3001,'WARD','Sales',200,to_date('01-11-07','DD-MM-YY'),100000,0,31)";
        Execute(add3001, conn);

        var add4001 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                + " VALUES(4001,'JONES','Sales',200,to_date('01-11-07','DD-MM-YY'),120000,0,41)";
        Execute(add4001, conn);

        var add4002 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                + " VALUES(4002,'MARTIN','Sales',200,to_date('01-11-07','DD-MM-YY'),100000,0,41)";
        Execute(add4002, conn);

        var add5001 = "INSERT INTO emp1(empno,ename,job,mgr, hiredate, sal, comm,  deptno) "
                + " VALUES(5001,'BLAKE','Sales',200,to_date('01-11-07','DD-MM-YY'),120000,0,51)";
        Execute(add5001, conn);

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
        Execute(deptSaleyProcedure, conn);

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
        Execute(empDeleteProcedure, conn);

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
        Execute(empInsertProcedure, conn);

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
        Execute(nullStatementProcedure, conn);

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
        Execute(returnIntoProcedure, conn);

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
        Execute(returnIntoFromDeleteProcedure, conn);

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
        Execute(selectIntoQueryProcedure, conn);

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
        Execute(selectIntoExceptionProcedure, conn);

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
        Execute(empCompUpdateProcedure, conn);

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
        Execute(statusQueryprocedure, conn);
    }

    private void Execute(string query, EDBConnection conn)
    {
        try
        {
            using var com = new EDBCommand(query, conn);
            com.CommandType = CommandType.Text;
            com.ExecuteNonQuery();
        }
        catch
        {
            // Ignore
        }
    }

    private bool CheckEmployeeExists(int empno)
    {
        using var conn = OpenConnection();
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
        using var conn = OpenConnection();
        //Call a procedure has assignment statement
        var commandText = "dept_salary_rpt(:param1,:param2,:param3,:param4,:param5,:param6)";
        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.AddWithValue("param1", 1001);

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Date, 10, "param2",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param3", null!) { Direction = ParameterDirection.Output });
        cstmt.Parameters.Add(new EDBParameter("param4", null!) { Direction = ParameterDirection.Output });
        cstmt.Parameters.Add(new EDBParameter("param5", null!) { Direction = ParameterDirection.Output });
        cstmt.Parameters.Add(new EDBParameter("param6", null!) { Direction = ParameterDirection.Output });

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();


        var date = cstmt.Parameters[1].Value as DateTime?;
        var title = cstmt.Parameters[2].Value.ToString();
        var baseSal = cstmt.Parameters[3].Value as int?;
        var comRate = cstmt.Parameters[4].Value as decimal?;
        var baseAnnual = cstmt.Parameters[5].Value as decimal?;

        var today = DateTime.Now;
        Assert.That(date?.ToShortDateString(), Is.EqualTo(today.ToShortDateString()));

        Assert.That(title.StartsWith("Report For Department # 1001"));

        Assert.That(baseSal, Is.EqualTo(35525));

        Assert.That(comRate, Is.Not.Null);
        Assert.That((double)comRate!.Value, Is.EqualTo(1.33).Within(0.01));

        Assert.That(baseAnnual, Is.Not.Null);
        Assert.That((double)baseAnnual!.Value, Is.EqualTo(47248.25).Within(0.01));
    }

    [Test]
    public void DeleteStatementExistsTest()
    {
        using var conn = OpenConnection();
        //Use delete statement to delete a employee from database
        Assert.That(CheckEmployeeExists(1001));
        var deleteExist = "emp_delete(:param1,:param2)";

        var cstmt = new EDBCommand(deleteExist, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1001));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        Assert.That(CheckEmployeeExists(1001), Is.False);
        var existMsg = cstmt.Parameters[1].Value.ToString();
        Assert.That(existMsg, Is.EqualTo("Deleted Employee # : 1001"));
    }

    [Test]
    public void DeleteStatementNotExistsTest()
    {
        using var conn = OpenConnection();
        // Delete non exist employee will return not found message
        Assert.That(CheckEmployeeExists(1002), Is.False);
        var deleteNotExist = "emp_delete(:param1,:param2)";

        var cstmt = new EDBCommand(deleteNotExist, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1002));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var notExistMsg = cstmt.Parameters[1].Value.ToString();
        Assert.That(notExistMsg, Is.EqualTo("Employee # 1002 not found"));
    }

    [Test]
    public void InsertStatementTest()
    {
        using var conn = OpenConnection();
        //User insert statement to insert a employee into database
        Assert.That(CheckEmployeeExists(2001), Is.False);
        var commandText = "emp_insert(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 2001));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, "ALLEN"));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, "Sales"));

        cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 200));

        var date = DateTime.Now;
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

        Assert.That(CheckEmployeeExists(2001));
    }

    [Test]
    public void NullStatementNotNullTest()
    {
        using var conn = OpenConnection();
        //Call divide_it procedure but NULL statement not executed
        var commandText = "divide_it(:param1,:param2,:param3)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 12));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 0));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var number = double.Parse(cstmt.Parameters[2].Value.ToString());

        Assert.That(number, Is.EqualTo(4).Within(0.1));
    }

    [Test]
    public void NullStatementTest()
    {
        using var conn = OpenConnection();
        //Call divide_it procedure and NULL statement executed
        var commandText = "divide_it(:param1,:param2,:param3)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 4));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 0));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 0));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        Assert.That(cstmt.Parameters[2].Value.ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void ReturningIntoStatementExistsTest()
    {
        using var conn = OpenConnection();
        //Update employee and use returing into statement to get employee information
        Assert.That(CheckEmployeeExists(3001));
        var commandText = "return_into(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3001));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 150000));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 110000));

        cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 10, "param5",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Varchar, 10, "param6",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param7", EDBTypes.EDBDbType.Numeric, 10, "param7",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param8", EDBTypes.EDBDbType.Numeric, 10, "param8",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param9", EDBTypes.EDBDbType.Integer, 10, "param9",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param10", EDBTypes.EDBDbType.Varchar, 10, "param10",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var empnoExists = int.Parse(cstmt.Parameters[3].Value.ToString());
        var enameExists = cstmt.Parameters[4].Value.ToString();
        var jobExists = cstmt.Parameters[5].Value.ToString();
        var salExists = double.Parse(cstmt.Parameters[6].Value.ToString());
        var commExists = double.Parse(cstmt.Parameters[7].Value.ToString());
        var deptnoExists = int.Parse(cstmt.Parameters[8].Value.ToString());
        var msgExists = cstmt.Parameters[9].Value.ToString();

        Assert.That(empnoExists, Is.EqualTo(3001));
        Assert.That(enameExists, Is.EqualTo("WARD"));
        Assert.That(jobExists, Is.EqualTo("Sales"));
        Assert.That(salExists, Is.EqualTo(150000));
        Assert.That(commExists, Is.EqualTo(110000));
        Assert.That(deptnoExists, Is.EqualTo(31));
        Assert.That(msgExists, Is.EqualTo("Updated Employee # : 3001"));
    }

    [Test]
    public void ReturningIntoStatementNotExistsTest()
    {
        using var conn = OpenConnection();
        //Update not existing employee get not found message
        Assert.That(CheckEmployeeExists(3002), Is.False);
        var commandText = "return_into(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3002));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 150000));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 110000));

        cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 10, "param5",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Varchar, 10, "param6",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param7", EDBTypes.EDBDbType.Numeric, 10, "param7",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param8", EDBTypes.EDBDbType.Numeric, 10, "param8",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param9", EDBTypes.EDBDbType.Integer, 10, "param9",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param10", EDBTypes.EDBDbType.Varchar, 10, "param10",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var enameNotExists = cstmt.Parameters[4].Value.ToString();
        var msgNotExists = cstmt.Parameters[9].Value.ToString();

        Assert.That(enameNotExists, Is.EqualTo(string.Empty));
        Assert.That(msgNotExists, Is.EqualTo("Employee # 3002 not found"));
    }

    [Test]
    public void ReturningIntoFromDeleteStatementExistsTest()
    {
        var ds = CreateDataSourceBuilder()
            .ConfigureTypeLoading(b => b.EnableTableCompositesLoading())
            .MapComposite<Employee>("emp1")
            .Build();
        using var conn = ds.OpenConnection();

        //Delete a employee and use return into statement to get information
        Assert.That(CheckEmployeeExists(5001));

        var commandText = "return_into_from_delete";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.AddWithValue("p_empno", 5001);
        cstmt.Parameters.Add(new EDBParameter("r_emp", null!) { Direction = ParameterDirection.Output });
        cstmt.Parameters.Add(new EDBParameter("msg", null!) { Direction = ParameterDirection.Output });

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        var emp = cstmt.Parameters[1].Value as Employee;
        var msg = cstmt.Parameters[2].Value.ToString();

        Assert.That(emp, Is.Not.Null);

        Assert.That(emp.ename, Is.EqualTo("BLAKE"));
        Assert.That(emp.job, Is.EqualTo("Sales"));
        Assert.That(emp.hiredate, Is.EqualTo(new DateTime(2007, 11, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        Assert.That(emp.sal, Is.EqualTo(120000));
        Assert.That(emp.comm, Is.EqualTo(0));
        Assert.That(emp.deptno, Is.EqualTo(51));

        Assert.That(msg, Is.EqualTo("Deleted Employee # : 5001"));
        Assert.That(CheckEmployeeExists(5001), Is.False);
    }

    [Test]
    public void ReturnIntoFromDeleteStatementNotExistsTest()
    {
        var ds = CreateDataSourceBuilder()
            .ConfigureTypeLoading(b => b.EnableTableCompositesLoading())
            .MapComposite<Employee>("emp1")
            .Build();
        using var conn = ds.OpenConnection();
        //Delete non existing employee return not found message
        Assert.That(CheckEmployeeExists(5002), Is.False);

        var commandText = "return_into_from_delete";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        EDBCommandBuilder.DeriveParameters(cstmt);

        cstmt.Parameters[0].Value = 5002;

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        var emp = cstmt.Parameters[1].Value as Employee;
        var msgNotExists = cstmt.Parameters[2].Value.ToString();

        Assert.That(msgNotExists, Is.EqualTo("Employee # 5002 not found"));
    }

    [Test]
    public void SelectIntoStatementExistsTest()
    {
        var ds = CreateDataSourceBuilder()
            .ConfigureTypeLoading(b => b.EnableTableCompositesLoading())
            .MapComposite<Employee>("emp1")
            .Build();
        using var conn = ds.OpenConnection();

        //Update employee and use select into statement to get employee information
        Assert.That(CheckEmployeeExists(4001));

        var commandText = "select_into_query";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.AddWithValue("empno", 4001);
        var empParam = cstmt.Parameters.Add(new EDBParameter("emp", null!) { Direction = ParameterDirection.Output });
        var avgsal = cstmt.Parameters.Add(new EDBParameter("avgsal", null!) { Direction = ParameterDirection.Output });
        var msg = cstmt.Parameters.Add(new EDBParameter("msg", null!) { Direction = ParameterDirection.Output });

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();


        Assert.That(empParam.Value, Is.Not.Null);
        var emp = empParam.Value as Employee;

        Assert.That(emp.ename, Is.EqualTo("JONES"));
        Assert.That(emp.job, Is.EqualTo("Sales"));
        Assert.That(emp.hiredate, Is.EqualTo(new DateTime(2007, 11, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        Assert.That(emp.sal, Is.EqualTo(120000));
        Assert.That(emp.comm, Is.EqualTo(0));
        Assert.That(emp.deptno, Is.EqualTo(41));

        Assert.That(avgsal, Is.Not.Null);
        Assert.That(avgsal!.Value, Is.Not.Null);
        Assert.That((double)(decimal)avgsal!.Value!, Is.EqualTo(110000));

        Assert.That(string.IsNullOrEmpty(msg.Value!.ToString()));

    }

    [Test]
    public void SelectIntoStatementNotExistsTest()
    {
        var ds = CreateDataSourceBuilder()
            .ConfigureTypeLoading(b => b.EnableTableCompositesLoading())
            .MapComposite<Employee>("emp1")
            .Build();
        using var conn = ds.OpenConnection();

        //Update non existing employee get not found message
        Assert.That(CheckEmployeeExists(4003), Is.False);

        var commandText = "select_into_query";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.AddWithValue("empno", 4003);
        cstmt.Parameters.Add(new EDBParameter("emp", null!) { Direction = ParameterDirection.Output });
        cstmt.Parameters.Add(new EDBParameter("avgsal", null!) { Direction = ParameterDirection.Output });
        var msg = cstmt.Parameters.Add(new EDBParameter("msg", null!) { Direction = ParameterDirection.Output });

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        Assert.That(msg.Value.ToString(), Is.EqualTo("Employee # 4003 not found"));
    }

    [Test]
    public void SelectIntoExceptionStatementTest()
    {
        using var conn = OpenConnection();
        //Call select into statement without exception
        var commandText = "select_into_exception_query(:param1,:param2,:param3)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 31));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var name = cstmt.Parameters[1].Value.ToString();
        var msg = cstmt.Parameters[2].Value.ToString();
        Assert.That(name, Is.EqualTo("WARD"));
        Assert.That(msg, Is.EqualTo(string.Empty));
    }

    [Test]
    public void SelectIntoExceptionStatementErrorTest()
    {
        using var conn = OpenConnection();
        //Call select into statement with TOO_MANY_ROWS exception
        var commandText = "select_into_exception_query(:param1,:param2,:param3)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 41));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var name = cstmt.Parameters[1].Value.ToString();
        var msg = cstmt.Parameters[2].Value.ToString();
        Assert.That(name, Is.EqualTo("JONES"));
        Assert.That(msg, Is.EqualTo("More than one employee found"));
    }

    [Test]
    public void UpdateStatementExistsTest()
    {
        using var conn = OpenConnection();
        //Update employee and get employee information
        Assert.That(CheckEmployeeExists(3001));
        var commandText = "emp_comp_update(:param1,:param2,:param3, :param4)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3001));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 250000));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 210000));

        cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 10, "param4",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var msgExists = cstmt.Parameters[3].Value.ToString();
        Assert.That(msgExists, Is.EqualTo("Updated Employee # : 3001"));

        var command = "SELECT sal, comm FROM emp1 where  empno = 3001";
        var selectCommand = new EDBCommand(command, conn);
        var selectResult = selectCommand.ExecuteReader();
        selectResult.Read();
        var sal = selectResult.GetDouble(0);
        var comm = selectResult.GetDouble(1);
        Assert.That(sal, Is.EqualTo(250000).Within(0.01));
        Assert.That(comm, Is.EqualTo(210000).Within(0.01));

        selectResult.Close();
    }

    [Test]
    public void UpdateStatementNotExistsTest()
    {
        using var conn = OpenConnection();
        //Update not existing employee get not found message
        Assert.That(CheckEmployeeExists(3002), Is.False);

        var commandText = "emp_comp_update(:param1,:param2,:param3, :param4)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 3002));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 250000));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 210000));

        cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 10, "param4",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var msgNotExists = cstmt.Parameters[3].Value.ToString();
        Assert.That(msgNotExists, Is.EqualTo("Employee # 3002 not found"));
    }

    [Test]
    public void ResultStatusStatementExistsTest()
    {
        using var conn = OpenConnection();
        //Update employees and get row counts

        var commandText = "status_query(:param1,:param2,:param3, :param4)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 41));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 10, "param4",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var count = int.Parse(cstmt.Parameters[1].Value.ToString());
        var msgFound = cstmt.Parameters[2].Value.ToString();
        var msgNotFound = cstmt.Parameters[3].Value.ToString();

        Assert.That(count, Is.EqualTo(2));
        Assert.That(msgFound, Is.EqualTo("# rows updated: 2"));
        Assert.That(msgNotFound, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ResultStatusStatementNotExistsTest()
    {
        using var conn = OpenConnection();
        //Update not existing employee get not found message

        var commandText = "status_query(:param1,:param2,:param3, :param4)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 42));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 10, "param4",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();
        var count = int.Parse(cstmt.Parameters[1].Value.ToString());
        var msgFound = cstmt.Parameters[2].Value.ToString();
        var msgNotFound = cstmt.Parameters[3].Value.ToString();

        Assert.That(count, Is.EqualTo(0));
        Assert.That(msgFound, Is.EqualTo(string.Empty));
        Assert.That(msgNotFound, Is.EqualTo("No rows were updated"));
    }
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604
