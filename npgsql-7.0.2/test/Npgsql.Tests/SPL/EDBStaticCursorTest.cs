using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;
using System.Threading;
using System.Collections;
using static System.Collections.Specialized.BitVector32;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2579: Regression Tests for Static Cursors in SPL

//Port JDBC tests to .NET from enhancements\spl\BasicStatementTest.java
namespace EnterpriseDB.EDBClient.Tests.SPL
{
    internal class EDBStaticCursorTest : TestBase
    {
        EDBConnection? conn = null;

        private static int EMPNO = 7369;
        private static string ENAME = "SMITH";
        private static int EMP_COUNT = 14;
        private static string[] EMP_DEPTS = {
            "SMITH works in department 20",
            "ALLEN works in department 30",
            "WARD works in department 30",
            "JONES works in department 20",
            "MARTIN works in department 30",
            "BLAKE works in department 30",
            "CLARK works in department 10",
            "SCOTT works in department 20",
            "KING works in department 10",
            "TURNER works in department 30",
            "ADAMS works in department 20",
            "JAMES works in department 30",
            "FORD works in department 20",
            "MILLER works in department 10" };
        private static string[] EMP_NAMES = {
            "7369 SMITH",
            "7499 ALLEN",
            "7521 WARD",
            "7566 JONES",
            "7654 MARTIN",
            "7698 BLAKE",
            "7782 CLARK",
            "7788 SCOTT",
            "7839 KING",
            "7844 TURNER",
            "7876 ADAMS",
            "7900 JAMES",
            "7902 FORD",
            "7934 MILLER" };
        private static string[] EMP_SALARIES = {
            "Name = SMITH, salary = 800.00",
            "Name = ALLEN, salary = 1600.00",
            "Name = WARD, salary = 1250.00",
            "Name = MARTIN, salary = 1250.00",
            "Name = TURNER, salary = 1500.00",
            "Name = ADAMS, salary = 1100.00",
            "Name = JAMES, salary = 950.00",
            "Name = MILLER, salary = 1300.00" };

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();

            Execute("DROP PROCEDURE fetching_rows;");
            Execute("DROP PROCEDURE fetching_rows_variable_type;");
            Execute("DROP PROCEDURE fetching_rows_record_rowtype;");
            Execute("DROP PROCEDURE using_rowtype_with_cursors;");
            Execute("DROP PROCEDURE cursor_attribute_isopen;");
            Execute("DROP PROCEDURE cursor_attribute_found;");
            Execute("DROP PROCEDURE cursor_attribute_not_found;");
            Execute("DROP PROCEDURE cursor_attribute_count;");
            Execute("DROP PROCEDURE cursor_for_loop;");

            Execute("DROP TABLE emp1 CASCADE");

            Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(10), sal NUMBER(10,2), deptno NUMBER(4))");

            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7369,'SMITH',800,20)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7499,'ALLEN',1600,30)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7521,'WARD',1250,30)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7566,'JONES',2975,20)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7654,'MARTIN',1250,30)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7698,'BLAKE',2850,30)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7782,'CLARK',2450,10)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7788,'SCOTT',3000,20)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7839,'KING',5000,10)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7844,'TURNER',1500,30)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7876,'ADAMS',1100,20)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7900,'JAMES',950,30)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7902,'FORD',3000,20)");
            Execute("INSERT INTO emp1(empno,ename,sal,deptno) VALUES(7934,'MILLER',1300,10)");

            //The following shows the FETCH statement.
            var fetchingRows = "CREATE OR REPLACE PROCEDURE fetching_rows( \n"
                             + "   v_empno   OUT     NUMBER(4), \n"
                             + "   v_ename   OUT     VARCHAR2(10)) \n"
                             + "IS\n"
                             + "    CURSOR emp_cur_3 IS SELECT empno, ename FROM emp1 \n"
                             + "        ORDER BY empno;\n" + "BEGIN\n"
                             + "    OPEN emp_cur_3;\n"
                             + "    FETCH emp_cur_3 INTO v_empno, v_ename;\n"
                             + "    CLOSE emp_cur_3;\n"
                             + "END;";
            Execute(fetchingRows);

            //Instead of explicitly declaring the data type of a target variable, you can
            //use %TYPE instead. In this way, if the data type of the database column changes,
            //the target variable declaration in the SPL program doesn't have to change.
            //%TYPE picks up the new data type of the specified column.
            var fetchingRowsVariableType = "CREATE OR REPLACE PROCEDURE fetching_rows_variable_type(\n"
                             + "    v_empno  OUT   emp1.empno%TYPE,\n"
                             + "    v_ename  OUT   emp1.ename%TYPE)\n"
                             + "IS\n"
                             + "    CURSOR emp_cur_3 IS SELECT empno, ename FROM emp1\n"
                             + "        ORDER BY empno;\n"
                             + "BEGIN\n"
                             + "    OPEN emp_cur_3;\n"
                             + "    FETCH emp_cur_3 INTO v_empno, v_ename;\n"
                             + "    CLOSE emp_cur_3;\n"
                             + "END;";
            Execute(fetchingRowsVariableType);

            //If all the columns in a table are retrieved in the order defined in the table,
            //you can use %ROWTYPE to define a record into which the FETCH statement places
            //the retrieved data. You can then access each field in the record using dot notation.
            var fetchingRowsRecordRowtype = "CREATE OR REPLACE PROCEDURE fetching_rows_record_rowtype(\n"
                                + "    v_emp_rec   OUT emp1%ROWTYPE)\n"
                                + "IS\n"
                                + "    CURSOR emp_cur_1 IS SELECT * FROM emp1 order by empno;\n"
                                + "BEGIN\n"
                                + "    OPEN emp_cur_1;\n"
                                + "    FETCH emp_cur_1 INTO v_emp_rec;\n"
                                + "    CLOSE emp_cur_1;\n"
                                + "END;";
            Execute(fetchingRowsRecordRowtype);

            //Using the %ROWTYPE attribute, you can define a record that contains fields
            //corresponding to all columns fetched from a cursor or cursor variable.
            //The %ROWTYPE attribute is prefixed by a cursor name or cursor variable name.
            //This example shows how you can use a cursor with %ROWTYPE to get information about
            //which employee works in which department
            var usingRowtypeWithCursors = "CREATE OR REPLACE PROCEDURE using_rowtype_with_cursors\n"
                           + "IS\n"
                           + "    CURSOR empcur IS SELECT ename, deptno FROM emp1;\n"
                           + "    myvar           empcur%ROWTYPE;\n"
                           + "BEGIN\n"
                           + "    OPEN empcur;\n"
                           + "    LOOP\n"
                           + "        FETCH empcur INTO myvar;\n"
                           + "        EXIT WHEN empcur%NOTFOUND;\n"
                           + "        DBMS_OUTPUT.PUT_LINE( myvar.ename || ' works in department '\n"
                           + "            || myvar.deptno );\n"
                           + "    END LOOP;\n"
                           + "    CLOSE empcur;\n"
                           + "END;";
            Execute(usingRowtypeWithCursors);

            //Use the %ISOPEN attribute to test whether a cursor is open.
            var cursorAttributeIsopen = "CREATE OR REPLACE PROCEDURE cursor_attribute_isopen(\n"
                    + " v_beforOpen  OUT Boolean,"
                    + " v_afterOpen  OUT Boolean,"
                    + " v_afterClose OUT Boolean)\n"
                    + "IS\n"
                    + "    CURSOR emp_cur_1 IS SELECT * FROM emp1;\n"
                    + "BEGIN\n"
                    + "    v_beforOpen := emp_cur_1%ISOPEN; \n"
                    + "    OPEN emp_cur_1;\n"
                    + "    v_afterOpen := emp_cur_1%ISOPEN; \n"
                    + "    CLOSE emp_cur_1;\n"
                    + "    v_afterClose := emp_cur_1%ISOPEN; \n"
                    + "END;";
            Execute(cursorAttributeIsopen);

            //The %FOUND attribute tests whether a row is retrieved from the result set of
            //the specified cursor after a FETCH on the cursor.
            var cursorAttributeFound = "CREATE OR REPLACE PROCEDURE cursor_attribute_found\n"
                            + "IS\n"
                            + "    v_emp_rec       emp1%ROWTYPE;\n"
                            + "    CURSOR emp_cur_1 IS SELECT * FROM emp1;\n"
                            + "BEGIN\n"
                            + "    OPEN emp_cur_1;\n"
                            + "    FETCH emp_cur_1 INTO v_emp_rec;\n"
                            + "    WHILE emp_cur_1%FOUND LOOP\n"
                             + "        DBMS_OUTPUT.PUT_LINE(v_emp_rec.empno || ' ' || v_emp_rec.ename);\n"
                            + "        FETCH emp_cur_1 INTO v_emp_rec;\n"
                             + "    END LOOP;\n"
                            + "    CLOSE emp_cur_1;\n"
                             + "END;";
            Execute(cursorAttributeFound);

            //The %NOTFOUND attribute is the logical opposite of %FOUND.
            var cursorAttributeNotFound = "CREATE OR REPLACE PROCEDURE cursor_attribute_not_found\n"
                               + "IS\n"
                               + "    v_emp_rec       emp1%ROWTYPE;\n"
                               + "    CURSOR emp_cur_1 IS SELECT * FROM emp1;\n"
                               + "BEGIN\n"
                               + "    OPEN emp_cur_1;\n"
                               + "    LOOP\n"
                               + "        FETCH emp_cur_1 INTO v_emp_rec;\n"
                               + "        EXIT WHEN emp_cur_1%NOTFOUND;\n"
                               + "        DBMS_OUTPUT.PUT_LINE(v_emp_rec.empno || ' ' || v_emp_rec.ename);\n"
                               + "    END LOOP;\n"
                               + "    CLOSE emp_cur_1;\n"
                               + "END;";
            Execute(cursorAttributeNotFound);

            //The %ROWCOUNT attribute returns an integer showing the number of
            //rows fetched so far from the specified cursor.
            var cursorAttributeCount = "CREATE OR REPLACE PROCEDURE cursor_attribute_count(\n"
                               + "   v_count  OUT Number)\n"
                               + "IS\n"
                               + "   v_emp_rec   emp1%ROWTYPE;\n"
                               + "   CURSOR emp_cur_1 IS SELECT * FROM emp1;\n"
                               + "BEGIN\n"
                               + "   OPEN emp_cur_1;\n"
                               + "      LOOP\n"
                               + "        FETCH emp_cur_1 INTO v_emp_rec;\n"
                               + "        EXIT WHEN emp_cur_1%NOTFOUND;\n"
                               + "      END LOOP;\n"
                               + "        v_count := emp_cur_1%ROWCOUNT;\n"
                               + "   CLOSE emp_cur_1;\n"
                               + "END;";
            Execute(cursorAttributeCount);

            //The cursor FOR loop is a loop construct that eliminates
            //the need to individually code these statements.
            var cursorForLoop = "CREATE OR REPLACE PROCEDURE cursor_for_loop\n"
                           + "IS\n"
                           + "    CURSOR emp_cur_1 IS SELECT * FROM emp1 order by empno;\n"
                           + "BEGIN\n"
                           + "    FOR v_emp_rec IN emp_cur_1 LOOP\n"
                           + "        DBMS_OUTPUT.PUT_LINE(v_emp_rec.empno || ' ' || v_emp_rec.ename);\n"
                           + "    END LOOP;\n"
                           + "END;";
            Execute(cursorForLoop);

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
        public void FetchingRowsTest()
        {
            var command = "fetching_rows(:param1,:param2)";

            var cstmt = new EDBCommand(command, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 0));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var empno = int.Parse(cstmt.Parameters[0].Value.ToString());
            Assert.AreEqual(EMPNO, empno);
            var name = cstmt.Parameters[1].Value.ToString();
            Assert.AreEqual(ENAME, name);
        }

        [Test]
        public void FetchingRowsVariableTypeTest()
        {
            var command = "fetching_rows_variable_type(:param1,:param2)";

            var cstmt = new EDBCommand(command, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 0));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var empno = int.Parse(cstmt.Parameters[0].Value.ToString());
            Assert.AreEqual(EMPNO, empno);
            var name = cstmt.Parameters[1].Value.ToString();
            Assert.AreEqual(ENAME, name);
        }

        [Test]
        [Ignore("EC-2633")]
        public void FetchingRowsRecordRowtypeTest()
        {
            var command = "fetching_rows_record_rowtype";

            var cstmt = new EDBCommand(command, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.DeriveParameters();

            //cstmt.Prepare();
            //cstmt.ExecuteNonQuery();
        }

        [Test]
        public void UsingRowtypeWithCursorsTest()
        {
            var sqlStr = "using_rowtype_with_cursors";

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

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_DEPTS.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_DEPTS[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void CursorAttributeIsopenTest()
        {
            var command = "cursor_attribute_isopen(:param1, :param2, :param3)";

            var cstmt = new EDBCommand(command, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Boolean, 10, "param1",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Boolean, 10, "param2",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Boolean, 10, "param3",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var beforOpen = cstmt.Parameters[0].Value.ToString().StartsWith('t') ? true : false;
            var afterOpen = cstmt.Parameters[1].Value.ToString().StartsWith('t') ? true : false;
            var afterClose = cstmt.Parameters[2].Value.ToString().StartsWith('t') ? true : false;

            Assert.IsFalse(beforOpen);
            Assert.IsTrue(afterOpen);
            Assert.IsFalse(afterClose);
        }

        [Test]
        public void CursorAttributeFoundTest()
        {
            var sqlStr = "cursor_attribute_found";

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

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_NAMES.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_NAMES[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void CursorAttributeNotFoundTest()
        {
            var sqlStr = "cursor_attribute_not_found";

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

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_NAMES.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_NAMES[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        public void CursorAttributeRowCountTest()
        {
            var command = "cursor_attribute_count(:param1)";

            var cstmt = new EDBCommand(command, conn);
            cstmt.CommandType = CommandType.StoredProcedure;

            cstmt.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1",
                ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

            cstmt.Prepare();
            cstmt.ExecuteNonQuery();

            var empno = int.Parse(cstmt.Parameters[0].Value.ToString());
            Assert.AreEqual(EMP_COUNT, empno);
        }

        [Test]
        public void CursorForLoopTest()
        {
            var sqlStr = "cursor_for_loop";

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

                    cstmt.Prepare();
                    cstmt.ExecuteNonQuery();
                }
                mre.WaitOne(5000);
                Assert.AreEqual(EMP_NAMES.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_NAMES[i], notice.MessageText);
                }
            }
            finally
            {
                conn.Notice -= action;
            }
            mre.Close();
        }

        [Test]
        [Ignore("EC-2638: syntax error at or near \"my_record\"")]
        public void ParameterizedCursorTest()
        {
            //You can declare a static cursor that accepts parameters and can pass values
            //for those parameters when opening that cursor. This example creates a
            //parameterized cursor that displays the name and salary of all employees from
            //the emp table that have a salary less than a specified value. This information
            //is passed as a parameter.
            var sqlStr = "DECLARE\n"
                      + "    my_record       emp%ROWTYPE;\n"
                      + "    CURSOR c1 (max_wage NUMBER) IS\n"
                      + "        SELECT * FROM emp WHERE sal < max_wage;\n"
                      + "BEGIN\n"
                      + "    OPEN c1(2000);\n"
                      + "    LOOP\n"
                      + "        FETCH c1 INTO my_record;\n"
                      + "        EXIT WHEN c1%NOTFOUND;\n"
                      + "        DBMS_OUTPUT.PUT_LINE('Name = ' || my_record.ename || ', salary = '\n"
                      + "            || my_record.sal);\n"
                      + "    END LOOP;\n"
                      + "    CLOSE c1;\n"
                      + "END;";

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
                Assert.AreEqual(EMP_SALARIES.Length, notices.Count);
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(EMP_SALARIES[i], notice.MessageText);
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
