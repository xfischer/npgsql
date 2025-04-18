using System;
using NUnit.Framework;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;

//EC-2576: Regression Tests for Package in SPL

namespace EnterpriseDB.EDBClient.Tests.SPL;

[NonParallelizable]
internal class EDBPackageTest : EPASTestBase
{
    private static readonly string[] EMP_RESULT = [
        "2009 JACK SALES 01-01-2023 5000.00 0.00 1001 20"
        ];
    private static readonly string[] DEPT_EMP_RESULT = [
        "EMPLOYEES IN DEPT #30: SALES",
        "EMPNO ENAME",
        "----- -------",
        "7499  ALLEN",
        "7521  WARD",
        "7654  MARTIN",
        "7698  BLAKE",
        "7844  TURNER",
        "7900  JAMES",
        "**********************",
        "6 rows were retrieved"
        ];

    private static readonly string[] EMP_LIST_RESULT = [
        "7499 ALLEN",
        "7521 WARD",
        "7654 MARTIN",
        "7698 BLAKE",
        "7844 TURNER",
        "7900 JAMES",
        ];

    [SetUp]
    public void Init()
    {
        using var conn = OpenConnection();

        Execute("DROP PACKAGE BODY emp_admin;");
        Execute("DROP PACKAGE emp_admin;");
        Execute("DROP PACKAGE BODY emp_rpt;");
        Execute("DROP PACKAGE emp_rpt;");

        Execute("DROP TABLE emp1 CASCADE");
        Execute("DROP TABLE dept1 CASCADE");

        Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(20), job VARCHAR2(20), hiredate DATE, "
                + "sal NUMBER(10,2), comm NUMBER(10,2), mgr NUMBER(4), deptno NUMBER(4))");
        Execute("CREATE TABLE dept1(deptno NUMBER(4), dname VARCHAR2(14), loc VARCHAR2(20))");
        var addEmp = new string[] {
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7369,'SMITH','CLERK',"
                    + "to_date('17-12-1980','DD-MM-YYYY'),800,0,1002,20)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7499,'ALLEN','SALESMAN',"
                    + "to_date('20-02-1981','DD-MM-YYYY'),1600,300,1003,30)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7521,'WARD','SALESMAN',"
                    + "to_date('22-02-1981','DD-MM-YYYY'),1250,500,1003,30)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7566,'JONES','MANAGER',"
                    + "to_date('02-04-1981','DD-MM-YYYY'),2975,0,1003,20)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7654,'MARTIN','SALESMAN',"
                    + "to_date('28-09-1981','DD-MM-YYYY'),1250,1400,1003,30)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7698,'BLAKE','MANAGER',"
                    + "to_date('01-05-1981','DD-MM-YYYY'),2850,0,1003,30)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7782,'CLARK','MANAGER',"
                    + "to_date('09-06-1981','DD-MM-YYYY'),2450,0,1001,10)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7788,'SCOTT','ANALYST',"
                    + "to_date('19-04-1987','DD-MM-YYYY'),3000,0,1002,20)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7839,'KING','PRESIDENT',"
                    + "to_date('17-11-1981','DD-MM-YYYY'),5000,0,1001,10)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7844,'TURNER','SALESMAN',"
                    + "to_date('08-09-1981','DD-MM-YYYY'),1500,0,1003,30)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7876,'ADAMS','CLERK',"
                    + "to_date('23-05-1987','DD-MM-YYYY'),1100,0,1002,20)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7900,'JAMES','CLERK',"
                    + "to_date('03-12-1981','DD-MM-YYYY'),950,0,1003,30)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7902,'FORD','ANALYST',"
                    + "to_date('03-12-1981','DD-MM-YYYY'),3000,0,1002,20)",
            "INSERT INTO emp1(empno,ename,job,hiredate,sal,comm,mgr,deptno) VALUES(7934,'MILLER','CLERK',"
                    + "to_date('23-01-1982','DD-MM-YYYY'),1300,0,1001,10)" };
        for (var i = 0; i < addEmp.Length; i++)
        {
            Execute(addEmp[i]);
        }
        var addDept = new string[] {
            "insert into dept1 (deptno, dname, loc) values (10, 'ACCOUNTING','NEW YORK');",
            "insert into dept1 (deptno, dname, loc) values (20, 'RESEARCH','DALLAS');",
            "insert into dept1 (deptno, dname, loc) values (30, 'SALES','CHICAGO');",
            "insert into dept1 (deptno, dname, loc) values (40, 'OPERATIONS','BOSTON');"
            };
        for (var i = 0; i < addDept.Length; i++)
        {
            Execute(addDept[i]);
        }

        //This code sample creates the emp_admin package specification. This package
        //specification consists of two functions and two stored procedures.
        var empAdminPkg = "CREATE OR REPLACE PACKAGE emp_admin\n"
                + "IS\n"
                + "   FUNCTION get_dept_name (\n"
                + "      p_deptno NUMBER DEFAULT 10\n"
                + "   )\n"
                + "   RETURN VARCHAR2;\n"
                + "   FUNCTION update_emp_sal (\n"
                + "      p_empno NUMBER,\n"
                + "      p_raise NUMBER\n"
                + "   )\n"
                + "   RETURN NUMBER;\n"
                + "   PROCEDURE hire_emp (\n"
                + "      p_empno           NUMBER,\n"
                + "      p_ename           VARCHAR2,\n"
                + "      p_job             VARCHAR2,\n"
                + "      p_sal             NUMBER,\n"
                + "      p_hiredate        DATE      DEFAULT   sysdate,\n"
                + "      p_comm            NUMBER    DEFAULT   0,\n"
                + "      p_mgr             NUMBER,\n"
                + "      p_deptno          NUMBER    DEFAULT   10\n"
                + "   );\n"
                + "   PROCEDURE fire_emp (\n"
                + "      p_empno NUMBER\n"
                + "   );\n"
                + "END emp_admin;";
        Execute(empAdminPkg);

        //The body of the package contains the actual implementation behind the package
        //specification. For the emp_admin package specification in the example, this
        //code now create a package body that implements the specifications. The body
        //contains the implementation of the functions and stored procedures in the
        //specification.
        var empAdminPkgBody = "CREATE OR REPLACE PACKAGE BODY emp_admin\n"
                + "IS\n"
                + "   --\n"
                + "   --  Function that queries the 'dept1' table based on the department\n"
                + "   --  number and returns the corresponding department name.\n"
                + "   --\n"
                + "   FUNCTION get_dept_name (\n"
                + "      p_deptno        IN NUMBER DEFAULT 10\n"
                + "   )\n"
                + "   RETURN VARCHAR2\n"
                + "   IS\n"
                + "      v_dname         VARCHAR2(14);\n"
                + "   BEGIN\n"
                + "      SELECT dname INTO v_dname FROM dept1 WHERE deptno = p_deptno;\n"
                + "      RETURN v_dname;\n"
                + "   EXCEPTION\n"
                + "      WHEN NO_DATA_FOUND THEN\n"
                + "         DBMS_OUTPUT.PUT_LINE('Invalid department number ' || p_deptno);\n"
                + "         RETURN '';\n"
                + "   END;\n"
                + "   --\n"
                + "   --  Function that updates an employee's salary based on the\n"
                + "   --  employee number and salary increment/decrement passed\n"
                + "   --  as IN parameters.  Upon successful completion the function\n"
                + "   --  returns the new updated salary.\n"
                + "   --\n"
                + "   FUNCTION update_emp_sal (\n"
                + "      p_empno         IN NUMBER,\n"
                + "      p_raise         IN NUMBER\n"
                + "   )\n"
                + "   RETURN NUMBER\n"
                + "   IS\n"
                + "      v_sal           NUMBER := 0;\n"
                + "   BEGIN\n"
                + "      SELECT sal INTO v_sal FROM emp1 WHERE empno = p_empno;\n"
                + "      v_sal := v_sal + p_raise;\n"
                + "      UPDATE emp1 SET sal = v_sal WHERE empno = p_empno;\n"
                + "      RETURN v_sal;\n"
                + "   EXCEPTION\n"
                + "      WHEN NO_DATA_FOUND THEN\n"
                + "         DBMS_OUTPUT.PUT_LINE('Employee ' || p_empno || ' not found');\n"
                + "         RETURN -1;\n"
                + "      WHEN OTHERS THEN\n"
                + "         DBMS_OUTPUT.PUT_LINE('The following is SQLERRM:');\n"
                + "         DBMS_OUTPUT.PUT_LINE(SQLERRM);\n"
                + "         DBMS_OUTPUT.PUT_LINE('The following is SQLCODE:');\n"
                + "         DBMS_OUTPUT.PUT_LINE(SQLCODE);\n"
                + "         RETURN -1;\n"
                + "   END;\n"
                + "   --\n"
                + "   --  Procedure that inserts a new employee record into the 'emp1' table.\n" + "   --\n"
                + "   PROCEDURE hire_emp (\n"
                + "      p_empno         NUMBER,\n"
                + "      p_ename         VARCHAR2,\n"
                + "      p_job           VARCHAR2,\n"
                + "      p_sal           NUMBER,\n"
                + "      p_hiredate      DATE    DEFAULT sysdate,\n"
                + "      p_comm          NUMBER  DEFAULT 0,\n"
                + "      p_mgr           NUMBER,\n"
                + "      p_deptno        NUMBER  DEFAULT 10\n" + "   )\n"
                + "   AS\n"
                + "   BEGIN\n"
                + "      INSERT INTO emp1(empno, ename, job, sal, hiredate, comm, mgr, deptno)\n"
                + "         VALUES(p_empno, p_ename, p_job, p_sal,\n"
                + "                p_hiredate, p_comm, p_mgr, p_deptno);\n"
                + "   END;\n"
                + "   --\n"
                + "   --  Procedure that deletes an employee record from the 'emp1' table based\n"
                + "   --  on the employee number.\n"
                + "   --\n"
                + "   PROCEDURE fire_emp (\n"
                + "      p_empno         NUMBER\n"
                + "   )\n"
                + "   AS\n"
                + "   BEGIN\n"
                + "      DELETE FROM emp1 WHERE empno = p_empno;\n"
                + "   END;\n"
                + "END;\n";
        Execute(empAdminPkgBody);

        //The package specification of emp_rpt shows the declaration of a record type emprec_typ
        //and a weakly typed REF CURSOR, emp_refcur as publicly accessible. It also shows two
        //functions and two procedures. The function, open_emp_by_dept, returns the REF CURSOR
        //type EMP_REFCUR. Procedures fetch_emp and close_refcur both declare a weakly typed
        //REF CURSOR as a formal parameter.
        var empRptPkg = "CREATE OR REPLACE PACKAGE emp_rpt\n"
                + "IS\n"
                + "    TYPE emprec_typ IS RECORD (\n"
                + "        empno       NUMBER(4),\n"
                + "        ename       VARCHAR(10)\n"
                + "    );\n"
                + "    TYPE emp_refcur IS REF CURSOR;\n"
                + "\n"
                + "    FUNCTION get_dept_name (\n"
                + "        p_deptno    IN NUMBER\n"
                + "    ) RETURN VARCHAR2;\n"
                + "    FUNCTION open_emp_by_dept (\n"
                + "        p_deptno    IN emp1.deptno%TYPE\n"
                + "    ) RETURN EMP_REFCUR;\n"
                + "    PROCEDURE fetch_emp (\n"
                + "        p_refcur    IN OUT SYS_REFCURSOR\n"
                + "    );\n"
                + "    PROCEDURE close_refcur (\n"
                + "        p_refcur    IN OUT SYS_REFCURSOR\n"
                + "    );\n"
                + "END emp_rpt;";
        Execute(empRptPkg);

        //The package body shows the declaration of several private variables: a static
        //cursor dept_cur, a table type depttab_typ, a table variable t_dept, an integer
        //variable t_dept_max, and a record variable r_emp.
        var empRptPkgBody = "CREATE OR REPLACE PACKAGE BODY emp_rpt\n"
                + "IS\n"
                + "    CURSOR dept_cur IS SELECT * FROM dept1;\n"
                + "    TYPE depttab_typ IS TABLE of dept1%ROWTYPE\n"
                + "        INDEX BY BINARY_INTEGER;\n"
                + "    t_dept          DEPTTAB_TYP;\n"
                + "    t_dept_max      INTEGER := 1;\n"
                + "    r_emp           EMPREC_TYP;\n"
                + "\n"
                + "    FUNCTION get_dept_name (\n"
                + "        p_deptno    IN NUMBER\n"
                + "    ) RETURN VARCHAR2\n"
                + "    IS\n"
                + "    BEGIN\n"
                + "        FOR i IN 1..t_dept_max LOOP\n"
                + "            IF p_deptno = t_dept(i).deptno THEN\n"
                + "                RETURN t_dept(i).dname;\n"
                + "            END IF;\n"
                + "        END LOOP;\n"
                + "        RETURN 'Unknown';\n"
                + "    END;\n"
                + "\n"
                + "    FUNCTION open_emp_by_dept(\n"
                + "        p_deptno        IN emp1.deptno%TYPE\n"
                + "    ) RETURN EMP_REFCUR\n"
                + "    IS\n"
                + "        emp_by_dept EMP_REFCUR;\n"
                + "    BEGIN\n"
                + "        OPEN emp_by_dept FOR SELECT empno, ename FROM emp1\n"
                + "            WHERE deptno = p_deptno;\n"
                + "        RETURN emp_by_dept;\n"
                + "    END;\n"
                + "\n"
                + "    PROCEDURE fetch_emp (\n"
                + "        p_refcur      IN OUT SYS_REFCURSOR\n"
                + "    )\n"
                + "    IS\n"
                + "    BEGIN\n"
                + "        DBMS_OUTPUT.PUT_LINE('EMPNO ENAME');\n"
                + "        DBMS_OUTPUT.PUT_LINE('----- -------');\n"
                + "        LOOP\n"
                + "            FETCH p_refcur INTO r_emp;\n"
                + "            EXIT WHEN p_refcur%NOTFOUND;\n"
                + "            DBMS_OUTPUT.PUT_LINE(r_emp.empno || '  ' || r_emp.ename);\n"
                + "        END LOOP;\n"
                + "    END;\n"
                + "\n"
                + "    PROCEDURE close_refcur (\n"
                + "        p_refcur      IN OUT SYS_REFCURSOR\n"
                + "    )\n"
                + "    IS\n"
                + "    BEGIN\n"
                + "        CLOSE p_refcur;\n"
                + "    END;\n"
                + "BEGIN\n"
                + "    OPEN dept_cur;\n"
                + "    LOOP\n"
                + "        FETCH dept_cur INTO t_dept(t_dept_max);\n"
                + "        EXIT WHEN dept_cur%NOTFOUND;\n"
                + "        t_dept_max := t_dept_max + 1;\n"
                + "    END LOOP;\n"
                + "    CLOSE dept_cur;\n"
                + "    t_dept_max := t_dept_max - 1;\n"
                + "END emp_rpt;";
        Execute(empRptPkgBody);
    }

    [TearDown]
    public void Dispose()
    {
        Execute("DROP TABLE emp1");
    }

    private void Execute(string query)
    {
        try
        {
            using var conn = OpenConnection();
            using var com = new EDBCommand(query, conn);
            com.CommandType = CommandType.Text;
            com.ExecuteNonQuery();
        }
        catch
        {
            // swallow exception
        }
    }

    //Got data from resultset and create a list of String
    private static List<string> GetResultSetData(EDBDataReader rs)
    {
        var list = new List<string>();
        var columnSchema = rs.GetColumnSchema();
        while (rs.Read())
        {
            var columns = columnSchema.Count;
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
                    str.Append(' ');
                }
            }
            list.Add(str.ToString());
        }
        return list;
    }

    private double GetEmpSalary(int empno)
    {
        using var conn = OpenConnection();
        //Get the salary infomation of one employee
        var command = "select sal from emp1 where empno=" + empno;

        var selectCommand = new EDBCommand(command, conn);
        var selectResult = selectCommand.ExecuteReader();
        selectResult.Read();
        var sal = selectResult.GetDouble(0);
        selectResult.Close();

        return sal;
    }

    [Test]
    public void GetDeptNameTest()
    {
        using var conn = OpenConnection();
        //call function get_dept_name in package emp_admin
        var commandText = "emp_admin.get_dept_name(:param1)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));

        cstmt.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Varchar, 10, "ret",
            ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        var dname = cstmt.Parameters[1].Value!.ToString();
        Assert.AreEqual("RESEARCH", dname);
    }

    [Test]
    public void HireEmpTest()
    {
        using var conn = OpenConnection();
        //call procedure hire_emp in package emp_admin to insert employee
        //record
        var commandText = "emp_admin.hire_emp(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 2009));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, "JACK"));

        cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Varchar, 10, "param3",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, "SALES"));

        cstmt.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Numeric, 10, "param4",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 5000));

        var date = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        cstmt.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Date, 10, "param5",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, date));

        cstmt.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Numeric, 10, "param6",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 0));

        cstmt.Parameters.Add(new EDBParameter("param7", EDBTypes.EDBDbType.Numeric, 10, "param7",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1001));

        cstmt.Parameters.Add(new EDBParameter("param8", EDBTypes.EDBDbType.Numeric, 10, "param8",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        //check if emp1 inserted
        var getEmp = @"SELECT empno, ename, job, to_char(hiredate, 'DD-MM-YYYY') AS ""hiredate"", 
                    sal, comm, mgr, deptno  FROM emp1 WHERE empno =2009;";

        var selectCommand = new EDBCommand(getEmp, conn);
        var rs = selectCommand.ExecuteReader();

        var empList = GetResultSetData(rs);
        Assert.AreEqual(EMP_RESULT.Length, empList.Count);
        for (var i = 0; i < empList.Count; i++)
        {
            Assert.AreEqual(EMP_RESULT[i], empList[i]);
        }
    }

    [Test]
    public void FireEmpTest()
    {
        using var conn = OpenConnection();
        //call procedure fire_emp in package emp_admin to delete employee
        var commandText = "emp_admin.fire_emp(:param1)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        //check if employee exists
        var getEmp = "SELECT * from emp1 WHERE empno =2009;";

        var selectCommand = new EDBCommand(getEmp, conn);
        var selectResult = selectCommand.ExecuteReader();
        Assert.IsFalse(selectResult.Read());
        selectResult.Close();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void UpdateEmpSalTest(bool declareReturnValue)
    {
        using var conn = OpenConnection();
        //call function update_emp_sal in package emp_admin to update
        //salary of given employee
        var commandText = "emp_admin.update_emp_sal(:param1, :param2)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 7369));

        cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1000));

        if (declareReturnValue)
        {
            cstmt.Parameters.Add(new EDBParameter("retVal", EDBTypes.EDBDbType.Numeric, 10, "retVal",
                ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1000));
        }

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        //check if salary updated
        var sal = GetEmpSalary(7369);
        Assert.AreEqual(1800.00, sal, 0.01);
    }

    [Test]
    [Ignore("EC-2640: 42601: missing \";\" at end of SQL statement")]
    public void UsingPackagesWithUserDefinedTypesTest()
    {
        using var conn = OpenConnection();
        //The following anonymous block runs function and procedures in package emp_rpt.
        //In the anonymous block's declaration section, note the declaration of cursor
        //variable v_emp_cur using the package’s public REF CURSOR type, EMP_REFCUR.v_emp_cur
        //contains the pointer to the result set that's passed between the package
        //function and procedures.
        var sql = "DECLARE\n"
            + "    v_deptno dept.deptno%TYPE DEFAULT 30;\n"
            + "    v_emp_cur emp_rpt.EMP_REFCUR;\n"
            + "BEGIN\n"
            + "    v_emp_cur := emp_rpt.open_emp_by_dept(v_deptno);\n"
            + "    DBMS_OUTPUT.PUT_LINE('EMPLOYEES IN DEPT #' || v_deptno ||\n"
            + "        ': ' || emp_rpt.get_dept_name(v_deptno));\n"
            + "    emp_rpt.fetch_emp(v_emp_cur);\n"
            + "    DBMS_OUTPUT.PUT_LINE('**********************');\n"
            + "    DBMS_OUTPUT.PUT_LINE(v_emp_cur%ROWCOUNT || ' rows were retrieved');\n"
            + "    emp_rpt.close_refcur(v_emp_cur);\n"
            + "END;";

        var mre = new ManualResetEvent(false);
        var notices = new ArrayList();
        void action(object sender, EDBNoticeEventArgs args)
        {
            notices.Add(args.Notice);
            mre.Set();
        }
        conn.Notice += action;
        try
        {
            try
            {
                using var com = new EDBCommand(sql, conn);
                com.CommandType = CommandType.Text;
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            mre.WaitOne(5000);
            Assert.AreEqual(DEPT_EMP_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(DEPT_EMP_RESULT[i], notice!.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }

    [Test]
    [Ignore("EC-2640: 42601: missing \";\" at end of SQL statement")]
    public void UsingPackagesWithUserDefinedTypesRecordVariableTest()
    {
        using var conn = OpenConnection();
        //The following anonymous block shows another way to achieve the same result.
        //Instead of using the package procedures fetch_emp and close_refcur, the logic
        //of these programs is coded directly into the anonymous block. In the anonymous
        //block’s declaration section, note the addition of record variable r_emp,
        //declared using the package’s public record type, EMPREC_TYP.
        var sql = "DECLARE\n"
            + "    v_deptno     dept.deptno%TYPE DEFAULT 30;\n"
            + "    v_emp_cur    emp_rpt.EMP_REFCUR;\n"
            + "    r_emp        emp_rpt.EMPREC_TYP;\n"
            + "BEGIN\n"
            + "    v_emp_cur := emp_rpt.open_emp_by_dept(v_deptno);\n"
            + "    DBMS_OUTPUT.PUT_LINE('EMPLOYEES IN DEPT #' || v_deptno ||\n"
            + "        ': ' || emp_rpt.get_dept_name(v_deptno));\n"
            + "    DBMS_OUTPUT.PUT_LINE('EMPNO ENAME');\n"
            + "    DBMS_OUTPUT.PUT_LINE('----- -------');\n"
            + "    LOOP\n"
            + "        FETCH v_emp_cur INTO r_emp;\n"
            + "        EXIT WHEN v_emp_cur%NOTFOUND;\n"
            + "        DBMS_OUTPUT.PUT_LINE(r_emp.empno || '  ' ||\n"
            + "            r_emp.ename);\n"
            + "    END LOOP;\n" + "    DBMS_OUTPUT.PUT_LINE('**********************');\n"
            + "    DBMS_OUTPUT.PUT_LINE(v_emp_cur%ROWCOUNT || ' rows were retrieved');\n"
            + "    CLOSE v_emp_cur;\n"
            + "END;";

        var mre = new ManualResetEvent(false);
        var notices = new ArrayList();
        void action(object sender, EDBNoticeEventArgs args)
        {
            notices.Add(args.Notice);
            mre.Set();
        }
        conn.Notice += action;
        try
        {
            try
            {
                using var com = new EDBCommand(sql, conn);
                com.CommandType = CommandType.Text;
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            mre.WaitOne(5000);
            Assert.AreEqual(DEPT_EMP_RESULT.Length, notices.Count);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Assert.AreEqual(DEPT_EMP_RESULT[i], notice!.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }

    [Test, Timeout(1500)]
    public void UsingPackagesWithUserDefinedTypesFunctionCallTest()
    {
        using var conn = OpenConnection();
        //The following code called function emp_rpt.open_emp_by_dept and
        //and returned cusor emp_rpt.EMP_REFCUR as ResultSet
        var tran = conn.BeginTransaction();
        var commandText = "emp_rpt.open_emp_by_dept(:param1)";

        var cstmt = new EDBCommand(commandText, conn)
        {
            CommandType = CommandType.StoredProcedure,
            Transaction = tran
        };

        cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
            ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 30));

        cstmt.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Refcursor, 10, "ret",
            ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null!));

        cstmt.Prepare();
        cstmt.ExecuteNonQuery();

        var cursorName = cstmt.Parameters[1].Value!.ToString();
        cstmt.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
        cstmt.CommandType = CommandType.Text;
        var rst = cstmt.ExecuteReader(CommandBehavior.SequentialAccess);

        var list = GetResultSetData(rst);

        Assert.IsNotNull(rst);

        for (var i = 0; i < list.Count; i++)
        {
            Assert.AreEqual(EMP_LIST_RESULT[i], list[i]);
        }
        rst.Close();
        tran.Commit();
    }
}
