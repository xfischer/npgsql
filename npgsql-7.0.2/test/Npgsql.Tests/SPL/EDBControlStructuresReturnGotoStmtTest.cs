using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2582: Regression tests for Return Statement and Goto Statement in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    public class EDBControlStructuresReturnGotoStmtTest : EPASTestBase
    {
        EDBConnection? conn = null;

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP FUNCTION emp_comp");
            Execute("DROP PROCEDURE verify_emp");

            Execute("DROP TABLE emp1 CASCADE");

            Execute("CREATE TABLE emp1(empno NUMBER(8),  ename VARCHAR2(10),job VARCHAR2(9), "
                + "hiredate DATE, sal NUMBER(10,2))");

            Execute("INSERT INTO emp1(empno,ename,job,hiredate, sal) "
                        + "VALUES(1001,'SMITH','Sales',to_date('01-11-07','DD-MM-YY'),120000) ");
            Execute("INSERT INTO emp1(empno,ename, job,hiredate, sal) "
                        + "VALUES(4001,null, 'Sales',to_date('01-11-07','DD-MM-YY'),120000)");
            Execute("INSERT INTO emp1(empno,ename,job,hiredate, sal) "
                        + "VALUES(4002,'MARTIN',null,to_date('01-11-07','DD-MM-YY'),100000)");
            Execute("INSERT INTO emp1(empno,ename,job,hiredate, sal)"
                        + " VALUES(5001,'BLAKE','Sales',null,120000)");

            //The following example uses the RETURN statement returns a value to the caller:
            var empCompFunc = "CREATE OR REPLACE FUNCTION emp_comp (\n"
                               + "    p_sal           NUMBER,\n"
                               + "    p_comm          NUMBER\n"
                               + ") RETURN NUMBER\n"
                               + "IS\n"
                               + "BEGIN\n"
                               + "    RETURN (p_sal + NVL(p_comm, 0)) * 24;\n"
                               + "END emp_comp;";
            Execute(empCompFunc);
            //The following example verifies that an employee record contains a name,
            //job description, and employee hire date; if any piece of information is missing,
            //a GOTO statement transfers the point of execution to a statement that prints a
            //message that the employee is not valid.
            var verifyEmpProc = "CREATE OR REPLACE PROCEDURE verify_emp (\n"
                                 + "    p_empno    IN     NUMBER, \n"
                                 + "    msg        OUT    VARCHAR2(60) \n"
                                 + ")\n"
                                 + "IS\n"
                                 + "    v_ename         emp1.ename%TYPE;\n"
                                 + "    v_job           emp1.job%TYPE;\n"
                                 + "    v_hiredate      emp1.hiredate%TYPE;\n"
                                 + "BEGIN\n"
                                 + "    SELECT ename, job, hiredate\n"
                                 + "        INTO v_ename, v_job, v_hiredate FROM emp1\n"
                                 + "        WHERE empno = p_empno;\n"
                                 + "    IF v_ename IS NULL THEN\n"
                                 + "        GOTO invalid_emp;\n"
                                 + "    END IF;\n"
                                 + "    IF v_job IS NULL THEN\n"
                                 + "        GOTO invalid_emp;\n"
                                 + "    END IF;\n"
                                 + "    IF v_hiredate IS NULL THEN\n"
                                 + "        GOTO invalid_emp;\n" + "    END IF;\n"
                                 + "    msg := 'Employee ' || p_empno ||\n"
                                 + "        ' validated without errors.';\n"
                                 + "    RETURN;\n"
                                 + "    <<invalid_emp>> msg := 'Employee ' || p_empno ||\n"
                                 + "        ' is not a valid employee.';\n"
                                 + "END;";
            Execute(verifyEmpProc);
        }

        [TearDown]
        public void Dispose()
        {
            //Execute("DROP FUNCTION emp_comp;");
            //Execute("DROP PROCEDURE verify_emp;");

            //Execute("DROP TABLE emp1");
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
        public void ReturnStatementTest()
        {
            //Test return statement
            var commandText = "emp_comp(:param1,:param2)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1000));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 200));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3",
                ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var value = double.Parse(cstmt.Parameters[2].Value.ToString());

            Assert.AreEqual(value, 28800.00, 0.01);
        }

        [Test]
        public void GoToStatementTest()
        {
            //Test goto statement with valid employee
            var commandText = "verify_emp(:param1,:param2)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1001));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var value = cstmt.Parameters[1].Value.ToString();

            Assert.AreEqual(value, "Employee 1001 validated without errors.");
        }

        [Test]
    public void GoToStatementInvalidNameTest()
        {
            //Test goto statment with null ename
            var commandText = "verify_emp(:param1,:param2)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 4001));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var value = cstmt.Parameters[1].Value.ToString();

            Assert.AreEqual(value, "Employee 4001 is not a valid employee.");

//        Assert.assertEquals("Employee 4001 is not a valid employee.", value);
    }

        [Test]
    public void GoToStatementInvalidJobTest()
        {
            //Test goto statment with null job
            var commandText = "verify_emp(:param1,:param2)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 4002));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var value = cstmt.Parameters[1].Value.ToString();

            Assert.AreEqual(value, "Employee 4002 is not a valid employee.");
    }

        [Test]
    public void GoToStatementInvalidDateTest()
        {
            //Test goto statment with null hiredate
            var commandText = "verify_emp(:param1,:param2)";

            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 5001));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();
            var value = cstmt.Parameters[1].Value.ToString();

            Assert.AreEqual(value, "Employee 5001 is not a valid employee.");
    }
}
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
