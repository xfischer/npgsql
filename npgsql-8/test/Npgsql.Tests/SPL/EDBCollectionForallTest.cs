using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2574: Regression Tests for Working with Collections in SPL

//These tests are implemented in JDBC as Anonymous block.
//We have implemented them in stored procedures because anonymous blocks do not work in .NET.

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    [NonParallelizable]
    internal class EDBCollectionForallTest : EPASTestBase
    {
        EDBConnection? conn = null;
                
        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            TestUtil.dropTable(conn, "emp1 CASCADE");
            TestUtil.dropTable(conn, "emp_copy CASCADE");

            Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(20), job VARCHAR2(20), sal NUMBER(10,2), deptno NUMBER(4))");
            var addEmp = new string[] {
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(9001,'SMITH','SOFTWARE ENGINEER',800,20)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(9002,'ALLEN','SALESMAN',1600,30)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(9003,'WARD','SALESMAN',1250,30)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(9004,'JONES','SOFTWARE ENGINEER',2975,20)",
                "INSERT INTO emp1(empno,ename,job,sal,deptno) VALUES(9005,'MARTIN','SALESMAN',1250,30)" };
            for (var i = 0; i < addEmp.Length; i++)
            {
                Execute(addEmp[i]);
            }
            Execute("CREATE TABLE emp_copy(LIKE emp1);");
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

        private double getEmpSalary(int empno)
        {
            var command = "select sal from emp1 where empno=" + empno;

            var seletCommand = new EDBCommand(command, conn);
            EDBDataReader selectResult = seletCommand.ExecuteReader();
            selectResult.Read();

            var sal = selectResult.GetDouble(0);

            selectResult.Close();

            return sal;
        }

        private int getEmpCount()
        {
            var command = "select count(*) from emp1";

            var seletCommand = new EDBCommand(command, conn);
            EDBDataReader selectResult = seletCommand.ExecuteReader();
            selectResult.Read();

            var count = selectResult.GetInt32(0);

            selectResult.Close();

            return count;
        }

        [Test]
        public void EmpCopyTest()
        {
            //t_emp is an associative array of type emp_tbl. The SELECT statement uses the BULK
            //COLLECT INTO command to populate the t_emp array. After the t_emp array is
            //populated, the FORALL statement iterates through the values (i) in the t_emp array
            //index and inserts a row for each record into emp_copy.
            Execute("DROP PROCEDURE EmpCopy_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE EmpCopy_SP()\n"
                + " IS\n"
                + " DECLARE\n"
                + "    TYPE emp_tbl IS TABLE OF emp1%ROWTYPE INDEX BY BINARY_INTEGER;\n"
                + "    t_emp emp_tbl;\n"
                + "BEGIN\n"
                + "    SELECT * FROM emp1 BULK COLLECT INTO t_emp;\n"
                + "    FORALL i IN t_emp.FIRST .. t_emp.LAST\n"
                + "     INSERT INTO emp_copy VALUES t_emp(i);\n"
                + "END;";

            //Create SP.
            Execute(sqlStr);

            //Execute SP.
            using (var cstmt = new EDBCommand("EmpCopy_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            var count = getEmpCount();
            Assert.AreEqual(5, count);
        }

        [Test]
        public void UpdateTest()
        {
            //This example uses a FORALL statement to update the salary of three employees.
            Execute("DROP PROCEDURE Update_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE Update_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                      + "    TYPE empno_tbl  IS TABLE OF emp1.empno%TYPE INDEX BY BINARY_INTEGER;\n"
                      + "    TYPE sal_tbl    IS TABLE OF emp1.ename%TYPE INDEX BY BINARY_INTEGER;\n"
                      + "    t_empno         EMPNO_TBL;\n"
                      + "    t_sal           SAL_TBL;\n"
                      + "BEGIN\n"
                      + "    t_empno(1)  := 9001;\n"
                      + "    t_sal(1)    := 3350.00;\n"
                      + "    t_empno(2)  := 9002;\n"
                      + "    t_sal(2)    := 2000.00;\n"
                      + "    t_empno(3)  := 9003;\n"
                      + "    t_sal(3)    := 4100.00;\n"
                      + "    FORALL i IN t_empno.FIRST..t_empno.LAST\n"
                      + "        UPDATE emp1 SET sal = t_sal(i) WHERE empno = t_empno(i);\n"
                      + "END;";

            //Create SP.
            Execute(sqlStr);

            //Execute SP.
            using (var cstmt = new EDBCommand("Update_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            Assert.AreEqual(3350.00, getEmpSalary(9001), 0.01);
            Assert.AreEqual(2000.00, getEmpSalary(9002), 0.01);
            Assert.AreEqual(4100.00, getEmpSalary(9003), 0.01);
        }

        [Test]
        public void DeleteTest()
        {
            //This example deletes three employees in a FORALL statement:
            Execute("DROP PROCEDURE Delete_SP;");

            var sqlStr = "CREATE OR REPLACE PROCEDURE Delete_SP()\n"
                      + " IS\n"
                      + " DECLARE\n"
                                  + "    TYPE empno_tbl  IS TABLE OF emp1.empno%TYPE INDEX BY BINARY_INTEGER;\n"
                      + "    t_empno         EMPNO_TBL;\n"
                      + "BEGIN\n"
                      + "    t_empno(1)  := 9001;\n"
                      + "    t_empno(2)  := 9002;\n"
                      + "    t_empno(3)  := 9003;\n"
                      + "    FORALL i IN t_empno.FIRST..t_empno.LAST\n"
                      + "        DELETE FROM emp1 WHERE empno = t_empno(i);\n"
                      + "END;";

            //Create SP.
            Execute(sqlStr);

            //Execute SP.
            using (var cstmt = new EDBCommand("Delete_SP", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            var count = getEmpCount();
            Assert.AreEqual(2, count);
        }
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

