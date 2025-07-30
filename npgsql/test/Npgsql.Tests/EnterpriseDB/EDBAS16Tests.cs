using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
//using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Dynamic;
using System.IO;
using EnterpriseDB.EDBClient.Tests.Support;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;

#pragma warning disable CS8602
[TestFixture]
[NonParallelizable]
internal class EDBAS16Tests : EPASTestBase
{
    private static async Task<int> Execute(EDBConnection conn, string query, bool ignoreResult)
    {
        try
        {
            //await using var conn = await OpenConnectionAsync();

            using (var com = new EDBCommand("", conn))
            {
                com.CommandType = CommandType.Text;

                com.CommandText = query;
                return await com.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            //In case of drop statement, the object may not exist.
            //So we do not care about the result.
            if (!ignoreResult)
                Assert.Fail(ex.Message);
        }

        return 0;
    }
    private async Task<int> Execute(string query, bool ignoreResult)
    {
        try
        {
            await using var conn = await OpenConnectionAsync();

            using (var com = new EDBCommand("", conn))
            {
                com.CommandType = CommandType.Text;

                com.CommandText = query;
                return await com.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            //In case of drop statement, the object may not exist.
            //So we do not care about the result.
            if (!ignoreResult)
                Assert.Fail(ex.Message);
        }

        return 0;
    }

    //Simple select returning single value
    private static async Task<object?> ExecuteSimpleReader(EDBConnection conn, string query)
    {
        object? val = null;
        try
        {
            using (var com = new EDBCommand("", conn))
            {
                com.CommandType = CommandType.Text;

                com.CommandText = query;
                var reader = await com.ExecuteReaderAsync();

                Assert.IsTrue(reader.HasRows);

                if (await reader.ReadAsync())
                {
                    val = reader.GetValue(0);
                }
                await reader.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
        return val;
    }

    private static async Task<DateTime?> ExecuteDateTimeReader(EDBConnection conn, string query)
    {
        DateTime? val = null;
        try
        {
            using (var com = new EDBCommand("", conn))
            {
                com.CommandType = CommandType.Text;

                com.CommandText = query;
                var reader = await com.ExecuteReaderAsync();

                Assert.IsTrue(reader.HasRows);

                if (await reader.ReadAsync())
                {
                    val = reader.GetDateTime(0);
                }
                await reader.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
        return val;
    }

    //Executes a stored procedure with no arguments
    //If there are any messages from dbms_output.put_line, they are retured as a list.
    public static async Task<List<string>> ExecuteProcNotice(EDBConnection conn, string sqlStr)
    {
        var messages = new List<string>();

        //await using var conn = await OpenConnectionAsync();

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

                await cstmt.PrepareAsync();
                await cstmt.ExecuteNonQueryAsync();
            }
            mre.WaitOne(5000);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                messages.Add(notice.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();

        return messages;
    }

    //--DB-1712 : Implement DBMS_UTILITY Subprograms Not Currently Implemented in Advanced Server
    [Test]
    public async Task DBMS_UTILITYSubprogramsTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore
        //Clean
        await Execute(conn, "DROP PROCEDURE test_expand_sql_text_select_valid", true);
        await Execute(conn, "DROP PROCEDURE test_expand_sql_text_noselect_invalid", true);

        await Execute(conn, "DROP VIEW v1", true);
        await Execute(conn, "DROP VIEW v2", true);
        await Execute(conn, "DROP TABLE db1712_t1 CASCADE", true);
        await Execute(conn, "DROP TABLE db1712_t2 CASCADE", true);
        await Execute(conn, "DROP TABLE db1712_t3 CASCADE", true);

        //Setup
        await Execute(conn, "CREATE TABLE db1712_t1(c1 INT, c2 TEXT);", false);
        await Execute(conn, "CREATE TABLE db1712_t2(deptno NUMBER(2) PRIMARY KEY, dname VARCHAR2(14), loc VARCHAR2(13));", false);
        await Execute(conn, "CREATE TABLE db1712_t3(empno NUMBER(4) PRIMARY KEY, ename VARCHAR2(10), job VARCHAR2(9), " +
            "deptno NUMBER(2) CONSTRAINT FK_DEPTNO REFERENCES db1712_t2);", false);

        await Execute(conn, "CREATE VIEW v1 AS SELECT * FROM db1712_t1;", false);
        await Execute(conn, "CREATE OR REPLACE VIEW v2 AS SELECT e.empno, e.ename, e.job, e.deptno, d.dname FROM db1712_t3 " +
            "e JOIN db1712_t2 d ON e.deptno = d.deptno;", false);

        await Execute(conn, "CREATE OR REPLACE PROCEDURE test_expand_sql_text_select_valid()\n"
                    + "IS\n"
                    + "  result CLOB;\n"
                    + "BEGIN\n"
                    + "  DBMS_UTILITY.expand_sql_text (\n"
                    + "    input_sql_text  => 'SELECT * from v1',\n"
                    + "    output_sql_text => result\n"
                    + "  );\n"
                    + "  DBMS_OUTPUT.put_line(result);\n"
                    + "END;", false);

        await Execute(conn, "CREATE OR REPLACE PROCEDURE test_expand_sql_text_noselect_invalid()\n"
                    + "IS\n"
                    + "  l_clob text;\n"
                    + "BEGIN\n"
                    + "  DBMS_UTILITY.expand_sql_text (\n"
                    + "    input_sql_text  => 'DELETE FROM non_existing_view',\n"
                    + "    output_sql_text => l_clob\n"
                    + "  );\n"
                    + "  DBMS_OUTPUT.put_line(l_clob);\n"
                    + "END;\n", false);

        //--Test expand_sql_text() for simple view
        var msg1 = " SELECT c1,\n"
                    + "    c2\n"
                    + "   FROM ( SELECT db1712_t1.c1,\n"
                    + "            db1712_t1.c2\n"
                    + "           FROM db1712_t1) v1";
        var message1 = await ExecuteProcNotice(conn, "test_expand_sql_text_select_valid");
        Assert.AreEqual(1, message1.Count);
        Assert.AreEqual(msg1, message1[0]);

        //--Only SELECT command is allowed in input string
        //In this case the procedure call throws exception.
        var msg2 = "P0001: only SELECT statement is supported by DBMS_UTILITY.EXPAND_SQL_TEXT";
        try
        {
            var message2 = await ExecuteProcNotice(conn, "test_expand_sql_text_noselect_invalid");
            Assert.AreEqual(5, message2.Count);
        }
        catch (Exception ex)
        {
            Assert.AreEqual(msg2, ex.Message);
        }
    }

    //--DB-1960 : Implement the Oracle SESSIONTIMEZONE function in Advanced Server
    [Test]
    public async Task Oracle_SESSIONTIMEZONE_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore
        //Clean
        var tz1 = await ExecuteSimpleReader(conn, "SELECT sessiontimezone FROM dual;");
        Assert.IsNotNull(tz1);

        ////We are only checking the first part as we are not sure about the second part.
        ////Also the actual test is to set and get values.
        // XFI : removed, results may vary as initial timezone can be different depending on local setup
        //Assert.IsTrue(tz1.ToString().StartsWith("America"));

        await Execute(conn, "SET timezone TO '-5:30';", true);

        var tz2 = await ExecuteSimpleReader(conn, "SELECT sessiontimezone FROM dual;");
        Assert.IsNotNull(tz2);
        Assert.AreEqual("-5:30", tz2.ToString());

        await Execute(conn, "ALTER SESSION SET timezone='Europe/Berlin';", true);

        var tz3 = await ExecuteSimpleReader(conn, "SELECT sessiontimezone FROM dual;");
        Assert.IsNotNull(tz3);
        Assert.AreEqual("Europe/Berlin", tz3.ToString());
    }

    //--DB-2028 : FETCH cursor BULK COLLECT INTO x, y
    [Test]
    public async Task FetchCursorBulkCollectIntoxyTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore
        //Clean

        await Execute(conn, "DROP table db2028_tab", true);
        await Execute(conn, "DROP type db2028_tt", true);
        await Execute(conn, "DROP type db2028_tt1", true);

        await Execute(conn, "create type db2028_tt as object(a int, b varchar2(10));", false);
        await Execute(conn, "create type db2028_tt1 as object(c varchar2(10), d int);", false);
        await Execute(conn, "create table db2028_tab(x db2028_tt, s1 number, y db2028_tt1, s2 timestamp);", false);
        await Execute(conn, "insert into db2028_tab values(db2028_tt(1, '10'), 1.1, db2028_tt1('100', 1000), '1-Jan-2021 12:30:11 PM');", false);
        await Execute(conn, "insert into db2028_tab values(db2028_tt(2, '20'), 2.2, db2028_tt1('200', 2000), '2-Feb-2022 08:40:52 AM');", false);
        await Execute(conn, "insert into db2028_tab values(db2028_tt(3, '30'), 3.3, db2028_tt1('300', 3000), '3-Mar-2023 04:20:33 AM');", false);

        await Execute(conn, "CREATE OR REPLACE PROCEDURE fetch_cursor_bulk_collection_intoxy()\n"
            + "IS\n"
            + "  TYPE tt_tbl  IS TABLE OF db2028_tt  INDEX BY BINARY_INTEGER;\n"
            + "  TYPE s1_tbl  IS TABLE OF number     INDEX BY BINARY_INTEGER;\n"
            + "  TYPE tt1_tbl IS TABLE OF db2028_tt1 INDEX BY BINARY_INTEGER;\n"
            + "  TYPE s2_tbl  IS TABLE OF timestamp  INDEX BY BINARY_INTEGER;\n"
            + "  x  tt_tbl;\n"
            + "  s1 s1_tbl;\n"
            + "  y  tt1_tbl;\n"
            + "  s2 s2_tbl;\n"
            + "  CURSOR c1 IS SELECT * FROM db2028_tab ORDER BY s1;\n"
            + "BEGIN\n"
            + "  OPEN c1;\n"
            + "  FETCH c1 BULK COLLECT INTO x, s1, y, s2;\n"
            + "  FOR i IN 1..x.count LOOP\n"
            + "    dbms_output.put_line(x(i) || ' ' || s1(i) || ' ' || y(i) || ' \"' || s2(i) || '\"');\n"
            + "  END LOOP;\n"
            + "  CLOSE c1;\n"

            + "  SELECT * BULK COLLECT INTO x, s1, y, s2 FROM db2028_tab ORDER BY s1;\n"
            + "  FOR i IN 1..x.count LOOP\n"
            + "    dbms_output.put_line(x(i) || ' ' || s1(i) || ' ' || y(i) || ' \"' || s2(i) || '\"');\n"
            + "  END LOOP;\n"
            + "end;\n", false);

        var listMsg = new List<string>()
            {
            "(1,10) 1.1 (100,1000) \"01-JAN-21 12:30:11\"",
            "(2,20) 2.2 (200,2000) \"02-FEB-22 08:40:52\"",
            "(3,30) 3.3 (300,3000) \"03-MAR-23 04:20:33\"",
            "(1,10) 1.1 (100,1000) \"01-JAN-21 12:30:11\"",
            "(2,20) 2.2 (200,2000) \"02-FEB-22 08:40:52\"",
            "(3,30) 3.3 (300,3000) \"03-MAR-23 04:20:33\""
            };
        var messages = await ExecuteProcNotice(conn, "fetch_cursor_bulk_collection_intoxy");
        Assert.AreEqual(6, messages.Count);

        for (var i = 0; i < messages.Count; i++)
            Assert.AreEqual(listMsg[i], messages[i]);
    }

    //--DB-1955 : Implement the Oracle DBTIMEZONE function in Advanced Server
    [Test, Timeout(15000)]
    public async Task Oracle_DBTIMEZONE_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore
        //Clean

        await Execute("DROP DATABASE dbtimezonedb1", true);
        await Execute("DROP USER dbtimezoneuser1", true);

        await Execute("CREATE USER dbtimezoneuser1;", false);
        await Execute("ALTER USER dbtimezoneuser1 WITH PASSWORD 'edb';", false);
        await Execute("CREATE DATABASE dbtimezonedb1 OWNER dbtimezoneuser1;", false);

        var newConnString = string.Format("port={0};Server={1};Username={2};Password={3};Database={4};Timeout=0;Command Timeout=0;SSL Mode=Disable",
            conn.Port, conn.Host, "dbtimezoneuser1", "edb", "dbtimezonedb1");

        await conn.CloseAsync();
        await conn.DisposeAsync();

        await using var conn2 = await OpenConnectionAsync(newConnString);

        var dbtz1 = await ExecuteSimpleReader(conn2, "SELECT dbtimezone FROM dual;");
        Assert.IsNotNull(dbtz1);
        Assert.AreEqual("+00:00", dbtz1.ToString());

        await Execute(conn2, "ALTER ROLE dbtimezoneuser1 IN DATABASE dbtimezonedb1 SET timezone='+4:30';", true);

        var dbtz2 = await ExecuteSimpleReader(conn2, "SELECT dbtimezone FROM dual;");
        Assert.IsNotNull(dbtz2);
        Assert.AreEqual("+00:00", dbtz2.ToString());
    }

    //--DB-1975 : Implement version of Oracle TO_TIMESTAMP_TZ function with single option
    /*
     SET TIME ZONE -4;
    SELECT TO_TIMESTAMP_TZ('20-MAR-20 04:30:00.123456 PM +03:00') FROM DUAL;
             to_timestamp_tz          
    ----------------------------------
     20-MAR-20 09:30:00.123456 -04:00
    (1 row)

    SET TIME ZONE 5.5;
    SELECT TO_TIMESTAMP_TZ('06-OCT-85 06.40:14.745623 AM +06:00') FROM DUAL;
             to_timestamp_tz          
    ----------------------------------
     06-OCT-85 06:10:14.745623 +05:30
    (1 row)
     */
    [Test]
    public async Task Oracle_TO_TIMESTAMP_TZ_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore

        await Execute(conn, "SET TIME ZONE -4;", true);

        var tz2 = await ExecuteDateTimeReader(conn, "SELECT TO_TIMESTAMP_TZ('20-MAR-20 04:30:00.123456 PM +03:00') FROM DUAL;");
        Assert.IsNotNull(tz2);
        Assert.AreEqual(new DateTime(2020, 3, 20, 13, 30, 0).ToString(), tz2.ToString());

        await Execute(conn, "SET TIME ZONE 5.5;", true);

        var tz3 = await ExecuteDateTimeReader(conn, "SELECT TO_TIMESTAMP_TZ('06-OCT-85 06.40:14.745623 AM +06:00') FROM DUAL;");
        Assert.IsNotNull(tz3);
        Assert.AreEqual(new DateTime(1985, 10, 6, 0, 40, 14).ToString(), tz3.ToString());
    }

    //--DB-1958 : Implement the Oracle NANVL function in Advanced Server
    [Test]
    public async Task Oracle_NANVL_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore
        var query1 = "SELECT nanvl(124346, 1), nanvl('NaN', 2), nanvl(124346::int, 3), nanvl(124346::int8, 4);";
        using (var com = new EDBCommand("", conn))
        {
            com.CommandType = CommandType.Text;

            com.CommandText = query1;
            var reader = await com.ExecuteReaderAsync();

            Assert.IsTrue(reader.HasRows);

            if (await reader.ReadAsync())
            {
                Assert.AreEqual(124346, reader.GetDouble(0));
                Assert.AreEqual(2, reader.GetDouble(1));
                Assert.AreEqual(124346, reader.GetDouble(2));
                Assert.AreEqual(124346, reader.GetDouble(3));
            }
            await reader.CloseAsync();
        }

        var query2 = "SELECT nanvl('NaN', 1::numeric), nanvl(124346, 2::numeric), nanvl('NaN', 'NaN'::numeric);";
        using (var com = new EDBCommand("", conn))
        {
            com.CommandType = CommandType.Text;

            com.CommandText = query2;
            var reader = await com.ExecuteReaderAsync();

            Assert.IsTrue(reader.HasRows);

            if (await reader.ReadAsync())
            {
                Assert.AreEqual(1, reader.GetDouble(0));
                Assert.AreEqual(124346, reader.GetDouble(1));
                Assert.AreEqual(double.NaN, reader.GetDouble(2));
            }
            await reader.CloseAsync();
        }
    }

    //--DB-1957 : Implement the Oracle LNNVL function in Advanced Server
    /*
     SELECT lnnvl(false);
     lnnvl 
    -------
     t
    (1 row)

    CREATE TABLE t1 (id int, col1 int);
    INSERT INTO t1 VALUES (10,NULL), (1,1), (2,2), (3,3);
    SELECT * FROM t1 WHERE lnnvl(col1 > 2) ORDER BY id;
     id | col1 
    ----+------
      1 |    1
      2 |    2
     10 |     
    (3 rows)
     */
    [Test]
    public async Task Oracle_LNNVL_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore
        var query1 = "SELECT lnnvl(false);";
        using (var com = new EDBCommand("", conn))
        {
            com.CommandType = CommandType.Text;

            com.CommandText = query1;
            var reader = await com.ExecuteReaderAsync();

            Assert.IsTrue(reader.HasRows);

            if (await reader.ReadAsync())
            {
                Assert.IsTrue(reader.GetBoolean(0));
            }
            await reader.CloseAsync();
        }

        await Execute(conn, "DROP TABLE t1", true);
        await Execute(conn, "CREATE TABLE t1(id int, col1 int);", false);
        await Execute(conn, "INSERT INTO t1 VALUES(10,NULL), (1, 1), (2, 2), (3, 3);", false);

        var query2 = "SELECT * FROM t1 WHERE lnnvl(col1 > 2) ORDER BY id;";
        using (var com = new EDBCommand("", conn))
        {
            com.CommandType = CommandType.Text;

            com.CommandText = query2;
            var reader = await com.ExecuteReaderAsync();

            Assert.IsTrue(reader.HasRows);

            if (await reader.ReadAsync())
            {
                Assert.AreEqual(1, reader.GetInt32(0));
                Assert.AreEqual(1, reader.GetInt32(1));
            }

            if (await reader.ReadAsync())
            {
                Assert.AreEqual(2, reader.GetInt32(0));
                Assert.AreEqual(2, reader.GetInt32(1));
            }

            if (await reader.ReadAsync())
            {
                Assert.AreEqual(10, reader.GetInt32(0));
                var obj = reader.GetValue(1);
                Assert.AreEqual(string.Empty, obj.ToString());
            }
            await reader.CloseAsync();
        }
    }

    //--DB-1956 : Implement the Oracle DUMP function in Advanced Server
    /*
    select dump(to_timestamp('11-06-2021 12:45:24','MM-DD-YYYY HH:MI:SS'), 1008);
                       dump                            
    -----------------------------------------------------------
     Typ=1184 Len=8 CharacterSet=UTF8: 0,153,325,73,16,163,2,0
    (1 row)

    select dump('CHALLENGE',16);
            dump                   
    ------------------------------------------
     Typ=25 Len=9: 43,48,41,4c,4c,45,4e,47,45
    (1 row)

    select dump(100/4, 10);
            dump            
    ----------------------------
     Typ=1700 Len=4: 0,128,25,0
    (1 row)
        */
    [Test]
    public async Task Oracle_DUMP_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore

        //The output of this query is different in
        //1: The result file shared by server team.
        //2: When I ran it manually on PSQL
        //3: On .NET side.

        //Server Team: Typ=1184 Len=8 CharacterSet=UTF8: 0,153,325,73,16,163,2,0
        //PSQL:        Typ=1184 Len=8 CharacterSet=UTF8: 0,1,120,62,26,163,2,0
        //.NET Driver: Typ=1184 Len=8 CharacterSet=UTF8: 0,355,12,266,30,163,2,0
        var dump1 = await ExecuteSimpleReader(conn, "select dump(to_timestamp('11-06-2021 12:45:24','MM-DD-YYYY HH:MI:SS'), 1008);");
        Assert.IsNotNull(dump1);
        //Assert.AreEqual("Typ=1184 Len=8 CharacterSet=UTF8: 0,153,325,73,16,163,2,0", dump1.ToString());

        var dump2 = await ExecuteSimpleReader(conn, "select dump('CHALLENGE',16);");
        Assert.IsNotNull(dump2);
        Assert.AreEqual("Typ=25 Len=9: 43,48,41,4c,4c,45,4e,47,45", dump2.ToString());

        var dump3 = await ExecuteSimpleReader(conn, "select dump(100/4, 10);");
        Assert.IsNotNull(dump3);
        Assert.AreEqual("Typ=1700 Len=4: 0,128,25,0", dump3.ToString());
    }

    //---DB-1419 : case #72516:Calling the cursor recursively in a function will result in an error
    [Test]
    public async Task RecursiveCursorCallInFuncErrorTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore
        //Clean
        await Execute(conn, "DROP PROCEDURE test_expand_sql_text_select_valid", true);
        await Execute(conn, "DROP TABLE rm23152_tab CASCADE", true);

        //Setup
        await Execute(conn, "CREATE TABLE rm23152_tab(a int, b varchar2);", false);
        await Execute(conn, "INSERT INTO rm23152_tab VALUES (1, 'A');", false);
        await Execute(conn, "INSERT INTO rm23152_tab VALUES (1, 'A');", false);

        await Execute(conn, "CREATE OR REPLACE FUNCTION db1419_rec_func(cnt int) RETURN text IS\n"
            + "DECLARE\n"
            + "  CURSOR cur IS SELECT * FROM rm23152_tab LIMIT 1;\n"
            + "            BEGIN\n"
            + "              FOR rec IN cur LOOP\n"
            + "    IF cnt = 3 THEN\n"
            + "      exit;\n"
            + "            END IF;\n"
            + "            RAISE NOTICE 'Func Round % rec = %', cnt, rec;\n"
            + "            PERFORM db1419_rec_func(cnt +1); --Recursive call to the function\n"
            + "  END LOOP;\n"
            + "            RETURN 'Done';\n"
            + "            END; ", false);

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
            using (var cstmt = new EDBCommand("SELECT db1419_rec_func(0)", conn))
            {
                cstmt.CommandType = CommandType.Text;

                var reader = await cstmt.ExecuteReaderAsync();

                Assert.IsTrue(reader.HasRows);

                if (await reader.ReadAsync())
                {
                    Assert.AreEqual("Done", reader.GetString(0));
                }
            }
            mre.WaitOne(5000);
            Assert.AreEqual(3, notices.Count);

            var notice1 = (PostgresNotice?)notices[0];
            Assert.AreEqual("Func Round 0 rec = (1,A)", notice1.MessageText);

            var notice2 = (PostgresNotice?)notices[1];
            Assert.AreEqual("Func Round 1 rec = (1,A)", notice2.MessageText);

            var notice3 = (PostgresNotice?)notices[2];
            Assert.AreEqual("Func Round 2 rec = (1,A)", notice3.MessageText);
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }

    //--DB-2038 : Support MULTISET INTERSECT and MULTISET EXCEPT
    [Test]
    public async Task Multiset_Intersect_ExceptTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore

        await Execute(conn, "DROP PROCEDURE multiset_intersect_test", true);
        await Execute(conn, "DROP PROCEDURE multiset_except_test", true);

        await Execute(conn, "CREATE OR REPLACE PROCEDURE multiset_intersect_test()\n"
        + "IS\n"
        + "  TYPE name_typ IS TABLE OF VARCHAR(50);\n"
        + "  color_name  name_typ;\n"
        + "  fruit_name  name_typ;\n"
        + "  common_name name_typ;\n"
        + "BEGIN\n"
        + "  color_name := name_typ('Red', 'Green', 'Blue', 'Orange', 'Peach', 'Yellow', 'Peach');\n"
        + "  fruit_name := name_typ('Mango', 'Orange', 'Grapes', 'Banana', 'Peach', 'Peach');\n"
        + "  common_name := color_name MULTISET INTERSECT UNIQUE fruit_name;\n"
        + "  FOR i IN common_name.FIRST .. common_name.LAST LOOP\n"
        + "    DBMS_OUTPUT.PUT_LINE(common_name(i));\n"
        + "  END LOOP;\n"
        + "END;", false);

        await Execute(conn, "CREATE OR REPLACE PROCEDURE multiset_except_test()\n"
        + "IS\n"
        + "  TYPE name_typ IS TABLE OF VARCHAR(50);\n"
        + "  color_name  name_typ;\n"
        + "  fruit_name  name_typ;\n"
        + "  common_name name_typ;\n"
        + "BEGIN\n"
        + "  color_name := name_typ('Red', 'Green', 'Blue', 'Blue', 'Orange', 'Peach', 'Yellow');\n"
        + "  fruit_name := name_typ('Mango', 'Orange', 'Grapes', 'Banana', 'Peach');\n"
        + "  common_name := color_name MULTISET EXCEPT UNIQUE fruit_name;\n"
        + "  FOR i IN common_name.FIRST .. common_name.LAST LOOP\n"
        + "    DBMS_OUTPUT.PUT_LINE(common_name(i));\n"
        + "  END LOOP;\n"
        + "END;", false);

        var messages1 = await ExecuteProcNotice(conn, "multiset_intersect_test");
        Assert.AreEqual(2, messages1.Count);
        Assert.AreEqual("Orange", messages1[0]);
        Assert.AreEqual("Peach", messages1[1]);

        var messages2 = await ExecuteProcNotice(conn, "multiset_except_test");
        Assert.AreEqual(4, messages2.Count);
        Assert.AreEqual("Blue", messages2[0]);
        Assert.AreEqual("Green", messages2[1]);
        Assert.AreEqual("Red", messages2[2]);
        Assert.AreEqual("Yellow", messages2[3]);
    }

    //--DB-2160 : Implement Oracle NLS CHARSET functions in Advanced Server
    [Test]
    public async Task Oracle_NLS_CHARSET_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore
        var val1 = await ExecuteSimpleReader(conn, "Select nls_charset_id('utf8');");
        Assert.IsNotNull(val1);
        Assert.AreEqual("6", val1.ToString());

        var val2 = await ExecuteSimpleReader(conn, "Select 1 from dual where nls_charset_name(98) is null;");
        Assert.IsNotNull(val2);
        Assert.AreEqual("1", val2.ToString());

        var val3 = await ExecuteSimpleReader(conn, "Select nls_charset_decl_len(100,nls_charset_id('utf8'));");
        Assert.IsNotNull(val3);
        Assert.AreEqual("100", val3.ToString());
    }

    //--DB-1709 : Implement DBMS_SQL Subprograms Not Currently Implemented in Advanced Server
    //--DB-1750 : Implement dbms_sql.define_array
    //--DB-2145 : Implement dbms_sql.describe_columns3
    [Test]
    public async Task DBMS_SQL_SubprogramsTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore

        await Execute(conn, "DROP TABLE employees_db1750", true);
        await Execute(conn, "DROP TABLE projecttab_db2145", true);
        await Execute(conn, "DROP TYPE PROJECT_T_DB2145", true);

        //--dbms_sql.define_array
        //--Random order calling for DEFINE_ARRAY and COLUMN_VALUE
        await Execute(conn, "CREATE TABLE employees_db1750(first_name VARCHAR(30), salary NUMBER,"
            + "department_id INTEGER, dob TIMESTAMP, height FLOAT, rid ROWID, ctext CLOB, btext BLOB); ", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user1', 10000, 1, TO_TIMESTAMP('07/09/1979', 'dd/mm/yyyy'), 5.2, 1000, 'clob_1', 'blob_1');", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user2', 20000, 1, TO_TIMESTAMP('07/09/1980', 'dd/mm/yyyy'), 5.3, 1001, 'clob_2', 'blob_2');", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user3', 30000, 1, TO_TIMESTAMP('07/09/1981', 'dd/mm/yyyy'), 5.4, 1002, 'clob_3', 'blob_3');", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user4', 40000, 1, TO_TIMESTAMP('07/09/1982', 'dd/mm/yyyy'), 5.5, 1003, 'clob_4', 'blob_4');", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user5', 50000, 1, TO_TIMESTAMP('07/09/1983', 'dd/mm/yyyy'), 5.6, 1004, 'clob_5', 'blob_5');", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user6', 60000, 1, TO_TIMESTAMP('07/09/1984', 'dd/mm/yyyy'), 5.7);", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user7', 70000, 1, TO_TIMESTAMP('07/09/1985', 'dd/mm/yyyy'), 5.8);", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user8', 80000, 1, TO_TIMESTAMP('07/09/1986', 'dd/mm/yyyy'), 5.9);", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user9', 90000, 2, TO_TIMESTAMP('07/09/1987', 'dd/mm/yyyy'), 4.9);", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user10', 100000, 2, TO_TIMESTAMP('07/09/1988', 'dd/mm/yyyy'), 5.0, 1005, 'clob_6', 'blob_7');", false);
        await Execute(conn, "INSERT INTO employees_db1750 VALUES('user11', 110000, 2, TO_TIMESTAMP('07/09/1989', 'dd/mm/yyyy'), 5.1);", false);

        await Execute(conn, "CREATE OR REPLACE PROCEDURE define_array_test()\n"
        + "IS\n"
        + "    names    DBMS_SQL.VARCHAR2_TABLE;\n"
        + "    sals     DBMS_SQL.NUMBER_TABLE;\n"
        + "    dob      DBMS_SQL.TIMESTAMP_TABLE;\n"
        + "    height   DBMS_SQL.FLOAT_TABLE;\n"
        + "    dpt_id   DBMS_SQL.INTEGER_TABLE;\n"
        + "    c        NUMBER;\n"
        + "    r        NUMBER;\n"
        + "    sql_stmt VARCHAR2(32767) :=\n"
        + "        'SELECT first_name, salary, department_id, dob, height FROM employees_db1750 WHERE department_id = :b1';\n"
        + "BEGIN\n"
        + "    c := DBMS_SQL.OPEN_CURSOR;\n"
        + "    DBMS_SQL.PARSE(c, sql_stmt, dbms_sql.native);\n"
        + "    DBMS_SQL.BIND_VARIABLE(c, 'b1', 1);\n"
        + "    DBMS_SQL.DEFINE_ARRAY(c, 1, names, 5, 11);\n"
        + "    DBMS_SQL.DEFINE_ARRAY(c, 5, height, 5, 11);\n"
        + "    DBMS_SQL.DEFINE_ARRAY(c, 4, dob, 5, 11);\n"
        + "    DBMS_SQL.DEFINE_ARRAY(c, 2, sals, 5, 11);\n"
        + "    DBMS_SQL.DEFINE_ARRAY(c, 3, dpt_id, 5, 11);\n"
        + "    r := DBMS_SQL.EXECUTE(c);\n"
        + "    LOOP\n"
        + "      r := DBMS_SQL.FETCH_ROWS(c);\n"
        + "      EXIT WHEN r = 0;\n"
        + "      DBMS_SQL.COLUMN_VALUE(c, 5, height);\n"
        + "      DBMS_SQL.COLUMN_VALUE(c, 1, names);\n"
        + "      DBMS_SQL.COLUMN_VALUE(c, 2, sals);\n"
        + "      DBMS_SQL.COLUMN_VALUE(c, 3, dpt_id);\n"
        + "      DBMS_SQL.COLUMN_VALUE(c, 4, dob);\n"
        + "    END LOOP;\n"
        + "    DBMS_SQL.CLOSE_CURSOR(c);\n"
        + "    DBMS_OUTPUT.PUT_LINE('last index in table = ' || sals.LAST);\n"
        + "    -- loop through the names and sals collections\n"
        + "    FOR i IN sals.FIRST .. sals.LAST  LOOP\n"
        + "      DBMS_OUTPUT.PUT_LINE('salary = ' || sals(i) || ' name = ' ||names(i) || ' department_id = ' || dpt_id(i) || ' dob = ' || dob(i) || ' height = ' || height(i) );\n"
        + "    END LOOP;\n"
        + "END;", false);

        var msgExpected1 = new List<string>()
        {
            "last index in table = 18",
            "salary = 10000 name = user1 department_id = 1 dob = 07-SEP-79 00:00:00 height = 5.2",
            "salary = 20000 name = user2 department_id = 1 dob = 07-SEP-80 00:00:00 height = 5.3",
            "salary = 30000 name = user3 department_id = 1 dob = 07-SEP-81 00:00:00 height = 5.4",
            "salary = 40000 name = user4 department_id = 1 dob = 07-SEP-82 00:00:00 height = 5.5",
            "salary = 50000 name = user5 department_id = 1 dob = 07-SEP-83 00:00:00 height = 5.6",
            "salary = 60000 name = user6 department_id = 1 dob = 07-SEP-84 00:00:00 height = 5.7",
            "salary = 70000 name = user7 department_id = 1 dob = 07-SEP-85 00:00:00 height = 5.8",
            "salary = 80000 name = user8 department_id = 1 dob = 07-SEP-86 00:00:00 height = 5.9",
        };

        var messages1 = await ExecuteProcNotice(conn, "define_array_test");
        Assert.AreEqual(msgExpected1.Count, messages1.Count);
        for (var i = 0; i < msgExpected1.Count; i++)
            Assert.AreEqual(msgExpected1[i], messages1[i]);

        //--dbms_sql.describe_columns3
        await Execute(conn, "CREATE TYPE PROJECT_T_DB2145 AS OBJECT\n"
            + "  ( projname          VARCHAR2(20),\n"
            + "  mgr               VARCHAR2(20));", false);

        await Execute(conn, "CREATE TABLE projecttab_db2145(deptno NUMBER, project PROJECT_T_DB2145);", false);

        await Execute(conn, "CREATE OR REPLACE PROCEDURE describe_columns3_test()\n"
        + "IS\n"
        + "  curid      NUMBER;\n"
        + "  desctab    DBMS_SQL.DESC_TAB3;\n"
        + "  colcnt     NUMBER;\n"
        + "  sql_stmt   VARCHAR2(200) := 'SELECT * FROM projecttab_db2145';\n"
        + "BEGIN\n"
        + "    curid := DBMS_SQL.OPEN_CURSOR;\n"
        + "    DBMS_SQL.PARSE(curid, sql_stmt, DBMS_SQL.NATIVE);\n"
        + "    DBMS_SQL.DESCRIBE_COLUMNS3(curid, colcnt, desctab);\n"
        + "    FOR i IN 1 .. colcnt LOOP\n"
        + "      IF desctab(i).col_type = 109 THEN\n"
        + "        DBMS_OUTPUT.PUT(desctab(i).col_name || ' is user-defined type: ');\n"
        + "        DBMS_OUTPUT.PUT_LINE('COL_TYPE_NAME = ' || desctab(i).col_type_name\n"
        + "        || ' COL_TYPE_NAME_LEN = ' || desctab(i).col_type_name_len);\n"
        + "      ELSE\n"
        + "        DBMS_OUTPUT.PUT(desctab(i).col_name || ' is not user-defined type: ');\n"
        + "        DBMS_OUTPUT.PUT_LINE('COL_TYPE_NAME is NULL and '\n"
        + "        || 'COL_TYPE_NAME_LEN = ' || desctab(i).col_type_name_len);\n"
        + "      END IF;\n"
        + "    END LOOP;\n"
        + "    DBMS_SQL.CLOSE_CURSOR(curid);\n"
        + "END;\n", false);

        var msgExpected2 = new List<string>()
        {
        "deptno is not user-defined type: COL_TYPE_NAME is NULL and COL_TYPE_NAME_LEN = 0",
        "project is user-defined type: COL_TYPE_NAME = project_t_db2145 COL_TYPE_NAME_LEN = 16"
        };

        var messages2 = await ExecuteProcNotice(conn, "describe_columns3_test");
        Assert.AreEqual(msgExpected2.Count, messages2.Count);
        for (var i = 0; i < msgExpected2.Count; i++)
            Assert.AreEqual(msgExpected2[i], messages2[i]);
    }

    //--DB-2155 : Request of Synonyms of Procedure to work as a procedure cross schemas
    [Test]
    public async Task RequestSynonymsProcedureCrossSchemasTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore
        //Clean
        await Execute(conn, "DROP PACKAGE BODY db2155_sch_1.db2155_pkg", true);
        await Execute(conn, "DROP PACKAGE db2155_sch_1.db2155_pkg", true);
        await Execute(conn, "DROP SCHEMA db2155_sch_1 CASCADE", true);
        await Execute(conn, "DROP SCHEMA db2155_sch_2 CASCADE", true);

        await Execute(conn, "CREATE SCHEMA db2155_sch_1;", false);
        await Execute(conn, "CREATE SCHEMA db2155_sch_2;", false);
        await Execute(conn, "CREATE OR REPLACE PACKAGE db2155_sch_1.db2155_pkg IS\n"
        + "  PROCEDURE proc;\n"
        + "  FUNCTION func() RETURN int;\n"
        + "  TYPE typ IS RECORD (a int, b int);\n"
        + "  TYPE colltyp IS TABLE OF int;\n"
        + "  var int := 10;\n"
        + "  CURSOR cur1 IS SELECT 1 FROM dual;\n"
        + "  CURSOR cur2(cv text) IS SELECT 2 FROM dual WHERE dummy = cv;\n"
        + "  exp EXCEPTION;\n"
        + "  PRAGMA EXCEPTION_INIT(exp, -21000);\n"
        + "END;", false);
        await Execute(conn, "CREATE OR REPLACE PACKAGE BODY db2155_sch_1.db2155_pkg IS\n"
        + "  PROCEDURE proc IS\n"
        + "  BEGIN\n"
        + "    dbms_output.put_line('In package procedure');\n"
        + "  END;\n"

        + "  FUNCTION func() RETURN int IS\n"
        + "  BEGIN\n"
        + "    dbms_output.put_line('In package function');\n"
        + "    return 1;\n"
        + "  END;\n"
        + "END;", false);

        //--Set the search path to db2155_sch_1 and call the package procedure
        await Execute(conn, "SET search_path = db2155_sch_1;", false);

        var messages1 = await ExecuteProcNotice(conn, "db2155_pkg.proc");
        Assert.AreEqual(1, messages1.Count);
        Assert.AreEqual("In package procedure", messages1[0]);

        var val1 = await ExecuteSimpleReader(conn, "SELECT db2155_pkg.func");
        Assert.IsNotNull(val1);
        Assert.AreEqual("1", val1.ToString());

        await Execute(conn, "CREATE TABLE db2155_test_syn(a db2155_pkg.typ);", false);
        await Execute(conn, "DROP TABLE db2155_test_syn;", false);
        var val2 = await ExecuteSimpleReader(conn, "SELECT db2155_pkg.var;");
        Assert.IsNotNull(val2);
        Assert.AreEqual("10", val2.ToString());

        //--Create a synonym in schema db2155_sch_2 for db2155_sch_1.db2155_pkg
        await Execute(conn, "SET search_path = db2155_sch_2;", false);
        await Execute(conn, "CREATE OR REPLACE SYNONYM db2155_syn FOR db2155_sch_1.db2155_pkg;", false);

        var messages2 = await ExecuteProcNotice(conn, "db2155_syn.proc");
        Assert.AreEqual(1, messages2.Count);
        Assert.AreEqual("In package procedure", messages2[0]);

        var val3 = await ExecuteSimpleReader(conn, "SELECT db2155_syn.func");
        Assert.IsNotNull(val3);
        Assert.AreEqual("1", val3.ToString());

        await Execute(conn, "CREATE TABLE db2155_test_syn(a db2155_syn.typ);", false);
        await Execute(conn, "DROP TABLE db2155_test_syn;", false);
        var val4 = await ExecuteSimpleReader(conn, "SELECT db2155_syn.var;");
        Assert.IsNotNull(val4);
        Assert.AreEqual("10", val4.ToString());

    }

    //--DB-1706 : Implement UTL_FILE Subprograms Not Currently Implemented in Advanced Server
    //--Group 1 DB-1926: [UTL_FILE] Group 1 : Add utl_file.fgetattr()
    //--Group 2 DB-1929: [UTL_FILE] Group 3 : Add fgetpos(), fseek(), fopen_nchar(), and put_nchar().

    //This test was written when AS 16 was not available on Windows.
    //It is working fine but this test should be kept ignored because it may have
    //different bahaviour on different machines.
    [Test, EDBExplicit("Requires directory access and may fail as false negative. Read comments above")]
    public async Task UTL_FILE_SubprogramsTest()
    {
        await using var conn = await OpenConnectionAsync();

#nullable disable
        TestUtil.MinimumPgVersion(conn, "16.0.0");
#nullable restore

        await Execute(conn, "create or replace directory tmp as '/tmp';", false);

        await Execute(conn, "CREATE OR REPLACE PROCEDURE utl_file_fgetattr_test\n"
            + "IS\n"
            + "  fexists boolean;\n"
            + "  flen number;\n"
            + "  fblksz number;\n"
            + "  dummy_fblksz number := 4096;\n"
            + "  isWin boolean;\n"
            + "begin\n"
            + "  SELECT\n"
            + "   (os LIKE '%w64%') OR\n"
            + "    (os LIKE '%w32%') OR\n"
            + "    (os LIKE '%mingw%') OR\n"
            + "    (os LIKE '%visual studio%') INTO isWin\n"
            + "  FROM\n"
            + "    (SELECT substr(substr(version(), strpos(version(), ' on ') + 3), 1,\n"
            + "            strpos(substr(version(), strpos(version(), ' on ') + 3), ', compiled by') - 1)\n"
            + "    ) AS os;\n"
            + "  utl_file.fgetattr(fexists => fexists, file_length => flen, block_size => fblksz, location => 'tmp', filename => 'regress_orafce1');\n"
            + "  dbms_output.put_line('fexists: ' || fexists || ', file_length: ' || flen ||\n"
            + "    ', block_size: ' || (CASE WHEN isWin = FALSE THEN fblksz ELSE dummy_fblksz END));\n"

            + "  utl_file.fgetattr('tmp', 'regress_orafce2', fexists, flen, fblksz);\n"
            + "  dbms_output.put_line('fexists: ' || fexists || ', file_length: ' || flen ||\n"
            + "    ', block_size: ' || (CASE WHEN isWin = FALSE THEN fblksz ELSE dummy_fblksz END));\n"

            + "  utl_file.fgetattr('tmp', 'regress_orafce3', fexists, flen, fblksz);\n"
            + "  dbms_output.put_line('fexists: ' || fexists || ', file_length: ' || flen ||\n"
            + "    ', block_size: ' || (CASE WHEN isWin = FALSE THEN fblksz ELSE dummy_fblksz END));\n"

            + "  utl_file.fgetattr('tmp', 'regress_orafce4', fexists, flen, fblksz);\n"
            + "  dbms_output.put_line('fexists: ' || fexists || ', file_length: ' || flen ||\n"
            + "    ', block_size: ' || (CASE WHEN isWin = FALSE THEN fblksz ELSE dummy_fblksz END));\n"

            + "  utl_file.fgetattr('tmp', 'regress_orafce5', fexists, flen, fblksz);\n"
            + "  dbms_output.put_line('fexists: ' || fexists || ', file_length: ' || flen ||\n"
            + "    ', block_size: ' || (CASE WHEN isWin = FALSE THEN fblksz ELSE dummy_fblksz END));\n"
            + "end;", false);

        var msgExpected1 = new List<string>()
        {
            "fexists: false, file_length: , block_size: ",
            "fexists: false, file_length: , block_size: ",
            "fexists: false, file_length: , block_size: ",
            "fexists: false, file_length: , block_size: ",
            "fexists: false, file_length: , block_size: "
        };

        var messages1 = await ExecuteProcNotice(conn, "utl_file_fgetattr_test");
        Assert.AreEqual(5, messages1.Count);
        for (var i = 0; i < messages1.Count; i++)
            Assert.AreEqual(msgExpected1[i], messages1[i]);

        await Execute(conn, "CREATE OR REPLACE PROCEDURE utl_file_fgetpos_fseek_test\n"
            + "IS\n"
            + "  f utl_file.file_type;\n"
            + "  pos int;\n"
            + "begin\n"
            + "  f := utl_file.fopen('tmp', 'regress_orafce', 'r');\n"
            + "  utl_file.fseek(f, 15);\n"
            + "  pos := utl_file.fgetpos(f);\n"
            + "  dbms_output.put_line('1.position: ' || pos);\n"
            + "  utl_file.fseek(f, NULL, -5);\n"
            + "  pos := utl_file.fgetpos(f);\n"
            + "  dbms_output.put_line('2.position: ' || pos);\n"
            + "  utl_file.fclose(f);\n"
            + "end;", false);

        var msgExpected2 = new List<string>()
        {
            "1.position: 15",
            "2.position: 10"
        };
        var messages2 = await ExecuteProcNotice(conn, "utl_file_fgetpos_fseek_test");
        Assert.AreEqual(2, messages2.Count);
        for (var i = 0; i < messages2.Count; i++)
            Assert.AreEqual(msgExpected2[i], messages2[i]);

        //--Test fopen_nchar/put_nchar
        await Execute(conn, "create or replace function open_put_nchar_test return void as\n"
            + "declare\n"
            + "  f utl_file.file_type;\n"
            + "  t text;\n"
            + "begin\n"
            + "  f := utl_file.fopen_nchar('tmp', 'regress_orafce6', 'w');\n"
            + "  utl_file.put_nchar(f, 'Hello - 1'::text);\n"
            + "  utl_file.put_nchar(f, 100::numeric);\n"
            + "  utl_file.put_nchar(f, '2006-08-13 12:34:56'::timestamp);\n"
            + "  utl_file.put_nchar(f, '2001-12-27 04:05:06.789-08'::pg_catalog.date);\n"
            + "  utl_file.put_nchar(f, '2001-12-27 04:05:06.789-08'::time);\n"
            + "  utl_file.fclose(f);\n"
            + "end;", false);

        //No messages are expected in this case.
        var messages3 = await ExecuteProcNotice(conn, "open_put_nchar_test");
        Assert.AreEqual(0, messages3.Count);

        //--Prerequisite function covers test for fopen_nchar() and put_nchar()
        await Execute(conn, "create or replace function readmy_file_nchar return void as\n"
            + "declare\n"
            + "  f utl_file.file_type;\n"
            + "  txt1 text;\n"
            + "begin\n"
            + "  f := utl_file.fopen_nchar('tmp', 'regress_orafce6', 'r');\n"
            + "  loop\n"
            + "    utl_file.get_line_nchar(f, txt1);\n"
            + "    raise notice '%', txt1;\n"
            + "  end loop;\n"
            + "exception\n"
            + "  when no_data_found then\n"
            + "    raise notice 'finish % ', sqlerrm;\n"
            + "    utl_file.fclose(f);\n"
            + "end;", false);

        var messages4 = await ExecuteProcNotice(conn, "readmy_file_nchar");
        Assert.AreEqual(2, messages4.Count);


    }


    private static async Task SetUpXMLType(EDBConnection conn)
    {
        await Execute(conn, "drop procedure xml_funcs_proc;", true);
        await Execute(conn, "drop table person_xmltype;", true);

        var tblSql = """
            CREATE TABLE person_xmltype
            (
                person_id   integer,
                person_data xmltype
            );
            """;
        await Execute(conn, tblSql, false);

        var insSql = """
                            INSERT INTO person_xmltype
                            (person_id, person_data)
                            VALUES
                            (1, '<PDRecord><PDName>test_user1</PDName><PDID>1</PDID><PDEmail>test_user1@testmail.com</PDEmail></PDRecord>'::xml),
                            (2, '<PDRecord><PDName>test_user2</PDName><PDID>2</PDID><PDEmail>test_user2@testmail.com</PDEmail></PDRecord>'::xml),
                            (3, '<PDRecord><PDName>test_user3</PDName><PDID>3</PDID><PDEmail>test_user3@testmail.com</PDEmail></PDRecord>'::xml),
                            (4, '<PDRecord><PDName>test_user4</PDName><PDID>4</PDID><PDEmail>test_user4@testmail.com</PDEmail></PDRecord>'::xml),
                            (5, '<PDRecord><PDName>test_user5</PDName><PDID>5</PDID><PDEmail>test_user5@testmail.com</PDEmail></PDRecord>'::xml);
            """;
        await Execute(conn, insSql, false);

        var procSql = "CREATE OR REPLACE PROCEDURE xml_funcs_proc()\n"
    + "IS\n"
    + "    xmltype_data XMLTYPE;\n"
    + "BEGIN\n"
    + "    SELECT person_data.EXTRACT_XML('/PDRecord/PDID/text()') INTO xmltype_data FROM person_xmltype LIMIT 1;\n"
    + "    DBMS_OUTPUT.PUT_LINE(xmltype_data.getNumberval());\n"
    + "    SELECT person_data.EXTRACT_XML('/PDRecord/PDEmail/text()') INTO xmltype_data FROM person_xmltype LIMIT 1;\n"
    + "    DBMS_OUTPUT.PUT_LINE(xmltype_data.getStringVal());\n"
    + "    DBMS_OUTPUT.PUT_LINE(xmltype_data.getClobVal());\n"
    + "END;\n";

        await Execute(conn, procSql, false);
    }

    private static async Task RunQueryAndVerifyResultAsync(EDBConnection conn, string query, string[] expected)
    {
        try
        {
            using (var com = new EDBCommand("", conn))
            {
                com.CommandType = CommandType.Text;

                com.CommandText = query;
                var reader = await com.ExecuteReaderAsync();

                Assert.IsTrue(reader.HasRows);

                var i = 0;
                while (await reader.ReadAsync())
                {
                    Assert.AreEqual(expected[i], reader.GetString(0));
                    i++;
                }
                Assert.AreEqual(expected.Length, i);
                await reader.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    //--DB-1753 : XMLType: create Oracle-compatible XMLType as object type and implement its member functions.
    [Test]
    public async Task DB_1753_ExtractXMLAsTextTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MaximumPgVersionExclusive(conn, "17.0.0");
        TestUtil.MinimumPgVersion(conn, "16.0.0");

        await SetUpXMLType(conn);

        string[] expected =
            {
        "<PDRecord><PDName>test_user1</PDName><PDID>1</PDID><PDEmail>test_user1@testmail.com</PDEmail></PDRecord>",
        "<PDRecord><PDName>test_user2</PDName><PDID>2</PDID><PDEmail>test_user2@testmail.com</PDEmail></PDRecord>",
        "<PDRecord><PDName>test_user3</PDName><PDID>3</PDID><PDEmail>test_user3@testmail.com</PDEmail></PDRecord>",
        "<PDRecord><PDName>test_user4</PDName><PDID>4</PDID><PDEmail>test_user4@testmail.com</PDEmail></PDRecord>",
        "<PDRecord><PDName>test_user5</PDName><PDID>5</PDID><PDEmail>test_user5@testmail.com</PDEmail></PDRecord>"
    };

        await RunQueryAndVerifyResultAsync(conn, "SELECT person_data FROM person_xmltype ORDER BY person_id", expected);

    }
}
#pragma warning restore CS8602

