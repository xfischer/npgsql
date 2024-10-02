using EDBTypes;
using EnterpriseDB.EDBClient.Tests.Support;
using Npgsql.Tests.Support;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2591: Regression Tests for Nested Tables

namespace EnterpriseDB.EDBClient.Tests.SPL;

[NonParallelizable]
internal class EDBNestedTableTest : EPASTestBase
{

    public class dept_obj_typ
    {
        public string? dname;
        public string? loc;
    }

    public class emp_rec_typ
    {
        public decimal empno;
        public string? ename;
    }

    public class JointComposite
    {
        public emp_rec_typ emp;
        public dept_obj_typ loc;
    }

    enum Mood { Sad, Ok, Happy }

    private static string[] enames = new string[] { "SMITH", "ALLEN", "WARD", "JONES", "MARTIN",
        "BLAKE", "CLARK", "SCOTT", "KING", "TURNER" };
    private static decimal[] empnos = new decimal[] { 7369, 7499, 7521, 7566, 7654, 7698, 7782, 7788, 7839, 7844 };
    private static string[] deptNames = new string[] { "ACCOUNTING", "OPERATIONS", "RESEARCH", "SALES" };
    private static string[] deptLocs = new string[] { "NEW YORK", "BOSTON", "DALLAS", "CHICAGO" };
    //private static string[] deptLocs = new string[] { "T(_)_,_", "NEW YORK", "BOSTON", "DALLAS" };
    private static int EMP_TOTAL = enames.Length;
    private static int DEPT_TOTAL = deptNames.Length;

    [OneTimeSetUp]
    public void Init()
    {
        TearDown();

        using var conn = OpenConnection();

        Execute("CREATE TABLE emp1(empno NUMBER(4),  ename VARCHAR2(10))", true);
        for (var i = 0; i < EMP_TOTAL; i++)
        {
            var addCommand = "INSERT INTO emp1(empno,ename) VALUES(:empno, :ename)";

            using (var cstmt = new EDBCommand(addCommand, conn))
            {
                cstmt.CommandType = CommandType.Text;
                cstmt.Parameters.Add(new EDBParameter("empno", EDBTypes.EDBDbType.Numeric, 10, "empno",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, empnos[i]));

                cstmt.Parameters.Add(new EDBParameter("ename", EDBTypes.EDBDbType.Varchar, 10, "ename",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, enames[i]));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
        }

        Execute("CREATE TABLE dept1(dname VARCHAR2(14),  loc  VARCHAR2(13))", true);
        var createTypeSql = "CREATE TYPE dept_obj_typ AS OBJECT (\n"
                             + " dname   VARCHAR2(14),\n"
                             + " loc     VARCHAR2(13)\n"
                             + ");";
        Execute(createTypeSql, true);

        for (var i = 0; i < DEPT_TOTAL; i++)
        {
            var addDeptCommand = "INSERT INTO dept1(dname,loc) VALUES(:dname,:loc)";

            using (var cstmt = new EDBCommand(addDeptCommand, conn))
            {
                cstmt.CommandType = CommandType.Text;
                cstmt.Parameters.Add(new EDBParameter("dname", EDBTypes.EDBDbType.Varchar, 10, "dname",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, deptNames[i]));

                cstmt.Parameters.Add(new EDBParameter("loc", EDBTypes.EDBDbType.Varchar, 10, "loc",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, deptLocs[i]));

                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }
        }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Execute("DROP type dept_obj_typ");

        using var conn = OpenConnection();
        TestUtil.dropTable(conn, "emp1 CASCADE");
        TestUtil.dropTable(conn, "dept1 CASCADE");
    }

    private int Execute(string query, bool shouldPass = false)
    {
        try
        {

            using var conn = OpenConnection();
            using (var com = new EDBCommand(query, conn))
            {
                com.CommandType = CommandType.Text;
                return com.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            if (shouldPass)
                Assert.Fail(ex.Message);
        }

        return 0;
    }

    [Test]
    public async Task SimpleNestedTableTest_ArrayList([Values] bool deriveParameters)
    {
        try
        {
            //A procedure has an output parameter which is nested table
            // with four elements.
            var createPkg = " CREATE OR REPLACE PACKAGE pkgSimpleTest Is \n"
                    + "   TYPE dname_tbl_typ IS TABLE OF VARCHAR2(14); \n"
                    + "   Procedure simpleNestedTableTest(dname_tbl Out dname_tbl_typ); \n"
                    + " End pkgSimpleTest; ";
            Execute(createPkg, true);

            var pkgBody = " CREATE OR REPLACE PACKAGE BODY pkgSimpleTest \n"
                           + "  Is \n"
                           + "  Procedure simpleNestedTableTest(dname_tbl Out dname_tbl_typ) \n"
                           + "    Is \n"
                           + "    DECLARE \n"
                           + "     CURSOR dept_cur IS SELECT dname FROM dept1 ORDER BY dname; \n"
                           + "     i INTEGER := 0; \n"
                           + "    BEGIN\n"
                           + "      dname_tbl := dname_tbl_typ(NULL, NULL, NULL, NULL); \n"
                           + "      FOR r_dept IN dept_cur LOOP \n"
                           + "        i := i + 1; \n"
                           + "        dname_tbl(i) := r_dept.dname; \n"
                           + "      END LOOP; \n"
                           + "  End simpleNestedTableTest; \n"
                           + " End pkgSimpleTest;";
            Execute(pkgBody, true);

            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgsimpletest.dname_tbl_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgSimpleTest.simpleNestedTableTest";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            if (deriveParameters)
            {
                cstmt.DeriveParameters();
            }
            else
            {
                var tableOfParam = cstmt.Parameters.Add(new EDBParameter()
                {
                    Direction = ParameterDirection.Output,
                    DataTypeName = "pkgsimpletest.dname_tbl_typ"
                });
            }

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgsimpletest.dname_tbl_typ", cstmt.Parameters[0].DataTypeName);


            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = cstmt.Parameters[0].Value;

            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(DEPT_TOTAL, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(string));

            for (int i = 0; i < DEPT_TOTAL; i++)
            {
                Assert.AreEqual(deptNames[i], arrayList[i]);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgSimpleTest;");
            Execute("DROP PACKAGE pkgSimpleTest;");
        }
    }

    [Test]
    public async Task SimpleNestedTableTest_Integer_ArrayList([Values] bool deriveParameters)
    {
        try
        {

            //A procedure has an output parameter which is nested table
            // with four elements.
            var createPkg = " CREATE OR REPLACE PACKAGE pkgSimpleTestInt Is \n"
                    + "   TYPE dname_int_tbl_typ IS TABLE OF INT; \n"
                    + "   Procedure simpleNestedTableTestInt(dname_tbl Out dname_int_tbl_typ); \n"
                    + " End pkgSimpleTestInt; ";
            Execute(createPkg, true);

            var pkgBody = " CREATE OR REPLACE PACKAGE BODY pkgSimpleTestInt \n"
                           + "  Is \n"
                           + "  Procedure simpleNestedTableTestInt(dname_tbl Out dname_int_tbl_typ) \n"
                           + "    Is \n"
                           + "    DECLARE \n"
                           + "     CURSOR emp_cur IS SELECT empno FROM emp1 ORDER BY empno; \n"
                           + "     i INTEGER := 0; \n"
                           + "    BEGIN\n"
                           + "      dname_tbl := dname_int_tbl_typ(NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL); \n"
                           + "      FOR r_emp IN emp_cur LOOP \n"
                           + "        i := i + 1; \n"
                           + "        dname_tbl(i) := r_emp.empno; \n"
                           + "      END LOOP; \n"
                           + "  End simpleNestedTableTestInt; \n"
                           + " End pkgSimpleTestInt;";
            Execute(pkgBody, true);

            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgsimpletestint.dname_int_tbl_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgsimpletestint.simpleNestedTableTestInt";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            if (deriveParameters)
            {
                cstmt.DeriveParameters();
            }
            else
            {
                var tableOfParam = cstmt.Parameters.Add(new EDBParameter()
                {
                    Direction = ParameterDirection.Output,
                    DataTypeName = "pkgsimpletestint.dname_int_tbl_typ"
                });
            }

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgsimpletestint.dname_int_tbl_typ", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = cstmt.Parameters[0].Value;

            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(EMP_TOTAL, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(int));

            for (int i = 0; i < EMP_TOTAL; i++)
            {
                Assert.AreEqual(arrayList[i].GetType(), typeof(int));
                Assert.AreEqual(empnos[i], arrayList[i]);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgSimpleTestInt;");
            Execute("DROP PACKAGE pkgSimpleTestInt;");
        }
    }

    [Test]
    public async Task NestedTableExtendTest_Composite() //([Values] bool deriveParameters)
    {
        try
        {


            //the creation of an empty table with the constructor emp_tbl_typ()
            //as the first statement in the executable section of the anonymous block.
            //The EXTEND collection method is then used to add an element to the
            //table for each employee returned from the result set.
            var createPkg = " CREATE OR REPLACE PACKAGE pkgExtendTest Is \n"
                             + "   TYPE emp_rec_typ IS RECORD ( \n"
                             + "      empno  NUMBER(4), \n"
                             + "      ename       VARCHAR2(10) \n"
                             + "     );\n"
                             + "   TYPE emp_tbl_typ IS TABLE OF emp_rec_typ; \n"
                             + "   Procedure nestedTableExtendTest(emp_tbl Out emp_tbl_typ); "
                             + " End pkgExtendTest;";
            Execute(createPkg, true);

            var pkgBody = " CREATE OR REPLACE PACKAGE BODY pkgExtendTest \n"
                           + " Is \n"
                           + "  Procedure nestedTableExtendTest(emp_tbl Out emp_tbl_typ) \n "
                           + "   Is \n"
                           + "   DECLARE \n"
                           + "      CURSOR emp_cur IS SELECT empno, ename FROM emp1 WHERE ROWNUM <= 10 order by empno; \n"
                           + "      i  INTEGER := 0; \n"
                           + "   BEGIN\n"
                           + "    emp_tbl := emp_tbl_typ(); \n"
                           + "    FOR r_emp IN emp_cur LOOP \n"
                           + "        i := i + 1; \n"
                           + "        emp_tbl.EXTEND; \n"
                           + "        emp_tbl(i) := r_emp; \n"
                           + "    END LOOP; \n"
                           + "  End nestedTableExtendTest; "
                           + " End pkgExtendTest;";
            Execute(pkgBody, true);


            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgextendtest.emp_tbl_typ");
            dataSourceBuilder.MapComposite<emp_rec_typ>("pkgextendtest.emp_rec_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgExtendTest.nestedTableExtendTest";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            EDBParameter? tableOfParam = null;
            //if (deriveParameters)
            //{
            //    cstmt.DeriveParameters();
            //}
            //else
            //{
            tableOfParam = cstmt.Parameters.Add(new EDBParameter()
            {
                Direction = ParameterDirection.Output,
                DataTypeName = "pkgextendtest.emp_tbl_typ"
            });
            //}

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgextendtest.emp_tbl_typ", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = tableOfParam.Value;
            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(10, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(emp_rec_typ));

            for (int i = 0; i < arrayList.Count; i++)
            {
                emp_rec_typ tuple = (emp_rec_typ)arrayList[i]!;

                Assert.AreEqual(empnos[i], tuple.empno);
                Assert.AreEqual(enames[i], tuple.ename);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgExtendTest;");
            Execute("DROP PACKAGE pkgExtendTest;");
        }
    }

    [Test]
    public async Task NestedTableExtendTest_ArrayList() //([Values] bool deriveParameters)
    {
        try
        {


            //the creation of an empty table with the constructor emp_tbl_typ()
            //as the first statement in the executable section of the anonymous block.
            //The EXTEND collection method is then used to add an element to the
            //table for each employee returned from the result set.
            var createPkg = " CREATE OR REPLACE PACKAGE pkgExtendTest Is \n"
                             + "   TYPE emp_rec_typ IS RECORD ( \n"
                             + "      empno  NUMBER(4), \n"
                             + "      ename       VARCHAR2(10) \n"
                             + "     );\n"
                             + "   TYPE emp_tbl_typ IS TABLE OF emp_rec_typ; \n"
                             + "   Procedure nestedTableExtendTest(emp_tbl Out emp_tbl_typ); "
                             + " End pkgExtendTest;";
            Execute(createPkg, true);

            var pkgBody = " CREATE OR REPLACE PACKAGE BODY pkgExtendTest \n"
                           + " Is \n"
                           + "  Procedure nestedTableExtendTest(emp_tbl Out emp_tbl_typ) \n "
                           + "   Is \n"
                           + "   DECLARE \n"
                           + "      CURSOR emp_cur IS SELECT empno, ename FROM emp1 WHERE ROWNUM <= 10 order by empno; \n"
                           + "      i  INTEGER := 0; \n"
                           + "   BEGIN\n"
                           + "    emp_tbl := emp_tbl_typ(); \n"
                           + "    FOR r_emp IN emp_cur LOOP \n"
                           + "        i := i + 1; \n"
                           + "        emp_tbl.EXTEND; \n"
                           + "        emp_tbl(i) := r_emp; \n"
                           + "    END LOOP; \n"
                           + "  End nestedTableExtendTest; "
                           + " End pkgExtendTest;";
            Execute(pkgBody, true);


            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgextendtest.emp_tbl_typ");
            //dataSourceBuilder.MapComposite<emp_rec_typ>("pkgextendtest.emp_rec_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgExtendTest.nestedTableExtendTest";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            EDBParameter? tableOfParam = null;
            //if (deriveParameters)
            //{
            //    cstmt.DeriveParameters();
            //}
            //else
            //{
            tableOfParam = cstmt.Parameters.Add(new EDBParameter()
            {
                Direction = ParameterDirection.Output,
                DataTypeName = "pkgextendtest.emp_tbl_typ"
            });
            //}

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgextendtest.emp_tbl_typ", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = tableOfParam.Value;
            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(10, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(ArrayList));

            for (int i = 0; i < arrayList.Count; i++)
            {
                ArrayList tuple = (ArrayList)arrayList[i]!;

                Assert.AreEqual(2, tuple.Count);

                Assert.AreEqual(tuple[0].GetType(), typeof(decimal));
                Assert.AreEqual(tuple[1].GetType(), typeof(string));
                Assert.AreEqual(empnos[i], tuple[0]);
                Assert.AreEqual(enames[i], tuple[1]);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgExtendTest;");
            Execute("DROP PACKAGE pkgExtendTest;");
        }
    }

    [Test]
    public async Task NestedTableExtendTest_ArrayList_DeriveParams() //([Values] bool deriveParameters)
    {
        try
        {
            //the creation of an empty table with the constructor emp_tbl_typ()
            //as the first statement in the executable section of the anonymous block.
            //The EXTEND collection method is then used to add an element to the
            //table for each employee returned from the result set.
            var createPkg = " CREATE OR REPLACE PACKAGE pkgExtendTest Is \n"
                             + "   TYPE emp_rec_typ IS RECORD ( \n"
                             + "      empno  NUMBER(4), \n"
                             + "      ename       VARCHAR2(10) \n"
                             + "     );\n"
                             + "   TYPE emp_tbl_typ IS TABLE OF emp_rec_typ; \n"
                             + "   Procedure nestedTableExtendTest(emp_tbl Out emp_tbl_typ); "
                             + " End pkgExtendTest;";
            Execute(createPkg, true);

            var pkgBody = " CREATE OR REPLACE PACKAGE BODY pkgExtendTest \n"
                           + " Is \n"
                           + "  Procedure nestedTableExtendTest(emp_tbl Out emp_tbl_typ) \n "
                           + "   Is \n"
                           + "   DECLARE \n"
                           + "      CURSOR emp_cur IS SELECT empno, ename FROM emp1 WHERE ROWNUM <= 10 order by empno; \n"
                           + "      i  INTEGER := 0; \n"
                           + "   BEGIN\n"
                           + "    emp_tbl := emp_tbl_typ(); \n"
                           + "    FOR r_emp IN emp_cur LOOP \n"
                           + "        i := i + 1; \n"
                           + "        emp_tbl.EXTEND; \n"
                           + "        emp_tbl(i) := r_emp; \n"
                           + "    END LOOP; \n"
                           + "  End nestedTableExtendTest; "
                           + " End pkgExtendTest;";
            Execute(pkgBody, true);

            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgextendtest.emp_tbl_typ");
            //dataSourceBuilder.MapComposite<dept_obj_typ>("pkgobjecttypetest.dept_obj_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgExtendTest.nestedTableExtendTest";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            EDBParameter? tableOfParam = null;
            //if (deriveParameters)
            //{
            cstmt.DeriveParameters();
            //}
            //else
            //{
            //tableOfParam = cstmt.Parameters.Add(new EDBParameter()
            //{
            //    Direction = ParameterDirection.Output,
            //    DataTypeName = "pkgextendtest.emp_tbl_typ"
            //});
            //}

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgextendtest.emp_tbl_typ", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = cstmt.Parameters[0].Value;
            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(10, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(ArrayList));

            for (int i = 0; i < arrayList.Count; i++)
            {
                ArrayList tuple = (ArrayList)arrayList[i]!;

                Assert.AreEqual(2, tuple.Count);

                Assert.AreEqual(tuple[0].GetType(), typeof(decimal));
                Assert.AreEqual(tuple[1].GetType(), typeof(string));
                Assert.AreEqual(empnos[i], tuple[0]);
                Assert.AreEqual(enames[i], tuple[1]);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgExtendTest;");
            Execute("DROP PACKAGE pkgExtendTest;");
        }
    }

    [Test]
    public async Task NestedTableExtendTest_ArrayList_OtherParam([Values] bool deriveParameters)
    {
        try
        {
            //the creation of an empty table with the constructor emp_tbl_typ()
            //as the first statement in the executable section of the anonymous block.
            //The EXTEND collection method is then used to add an element to the
            //table for each employee returned from the result set.
            var createPkg = " CREATE OR REPLACE PACKAGE pkgExtendTest Is \n"
                             + "   TYPE emp_rec_typ IS RECORD ( \n"
                             + "      empno  NUMBER(4), \n"
                             + "      ename       VARCHAR2(10) \n"
                             + "     );\n"
                             + "   TYPE emp_tbl_typ IS TABLE OF emp_rec_typ; \n"
                             + "   Procedure nestedTableExtendTest(test_num Out int, emp_tbl Out emp_tbl_typ, test_num2 Out int); "
                             + " End pkgExtendTest;";
            Execute(createPkg, true);

            var pkgBody = " CREATE OR REPLACE PACKAGE BODY pkgExtendTest \n"
                           + " Is \n"
                           + "  Procedure nestedTableExtendTest(test_num Out int, emp_tbl Out emp_tbl_typ, test_num2 Out int) \n "
                           + "   Is \n"
                           + "   DECLARE \n"
                           + "      CURSOR emp_cur IS SELECT empno, ename FROM emp1 WHERE ROWNUM <= 10 order by empno; \n"
                           + "      i  INTEGER := 0; \n"
                           + "   BEGIN\n"
                           + "    test_num := 123; \n"
                           + "    test_num2 := 456; \n"
                           + "    emp_tbl := emp_tbl_typ(); \n"
                           + "    FOR r_emp IN emp_cur LOOP \n"
                           + "        i := i + 1; \n"
                           + "        emp_tbl.EXTEND; \n"
                           + "        emp_tbl(i) := r_emp; \n"
                           + "    END LOOP; \n"
                           + "  End nestedTableExtendTest; "
                           + " End pkgExtendTest;";
            Execute(pkgBody, true);

            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgextendtest.emp_tbl_typ");
            //dataSourceBuilder.MapComposite<emp_rec_typ>("pkgextendtest.emp_rec_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgExtendTest.nestedTableExtendTest";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;


            // DeriveParameters works but parameters directions are wrong (INOUT instead of OUT), this is a backend issue
            if (deriveParameters)
            {
                cstmt.DeriveParameters();
                foreach (var p in cstmt.Parameters)
                {
                    // Fixup parameter description
                    ((EDBParameter)p).Direction = ParameterDirection.Output;
                }
            }
            else
            {
                cstmt.Parameters.Add(new EDBParameter()
                {
                    Direction = ParameterDirection.Output,
                    DataTypeName = "integer"
                });
                cstmt.Parameters.Add(new EDBParameter()
                {
                    Direction = ParameterDirection.Output,
                    DataTypeName = "pkgextendtest.emp_tbl_typ"
                });
                cstmt.Parameters.Add(new EDBParameter()
                {
                    Direction = ParameterDirection.Output,
                    DataTypeName = "integer"
                });
            }

            Assert.AreEqual(3, cstmt.Parameters.Count);
            Assert.AreEqual("integer", cstmt.Parameters[0].DataTypeName);
            Assert.AreEqual("pkgextendtest.emp_tbl_typ", cstmt.Parameters[1].DataTypeName);
            Assert.AreEqual("integer", cstmt.Parameters[2].DataTypeName);

            //cstmt.Parameters[0].Direction = ParameterDirection.Output;
            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValueInt = cstmt.Parameters[0].Value;
            Assert.IsNotNull(paramValueInt);
            Assert.IsInstanceOf<int>(paramValueInt);
            Assert.AreEqual(123, paramValueInt);

            var paramValue = cstmt.Parameters[1].Value;
            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);
            paramValueInt = cstmt.Parameters[2].Value;
            Assert.IsNotNull(paramValueInt);
            Assert.IsInstanceOf<int>(paramValueInt);
            Assert.AreEqual(456, paramValueInt);
            
            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(10, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(ArrayList));

            for (int i = 0; i < arrayList.Count; i++)
            {
                ArrayList tuple = (ArrayList)arrayList[i]!;

                Assert.AreEqual(2, tuple.Count);

                Assert.AreEqual(tuple[0].GetType(), typeof(decimal));
                Assert.AreEqual(tuple[1].GetType(), typeof(string));
                Assert.AreEqual(empnos[i], tuple[0]);
                Assert.AreEqual(enames[i], tuple[1]);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgExtendTest;");
            Execute("DROP PACKAGE pkgExtendTest;");
        }
    }

    [Test]
    public async Task ObjectTypeNestedTableTest_ArrayList() //([Values] bool deriveParameters)
    {
        try
        {
            Execute("DROP PACKAGE BODY pkgObjectTypeTest;", false);
            Execute("DROP PACKAGE pkgObjectTypeTest;", false);

            //The following procedure defines a nested table type whose element
            //consists of the dept_obj_typ object type. A nested table variable is declared,
            //initialized, and then populated from the dept1 table.
            var createPkg = "CREATE OR REPLACE PACKAGE pkgObjectTypeTest Is \n"
                    + "  TYPE dept_tbl_typ IS TABLE OF dept_obj_typ; \n"
                    + "  Procedure objectTypeNestedTableTest(dept_tbl Out dept_tbl_typ); \n"
                    + "End pkgObjectTypeTest;";
            Execute(createPkg, true);

            var pkgBody = "CREATE OR REPLACE PACKAGE BODY pkgObjectTypeTest \n"
                           + "  Is "
                           + "    Procedure objectTypeNestedTableTest(dept_tbl  Out dept_tbl_typ) "
                           + "     Is "
                           + "     DECLARE\n"
                           + "       CURSOR dept_cur IS SELECT dname, loc FROM dept1 ORDER BY dname;\n"
                           + "       i               INTEGER := 0;\n"
                           + "    BEGIN\n"
                           + "        dept_tbl := dept_tbl_typ( \n"
                           + "           dept_obj_typ(NULL,NULL), \n"
                           + "           dept_obj_typ(NULL,NULL), \n"
                           + "           dept_obj_typ(NULL,NULL), \n"
                           + "           dept_obj_typ(NULL,NULL) \n"
                           + "        ); \n"
                           + "        FOR r_dept IN dept_cur LOOP \n"
                           + "          i := i + 1; \n"
                           + "          dept_tbl(i).dname := r_dept.dname; \n"
                           + "          dept_tbl(i).loc   := r_dept.loc; \n"
                           + "       END LOOP;\n"
                           + "   End objectTypeNestedTableTest; \n "
                           + "End pkgObjectTypeTest;";
            Execute(pkgBody);

            // what we would like
            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgobjecttypetest.dept_tbl_typ");
            //dataSourceBuilder.MapComposite<dept_obj_typ>("pkgobjecttypetest.dept_obj_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgObjectTypeTest.objectTypeNestedTableTest";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            //if (deriveParameters)
            //{
            //    cstmt.DeriveParameters();
            //}
            //else
            //{
            var tableOfParam = cstmt.Parameters.Add(new EDBParameter()
            {
                Direction = ParameterDirection.Output,
                DataTypeName = "pkgobjecttypetest.dept_tbl_typ"
            });
            //}

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgobjecttypetest.dept_tbl_typ", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = cstmt.Parameters[0].Value;
            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(4, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(ArrayList));


            for (int i = 0; i < DEPT_TOTAL; i++)
            {

                ArrayList tuple = (ArrayList)arrayList[i]!;
                Assert.AreEqual(2, tuple.Count);

                Assert.AreEqual(tuple[0].GetType(), typeof(string));
                Assert.AreEqual(tuple[1].GetType(), typeof(string));
                Assert.AreEqual(deptNames[i], tuple[0]);
                Assert.AreEqual(deptLocs[i], tuple[1]);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgObjectTypeTest;");
            Execute("DROP PACKAGE pkgObjectTypeTest;");
        }
    }

    [Test]
    public async Task ObjectTypeNestedTableTest_Composite() //([Values] bool deriveParameters)
    {
        try
        {
            //The following procedure defines a nested table type whose element
            //consists of the dept_obj_typ object type. A nested table variable is declared,
            //initialized, and then populated from the dept1 table.
            var createPkg = "CREATE OR REPLACE PACKAGE pkgObjectTypeTestComposite Is \n"
                    + "  TYPE dept_tbl_typ_composite IS TABLE OF dept_obj_typ; \n"
                    + "  Procedure objectTypeNestedTableTestComposite(dept_tbl Out dept_tbl_typ_composite); \n"
                    + "End pkgObjectTypeTestComposite;";
            Execute(createPkg, true);

            var pkgBody = "CREATE OR REPLACE PACKAGE BODY pkgObjectTypeTestComposite \n"
                           + "  Is "
                           + "    Procedure objectTypeNestedTableTestComposite(dept_tbl  Out dept_tbl_typ_composite) "
                           + "     Is "
                           + "     DECLARE\n"
                           + "       CURSOR dept_cur IS SELECT dname, loc FROM dept1 ORDER BY dname;\n"
                           + "       i               INTEGER := 0;\n"
                           + "    BEGIN\n"
                           + "        dept_tbl := dept_tbl_typ_composite( \n"
                           + "           dept_obj_typ(NULL,NULL), \n"
                           + "           dept_obj_typ(NULL,NULL), \n"
                           + "           dept_obj_typ(NULL,NULL), \n"
                           + "           dept_obj_typ(NULL,NULL) \n"
                           + "        ); \n"
                           + "        FOR r_dept IN dept_cur LOOP \n"
                           + "          i := i + 1; \n"
                           + "          dept_tbl(i).dname := r_dept.dname; \n"
                           + "          dept_tbl(i).loc   := r_dept.loc; \n"
                           + "       END LOOP;\n"
                           + "   End objectTypeNestedTableTestComposite; \n "
                           + "End pkgObjectTypeTestComposite;";
            Execute(pkgBody, true);

            // what we would like
            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgobjecttypetestcomposite.dept_tbl_typ_composite");
            dataSourceBuilder.MapComposite<dept_obj_typ>("public.dept_obj_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgObjectTypeTestComposite.objectTypeNestedTableTestComposite";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.DeriveParameters();

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgobjecttypetestcomposite.dept_tbl_typ_composite", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = cstmt.Parameters[0].Value;
            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(4, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(dept_obj_typ));


            for (int i = 0; i < DEPT_TOTAL; i++)
            {

                dept_obj_typ tuple = (dept_obj_typ)arrayList[i]!;

                Assert.AreEqual(deptNames[i], tuple.dname);
                Assert.AreEqual(deptLocs[i], tuple.loc);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgObjectTypeTestComposite;");
            Execute("DROP PACKAGE pkgObjectTypeTestComposite;");
        }
    }

    [Test]
    public async Task ObjectTypeNestedTableTest_NestedCompositeJoint_Unmapped() //([Values] bool deriveParameters)
    {
        try
        {
            //The following procedure defines a nested table type whose element
            //consists of the dept_obj_typ object type. A nested table variable is declared,
            //initialized, and then populated from the dept1 table.
            var createPkg = "CREATE OR REPLACE PACKAGE pkgObjectTypeTestNestedCompositeJoint Is \n"
                            + "   TYPE emp_rec_typ IS RECORD ( \n"
                            + "      empno  NUMBER(4), \n"
                            + "      ename       VARCHAR2(10) \n"
                            + "     );\n"
                            + "   TYPE dept_rec_typ IS RECORD ( \n"
                            + "      dname  VARCHAR2(14), \n"
                            + "      loc  VARCHAR2(13) \n"
                            + "     );\n"
                            + "   TYPE joint_composite IS RECORD ( \n"
                            + "     emp   emp_rec_typ,\n"
                            + "     loc     dept_rec_typ\n"
                            + "     );\n"
                            + "   TYPE comp_tbl_typ IS TABLE OF joint_composite; \n"
                    + "  Procedure objectTypeNestedTableTestCompositeJoint(comp_tbl Out comp_tbl_typ); \n"
                    + "End pkgObjectTypeTestNestedCompositeJoint;";
            Execute(createPkg, true);

            var pkgBody = "CREATE OR REPLACE PACKAGE BODY pkgObjectTypeTestNestedCompositeJoint \n"
                           + "  Is "
                           + "    Procedure objectTypeNestedTableTestCompositeJoint(comp_tbl Out comp_tbl_typ) "
                           + "     Is "
                           + "     DECLARE\n"
                           + "       CURSOR total_cur IS SELECT dname, loc, empno, ename FROM dept1, emp1;\n"
                           + "       i               INTEGER := 0;\n"
                           + "    BEGIN\n"
                           + "        comp_tbl := comp_tbl_typ(); \n"
                           + "        FOR r_total IN total_cur LOOP \n"
                           + "          i := i + 1; \n"
                           + "          comp_tbl.EXTEND;"
                           + "          comp_tbl(i).emp := emp_rec_typ(r_total.empno, r_total.ename); \n"
                           + "          comp_tbl(i).loc := dept_rec_typ(r_total.dname, r_total.loc); \n"
                           + "       END LOOP;\n"
                           + "   End objectTypeNestedTableTestCompositeJoint; \n "
                           + "End pkgObjectTypeTestNestedCompositeJoint;";
            Execute(pkgBody, true);

            // what we would like
            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgobjecttypetestnestedcompositejoint.comp_tbl_typ");
            //dataSourceBuilder.MapComposite<dept_obj_typ>("pkgobjecttypetestnestedcompositejoint.dept_rec_typ");
            //dataSourceBuilder.MapComposite<JointComposite>("pkgobjecttypetestnestedcompositejoint.joint_composite");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgObjectTypeTestNestedCompositeJoint.objectTypeNestedTableTestCompositeJoint";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.DeriveParameters();

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgobjecttypetestnestedcompositejoint.comp_tbl_typ", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = cstmt.Parameters[0].Value;
            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;

            Assert.AreEqual(EMP_TOTAL * DEPT_TOTAL, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(ArrayList));

            int arrayIndex = 0;
            for (int empIndex = 0; empIndex < EMP_TOTAL; empIndex++)
            {
                for (int depIndex = 0; depIndex < DEPT_TOTAL; depIndex++)
                {
                    ArrayList tuple = (ArrayList)arrayList[arrayIndex++]!;

                    Assert.AreEqual(2, tuple.Count);
                    CollectionAssert.AllItemsAreInstancesOfType(tuple, typeof(ArrayList));

                    var empTuple = (ArrayList)tuple[0]!;
                    var deptTuple = (ArrayList)tuple[1]!;

                    Assert.AreEqual(empTuple[0].GetType(), typeof(decimal));
                    Assert.AreEqual(empTuple[1].GetType(), typeof(string));
                    Assert.AreEqual(deptTuple[0].GetType(), typeof(string));
                    Assert.AreEqual(deptTuple[1].GetType(), typeof(string));
                    //Assert.AreEqual(deptNames[i], tuple[0]);
                    //Assert.AreEqual(deptLocs[i], tuple[1]);
                }
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgObjectTypeTestNestedCompositeJoint;");
            Execute("DROP PACKAGE pkgObjectTypeTestNestedCompositeJoint;");
        }
    }

    [Test]
    public async Task ObjectTypeNestedTableTest_NestedCompositeJoint_Mapped() //([Values] bool deriveParameters)
    {
        try
        {
            //The following procedure defines a nested table type whose element
            //consists of the dept_obj_typ object type. A nested table variable is declared,
            //initialized, and then populated from the dept1 table.
            var createPkg = "CREATE OR REPLACE PACKAGE pkgObjectTypeTestNestedCompositeJoint Is \n"
                            + "   TYPE emp_rec_typ IS RECORD ( \n"
                            + "      empno  NUMBER(4), \n"
                            + "      ename       VARCHAR2(10) \n"
                            + "     );\n"
                            + "   TYPE dept_rec_typ IS RECORD ( \n"
                            + "      dname  VARCHAR2(14), \n"
                            + "      loc  VARCHAR2(13) \n"
                            + "     );\n"
                            + "   TYPE joint_composite IS RECORD ( \n"
                            + "     emp   emp_rec_typ,\n"
                            + "     loc     dept_rec_typ\n"
                            + "     );\n"
                            + "   TYPE comp_tbl_typ IS TABLE OF joint_composite; \n"
                    + "  Procedure objectTypeNestedTableTestCompositeJoint(comp_tbl Out comp_tbl_typ); \n"
                    + "End pkgObjectTypeTestNestedCompositeJoint;";
            Execute(createPkg, true);

            var pkgBody = "CREATE OR REPLACE PACKAGE BODY pkgObjectTypeTestNestedCompositeJoint \n"
                           + "  Is "
                           + "    Procedure objectTypeNestedTableTestCompositeJoint(comp_tbl Out comp_tbl_typ) "
                           + "     Is "
                           + "     DECLARE\n"
                           + "       CURSOR total_cur IS SELECT dname, loc, empno, ename FROM dept1, emp1;\n"
                           + "       i               INTEGER := 0;\n"
                           + "    BEGIN\n"
                           + "        comp_tbl := comp_tbl_typ(); \n"
                           + "        FOR r_total IN total_cur LOOP \n"
                           + "          i := i + 1; \n"
                           + "          comp_tbl.EXTEND;"
                           + "          comp_tbl(i).emp := emp_rec_typ(r_total.empno, r_total.ename); \n"
                           + "          comp_tbl(i).loc := dept_rec_typ(r_total.dname, r_total.loc); \n"
                           + "       END LOOP;\n"
                           + "   End objectTypeNestedTableTestCompositeJoint; \n "
                           + "End pkgObjectTypeTestNestedCompositeJoint;";
            Execute(pkgBody, true);

            // what we would like
            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgobjecttypetestnestedcompositejoint.comp_tbl_typ");
            dataSourceBuilder.MapComposite<emp_rec_typ>("pkgobjecttypetestnestedcompositejoint.emp_rec_typ");
            dataSourceBuilder.MapComposite<dept_obj_typ>("pkgobjecttypetestnestedcompositejoint.dept_rec_typ");
            dataSourceBuilder.MapComposite<JointComposite>("pkgobjecttypetestnestedcompositejoint.joint_composite");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgObjectTypeTestNestedCompositeJoint.objectTypeNestedTableTestCompositeJoint";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.DeriveParameters();

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgobjecttypetestnestedcompositejoint.comp_tbl_typ", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = cstmt.Parameters[0].Value;
            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(DEPT_TOTAL * EMP_TOTAL, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(JointComposite));

            for (int i = 0; i < DEPT_TOTAL * EMP_TOTAL; i++)
            {
                JointComposite tuple = (JointComposite)arrayList[i]!;

                Assert.IsInstanceOf(typeof(emp_rec_typ), tuple.emp);
                Assert.AreEqual(empnos[i / DEPT_TOTAL], tuple.emp.empno);
                Assert.AreEqual(enames[i / DEPT_TOTAL], tuple.emp.ename);

                Assert.IsInstanceOf(typeof(dept_obj_typ), tuple.loc);
                Assert.AreEqual(deptLocs[i % DEPT_TOTAL], tuple.loc.loc);
                Assert.AreEqual(deptNames[i % DEPT_TOTAL], tuple.loc.dname);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgObjectTypeTestNestedCompositeJoint;");
            Execute("DROP PACKAGE pkgObjectTypeTestNestedCompositeJoint;");
        }
    }

    /// <summary>
    /// Test written as NUnit did not support Guids
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task DomainTypeNestedTableTest_Uuid()
    {
        var testArray = new Guid[] { Guid.Empty, Guid.NewGuid() };
        var expectedType = typeof(Guid);

        await DomainTypeNestedTableTest<Guid>("uuid", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Timestamp()
    {
        var testArray = new DateTime[] { new DateTime(2024,12,30,12,59,59, DateTimeKind.Unspecified).RemoveSecondsFraction()
            , new DateTime(2024,12,30,0,0,0, DateTimeKind.Unspecified).RemoveSecondsFraction() };
        var expectedType = typeof(DateTime);

        await DomainTypeNestedTableTest<DateTime>("timestamp without time zone", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_TimestampTz()
    {
        var testArray = new DateTime[] { new DateTime(2024,12,30,12,59,59, DateTimeKind.Local).RemoveSecondsFraction()
            , new DateTime(2024,12,30,0,0,0, DateTimeKind.Local).RemoveSecondsFraction() };
        var expectedType = typeof(DateTime);

        await DomainTypeNestedTableTest<DateTime>("timestamp with time zone", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Time()
    {
        // 
        var testArray = new TimeSpan[] {
            new TimeSpan(hours: 4,minutes: 5,seconds: 6)
            ,new TimeSpan(days: 0, hours: 4,minutes: 5,seconds: 6, milliseconds: 789)
        };

        var testArrayStrDeclaration = new string[] {
        "'04:05:06'::time without time zone"
        ,"'04:05:06.789'::time without time zone"
        };

        await DomainTypeNestedTableTest_RawStrings<TimeSpan>("time without time zone", testArrayStrDeclaration, testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Name()
    {
        // 
        var testArray = new string[] {
            "abcd", "123abcd456"
        };

        var testArrayStrDeclaration = new string[] {
        "'abcd'::name" , "'123abcd456'::name"
        };

        await DomainTypeNestedTableTest_RawStrings<string>("name", testArrayStrDeclaration, testArray);
    }

    [Test]
    [Ignore("Not supported: error 42602 table definition cannot be an array type")]
    public async Task DomainTypeNestedTableTest_Array()
    {
        // 
        var testArray = new[] {
            new int[] { 1, 2, 3 }
            ,new int[] { 1, -2, 3 }
        };

        var testArrayStrDeclaration = new string[] {
        "{{1,2,3},{1,-2,3}}::integer"
        };

        await DomainTypeNestedTableTest_RawStrings<int[]>("integer[]", testArrayStrDeclaration, testArray);
    }

    [Test]
    [Ignore("Not supported: error 42602: table definition should be a primitive or composite type")]
    public async Task DomainTypeNestedTableTest_Range()
    {
        // 
        var testArray = new EDBRange<int>[] {
            new EDBRange<int>(1,true, 10, false)
        };

        var testArrayStrDeclaration = new string[] {
        "[1,10)::int4range"
        };

        await DomainTypeNestedTableTest_RawStrings<EDBRange<int>>("int4range", testArrayStrDeclaration, testArray);
    }

    [Test]
    [Ignore("Not supported: error 42602: table definition should be a primitive or composite type")]
    public async Task DomainTypeNestedTableTest_Record()
    {
        // 
        var testArray = new Tuple<int, string>[] {
            new Tuple<int, string>(1, "foo")
        };

        var testArrayStrDeclaration = new string[] {
        "(1,'foo'::text)::record"
        };

        await DomainTypeNestedTableTest_RawStrings<Tuple<int, string>>("record", testArrayStrDeclaration, testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Char()
    {
        // 
        var testArray = new char[] {
            '0','a'
        };

        var testArrayStrDeclaration = new string[] {
        "'0'::char" , "'a'::char"
        };

        await DomainTypeNestedTableTest_RawStrings<char>("char", testArrayStrDeclaration, testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Xid()
    {
        // 
        var testArray = new uint[] {
            29603,
            29604
        };

        var testArrayStrDeclaration = new string[] {
        "'29603'::xid"
        ,"'29604'::xid"
        };

        await DomainTypeNestedTableTest_RawStrings<uint>("xid", testArrayStrDeclaration, testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Oid()
    {
        // 
        var testArray = new uint[] {
            29603,
            29604
        };

        var testArrayStrDeclaration = new string[] {
        "29603::oid"
        ,"29604::oid"
        };

        await DomainTypeNestedTableTest_RawStrings<uint>("oid", testArrayStrDeclaration, testArray);
    }
    [Test]
    public async Task DomainTypeNestedTableTest_Cid()
    {
        // 
        var testArray = new uint[] {
            29603,
            29604
        };

        var testArrayStrDeclaration = new string[] {
        "'29603'::cid"
        ,"'29604'::cid"
        };

        await DomainTypeNestedTableTest_RawStrings<uint>("cid", testArrayStrDeclaration, testArray);
    }


    [Test, Explicit("Should be tested with data from DB that should be timestamp with EPAS redwood")]
    public async Task DomainTypeNestedTableTest_TimeTz()
    {
        var testArray = new DateTimeOffset[] { DateTimeOffset.Now.RemoveSecondsFraction()
            , DateTimeOffset.Now.AddMinutes(8).RemoveSecondsFraction() };
        var expectedType = typeof(DateTimeOffset);

        await DomainTypeNestedTableTest<DateTimeOffset>("time with time zone", testArray);
    }

    [Test]
    [EDBExplicit("EDBInterval should be replaced by NodaTime or TimeSpan, see https://www.npgsql.org/doc/types/datetime.html#detailed-behavior-reading-values-from-the-database")]
    public async Task DomainTypeNestedTableTest_Interval()
    {
        // 1 day 12 hours 59 min 10 sec
        // 123 ms
        var testArray = new EDBInterval[] {
            new EDBInterval(1,2,3*60*1000*1000)
            , new EDBInterval(0,0,0) };

        await DomainTypeNestedTableTest<EDBInterval, TimeSpan>("interval", testArray, testArray.Select(a => a.ToTimeSpan()).ToArray());
    }

    [Test]
    public async Task NestedTableTest_Enum()
    {
        // EnterpriseDB EnableUnmappedTypes active by default
        await using var dataSource = CreateDataSource();
        Execute("CREATE TYPE mood AS ENUM ('sad', 'ok', 'happy')", false);

        // 
        var testArray = new Mood[] {
            Mood.Ok,
            Mood.Happy
        };

        var testArrayStrDeclaration = new string[] {
        "'ok'::mood"
        ,"'happy'::mood"
        };

        await DomainTypeNestedTableTest_RawStrings<Mood>("mood", testArrayStrDeclaration, testArray
            , builder =>
            {
                builder.MapEnum<Mood>("mood");
            });

        Execute("DROP TYPE mood", false);

    }

    [Test]
    public async Task NestedTableTest_UnmappedEnum_AsString()
    {
        // EnterpriseDB EnableUnmappedTypes active by default
        await using var dataSource = CreateDataSource();
        Execute("CREATE TYPE mood AS ENUM ('sad', 'ok', 'happy')", false);

        // 
        var testArray = new string[] {
            "ok",
            "happy"
        };

        var testArrayStrDeclaration = new string[] {
        "'ok'::mood"
        ,"'happy'::mood"
        };

        await DomainTypeNestedTableTest_RawStrings<string>("mood", testArrayStrDeclaration, testArray
            , builder =>
            {
                builder.DisableUnmappedTypes();
            });

        Execute("DROP TYPE mood", false);

    }

    [Test]
    public async Task DomainTypeNestedTableTest_Cidr()
    {
        var testArray = new EDBCidr[] {
            new EDBCidr("192.168.1.0/24")
            , new EDBCidr("192.168.1.0/24") };

        await DomainTypeNestedTableTest<EDBCidr>("cidr", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_TsQuery()
    {
        // 
        var testArray = new EDBTsQuery[] {
            EDBTsQuery.Parse( "fat & rat")
            ,EDBTsQuery.Parse("super:*")
        };

        var testArrayStrDeclaration = new string[] {
        "'fat & rat'::tsquery"
        ,"'super:*'::tsquery"
        };

        await DomainTypeNestedTableTest_RawStrings<EDBTsQuery>("tsquery", testArrayStrDeclaration, testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_TsVector()
    {
        // 
        var testArray = new EDBTsVector[] {
            EDBTsVector.Parse("The Fat Rats")
            , new EDBTsVector(new () {
                    new ("a", new () {
                        new (1, EDBTsVector.Lexeme.Weight.A)})
                    ,new ("fat", new () {
                        new (2, EDBTsVector.Lexeme.Weight.B), new (4, EDBTsVector.Lexeme.Weight.C)})
                    ,new ("cat", new () {
                        new (5, EDBTsVector.Lexeme.Weight.D)})
            })
        };

        var testArrayStrDeclaration = new string[] {
        "'The Fat Rats'::tsvector",
        "'a:1A fat:2B,4C cat:5D'::tsvector"
        };

        await DomainTypeNestedTableTest_RawStrings<EDBTsVector>("tsvector", testArrayStrDeclaration, testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Path()
    {
        var path1 = new EDBPath(open: true);
        path1.Add(new(0, 0));
        path1.Add(new(-2.5, 43.2));
        path1.Add(new(10, 10));

        var path2 = new EDBPath(open: false);
        path2.Add(new(0, 0));
        path2.Add(new(-2.5, 43.2));
        path2.Add(new(10, 10));
        path2.Add(new(0, 0));

        var testArray = new EDBPath[] { path1, path2 };

        await DomainTypeNestedTableTest<EDBPath>("path", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Polygon()
    {
        var poly1 = new EDBPolygon();
        poly1.Add(new(0, 0));
        poly1.Add(new(-2.5, 43.2));
        poly1.Add(new(10, 10));
        poly1.Add(new(0, 0));

        var poly2 = new EDBPolygon();
        poly2.Add(new(0, 0));
        poly2.Add(new(10, 10));
        poly2.Add(new(-2.5, 43.2));
        poly2.Add(new(0, 0));

        var testArray = new EDBPolygon[] { poly1, poly2 };

        await DomainTypeNestedTableTest<EDBPolygon>("polygon", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Line()
    {
        var line1 = new EDBLine(1, 2, 3);

        var line2 = new EDBLine(-1.2, 4.5, -3.2);

        var testArray = new EDBLine[] { line1, line2 };

        await DomainTypeNestedTableTest<EDBLine>("line", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Circle()
    {
        var circle1 = new EDBCircle(0, 0, 1);
        var circle2 = new EDBCircle(-1.3, 2.5, 10.2);

        var testArray = new EDBCircle[] { circle1, circle2 };

        await DomainTypeNestedTableTest<EDBCircle>("circle", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Box()
    {
        var box1 = new EDBBox(0, 0, 1, 1);
        var box2 = new EDBBox(-1.3, 2.5, 10.2, -4);

        var testArray = new EDBBox[] { box1, box2 };

        await DomainTypeNestedTableTest<EDBBox>("box", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Hstore_Single()
    {
        using var conn = await OpenConnectionAsync();
        await TestUtil.EnsureExtensionAsync(conn, "hstore", "9.1");

        var item1 = new Dictionary<string, string>();
        item1.Add("Key3", "ValueA");

        var testArray = new Dictionary<string, string>[] { item1 };

        await DomainTypeNestedTableTest<Dictionary<string, string>>("hstore", testArray);
    }

    [Test]
    public async Task DomainTypeNestedTableTest_Hstore_Multiple()
    {
        using var conn = await OpenConnectionAsync();
        await TestUtil.EnsureExtensionAsync(conn, "hstore", "9.1");

        var item1 = new Dictionary<string, string>();
        item1.Add("Key1", "Value1");
        item1.Add("Key2", "Value3,Value4;Value5");

        var item2 = new Dictionary<string, string>();
        item2.Add("Key3", "ValueA");

        var testArray = new Dictionary<string, string>[] { item1, item2 };

        await DomainTypeNestedTableTest<Dictionary<string, string>>("hstore", testArray);
    }

    [TestCaseSource(typeof(DomainTypeCases))]
    public async Task DomainTypeNestedTableTest<T>(string pgTypeName, T[] values)
        => await DomainTypeNestedTableTest<T, T>(pgTypeName, values);


    public async Task DomainTypeNestedTableTest<T, TExpected>(string pgTypeName, T[] values, TExpected[] expectedResults = null)
    {
        // must match server locale for params encoding
        TestUtil.SetCurrentCulture(System.Globalization.CultureInfo.GetCultureInfo("EN-us"));

        try
        {

            var createPkg = $"""
            CREATE OR REPLACE PACKAGE pkgDomainTypeTest Is
                TYPE type_tbl_type IS TABLE OF {pgTypeName};
                Procedure domainTestProc(type_tbl Out type_tbl_type);
            End pkgDomainTypeTest; 
            """;

            Execute(createPkg, true);

            var stringsParam = string.Join(",", ToOracleArrayDecl(values, pgTypeName));

            var pkgBody = $"""
            CREATE OR REPLACE PACKAGE BODY pkgDomainTypeTest IS
                Procedure domainTestProc(type_tbl Out type_tbl_type) IS
                BEGIN
                    type_tbl := type_tbl_type({stringsParam});
                END domainTestProc;
            END pkgDomainTypeTest;
            """;

            Execute(pkgBody, true);

            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgdomaintypetest.type_tbl_type");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();


            //await connection.ExecuteNonQueryAsync("SET datestyle TO ISO");

            var commandText = "pkgDomainTypeTest.domainTestProc";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.DeriveParameters();

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgdomaintypetest.type_tbl_type", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = cstmt.Parameters[0].Value;

            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(values.Length, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(TExpected));

            if (expectedResults != null)
            {
                for (int i = 0; i < expectedResults.Length; i++)
                {
                    Assert.AreEqual(expectedResults[i], arrayList[i]);
                }
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    Assert.AreEqual(values[i], arrayList[i]);
                }
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgDomainTypeTest;");
            Execute("DROP PACKAGE pkgDomainTypeTest;");
        }
    }

    public class DomainTypeCases : IEnumerable
    {

        public IEnumerator GetEnumerator()
        {
            // TestCaseData
            // string pgTypeName, T[] values

            yield return new TestCaseData("boolean", new bool[] { true, false, true });
            yield return new TestCaseData("double precision", new double[] { 0, 1.1, 2.02, -1, -2.345 });
            yield return new TestCaseData("smallint", new short[] { 0, 1, 2, -1 });
            yield return new TestCaseData("integer", new int[] { 0, 1, 2, -1 });
            yield return new TestCaseData("bigint", new long[] { 0, 1, 2, -1 });
            yield return new TestCaseData("numeric", new decimal[] { 0m, 1.1m, 2.02m, -1m, -2.345m });
            yield return new TestCaseData("real", new float[] { 0, 1.1f, 2.02f, -1f, -2.345f });
            yield return new TestCaseData("double precision", new double[] { 0, 1.1, 2.02, -1, -2.345 });
            yield return new TestCaseData("money", new decimal[] { 0m, 1.1m, 2.02m, -1m, -2.35m });

            yield return new TestCaseData("text", new string[] { "", "Test1", "Test 2" });
            yield return new TestCaseData("character varying", new string[] { "", "Test1", "Test 2" });
            yield return new TestCaseData("character", new char[] { 'a', 'T', 't' });
            yield return new TestCaseData("inet", new EDBInet[] { new EDBInet("127.0.0.1"), new EDBInet("192.168.1.1/24") });
            yield return new TestCaseData("date", new DateTime[] {
                new DateTime(2024, 12, 30, 23, 59, 59, DateTimeKind.Unspecified).RemoveSecondsFraction(),
                new DateTime(2024,12,30,0,0,0, DateTimeKind.Unspecified).RemoveSecondsFraction()
            });
            yield return new TestCaseData("macaddr", new PhysicalAddress[] { PhysicalAddress.Parse("00-B0-D0-63-C2-26"), PhysicalAddress.Parse("98-59-7A-58-0F-65") });

            yield return new TestCaseData("bit", new bool[] { true, false, true });
            yield return new TestCaseData("bit(3)", new BitArray[] {
                    new BitArray(new bool[] { true, false, true })
                    ,new BitArray(new bool[] { false, true ,true})
                });
            yield return new TestCaseData("bit varying", new BitArray[] {
                    new BitArray(new bool[] { true, false, true })
                    ,new BitArray(new bool[] { false })
                    ,new BitArray(new bool[] { false, false, true, true, true, false, false })
                });
            yield return new TestCaseData("point", new EDBPoint[] { new(0, 0), new(-4.2, 43.5) });
            yield return new TestCaseData("lseg", new EDBLSeg[] { new(0, 0, 10, 10), new(-4.2, 1.1, 0.2, -0.1) });
        }
    }

    private static string[] ToOracleArrayDecl<T>(T[] values, string pgDataType)
    {
        if (typeof(T) == typeof(DateTime))
            return values.Select(v => string.Concat('\'', (v as DateTime?)?.ToString("yyyy-MM-dd HH:mm:ss.fffffzz"), '\'')).ToArray();

        if (typeof(T) == typeof(string)
            || typeof(T) == typeof(char)
            || typeof(T) == typeof(char)
            || typeof(T) == typeof(Guid)
            || typeof(T) == typeof(DateTimeOffset)
            || typeof(T) == typeof(TimeSpan)
            || typeof(T) == typeof(EDBCidr)
            || typeof(T) == typeof(EDBInet))
            return values.Select(v => string.Concat('\'', v.ToString(), '\'')).ToArray();

        if (typeof(T) == typeof(PhysicalAddress))
            return values.Select(v => string.Concat("'", v.ToString(), "'::macaddr")).ToArray();

        if (typeof(T) == typeof(EDBTsQuery))
            return values.Select(v => string.Concat("to_tsquery($$", v.ToString(), "$$)")).ToArray();
        if (typeof(T) == typeof(EDBTsVector))
            return values.Select(v => string.Concat("to_tsvector($$", v.ToString(), "$$)")).ToArray();

        if (typeof(T) == typeof(EDBInterval))
            return values.Select(v =>
            {
                EDBInterval? i = v as EDBInterval?;
                return $"'{i.Value.Months} months {i.Value.Days} days {i.Value.Time / 1000} milliseconds'::interval";
            }).ToArray();

        if (typeof(T) == typeof(EDBPolygon)
            || typeof(T) == typeof(EDBPoint)
            || typeof(T) == typeof(EDBLSeg)
            || typeof(T) == typeof(EDBPath)
            || typeof(T) == typeof(EDBLine)
            || typeof(T) == typeof(EDBCircle)
            || typeof(T) == typeof(EDBBox))
        {
            return values.Select(v =>
            {
                return $"'{v.ToString()}'::{pgDataType}";

            }).ToArray();
        }

        if (typeof(T) == typeof(BitArray))
        {
            if (pgDataType == "bit varying")
            {
                return values.Select(v =>
                {
                    BitArray? i = v as BitArray;
                    if (i is null) ThrowHelper.ThrowArgumentNullException("bitarray");

                    var result = "";
                    for (int idx = 0; idx < i.Length; idx++)
                    {
                        result += i[idx] ? "1" : "0";
                    }
                    return "'" + result + "'::bit varying";
                }).ToArray();
            }
            else
            {
                return values.Select(v =>
                {
                    BitArray? i = v as BitArray;
                    if (i is null) ThrowHelper.ThrowArgumentNullException("bitarray");

                    var result = "";
                    for (int idx = 0; idx < i.Length; idx++)
                    {
                        result += i[idx] ? "1" : "0";
                    }
                    return "'" + result + "'::bit(" + i.Length + ")";
                }).ToArray();
            }
        }

        if (typeof(T) == typeof(Dictionary<string, string>)) // hstore
        {
            return values.Select(v =>
            {
                Dictionary<string, string>? i = v as Dictionary<string, string>;
                if (i is null) ThrowHelper.ThrowArgumentNullException("hstore");

                var result = string.Join(",", i.Select(kvp =>
                {
                    string kvpString;
                    if (kvp.Key.Contains(",") || kvp.Key.Contains(";") || kvp.Key.Contains(" "))
                    {
                        kvpString = $"\"{kvp.Key}\"=>";
                    }
                    else
                    {
                        kvpString = $"{kvp.Key}=>";
                    }

                    if (kvp.Value.Contains(",") || kvp.Value.Contains(";") || kvp.Value.Contains(" "))
                    {
                        kvpString += $"\"{kvp.Value}\"";
                    }
                    else
                    {
                        kvpString += $"{kvp.Value}";
                    }

                    return kvpString;
                }));
                return $"'{result}'::hstore";
            }).ToArray();
        }

        var result = values.Select(v => v.ToString()).ToArray();
        return result!;
    }

    public async Task DomainTypeNestedTableTest_RawStrings<TExpected>(string pgTypeName, string[] values, TExpected[] expectedResults, Action<EDBDataSourceBuilder> configuration = null)
    {
        // must match server locale for params encoding
        TestUtil.SetCurrentCulture(System.Globalization.CultureInfo.GetCultureInfo("EN-us"));

        try
        {

            var createPkg = $"""
                CREATE OR REPLACE PACKAGE pkgDomainTypeTest Is
                    TYPE type_tbl_type IS TABLE OF {pgTypeName};
                    Procedure domainTestProc(type_tbl Out type_tbl_type);
                End pkgDomainTypeTest; 
            """;

            Execute(createPkg, true);

            var stringsParam = string.Join(",", values);

            var pkgBody = $"""
                CREATE OR REPLACE PACKAGE BODY pkgDomainTypeTest IS
                    Procedure domainTestProc(type_tbl Out type_tbl_type) IS
                BEGIN
                        type_tbl := type_tbl_type({stringsParam});
                    END domainTestProc;
                END pkgDomainTypeTest;
            """;

            Execute(pkgBody, true);

            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgdomaintypetest.type_tbl_type");
            configuration?.Invoke(dataSourceBuilder);
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();


            //await connection.ExecuteNonQueryAsync("SET datestyle TO ISO");

            var commandText = "pkgDomainTypeTest.domainTestProc";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.DeriveParameters();

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgdomaintypetest.type_tbl_type", cstmt.Parameters[0].DataTypeName);

            await cstmt.PrepareAsync();
            await cstmt.ExecuteNonQueryAsync();

            var paramValue = cstmt.Parameters[0].Value;

            Assert.IsNotNull(paramValue);
            Assert.IsInstanceOf<ArrayList>(paramValue);

            var arrayList = (ArrayList)paramValue!;
            Assert.AreEqual(values.Length, arrayList.Count);
            CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(TExpected));


            for (int i = 0; i < expectedResults.Length; i++)
            {
                Assert.AreEqual(expectedResults[i], arrayList[i]);
            }
        }
        finally
        {
            Execute("DROP PACKAGE BODY pkgDomainTypeTest;");
            Execute("DROP PACKAGE pkgDomainTypeTest;");
        }
    }

}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

