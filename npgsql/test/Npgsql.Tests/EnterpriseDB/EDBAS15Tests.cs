using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections.Generic;
using EnterpriseDB.EDBClient.Tests;
using System.Threading;
using System.Collections;
using System.Reflection.PortableExecutable;
using static System.Collections.Specialized.BitVector32;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

public class ct1
{
    public int x;
    public int y;
}

public class ct3
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string x;
    public string y;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
/// <summary>
/// Tests around EC-1001
/// </summary>
/// 
[TestFixture]
[NonParallelizable]
public class EDBAS15Tests : EPASTestBase
{
    EDBConnection? con = null;

    [SetUp]
    public void Init()
    {
        con = OpenConnection();
    }

    [TearDown]
    public void Dispose()
    {
        TestUtil.closeDB(con);
        con?.Dispose();
    }

    private int Execute(string query)
    {
        try
        {
            using (var com = new EDBCommand(query, con))
            {
                com.CommandType = CommandType.Text;
                return com.ExecuteNonQuery();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return 0;
    }

    //For DB_2021_RowVarMultipleItemINTOListTest because Composite type functionality has changed.
    private static async Task<int> Execute(EDBConnection? con, string query)
    {
        try
        {
            await using (var com = new EDBCommand(query, con))
            {
                com.CommandType = CommandType.Text;
                return await com.ExecuteNonQueryAsync();
            }
        }
        catch { }
        return 0;
    }

    // //--DB-1609 : Support for full support of Oracle Style Outer (Like: LEFT OUTER JOIN statement using the +)
    [Test]
    public void DB_1609_OracleStyleOuterTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        Execute("DROP TABLE jobhist1");
        Execute("DROP TABLE emp_test1");
        Execute("DROP TABLE dept1");
        Execute("CREATE TABLE dept1 ("
       + "  dept1no          NUMBER(2) NOT NULL CONSTRAINT dept1_pk PRIMARY KEY,"
       + "  dname           VARCHAR2(14) CONSTRAINT dept1_dname_uq UNIQUE,"
       + "  loc             VARCHAR2(13)"
       + " );");

        Execute("CREATE TABLE emp_test1 ("
                   + "  emp1no           NUMBER(4) NOT NULL CONSTRAINT emp1_pk PRIMARY KEY,"
                   + "  ename           VARCHAR2(10),"
                   + "  job             VARCHAR2(9),"
                   + "  mgr             NUMBER(4),"
                   + "  hiredate        DATE,"
                   + "  sal             NUMBER(7,2) CONSTRAINT emp1_sal_ck CHECK (sal > 0),"
                   + "  comm            NUMBER(7,2),"
                   + "  dept1no          NUMBER(2) CONSTRAINT emp1_ref_dept1_fk"
                   + " REFERENCES dept1(dept1no)"
                   + " );");

        Execute("CREATE TABLE jobhist1 ("
                   + " emp1no           NUMBER(4) NOT NULL,"
                   + "  startdate       DATE NOT NULL,"
                   + "  enddate         DATE,"
                   + "  job             VARCHAR2(9),"
                   + "  sal             NUMBER(7,2),"
                   + "  comm            NUMBER(7,2),"
                   + "  dept1no          NUMBER(2),"
                   + "  chgdesc         VARCHAR2(80),"
                   + "  CONSTRAINT jobhist1_pk PRIMARY KEY (emp1no, startdate),"
                   + "  CONSTRAINT jobhist1_ref_emp1_fk FOREIGN KEY (emp1no)"
                   + " 	REFERENCES emp_test1(emp1no) ON DELETE CASCADE,"
                   + "  CONSTRAINT jobhist1_ref_dept1_fk FOREIGN KEY (dept1no)"
                   + " 	REFERENCES dept1 (dept1no) ON DELETE SET NULL,"
                   + "  CONSTRAINT jobhist1_date_chk CHECK (startdate <= enddate)"
                   + " );");

        Execute("GRANT ALL ON emp_test1 TO PUBLIC;");
        Execute("GRANT ALL ON dept1 TO PUBLIC;");
        Execute("GRANT ALL ON jobhist1 TO PUBLIC;");

        Execute("INSERT INTO dept1 VALUES (10,'ACCOUNTING','NEW YORK');");
        Execute("INSERT INTO dept1 VALUES (20,'RESEARCH','DALLAS');");
        Execute("INSERT INTO dept1 VALUES (30,'SALES','CHICAGO');");
        Execute("INSERT INTO dept1 VALUES (40,'OPERATIONS','BOSTON');");

        Execute("INSERT INTO emp_test1 VALUES (7369,'SMITH','CLERK',7902,'17-DEC-80',800,NULL,20);");
        Execute("INSERT INTO emp_test1 VALUES (7499,'ALLEN','SALESMAN',7698,'20-FEB-81',1600,300,30);");
        Execute("INSERT INTO emp_test1 VALUES (7521,'WARD','SALESMAN',7698,'22-FEB-81',1250,500,30);");
        Execute("INSERT INTO emp_test1 VALUES (7566,'JONES','MANAGER',7839,'02-APR-81',2975,NULL,20);");
        Execute("INSERT INTO emp_test1 VALUES (7654,'MARTIN','SALESMAN',7698,'28-SEP-81',1250,1400,30);");
        Execute("INSERT INTO emp_test1 VALUES (7698,'BLAKE','MANAGER',7839,'01-MAY-81',2850,NULL,30);");
        Execute("INSERT INTO emp_test1 VALUES (7782,'CLARK','MANAGER',7839,'09-JUN-81',2450,NULL,10);");
        Execute("INSERT INTO emp_test1 VALUES (7788,'SCOTT','ANALYST',7566,'19-APR-87',3000,NULL,20);");
        Execute("INSERT INTO emp_test1 VALUES (7839,'KING','PRESIDENT',NULL,'17-NOV-81',5000,NULL,10);");
        Execute("INSERT INTO emp_test1 VALUES (7844,'TURNER','SALESMAN',7698,'08-SEP-81',1500,0,30);");
        Execute("INSERT INTO emp_test1 VALUES (7876,'ADAMS','CLERK',7788,'23-MAY-87',1100,NULL,20);");
        Execute("INSERT INTO emp_test1 VALUES (7900,'JAMES','CLERK',7698,'03-DEC-81',950,NULL,30);");
        Execute("INSERT INTO emp_test1 VALUES (7902,'FORD','ANALYST',7566,'03-DEC-81',3000,NULL,20);");
        Execute("INSERT INTO emp_test1 VALUES (7934,'MILLER','CLERK',7782,'23-JAN-82',1300,NULL,10);");

        Execute("INSERT INTO jobhist1 VALUES (7369,'17-DEC-80',NULL,'CLERK',800,NULL,20,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7499,'20-FEB-81',NULL,'SALESMAN',1600,300,30,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7521,'22-FEB-81',NULL,'SALESMAN',1250,500,30,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7566,'02-APR-81',NULL,'MANAGER',2975,NULL,20,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7654,'28-SEP-81',NULL,'SALESMAN',1250,1400,30,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7698,'01-MAY-81',NULL,'MANAGER',2850,NULL,30,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7782,'09-JUN-81',NULL,'MANAGER',2450,NULL,10,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7788,'19-APR-87','12-APR-88','CLERK',1000,NULL,20,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7788,'13-APR-88','04-MAY-89','CLERK',1040,NULL,20,'Raise');");
        Execute("INSERT INTO jobhist1 VALUES (7788,'05-MAY-90',NULL,'ANALYST',3000,NULL,20,'Promoted to Analyst');");
        Execute("INSERT INTO jobhist1 VALUES (7839,'17-NOV-81',NULL,'PRESIDENT',5000,NULL,10,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7844,'08-SEP-81',NULL,'SALESMAN',1500,0,30,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7876,'23-MAY-87',NULL,'CLERK',1100,NULL,20,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7900,'03-DEC-81','14-JAN-83','CLERK',950,NULL,10,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7900,'15-JAN-83',NULL,'CLERK',950,NULL,30,'Changed to Dept 30');");
        Execute("INSERT INTO jobhist1 VALUES (7902,'03-DEC-81',NULL,'ANALYST',3000,NULL,20,'New Hire');");
        Execute("INSERT INTO jobhist1 VALUES (7934,'23-JAN-82',NULL,'CLERK',1300,NULL,10,'New Hire');");

        var query = "SELECT count(*) FROM emp_test1 e, dept1 d, jobhist1 j"
                            + " WHERE j.dept1no BETWEEN d.dept1no(+) AND e.dept1no(+);";
        try
        {
            using (var cmd = new EDBCommand(query, con))
            {
                var rs = cmd.ExecuteReader();

                //SELECT count(*) FROM emp_test1 e, dept1 d, jobhist1 j                                                                 
                //WHERE j.dept1no BETWEEN d.dept1no(+) AND e.dept1no(+);
                // count 
                //-------
                //   318
                //(1 row)

                rs.Read();
                Assert.That(rs.GetInt32(0), Is.EqualTo(318));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }

    //--DB-1384 : issue:Index hint is not getting inherited on partitions
    [Test]
    public void DB_1384IndexHintPartitionsTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore
        Execute("DROP TABLE t_1384");
        Execute("CREATE TABLE t_1384(col1 int, col2 int, col3 int)"
            + " PARTITION BY RANGE(col1)"
            + "(       PARTITION p1 VALUES LESS THAN(500),"
            + "	PARTITION p2 VALUES LESS THAN(1000)"
            + ");");

        Execute("ALTER TABLE t_1384 ADD PRIMARY KEY(col1);");
        Execute("CREATE INDEX idx1 ON t_1384(col2);");
        Execute("CREATE INDEX idx2 ON t_1384(col1, col2);");
        Execute("SET enable_hints = true;");
        Execute("SET trace_hints TO on;");

        var query = "EXPLAIN (COSTS OFF) SELECT /*+ INDEX(s t_1384_pkey) */ * FROM t_1384 s"
                    + " WHERE col2 = 10;";

        try
        {
            using (var cmd = new EDBCommand(query, con))
            {
                var rs = cmd.ExecuteReader();

                //test=# EXPLAIN (COSTS OFF) SELECT /*+ INDEX(s t_1384_pkey) */ * FROM t_1384 s
                //test-#         WHERE col2 = 10;
                //INFO:  [HINTS] SeqScan of [s] rejected due to INDEX hint.
                //INFO:  [HINTS] Parallel SeqScan of [s] rejected due to INDEX hint.
                //INFO:  [HINTS] Index Scan of [s].[t_1384_p1_col1_col2_idx] rejected due to INDEX hint.
                //INFO:  [HINTS] Index Scan of [s].[t_1384_p1_col2_idx] rejected due to INDEX hint.
                //INFO:  [HINTS] Index Scan of [s].[t_1384_p1_pkey] accepted.
                //INFO:  [HINTS] SeqScan of [s] rejected due to INDEX hint.
                //INFO:  [HINTS] Parallel SeqScan of [s] rejected due to INDEX hint.
                //INFO:  [HINTS] Index Scan of [s].[t_1384_p2_col1_col2_idx] rejected due to INDEX hint.
                //INFO:  [HINTS] Index Scan of [s].[t_1384_p2_col2_idx] rejected due to INDEX hint.
                //INFO:  [HINTS] Index Scan of [s].[t_1384_p2_pkey] accepted.
                //		     QUERY PLAN                      
                //-----------------------------------------------------
                // Append
                //   ->  Bitmap Heap Scan on t_1384_p1 s_1
                //	 Recheck Cond: (col2 = 10)
                //	 ->  Bitmap Index Scan on t_1384_p1_col2_idx
                //	       Index Cond: (col2 = 10)
                //   ->  Bitmap Heap Scan on t_1384_p2 s_2
                //	 Recheck Cond: (col2 = 10)
                //	 ->  Bitmap Index Scan on t_1384_p2_col2_idx
                //	       Index Cond: (col2 = 10)
                //(9 rows)

                //PSQL output shows some INFO: [HINTS] statements which are missing from .NET output.
                string[] output = {
            "Append",
            "  ->  Bitmap Heap Scan on t_1384_p1 s_1",
            "        Recheck Cond: (col2 = 10)",
            "        ->  Bitmap Index Scan on t_1384_p1_col2_idx",
            "              Index Cond: (col2 = 10)",
            "  ->  Bitmap Heap Scan on t_1384_p2 s_2",
            "        Recheck Cond: (col2 = 10)",
            "        ->  Bitmap Index Scan on t_1384_p2_col2_idx",
            "              Index Cond: (col2 = 10)"
            };

                var index = 0;
                while (rs.Read())
                {
                    Assert.That(rs.GetString(0).ToString(), Is.EqualTo(output[index]));
                    index++;
                }

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }

    //--DB-2021 : Quick fix/Assess of ERROR row variable cannot be part of a multiple-item INTO list
    [Test]
    public async Task DB_2021_RowVarMultipleItemINTOListTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        await using var adminConnection = await OpenConnectionAsync();
        await Execute(adminConnection, "DROP FUNCTION IF EXISTS db2021_fn2");
        await Execute(adminConnection, "DROP TYPE IF EXISTS ct1");
        await Execute(adminConnection, "DROP TYPE IF EXISTS ct3");
        await Execute(adminConnection, "CREATE TYPE ct1 AS (x int, y int);");
        await Execute(adminConnection, "CREATE TYPE ct3 AS (x text, y text);");
        await Execute(adminConnection, "CREATE OR REPLACE FUNCTION db2021_fn2(OUT o1 ct1, OUT o2 ct3) RETURN void AS "
                + " BEGIN "
                + "   o1.x := 10;"
                + "   o1.y := 20;"
                + "   o2.x := 'ten';"
                + "   o2.y := 'twenty';"
                + "   return;"
                + " END;");
        await adminConnection.CloseAsync();

        var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
        dataSourceBuilder.MapComposite<ct1>("public.ct1");
        dataSourceBuilder.MapComposite<ct3>("public.ct3");
        //dataSourceBuilder.MapComposite<ct1>();
        //dataSourceBuilder.MapComposite<ct3>();
        await using var dataSource = dataSourceBuilder.Build();

        await using var connection = await dataSource.OpenConnectionAsync();

        //await connection.OpenAsync();

        await connection.ReloadTypesAsync();

        //Close and reopen the connection so that custom types are reloaded.
        //TestUtil.closeDB(conn);
        //EDBConnection.GlobalTypeMapper.MapComposite<ct1>("public.ct1");
        //EDBConnection.GlobalTypeMapper.MapComposite<ct3>("public.ct3");
        //conn = OpenConnection();

        try
        {
            await using var callProc = new EDBCommand("db2021_fn2", connection);
            callProc.CommandType = CommandType.StoredProcedure;
            EDBCommandBuilder.DeriveParameters(callProc);
            await callProc.PrepareAsync();
            await callProc.ExecuteNonQueryAsync();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var obj1 = (ct1)callProc.Parameters[0].Value;
            Assert.That( obj1!.x, Is.EqualTo(10));
            Assert.That(obj1!.y, Is.EqualTo(20));

            var obj2 = (ct3)callProc.Parameters[1].Value;
            Assert.That(obj2!.x, Is.EqualTo("ten"));
            Assert.That(obj2!.y, Is.EqualTo("twenty"));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while executing SP: " + exp.Message + "\n" + exp.StackTrace);
        }

        await connection.CloseAsync();
    }

    //Same as DB_2021_RowVarMultipleItemINTOListTest but Procedure instead of Function.
    [Test]
    public async Task DB_2021_RowVarMultipleItemINTOListTest_Proc()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        await using var adminConnection = await OpenConnectionAsync();
        await Execute(adminConnection, "DROP PROCEDURE db2021_proc2");
        await Execute(adminConnection, "DROP TYPE ct1");
        await Execute(adminConnection, "DROP TYPE ct3");
        await Execute(adminConnection, "CREATE TYPE ct1 AS (x int, y int);");
        await Execute(adminConnection, "CREATE TYPE ct3 AS (x text, y text);");
        await Execute(adminConnection, "CREATE OR REPLACE PROCEDURE db2021_proc2(OUT o1 ct1, OUT o2 ct3) AS "
                + " BEGIN "
                + "   o1.x := 10;"
                + "   o1.y := 20;"
                + "   o2.x := 'ten';"
                + "   o2.y := 'twenty';"
                + " END;");
        await adminConnection.CloseAsync();

        var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
        dataSourceBuilder.MapComposite<ct1>("public.ct1");
        dataSourceBuilder.MapComposite<ct3>("public.ct3");
        await using var dataSource = dataSourceBuilder.Build();

        await using var connection = await dataSource.OpenConnectionAsync();

        await connection.ReloadTypesAsync();

        try
        {
            await using var callProc = new EDBCommand("db2021_proc2", connection);
            callProc.CommandType = CommandType.StoredProcedure;
            EDBCommandBuilder.DeriveParameters(callProc);
            await callProc.PrepareAsync();
            await callProc.ExecuteNonQueryAsync();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var obj1 = (ct1)callProc.Parameters[0].Value;
            Assert.That(obj1!.x, Is.EqualTo(10));
            Assert.That(obj1!.y, Is.EqualTo(20));

            var obj2 = (ct3)callProc.Parameters[1].Value;
            Assert.That(obj2!.x, Is.EqualTo("ten"));
            Assert.That(obj2!.y, Is.EqualTo("twenty"));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while executing SP: " + exp.Message + "\n" + exp.StackTrace);
        }

        await connection.CloseAsync();
    }

    //Same as DB_2021_RowVarMultipleItemINTOListTest but with INOUT instead of OUT parameters.
    [Test]
    public async Task DB_2021_RowVarMultipleItemINTOListTest2()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        await using var adminConnection = await OpenConnectionAsync();
        await Execute(adminConnection, "DROP FUNCTION db2021_fn1");
        await Execute(adminConnection, "DROP TYPE ct1");
        await Execute(adminConnection, "DROP TYPE ct3");
        await Execute(adminConnection, "CREATE TYPE ct1 AS (x int, y int);");
        await Execute(adminConnection, "CREATE TYPE ct3 AS (x text, y text);");
        await Execute(adminConnection, "CREATE OR REPLACE FUNCTION db2021_fn1(INOUT o1 ct1, INOUT o2 ct3) RETURN void AS "
                + " BEGIN "
                + "   o1.x := 10;"
                + "   o1.y := 20;"
                + "   o2.x := 'ten';"
                + "   o2.y := 'twenty';"
                + "   return;"
                + " END;");
        await adminConnection.CloseAsync();

        var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
        dataSourceBuilder.MapComposite<ct1>("public.ct1");
        dataSourceBuilder.MapComposite<ct3>("public.ct3");
        await using var dataSource = dataSourceBuilder.Build();

        await using var connection = await dataSource.OpenConnectionAsync();

        await connection.ReloadTypesAsync();

        try
        {
            await using var callProc = new EDBCommand("db2021_fn1", connection);
            callProc.CommandType = CommandType.StoredProcedure;
            EDBCommandBuilder.DeriveParameters(callProc);

            callProc.Parameters[0].Value = new ct1();
            callProc.Parameters[1].Value = new ct3();

            await callProc.PrepareAsync();
            await callProc.ExecuteNonQueryAsync();

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            var obj1 = (ct1)callProc.Parameters[0].Value;
            Assert.That(obj1!.x, Is.EqualTo(10));
            Assert.That(obj1!.y, Is.EqualTo(20));

            var obj2 = (ct3)callProc.Parameters[1].Value;
            Assert.That(obj2!.x, Is.EqualTo("ten"));
            Assert.That(obj2!.y, Is.EqualTo("twenty"));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while executing SP: " + exp.Message + "\n" + exp.StackTrace);
        }

        await connection.CloseAsync();
    }

    //--DB-1634 : Oracle Function : TO_DSINTERVAL()
    [Test]
    public void DB_1634_TO_DSINTERVALTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        var query = "select to_dsinterval('80 13:30:00');";

        try
        {
            using (var cmd = new EDBCommand(query, con))
            {
                var rs = cmd.ExecuteReader();

                //select to_dsinterval('80 13:30:00');
                //  to_dsinterval   
                //------------------
                // 80 days 13:30:00
                //(1 row)

                rs.Read();
                //Assert.That(rs.GetValue(0).ToString(), Is.EqualTo("80 days 13:30:00"));

                //GetInterval() method removed. GetValue() returns the following.
                Assert.That(rs.GetValue(0).ToString(), Is.EqualTo("80.13:30:00"));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }

    //--DB-1618 : Oracle Function : NVL(DOUBLE PRECISION)
    [Test]
    public void DB_1618_NVL_DOUBLEPRECISIONTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        Execute("DROP TABLE db1618_t1");
        Execute("CREATE TABLE db1618_t1(dp1 double precision, dp2 double precision, nr NUMERIC(4, 2), it INTEGER)");

        var rowcount = Execute("INSERT INTO db1618_t1 VALUES(11.11, NULL, 33.33, 44);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO db1618_t1 VALUES(NULL, 22.22, 55.55, 66.66);");
        Assert.That(rowcount, Is.EqualTo(1));

        var query = "SELECT NVL(dp1, dp2) FROM db1618_t1 ORDER BY 1;";

        try
        {
            using (var cmd = new EDBCommand(query, con))
            {
                var rs = cmd.ExecuteReader();

                //SELECT NVL(dp1, dp2) FROM db1618_t1 ORDER BY 1;
                //  nvl  
                //-------
                // 11.11
                // 22.22
                //(2 rows)

                rs.Read();
                Assert.That(rs.GetDouble(0),Is.EqualTo(11.11));
                rs.Read();
                Assert.That(rs.GetDouble(0), Is.EqualTo(22.22));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }

    //--DB-1692 : Oracle Function : from_tz ()
    [Test]
    public void DB_1692_from_tzTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        var query = "SELECT FROM_TZ(TIMESTAMP '2017-08-08 08:09:10', 'Asia/Kolkata') FROM DUAL;";

        try
        {
            using (var cmd = new EDBCommand(query, con))
            {
                var rs = cmd.ExecuteReader();

                //SELECT FROM_TZ(TIMESTAMP '2017-08-08 08:09:10', 'Asia/Kolkata') FROM DUAL;
                //          from_tz          
                //---------------------------
                // 07-AUG-17 19:39:10 -07:00
                //(1 row)

                rs.Read();
                Assert.That(rs.GetDateTime(0), Is.EqualTo(new DateTime(2017, 8, 8, 2, 39, 10)));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }
    //--DB-1708 : Implement Oracle TO_NCHAR Function in Advanced Server
    [Test]
    public void DB_1708_TO_NCHARTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        var query = "SELECT to_nchar(7654321, 'C9G999G999D99'),to_nchar(timestamp '2022-04-20 17:31:12.66', 'Day: MONTH DD, YYYY');";

        try
        {
            using (var cmd = new EDBCommand(query, con))
            {
                var rs = cmd.ExecuteReader();

                //SELECT to_nchar(7654321, 'C9G999G999D99'),to_nchar(timestamp '2022-04-20 17:31:12.66', 'Day: MONTH DD, YYYY');
                //   to_nchar    |           to_nchar            
                //---------------+-------------------------------
                //  7,654,321.00 | Wednesday: APRIL     20, 2022
                //(1 row)

                rs.Read();
                Assert.That(rs.GetString(0),Is.EqualTo(" 7,654,321.00"));
                Assert.That(rs.GetString(1),Is.EqualTo("Wednesday: APRIL     20, 2022"));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }
    //--DB-1703 : Oracle Function : Add TO_CLOB() & TO_BLOB()
    [Test]
    public void DB_1703_TO_CLOB_and_TO_BLOBTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        try
        {
            //SELECT to_blob('unknown string'), to_blob('row string'::RAW), to_blob('long raw string'::LONG RAW);
            //            to_blob             |        to_blob         |             to_blob              
            //--------------------------------+------------------------+----------------------------------
            // \x756e6b6e6f776e20737472696e67 | \x726f7720737472696e67 | \x6c6f6e672072617720737472696e67
            //(1 row)
            using (var cmd = new EDBCommand("SELECT to_blob('unknown string'), to_blob('row string'::RAW), to_blob('long raw string'::LONG RAW);", con))
            {
                var rs = cmd.ExecuteReader();
                rs.Read();
                Assert.That(rs.GetFieldType(0), Is.EqualTo(typeof(byte[])));
                Assert.That(rs.GetFieldType(1), Is.EqualTo(typeof(byte[])));
                Assert.That(rs.GetFieldType(2), Is.EqualTo(typeof(byte[])));

                rs.Close();
            }

            //SELECT to_clob('unknown string'), to_clob('c'::CHAR), to_clob('varchar2 string'::VARCHAR),
            //test-#        to_clob('n'::NCHAR), to_clob('nvarchar2 string'::NVARCHAR2), to_clob('clob string'::CLOB);
            //    to_clob     | to_clob |     to_clob     | to_clob |     to_clob      |   to_clob   
            //----------------+---------+-----------------+---------+------------------+-------------
            // unknown string | c       | varchar2 string | n       | nvarchar2 string | clob string
            //(1 row)
            var query = "SELECT to_clob('unknown string'), to_clob('c'::CHAR), to_clob('varchar2 string'::VARCHAR),"
                   + " to_clob('n'::NCHAR), to_clob('nvarchar2 string'::NVARCHAR2), to_clob('clob string'::CLOB);";
            using (var cmd = new EDBCommand(query, con))
            {
                var rs = cmd.ExecuteReader();
                rs.Read();
                Assert.That(rs.GetString(0), Is.EqualTo("unknown string"));
                Assert.That(rs.GetString(1), Is.EqualTo("c"));
                Assert.That(rs.GetString(2), Is.EqualTo("varchar2 string"));
                Assert.That(rs.GetString(3), Is.EqualTo("n"));
                Assert.That(rs.GetString(4), Is.EqualTo("nvarchar2 string"));
                Assert.That(rs.GetString(5), Is.EqualTo("clob string"));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }

    //--DB-1714 : Implement Oracle SQLERRM and SQLCODE Functions in Advanced Server
    [Test]
    public void DB_1714SQLERRM_and_SQLCODETest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        Execute("DROP FUNCTION excpt_test");
        Execute("DROP FUNCTION excpt_test2");

        //The actual FUNCTION in the Server test is as follows. We have created two functions for the two Oracle functions.
        /*
        CREATE OR REPLACE FUNCTION excpt_test2() RETURN text IS
        BEGIN
           BEGIN
           perform 1/0;
           exception
           when others then
           raise notice 'In SPL Exception, sqlcode(): %', sqlcode();
           raise notice 'In SPL Exception, sqlcode: %', sqlcode;
           raise notice 'In SPL Exception, sqlerrm(sqlcode): %', sqlerrm(sqlcode);
           raise notice 'In SPL Exception, sqlerrm(sqlcode()): %', sqlerrm(sqlcode());
           raise notice 'In SPL Exception, sqlerrm(): %', sqlerrm();
           raise notice 'In SPL Exception, sqlerrm: %', sqlerrm;
           return sqlerrm();
        END;
        END;
        */

        Execute("CREATE OR REPLACE FUNCTION excpt_test() RETURN int IS"
                + " BEGIN "
                + "    BEGIN "
                + "    perform 1/0; "
                + "    exception "
                + "    when others then "
                + "    return sqlcode; "
                + " END; "
                + " END;");

        Execute("CREATE OR REPLACE FUNCTION excpt_test2() RETURN text IS"
                + " BEGIN "
                + "    BEGIN "
                + "    perform 1/0; "
                + "    exception "
                + "    when others then "
                + "    return sqlerrm(); "
                + " END; "
                + " END;");

        try
        {
            using (var cmd = new EDBCommand("select excpt_test();", con))
            {
                var rs = cmd.ExecuteReader();

                rs.Read();
                Assert.That(rs.GetInt32(0), Is.EqualTo(-1476));

                rs.Close();
            }

            using (var cmd = new EDBCommand("select excpt_test2();", con))
            {
                var rs = cmd.ExecuteReader();

                rs.Read();
                Assert.That(rs.GetString(0), Is.EqualTo("division by zero"));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }

    //--DB-1711 : Implement Oracle TO_MULTI_BYTE Function in Advanced Server
    [Test]
    public void DB_1711_TO_MULTI_BYTETest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        try
        {
            using (var cmd = new EDBCommand("SELECT to_multi_byte('ABC&123');", con))
            {
                var rs = cmd.ExecuteReader();

                rs.Read();
                Assert.That(rs.GetString(0), Is.EqualTo("ＡＢＣ＆１２３"));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }

    //--DB-1814 : Implement Oracle TO_SINGLE_BYTE Function in Advanced Server
    [Test]
    public void DB_1814_TO_SINGLE_BYTETest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        try
        {
            using (var cmd = new EDBCommand("SELECT to_single_byte('ＡＢＣ＆１２３');", con))
            {
                var rs = cmd.ExecuteReader();

                rs.Read();
                Assert.That(rs.GetString(0), Is.EqualTo("ABC&123"));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }

    //--DB-1408 : [MP] Support for IN parameter mention for Dynamic SQL inside PL/SQL block
    [Test]
    public void DB_1408_INParamInPLSQLBlockTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        Execute("DROP PROCEDURE db1408_pr_in");
        Execute("DROP FUNCTION db1408_fn_double");
        Execute("DROP PROCEDURE db1408_pr_print");

        Execute("CREATE OR REPLACE PROCEDURE db1408_pr_print(n IN number) AS "
        + "BEGIN "
        + "  raise notice 'number %', n; "
        + "END;");

        Execute("CREATE OR REPLACE FUNCTION db1408_fn_double(n IN number) RETURN number IS "
                + "BEGIN "
                + "  raise notice 'number %', n; "
                + "  return n * 2; "
                + "END;");

        Execute("CREATE OR REPLACE PROCEDURE db1408_pr_in(arg_in number) IS "
                + "BEGIN "
                + "  EXECUTE IMMEDIATE 'BEGIN db1408_pr_print(:arg_in); END;' "
                + "    USING IN arg_in; "
                + "END;");

        //Call function
        var query = "select db1408_fn_double(10);";

        try
        {
            using (var cmd = new EDBCommand(query, con))
            {
                var rs = cmd.ExecuteReader();

                rs.Read();
                Assert.That(rs.GetInt32(0), Is.EqualTo(20));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }

        var mre = new ManualResetEvent(false);
        PostgresNotice? notice = null;
        NoticeEventHandler action = (sender, args) =>
        {
            notice = args.Notice;
            mre.Set();
        };
        con!.Notice += action;
        try
        {
            var callProc = new EDBCommand("db1408_pr_in(:arg_in)", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            callProc.Parameters.Add(new EDBParameter("arg_in", EDBTypes.EDBDbType.Numeric, 10, "arg_in", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 10));
            callProc.Prepare();
            callProc.ExecuteNonQuery();

            mre.WaitOne(5000);
            Assert.That(notice, Is.Not.Null, "No notice was emitted");
            Assert.That(notice!.MessageText, Is.EqualTo("number 10"));
            Assert.That(notice.Severity, Is.EqualTo("NOTICE"));
        }
        finally
        {
            con.Notice -= action;
        }
    }

    //--DB-1948 : Implement redwood compatible MERGE syntax in EPAS
    [Test]
    public void DB_1948RedwoodMERGETest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        Execute("DROP TABLE target;");
        Execute("DROP TABLE source;");

        Execute("CREATE TABLE target(tid integer, balance integer);");
        Execute("CREATE TABLE source(sid integer, delta integer);");

        var rowcount = Execute("INSERT INTO source VALUES (1, 100);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO source VALUES (2, 200);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO source VALUES (3, 300);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO source VALUES (4, 100);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO source VALUES (5, 300);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO source VALUES (6, 600);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO target VALUES (1, 0);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO target VALUES (2, 20);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO target VALUES (3, 0);");

        var anony = "BEGIN; "
                + " MERGE INTO target t "
                + " USING source s "
                + " ON (t.tid = s.sid) "
                + " WHEN MATCHED THEN "
                + "     UPDATE SET balance = s.delta WHERE balance = 0 "
                + " WHEN NOT MATCHED THEN "
                + "     INSERT VALUES (s.sid, s.delta) WHERE s.sid >= 5;";

        Execute(anony);

        try
        {
            using (var cmd = new EDBCommand("select * from target order by tid;", con))
            {
                var rs = cmd.ExecuteReader();

                //select * from target order by tid;
                // tid | balance 
                //-----+---------
                //   1 |     100
                //   2 |      20
                //   3 |     300
                //   5 |     300
                //   6 |     600
                //(5 rows)

                rs.Read();
                Assert.That(rs.GetInt32(0), Is.EqualTo(1));
                Assert.That(rs.GetInt32(1), Is.EqualTo(100));
                rs.Read();
                Assert.That(rs.GetInt32(0), Is.EqualTo(2));
                Assert.That(rs.GetInt32(1), Is.EqualTo(20));
                rs.Read();
                Assert.That(rs.GetInt32(0), Is.EqualTo(3));
                Assert.That(rs.GetInt32(1), Is.EqualTo(300));
                rs.Read();
                Assert.That(rs.GetInt32(0), Is.EqualTo(5));
                Assert.That(rs.GetInt32(1), Is.EqualTo(300));
                rs.Read();
                Assert.That(rs.GetInt32(0), Is.EqualTo(6));
                Assert.That(rs.GetInt32(1), Is.EqualTo(600));

                rs.Close();
            }
        }
        catch (Exception exp)
        {
            Assert.Fail("Error while retrieving values: " + exp.Message);
        }
    }

    //--DB-1536 : Need to be able to support various Oracle Compatible Stack Trace printing functions.
    [Test]
    public void DB_1536_OracleCompatibleStackTraceTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        Execute("DROP PROCEDURE how_much_fund");
        Execute("CREATE OR REPLACE PROCEDURE how_much_fund( ) AS"
        + "   low_fund EXCEPTION;"
        + "   bal_fund number := 0;"
        + "BEGIN"
        + "   if bal_fund < 1000 then"
        + "      raise low_fund;"
        + "   end if;"
        + "EXCEPTION "
        + "   WHEN low_fund then"
        + "   BEGIN"
        + "	DBMS_OUTPUT.PUT_LINE('low_fund raised. ...');"
        + "	DBMS_OUTPUT.PUT_LINE(DBMS_UTILITY.FORMAT_CALL_STACK);"
        + "   END;"
        + "   WHEN others then"
        + "	DBMS_OUTPUT.PUT_LINE('no_funds raised. ...');"
        + "END;");

        Execute("how_much_fund");
    }

    //DB-1691 : Oracle Function : XML extractvalue()
    [Test]
    public void DB_1691XMLextractvalueTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore

        Execute("DROP TABLE xmltest_db1691");
        Execute("CREATE TABLE xmltest_db1691(id int, data xml);");

        var rowcount = Execute("INSERT INTO xmltest_db1691 VALUES (1, '<menu><beers><name>Budvar</name><cost>free</cost><name>Carling</name><cost>lots</cost></beers></menu>'::xml);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO xmltest_db1691 VALUES (2, '<menu><beers><name>Molson</name><cost>free</cost><name>Carling</name><cost>lots</cost></beers></menu>'::xml);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO xmltest_db1691 VALUES (3, '<myns:menu xmlns:myns=\"http://myns.com\"><myns:beers><myns:name>Budvar</myns:name><myns:cost>free</myns:cost><myns:name>Carling</myns:name><myns:cost>lots</myns:cost></myns:beers></myns:menu>'::xml);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO xmltest_db1691 VALUES (4, '<myns:menu xmlns:myns=\"http://myns.com\"><myns:beers><myns:name>Molson</myns:name><myns:cost>free</myns:cost><myns:name>Carling</myns:name><myns:cost>lots</myns:cost></myns:beers></myns:menu>'::xml);");
        Assert.That(rowcount, Is.EqualTo(1));

        var query = "SELECT extractvalue(xmltest_db1691.data, '/menu/beers/name[position()=1]')"
                + "  FROM xmltest_db1691 ORDER BY id;";

        var cmd = new EDBCommand(query, con);
        var rs = cmd.ExecuteReader();

        //test=# SELECT extractvalue(xmltest_db1691.data, '/menu/beers/name[position()=1]')
        //test-#   FROM xmltest_db1691 ORDER BY id;
        // extractvalue 
        //--------------
        // Budvar
        // Molson

        rs.Read();
        Assert.That(rs.GetString(0), Is.EqualTo("Budvar"));
        rs.Read();
        Assert.That(rs.GetString(0), Is.EqualTo("Molson"));
    }

    //--DB-1410 : Support for HTP and HTF package
    [Test]
    public void DB_1410_HTP_HTF_PackageTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore
        //Cleanup just in case
        Execute("DROP PROCEDURE p;");

        //Actual test
        Execute("CREATE OR REPLACE PROCEDURE p "
        + " AS"
        + " BEGIN"

        + " htp.p(htf.base('www.google.com','e'));"
        + " htp.p(htf.base('',''));"
        + " htp.p(htf.base(''));"
        + " htp.p(htf.base());"
        + " htp.p(htf.base);"
        + " htp.showpage;"

        + " END;");

        //exec p; shows the following output on PSQL client.
        //<base target="www.google.com" e href="http://:" />
        //<base href="http://:" />
        //<base href="http://:" />
        //<base href="http://:" />
        //<base href="http://:" />
        //
        //Not sure how to get this in .NET.
        Execute("p");
    }

    //--DB-1425: SET ROW: Initial Investigation, and come with rough design.
    [Test]
    public void DB_1425SetRowTest()
    {
#nullable disable
        TestUtil.MinimumPgVersion(con, "15.0.0");
#nullable restore
        //Cleanup just in case.
        Execute("DROP TABLE db1425_t1;");

        //Actual test
        Execute("CREATE TABLE db1425_t1(a INT, z INT, b INT);");
        Execute("ALTER TABLE db1425_t1 DROP COLUMN z;");

        var rowcount = Execute("INSERT INTO db1425_t1 VALUES(1,2);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO db1425_t1 VALUES(2,2);");
        Assert.That(rowcount, Is.EqualTo(1));
        rowcount = Execute("INSERT INTO db1425_t1 VALUES(3,2);");
        Assert.That(rowcount, Is.EqualTo(1));

        var anony = """
            DECLARE    
            	TYPE rec IS RECORD (x INT, y INT);
            	rec_var rec;
            	row_var db1425_t1%ROWTYPE;
            	comp_var db1425_t1;
            BEGIN    
            	rec_var = row(1000, 1000);    
            	UPDATE db1425_t1 SET ROW=rec_var WHERE a = 1;	
            	row_var.a = 2000;	
            	row_var.b = 2000;    
            	UPDATE db1425_t1 SET ROW=row_var WHERE a = 2;	
            	comp_var = row(3000, 3000);    
            	UPDATE db1425_t1 SET ROW=comp_var WHERE a = 3;
            END;
            """;
        Execute(anony);

        using (var cmd = new EDBCommand("SELECT * FROM db1425_t1 ORDER BY a, b", con))
        {
            var rs = cmd.ExecuteReader();

            //SELECT * FROM db1425_t1 ORDER BY a, b;
            //  a   |  b   
            //------+------
            // 1000 | 1000
            // 2000 | 2000
            // 3000 | 3000
            //(3 rows)

            rs.Read();
            Assert.That(rs.GetInt32(0), Is.EqualTo(1000));
            Assert.That(rs.GetInt32(1), Is.EqualTo(1000));
            rs.Read();
            Assert.That(rs.GetInt32(0), Is.EqualTo(2000));
            Assert.That(rs.GetInt32(1), Is.EqualTo(2000));
            rs.Read();
            Assert.That(rs.GetInt32(0), Is.EqualTo(3000));
            Assert.That(rs.GetInt32(1), Is.EqualTo(3000));

            rs.Close();
        }
    }
}
