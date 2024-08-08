using EDBTypes;
using EnterpriseDB.EDBClient.Tests.Support;
using Npgsql.Tests.Support;
using NUnit.Framework;
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

//EC-2591: Regression Tests for Nested Tables

namespace EnterpriseDB.EDBClient.Tests.SPL
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

    [NonParallelizable]
    internal class EDBNestedTableTest : EPASTestBase
    {
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
            using var conn = OpenConnection();

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
        public async Task SimpleNestedTableTest_ArrayList([Values] bool async, [Values] bool deriveParameters)
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
            cstmt.AllResultTypesAreUnknown = true;

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

            if (async)
            {
                await cstmt.PrepareAsync();
                await cstmt.ExecuteNonQueryAsync();
            }
            else
            {
                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }


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
        public async Task SimpleNestedTableTest_Integer_ArrayList([Values] bool async, [Values] bool deriveParameters)
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
            cstmt.AllResultTypesAreUnknown = true;

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

            if (async)
            {
                await cstmt.PrepareAsync();
                await cstmt.ExecuteNonQueryAsync();
            }
            else
            {
                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }


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
        [NonParallelizable]
        public async Task NestedTableExtendTest_ArrayList([Values] bool async, [Values] bool deriveParameters)
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
            cstmt.AllResultTypesAreUnknown = true;


            EDBParameter? tableOfParam = null;
            if (deriveParameters)
            {
                cstmt.DeriveParameters();
            }
            else
            {
                tableOfParam = cstmt.Parameters.Add(new EDBParameter()
                {
                    Direction = ParameterDirection.Output,
                    DataTypeName = "pkgextendtest.emp_tbl_typ"
                });
            }

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgextendtest.emp_tbl_typ", cstmt.Parameters[0].DataTypeName);


            if (async)
            {
                await cstmt.PrepareAsync();
                await cstmt.ExecuteNonQueryAsync();
            }
            else
            {
                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

            var paramValue = deriveParameters ? cstmt.Parameters[0].Value : tableOfParam.Value;
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
        public async Task ObjectTypeNestedTableTest_ArrayList([Values] bool async, [Values] bool deriveParameters)
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

            // what we would like
            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseEDBIsTableOf("pkgobjecttypetest.dept_tbl_typ");
            //dataSourceBuilder.MapComposite<dept_obj_typ>("pkgobjecttypetest.dept_obj_typ");
            await using var dataSource = dataSourceBuilder.Build();
            await using var connection = await dataSource.OpenConnectionAsync();

            var commandText = "pkgObjectTypeTest.objectTypeNestedTableTest";
            var cstmt = new EDBCommand(commandText, connection);
            cstmt.CommandType = CommandType.StoredProcedure;
            cstmt.AllResultTypesAreUnknown = true;

            if (deriveParameters)
            {
                cstmt.DeriveParameters();
            }
            else
            {
                var tableOfParam = cstmt.Parameters.Add(new EDBParameter()
                {
                    Direction = ParameterDirection.Output,
                    DataTypeName = "pkgobjecttypetest.dept_tbl_typ"
                });
            }

            Assert.AreEqual(1, cstmt.Parameters.Count);
            Assert.AreEqual("pkgobjecttypetest.dept_tbl_typ", cstmt.Parameters[0].DataTypeName);

            if (async)
            {
                await cstmt.PrepareAsync();
                await cstmt.ExecuteNonQueryAsync();
            }
            else
            {
                cstmt.Prepare();
                cstmt.ExecuteNonQuery();
            }

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
            var testArray = new TimeSpan[] { new TimeSpan(hours: 1, minutes: 2,seconds: 3)
                , new TimeSpan(days:0, hours: 1, minutes: 2,seconds: 3,milliseconds:999) };
            var expectedType = typeof(TimeSpan);

            await DomainTypeNestedTableTest<TimeSpan>("time without time zone", testArray);
        }

        [Test]
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
            var expectedType = typeof(TimeSpan);


            await DomainTypeNestedTableTest<EDBInterval>("interval", testArray, (a) => a.Select(i => $"'{i.Months} months {i.Days} days {i.Time / 1000} milliseconds'::interval").ToArray());
        }

        [Test]
        public async Task DomainTypeNestedTableTest_Cidr()
        {

            // 1 day 12 hours 59 min 10 sec
            // 123 ms
            var testArray = new EDBCidr[] {
                new EDBCidr("192.168.1.0/24")
                , new EDBCidr("192.168.1.0/24") };
            var expectedType = typeof(EDBCidr);

            await DomainTypeNestedTableTest<EDBCidr>("cidr", testArray);
        }


        [TestCaseSource(typeof(DomainTypeCases))]
        public async Task DomainTypeNestedTableTest<T>(string pgTypeName, T[] values, Func<T[], string[]> stringRepresentations = null, Action<EDBConnection> configurationSteps = null)
        {
            // must match server locale for params encoding
            TestUtil.SetCurrentCulture(System.Globalization.CultureInfo.GetCultureInfo("EN-us"));


            var createPkg = $"""
                CREATE OR REPLACE PACKAGE pkgDomainTypeTest Is
                    TYPE type_tbl_type IS TABLE OF {pgTypeName};
                    Procedure domainTestProc(type_tbl Out type_tbl_type);
                End pkgDomainTypeTest; 
                """;

            Execute(createPkg, true);
            stringRepresentations ??= ToOracleArrayDecl;

            var stringsParam = string.Join(",", stringRepresentations.Invoke(values));

            var pkgBody = $"""
                CREATE OR REPLACE PACKAGE BODY pkgDomainTypeTest IS
                    Procedure domainTestProc(type_tbl Out type_tbl_type) IS
                    BEGIN
                        type_tbl := type_tbl_type({stringsParam});
                    END domainTestProc;
                END pkgDomainTypeTest;
                """;

            Execute(pkgBody, true);

            try
            {
                var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
                dataSourceBuilder.UseEDBIsTableOf("pkgdomaintypetest.type_tbl_type");
                await using var dataSource = dataSourceBuilder.Build();
                await using var connection = await dataSource.OpenConnectionAsync();

                configurationSteps?.Invoke(connection);

                //await connection.ExecuteNonQueryAsync("SET datestyle TO ISO");

                var commandText = "pkgDomainTypeTest.domainTestProc";
                var cstmt = new EDBCommand(commandText, connection);
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.AllResultTypesAreUnknown = true;

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
                CollectionAssert.AllItemsAreInstancesOfType(arrayList, typeof(T));

                for (int i = 0; i < values.Length; i++)
                {
                    Assert.AreEqual(values[i], arrayList[i]);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                Execute("DROP PACKAGE BODY pkgDomainTypeTest;");
            }
        }

        public class DomainTypeCases : IEnumerable
        {

            public IEnumerator GetEnumerator()
            {
                yield return new TestCaseData("boolean", new bool[] { true, false, true }, null, null);
                yield return new TestCaseData("double precision", new double[] { 0, 1.1, 2.02, -1, -2.345 }, null, null);
                yield return new TestCaseData("smallint", new short[] { 0, 1, 2, -1 }, null, null);
                yield return new TestCaseData("integer", new int[] { 0, 1, 2, -1 }, null, null);
                yield return new TestCaseData("bigint", new long[] { 0, 1, 2, -1 }, null, null);
                yield return new TestCaseData("numeric", new decimal[] { 0m, 1.1m, 2.02m, -1m, -2.345m }, null, null);
                yield return new TestCaseData("real", new float[] { 0, 1.1f, 2.02f, -1f, -2.345f }, null, null);
                yield return new TestCaseData("double precision", new double[] { 0, 1.1, 2.02, -1, -2.345 }, null, null);
                yield return new TestCaseData("money", new decimal[] { 0m, 1.1m, 2.02m, -1m, -2.35m }, null, null);// new Action<EDBConnection>(conn => conn.ExecuteNonQuery("SET lc_monetary='C'")));

                yield return new TestCaseData("text", new string[] { "", "Test1", "Test 2" }, null, null);
                yield return new TestCaseData("character varying", new string[] { "", "Test1", "Test 2" }, null, null);
                yield return new TestCaseData("character", new string[] { "a", "T", "t" }, null, null);
                yield return new TestCaseData("inet", new EDBInet[] { new EDBInet("127.0.0.1"), new EDBInet("192.168.1.1/24") }, null, null);
                yield return new TestCaseData("date", new DateTime[] {
                    new DateTime(2024, 12, 30, 23, 59, 59, DateTimeKind.Unspecified).RemoveSecondsFraction(),
                    new DateTime(2024,12,30,0,0,0, DateTimeKind.Unspecified).RemoveSecondsFraction()
                }, null, null);
            }
        }

        private static string[] ToOracleArrayDecl<T>(T[] values)
        {
            if (typeof(T) == typeof(DateTime))
                return values.Select(v => string.Concat('\'', (v as DateTime?)?.ToString("yyyy-MM-dd HH:mm:ss.fffffzz"), '\'')).ToArray();

            if (typeof(T) == typeof(string)
                || typeof(T) == typeof(char)
                || typeof(T) == typeof(Guid)
                || typeof(T) == typeof(DateTimeOffset)
                || typeof(T) == typeof(TimeSpan)
                || typeof(T) == typeof(EDBCidr)
                || typeof(T) == typeof(EDBInet))
                return values.Select(v => string.Concat('\'', v.ToString(), '\'')).ToArray();

            var result = values.Select(v => v.ToString()).ToArray();
            return result!;
        }
    }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.

}
