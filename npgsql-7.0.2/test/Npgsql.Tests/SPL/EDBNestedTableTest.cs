using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Data.SqlTypes;
using System.Xml.Linq;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2591: Regression Tests for Nested Tables

namespace EnterpriseDB.EDBClient.Tests.SPL
{
    public class dept_obj_typ
    {
        public string dname;
        public string loc;
    }

    internal class EDBNestedTableTest : TestBase
    {
        EDBConnection? conn = null;

        private static string[] enames = new string[] { "SMITH", "ALLEN", "WARD", "JONES", "MARTIN",
            "BLAKE", "CLARK", "SCOTT", "KING", "TURNER" };
        private static int[] empnos = new int[] { 7369, 7499, 7521, 7566, 7654, 7698, 7782, 7788, 7839, 7844 };
        private static string[] deptNames = new string[] { "ACCOUNTING", "OPERATIONS", "RESEARCH", "SALES" };
        private static string[] deptLocs = new string[] { "NEW YORK", "BOSTON", "DALLAS", "CHICAGO" };
        private static int EMP_TOTAL = enames.Length;
        private static int DEPT_TOTAL = deptNames.Length;

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();
            TestUtil.EnsureEDBAdvancedServer(conn);

            Execute("DROP PACKAGE BODY pkgSimpleTest;");
            Execute("DROP PACKAGE pkgSimpleTest;");
            Execute("DROP PACKAGE BODY pkgExtendTest;");
            Execute("DROP PACKAGE pkgExtendTest;");
            Execute("DROP PACKAGE BODY pkgObjectTypeTest;");
            Execute("DROP PACKAGE pkgObjectTypeTest;");
            Execute("DROP type dept_obj_typ");
            TestUtil.dropTable(conn, "emp1 CASCADE");
            TestUtil.dropTable(conn, "dept1 CASCADE");

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
            Execute(createTypeSql);

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

        [TearDown]
        public void Dispose()
        {
            TestUtil.closeDB(conn);
        }

        private int Execute(string query, bool shouldPass = false)
        {
            try
            {
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
        [Ignore("EC-2644: 42601: syntax error at end of input")]
        public void SimpleNestedTableTest()
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

            var commandText = "pkgSimpleTest.simpleNestedTableTest";
            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;
            EDBCommandBuilder.DeriveParameters(cstmt);

            Assert.AreEqual("test", cstmt.Parameters[0].Value.ToString());

            //CallableStatement cstmt = con.prepareCall(commandText);
            //cstmt.registerOutParameter(1, Types.ARRAY);
            //cstmt.execute();
            //Array arr = (Array)cstmt.getObject(1);
            //String names[] = (String[])arr.getArray();
            //Assert.assertEquals(DEPT_TOTAL, names.length);
            //for (int i = 0; i<DEPT_TOTAL; i++) {
            //    Assert.assertEquals(deptNames[i], names[i]);
            //}
        }

        [Test]
        [Ignore("EC-2644: 42601: syntax error at end of input")]
        public void NestedTableExtendTest()
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

            var commandText = "pkgExtendTest.nestedTableExtendTest";
            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;
            EDBCommandBuilder.DeriveParameters(cstmt);

            Assert.AreEqual("test", cstmt.Parameters[0].Value.ToString());

            //CallableStatement cstmt = con.prepareCall(commandText);
            //cstmt.registerOutParameter(1, Types.ARRAY);
            //cstmt.execute();
            //Array arr = (Array)cstmt.getObject(1);
            //Object[] items = (Object[])arr.getArray();
            //Assert.assertEquals(EMP_TOTAL, items.length);
            //for (int i = 0; i<EMP_TOTAL; i++) {
            //    Struct item = (Struct)items[i];
            //Object[] data = item.getAttributes();
            //BigDecimal empno = (BigDecimal)data[0];
            //String ename = (String)data[1];
            //Assert.assertEquals(empno.intValue(), empnos[i]);
            //    Assert.assertEquals(ename, enames[i]);
            //}
        }

        [Test]
        [Ignore("EC-2645: Couldn't find PostgreSQL type with OID 214516")]
        public void ObjectTypeNestedTableTest()
        {
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

            conn.ReloadTypes();

            //Close and reopen the connection so that custom types are reloaded.
            TestUtil.closeDB(conn);
            EDBConnection.GlobalTypeMapper.MapComposite<dept_obj_typ>("public.dept_obj_typ");

            conn = OpenConnection();

            var commandText = "pkgObjectTypeTest.objectTypeNestedTableTest";
            var cstmt = new EDBCommand(commandText, conn);
            cstmt.CommandType = CommandType.StoredProcedure;
            EDBCommandBuilder.DeriveParameters(cstmt);

            Assert.AreEqual("test", cstmt.Parameters[0].Value.ToString());

            //String commandText = "{call pkgObjectTypeTest.objectTypeNestedTableTest(?)}";
            //CallableStatement cstmt = con.prepareCall(commandText);
            //cstmt.registerOutParameter(1, Types.ARRAY);
            //cstmt.execute();
            //Array arr = (Array)cstmt.getObject(1);
            //Object[] items = (Object[])arr.getArray();
            //Assert.assertEquals(DEPT_TOTAL, items.length);
            //for (int i = 0; i<DEPT_TOTAL; i++) {
            //    Struct item = (Struct)items[i];
            //Object[] data = item.getAttributes();
            //String dname = (String)data[0];
            //String dloc = (String)data[1];
            //Assert.assertEquals(deptNames[i], dname);
            //    Assert.assertEquals(deptLocs[i], dloc);
            //}
        }
    }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

}
