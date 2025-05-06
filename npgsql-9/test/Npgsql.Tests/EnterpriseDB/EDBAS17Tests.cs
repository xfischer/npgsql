using System;
using NUnit.Framework;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.IO;
using EnterpriseDB.EDBClient.Tests.Support;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB;


[TestFixture]
[NonParallelizable]
internal class EDBAS17Tests : EPASTestBase
{
    private static async Task<int> Execute(EDBConnection conn, string query, bool ignoreResult)
    {
        try
        {

            using var com = new EDBCommand("", conn);
            com.CommandType = CommandType.Text;

            com.CommandText = query;
            return await com.ExecuteNonQueryAsync();
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
            using var com = new EDBCommand("", conn);
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
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
        return val;
    }

    private static async Task RunQueryAndVerifyResultAsync(EDBConnection conn, string query, string[] expected)
    {
        try
        {
            using var com = new EDBCommand("", conn);
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
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    //Executes a stored procedure with no arguments
    //If there are any messages from dbms_output.put_line, they are retured as a list.
    public static async Task<List<string>> ExecuteProcNotice(EDBConnection conn, string sqlStr)
    {
        var messages = new List<string>();


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
                messages.Add(notice!.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();

        return messages;
    }

    //--DB-2157 : Implement Oracle NLS_UPPER function in Advanced Server
    [Test]
    public async Task DB_2157_NLS_UPPER_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();


        TestUtil.MinimumPgVersion(conn, "17.0.0");


        //Run this command to make edb_nls_cf_insert work.
        //chmod 777 /usr/share/edb-as/17/contrib/edb_redwood_nls.config
        //Without the above command, this query will fail: SELECT edb_nls_cf_insert***
        //It works fine but I have commented it out as it will cause the tests to fail on subsequent calls:
        //i.e. the query for Gloße will return GLOSSE and not GLOßE on subsequent tests run.
        //await ExecuteSimpleReader(conn, "SELECT edb_nls_cf_insert('xdanish', 'default')");

        var val1 = await ExecuteSimpleReader(conn, "SELECT nls_upper('Gloße', 'NLS_SORT = xdanish');");
        Assert.IsNotNull(val1);
        Assert.AreEqual("GLOßE", val1!.ToString());

        //await ExecuteSimpleReader(conn, "SELECT edb_nls_cf_insert('xdanish', '\"pg_catalog\".\"da-x-icu\"')");

        //var val2 = await ExecuteSimpleReader(conn, "SELECT nls_upper('Gloße', 'NLS_SORT = xdanish');");
        //Assert.IsNotNull(val2);
        //Assert.AreEqual("GLOSSE", val2.ToString());

        var val3 = await ExecuteSimpleReader(conn, "SELECT nls_upper('abcDef', 'NLS_SORT = XGERMAN');");
        Assert.IsNotNull(val3);
        Assert.AreEqual("ABCDEF", val3!.ToString());
    }

    //--DB-2158 : Implement Oracle NLS_LOWER function in Advanced Server
    [Test]
    public async Task DB_2158_NLS_LOWER_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        //Run this command to make edb_nls_cf_insert work.
        //chmod 777 /usr/share/edb-as/17/contrib/edb_redwood_nls.config
        //Without the above command, this query will fail: SELECT edb_nls_cf_insert***
        //It works fine but I have commented it out as it will cause the tests to fail on subsequent calls:
        //i.e. the query for fasilə will return fasılə and not fasilə on subsequent tests run.
        //await ExecuteSimpleReader(conn, "SELECT edb_nls_cf_insert('xturkish', 'default')");

        var val1 = await ExecuteSimpleReader(conn, "SELECT nls_lower('FASILƏ', 'NLS_SORT = XTURKISH');");
        Assert.IsNotNull(val1);
        Assert.AreEqual("fasilə", val1!.ToString());


        //var val2 = await ExecuteSimpleReader(conn, "SELECT nls_lower('FASILƏ', 'NLS_SORT = XTURKISH');");
        //Assert.IsNotNull(val2);
        //Assert.AreEqual("fasılə", val2.ToString());

        var val3 = await ExecuteSimpleReader(conn, "SELECT nls_lower('AbcDeF pQr', 'NLS_SORT = XAZERBAIJANI');");
        Assert.IsNotNull(val3);
        Assert.AreEqual("abcdef pqr", val3!.ToString());
    }

    //--DB-2159 : Implement Oracle NLS_INITCAP function in Advanced Server
    [Test]
    public async Task DB_2159_NLS_INITCAP_FunctionTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        //Run this command to make edb_nls_cf_insert work.
        //chmod 777 /usr/share/edb-as/17/contrib/edb_redwood_nls.config
        //Without the above command, this query will fail: SELECT edb_nls_cf_insert***
        //It works fine but I have commented it out as it will cause the tests to fail on subsequent calls:
        //i.e. the query for Ijsland will return IJsland and not Ijsland on subsequent tests run.
        //await ExecuteSimpleReader(conn, "SELECT edb_nls_cf_insert('xdutch', 'default')");

        var val1 = await ExecuteSimpleReader(conn, "SELECT nls_initcap('ijsland', 'NLS_SORT = XDUTCH');");
        Assert.IsNotNull(val1);
        Assert.AreEqual("Ijsland", val1!.ToString());

        //await ExecuteSimpleReader(conn, "SELECT edb_nls_cf_insert('xdutch', '\"pg_catalog\".\"nl-NL-x-icu\"')");

        //var val2 = await ExecuteSimpleReader(conn, "SELECT nls_initcap('ijsland', 'NLS_SORT = XDUTCH');");
        //Assert.IsNotNull(val2);
        //Assert.AreEqual("IJsland", val2.ToString());

        var val3 = await ExecuteSimpleReader(conn, "SELECT nls_initcap('abcDef pQr', 'NLS_SORT = XTURKISH');");
        Assert.IsNotNull(val3);
        Assert.AreEqual("Abcdef Pqr", val3!.ToString());
    }

    //--DB-1953 : nvl: consider making first argument be of type "any"
    [Test]
    public async Task DB_1953_NVL_FirstArgAnyTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        //Clean
        await Execute(conn, "DROP package body core_pkg_nvl_date", true);
        await Execute(conn, "DROP package core_pkg_nvl_date", true);

        var pkgSql = "CREATE OR REPLACE PACKAGE core_pkg_nvl_date\n"
    + "IS\n"
    + "     d_date DATE;\n"
    + "     v_varchar varchar(200);\n"
    + "     FUNCTION test RETURN void;\n"
    + "END;\n";
        await Execute(conn, pkgSql, false);
        var bodySql = "CREATE OR REPLACE PACKAGE BODY core_pkg_nvl_date\n"
    + "IS\n"
    + " FUNCTION test RETURN void IS\n"
    + " BEGIN\n"
    + "	SELECT to_date('01-01-2000','dd-mm-yyyy') INTO d_date FROM dual;\n"
    + "	v_varchar := concat(d_date,' Monday');\n"
    + "	v_varchar := initcap(v_varchar);\n"
    + "	dbms_output.put_line('date = ' || nvl(NULL,d_date));\n"
    + " END;\n"
    + "END;\n";
        await Execute(conn, bodySql, false);

        var procMsg = "date = 01-JAN-00 00:00:00";
        var message1 = await ExecuteProcNotice(conn, "core_pkg_nvl_date.test");
        Assert.AreEqual(1, message1.Count);
        Assert.AreEqual(procMsg, message1[0]);
    }

    private static async Task SetUpXMLType(EDBConnection conn)
    {
        await Execute(conn, "drop procedure xml_funcs_proc;", true);
        await Execute(conn, "drop table person_xmltype;", true);

        var tblSql = "CREATE TABLE person_xmltype\n"
    + "(\n"
    + "  person_id   integer,\n"
    + "  person_data xmltype\n"
    + ");\n";
        await Execute(conn, tblSql, false);

        var insSql = "INSERT INTO person_xmltype\n"
    + "  (person_id, person_data)\n"
    + "VALUES\n"
    + "(1, xmltype('<PDRecord>\n"
    + "     <PDName>test_user1</PDName>\n"
    + "     <PDID>1</PDID>\n"
    + "     <PDEmail>test_user1@testmail.com</PDEmail>\n"
    + " </PDRecord>'::xml)\n"
    + "),\n"
    + "(2, xmltype('<PDRecord>\n"
    + "     <PDName>test_user2</PDName>\n"
    + "     <PDID>2</PDID>\n"
    + "     <PDEmail>test_user2@testmail.com</PDEmail>\n"
    + "   </PDRecord>'::xml)\n"
    + "),\n"
    + "(3, xmltype('<PDRecord>\n"
    + "     <PDName>test_user3</PDName>\n"
    + "     <PDID>3</PDID>\n"
    + "     <PDEmail>test_user3@testmail.com</PDEmail>\n"
    + "   </PDRecord>'::xml)\n"
    + "),\n"
    + "(4, xmltype('<PDRecord>\n"
    + "     <PDName>test_user4</PDName>\n"
    + "     <PDID>4</PDID>\n"
    + "     <PDEmail>test_user4@testmail.com</PDEmail>\n"
    + "   </PDRecord>'::TEXT)\n"
    + "),\n"
    + "(5, xmltype('<PDRecord>\n"
    + "     <PDName>test_user5</PDName>\n"
    + "     <PDID>5</PDID>\n"
    + "     <PDEmail>test_user5@testmail.com</PDEmail>\n"
    + "   </PDRecord>'::CLOB)\n"
    + ") ;\n";
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

    //--DB-1753 : XMLType: create Oracle-compatible XMLType as object type and implement its member functions.
    [Test]
    public async Task DB_1753_ExtractXMLAsTextTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpXMLType(conn);

        string[] expected =
            [
        "<PDName>test_user1</PDName>",
        "<PDName>test_user2</PDName>",
        "<PDName>test_user3</PDName>",
        "<PDName>test_user4</PDName>",
        "<PDName>test_user5</PDName>"
    ];

        string[] expected2 = [
            "<b xmlns=\"http://example.com\">test</b>",
            "<b xmlns=\"http://example.com\">test1</b>"
           ];

        await RunQueryAndVerifyResultAsync(conn, "SELECT CAST(person_data.EXTRACT_XML('/PDRecord/PDName') AS TEXT) FROM person_xmltype", expected);

        await RunQueryAndVerifyResultAsync(conn, "SELECT CAST(EXTRACT_XML(person_data, '/PDRecord/PDName') AS TEXT) FROM person_xmltype", expected);

        await RunQueryAndVerifyResultAsync(conn,
        "SELECT CAST(EXTRACT_XML(xmltype('<a xmlns=\"http://example.com\"><b>test</b><b>test1</b></a>'), '//mydefns:b', ARRAY[ARRAY['mydefns', 'http://example.com']])  AS TEXT)",
        expected2);
    }

    [Test]
    public async Task DB_1753_ExtractXMLValueTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpXMLType(conn);

        string[] expected =
            [
        "<PDName>test_user1</PDName>",
        "<PDName>test_user2</PDName>",
        "<PDName>test_user3</PDName>",
        "<PDName>test_user4</PDName>",
        "<PDName>test_user5</PDName>"
    ];

        string[] expected2 = [
            "<b xmlns=\"http://example.com\">test</b>",
            "<b xmlns=\"http://example.com\">test1</b>"
           ];

        await RunQueryAndVerifyResultAsync(conn, "SELECT person_data.EXTRACT_XML('/PDRecord/PDName') FROM person_xmltype", expected);

        await RunQueryAndVerifyResultAsync(conn, "SELECT EXTRACT_XML(person_data, '/PDRecord/PDName') FROM person_xmltype", expected);

        await RunQueryAndVerifyResultAsync(conn,
        "SELECT EXTRACT_XML(xmltype('<a xmlns=\"http://example.com\"><b>test</b><b>test1</b></a>'), '//mydefns:b', ARRAY[ARRAY['mydefns', 'http://example.com']])",
        expected2);

        await RunQueryAndVerifyResultAsync(conn, "SELECT person_data.EXTRACT_XML('/PDRecord/PDName') FROM person_xmltype", expected);

        await RunQueryAndVerifyResultAsync(conn, "SELECT EXTRACT_XML(person_data, '/PDRecord/PDName') FROM person_xmltype", expected);

        await RunQueryAndVerifyResultAsync(conn,
        "SELECT EXTRACT_XML(xmltype('<a xmlns=\"http://example.com\"><b>test</b><b>test1</b></a>'), '//mydefns:b', ARRAY[ARRAY['mydefns', 'http://example.com']])",
        expected2);
    }

    //--DB-1753 : XMLType: create Oracle-compatible XMLType as object type and implement its member functions.
    [Test]
    public async Task DB_1753_ExtractValueTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpXMLType(conn);

        string[] expected =
            [
        "test_user1",
        "test_user2",
        "test_user3",
        "test_user4",
        "test_user5"
    ];

        string[] expected2 = [
            "test"
           ];

        await RunQueryAndVerifyResultAsync(conn, "SELECT EXTRACTVALUE(person_data, '/PDRecord/PDName') FROM person_xmltype", expected);

        await RunQueryAndVerifyResultAsync(conn,
        "SELECT EXTRACTVALUE(xmltype('<a xmlns=\"http://example.com\"><b>test</b><b>test1</b></a>'),'//mydefns:b[position()=1]/text()',ARRAY[ARRAY['mydefns', 'http://example.com']])",
        expected2);
    }

    //--DB-1753 : XMLType: create Oracle-compatible XMLType as object type and implement its member functions.
    [Test]
    public async Task DB_1753_MiscFuncsTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpXMLType(conn);

        string[] values =
            [
        "1",
        "test_user1@testmail.com",
        "test_user1@testmail.com",
    ];

        var messages = await ExecuteProcNotice(conn, "xml_funcs_proc");
        Assert.AreEqual(3, messages.Count);
        Assert.AreEqual(values[0], messages[0]);
        Assert.AreEqual(values[1], messages[1]);
        Assert.AreEqual(values[2], messages[2]);
    }

    private static async Task SetUpXMLDOM(EDBConnection conn)
    {
        await Execute(conn, "drop procedure xml_dom_proc1;", true);
        await Execute(conn, "drop procedure xml_dom_proc2;", true);

        var procSql1 = "CREATE OR REPLACE PROCEDURE xml_dom_proc1()\n"
    + "IS\n"
    + "        xml_doc DBMS_XMLDOM.DOMDocument;\n"
    + "        root_node DBMS_XMLDOM.DOMNode;\n"
    + "        item_node DBMS_XMLDOM.DOMNode;\n"
    + "BEGIN\n"
    + "        xml_doc := DBMS_XMLDOM.newDOMDocument(XMLTYPE('<a:query xmlns:a=\"jabber:iq:roster\"><a:item a:subscription=\"both\" a:jid=\"romeo@example.com\"></a:item></a:query>'));\n"
    + "        root_node := DBMS_XMLDOM.getfirstchild(dbms_xmldom.makeNode(xml_doc));\n"
    + "        item_node := dbms_xmldom.getfirstchild(root_node);\n"
    + "        dbms_output.put_line('item node: ' || dbms_xmldom.getnodename(item_node));\n"
    + "        dbms_output.put_line('item attr: ' || dbms_xmldom.getattribute(dbms_xmldom.makeelement(item_node), 'jid', 'jabber:iq:roster'));\n"
    + "        DBMS_XMLDOM.freeDocument(xml_doc);\n"
    + "END;\n";

        await Execute(conn, procSql1, false);

        var procSql2 = "CREATE OR REPLACE PROCEDURE xml_dom_proc2()\n"
    + "IS\n"
    + "    l_xmltype XMLTYPE;\n"
    + "    l_domdoc dbms_xmldom.DOMDocument;\n"
    + "    l_root_node dbms_xmldom.DOMNode;\n"
    + "    l_department_element dbms_xmldom.DOMElement;\n"
    + "    l_departments_node dbms_xmldom.DOMNode;\n"
    + "    l_branch_element dbms_xmldom.DOMElement;\n"
    + "    l_branches_node dbms_xmldom.DOMNode;\n"
    + "BEGIN\n"
    + "    l_domdoc := dbms_xmldom.newDomDocument;\n"
    + "    l_root_node := dbms_xmldom.makeNode(l_domdoc);\n"
    + "    l_department_element := dbms_xmldom.createElement(l_domdoc, 'Departments');\n"
    + "    l_departments_node := dbms_xmldom.appendChild(l_root_node,dbms_xmldom.makeNode(l_department_element));\n"
    + "    l_branch_element := dbms_xmldom.createElement(l_domdoc, 'Branches');\n"
    + "    l_branches_node := dbms_xmldom.appendChild(l_root_node,dbms_xmldom.makeNode(l_branch_element));\n"
    + "    l_xmltype := dbms_xmldom.getXmlType(l_domdoc);\n"
    + "    DBMS_OUTPUT.PUT_LINE(l_xmltype.getStringVal());\n"
    + "END;\n";

        await Execute(conn, procSql2, false);
    }

    //--DB-2579 : Implementation of DBMS_XMLDOM package in EPAS
    [Test]
    public async Task DB_2579_XmlDomTest1()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpXMLDOM(conn);

        string[] values =
            [
        "item node: a:item",
        "item attr: romeo@example.com",
    ];

        var messages = await ExecuteProcNotice(conn, "xml_dom_proc1");
        Assert.AreEqual(2, messages.Count);
        Assert.AreEqual(values[0], messages[0]);
        Assert.AreEqual(values[1], messages[1]);
    }

    //--DB-2579 : Implementation of DBMS_XMLDOM package in EPAS
    [Test]
    public async Task DB_2579_XmlDomTest2()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpXMLDOM(conn);

        string[] values =
            [
        "<Departments/>\n<Branches/>\n",
    ];

        var messages = await ExecuteProcNotice(conn, "xml_dom_proc2");
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(values[0], messages[0]);
    }

    private static async Task SetUpDbmsAssert(EDBConnection conn)
    {
        await Execute(conn, "DROP PROCEDURE get_open_data_dbassert;", true);
        await Execute(conn, "DROP TABLE t1_dbassert;", true);
        await Execute(conn, "DROP TABLE こんにちは;", true);
        await Execute(conn, "DROP TABLE open_tab_dbassert;", true);
        await Execute(conn, "DROP TABLE secret_tab_dbassert;", true);

        await Execute(conn, "CREATE TABLE t1_dbassert (a INT);", false);
        await Execute(conn, "CREATE TABLE こんにちは ( a INT);", false);

        var tblSql1 = "CREATE TABLE open_tab_dbassert (\n"
        + "  code        VARCHAR2(5),\n"
        + "  description VARCHAR2(50)\n"
        + ");\n";
        await Execute(conn, tblSql1, false);
        await Execute(conn, "INSERT INTO open_tab_dbassert VALUES ('ONE', 'Description for ONE');", false);
        await Execute(conn, "INSERT INTO open_tab_dbassert VALUES ('TWO', 'Description for TWO');", false);

        var tblSql2 = "CREATE TABLE secret_tab_dbassert (\n"
        + "  code        VARCHAR2(5),\n"
        + "  description VARCHAR2(50)\n"
        + ");\n";
        await Execute(conn, tblSql2, false);
        await Execute(conn, "INSERT INTO secret_tab_dbassert VALUES ('CODE1', 'SECRET 1');", false);
        await Execute(conn, "INSERT INTO secret_tab_dbassert VALUES ('CODE2', 'SECRET 2');", false);

        var procSql = "CREATE OR REPLACE PROCEDURE get_open_data_dbassert(p_code IN VARCHAR2) AS\n"
            + "  l_sql     VARCHAR2(32767);\n"
            + "  c_cursor  SYS_REFCURSOR;\n"
            + "  l_buffer  VARCHAR2(32767);\n"
            + "BEGIN\n"
            + "  DBMS_OUTPUT.put_line('Raw input format: (' || p_code || ')');\n"
            + "  l_sql := 'SELECT description FROM open_tab_dbassert WHERE code = ''' || p_code || '''';\n"
            + " DBMS_OUTPUT.put_line(l_sql);\n"
            + "  OPEN c_cursor FOR l_sql;\n"
            + "  LOOP\n"
            + "    FETCH c_cursor INTO  l_buffer;\n"
            + "    EXIT WHEN c_cursor%NOTFOUND;\n"
            + "    DBMS_OUTPUT.put_line(l_buffer);\n"
            + "  END LOOP;\n"
            + "  close c_cursor;\n"
            + "  l_buffer:=null;\n"
            + "\n"
            + "  DBMS_OUTPUT.put_line('Input with DBMS_ASSERT : DBMS_ASSERT.ENQUOTE_LITERAL(' || p_code || ')');\n"
            + "  l_sql := 'SELECT description FROM open_tab_dbassert WHERE code = ' || sys.DBMS_ASSERT.ENQUOTE_LITERAL(p_code);\n"
            + " DBMS_OUTPUT.put_line(l_sql);\n"
            + "  OPEN c_cursor FOR l_sql;\n"
            + "  LOOP\n"
            + "    FETCH c_cursor INTO  l_buffer;\n"
            + "    EXIT WHEN c_cursor%NOTFOUND;\n"
            + "    DBMS_OUTPUT.put_line(l_buffer);\n"
            + "  END LOOP;\n"
            + "  close c_cursor;\n"
            + "END;\n";
        await Execute(conn, procSql, false);
    }

    //--DB-1961 : Implement the Oracle DBMS_ASSERT package in Advanced Server
    [Test]
    public async Task DB_1961_EnquoteNameTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsAssert(conn);

        var val1 = await ExecuteSimpleReader(conn, "SELECT 'test' || SYS.DBMS_ASSERT.ENQUOTE_NAME('  \"ObjectName\"  ') || 'test' FROM dual");
        Assert.IsNotNull(val1);
        Assert.AreEqual("test  \"ObjectName\"  test", val1!.ToString());
    }

    //--DB-1961 : Implement the Oracle DBMS_ASSERT package in Advanced Server
    [Test]
    public async Task DB_1961_SchemaNameTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsAssert(conn);

        //This query returns <DB>.sys so we get database from connection.
        var db = conn.Database;
        var val1 = await ExecuteSimpleReader(conn, "SELECT SYS.DBMS_ASSERT.SCHEMA_NAME(current_database() || '.sys') FROM DUAL");
        Assert.IsNotNull(val1);
        Assert.AreEqual(db + ".sys", val1!.ToString());
    }

    //--DB-1961 : Implement the Oracle DBMS_ASSERT package in Advanced Server
    [Test]
    public async Task DB_1961_SqlObjectName_en_Test()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsAssert(conn);

        var val1 = await ExecuteSimpleReader(conn, "SELECT SYS.DBMS_ASSERT.SQL_OBJECT_NAME('t1_dbassert') FROM DUAL");
        Assert.IsNotNull(val1);
        Assert.AreEqual("t1_dbassert", val1!.ToString());
    }

    //--DB-1961 : Implement the Oracle DBMS_ASSERT package in Advanced Server
    [Test]
    public async Task DB_1961_SqlObjectName_japanese_Test()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsAssert(conn);

        var val1 = await ExecuteSimpleReader(conn, "SELECT SYS.DBMS_ASSERT.SQL_OBJECT_NAME('こんにちは') FROM DUAL");
        Assert.IsNotNull(val1);
        Assert.AreEqual("こんにちは", val1!.ToString());
    }

    //--DB-1961 : Implement the Oracle DBMS_ASSERT package in Advanced Server
    [Test]
    public async Task DB_1961_SimpleSqlNameTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsAssert(conn);

        var val1 = await ExecuteSimpleReader(conn, "SELECT SYS.DBMS_ASSERT.SIMPLE_SQL_NAME('ABCD789$#_zxcvbnm') FROM DUAL");
        Assert.IsNotNull(val1);
        Assert.AreEqual("ABCD789$#_zxcvbnm", val1!.ToString());
    }

    //--DB-1961 : Implement the Oracle DBMS_ASSERT package in Advanced Server
    [Test]
    public async Task DB_1961_QualifiedSqlNameTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsAssert(conn);

        var val1 = await ExecuteSimpleReader(conn, "SELECT SYS.DBMS_ASSERT.QUALIFIED_SQL_NAME('aaa.bbb.ccc.\"aaaa\"\"aaa\"') FROM DUAL");
        Assert.IsNotNull(val1);
        Assert.AreEqual("aaa.bbb.ccc.\"aaaa\"\"aaa\"", val1!.ToString());
    }

    //--DB-1961 : Implement the Oracle DBMS_ASSERT package in Advanced Server
    [Test]
    public async Task DB_1961_EnquoteLiteralTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsAssert(conn);

        var val1 = await ExecuteSimpleReader(conn, "SELECT dbms_assert.enquote_literal('ENQUOTE LITERAL') FROM DUAL");
        Assert.IsNotNull(val1);
        Assert.AreEqual("\'ENQUOTE LITERAL\'", val1!.ToString());

        val1 = await ExecuteSimpleReader(conn, "SELECT dbms_assert.enquote_literal('ENQUOTE '''' LITERAL') FROM DUAL");
        Assert.IsNotNull(val1);
        Assert.AreEqual("\'ENQUOTE '' LITERAL\'", val1!.ToString());

        val1 = await ExecuteSimpleReader(conn, "SELECT dbms_assert.enquote_literal('''ENQUOTE LITERAL''') FROM DUAL");
        Assert.IsNotNull(val1);
        Assert.AreEqual("\'ENQUOTE LITERAL\'", val1!.ToString());
    }

    //--DB-1961 : Implement the Oracle DBMS_ASSERT package in Advanced Server
    [EDBExplicit("No reason")]
    [Test]
    public async Task DB_1961_SqlInjectionTest1()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsAssert(conn);

        //The purpose of this test is to test DBMS_ASSERT.ENQUOTE_LITERAL which is already tested above.

        //PSQL execution and output
        //edb=# EXEC get_open_data_dbassert('ONE'' OR ''1''=''1');
        //Raw input format: (ONE' OR '1'='1)
        //SELECT description FROM open_tab_dbassert WHERE code = 'ONE' OR '1'='1'
        //Description for ONE
        //Description for TWO
        //Input with DBMS_ASSERT : DBMS_ASSERT.ENQUOTE_LITERAL(ONE' OR '1'='1)
        //ERROR:  numeric or value error
        //CONTEXT:  edb-spl function get_open_data_dbassert(character varying) line 21 at assignment

        //Not sure how to pass this string 'ONE'' OR ''1''=''1' so it becomes ONE' OR '1'='1 in the procedure.
        //Tried many variations but does not work.
        //May be it is .NET way of avoiding SQL Injection which is the purpose of this test?

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
            using (var cstmt = new EDBCommand("get_open_data_dbassert(:p_code)", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.Parameters.Add(new EDBParameter("p_code", EDBTypes.EDBDbType.Varchar, 10, "p_code",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                cstmt.Parameters[0].Value = "ONE'' OR ''1''=''1";

                await cstmt.PrepareAsync();
                await cstmt.ExecuteNonQueryAsync();
            }
            mre.WaitOne(5000);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Console.WriteLine("Message: " + notice!.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }

    //--DB-1961 : Implement the Oracle DBMS_ASSERT package in Advanced Server
    [EDBExplicit("No reason")]
    [Test]
    public async Task DB_1961_SqlInjectionTest2()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsAssert(conn);

        //The purpose of this test is to test DBMS_ASSERT.ENQUOTE_LITERAL which is already tested above.

        //PSQL execution and output
        //EXEC get_open_data_dbassert('ONE'' UNION SELECT description FROM secret_tab_dbassert WHERE ''1''=''1');
        //Raw input format: (ONE' UNION SELECT description FROM secret_tab_dbassert WHERE '1'='1)
        //SELECT description FROM open_tab_dbassert WHERE code = 'ONE' UNION SELECT description FROM secret_tab_dbassert WHERE '1'='1'
        //SECRET 1
        //SECRET 2
        //Description for ONE
        //Input with DBMS_ASSERT : DBMS_ASSERT.ENQUOTE_LITERAL(ONE' UNION SELECT description FROM secret_tab_dbassert WHERE '1'='1)
        //ERROR:  numeric or value error
        //CONTEXT:  edb-spl function get_open_data_dbassert(character varying) line 19 at assignment

        //Could not make call through .NET.
        //Tried many variations but does not work.
        //May be it is .NET way of avoiding SQL Injection which is the purpose of this test?

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
            using (var cstmt = new EDBCommand("get_open_data_dbassert(:p_code)", conn))
            {
                cstmt.CommandType = CommandType.StoredProcedure;
                cstmt.Parameters.Add(new EDBParameter("p_code", EDBTypes.EDBDbType.Varchar, 10, "p_code",
                    ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                cstmt.Parameters[0].Value = "'ONE'' UNION SELECT description FROM secret_tab_dbassert WHERE ''1''=''1'";

                await cstmt.PrepareAsync();
                await cstmt.ExecuteNonQueryAsync();
            }
            mre.WaitOne(5000);
            for (var i = 0; i < notices.Count; i++)
            {
                var notice = (PostgresNotice?)notices[i];
                Console.WriteLine("Message: " + notice!.MessageText);
            }
        }
        finally
        {
            conn.Notice -= action;
        }
        mre.Close();
    }

    //--DB-2235 : Implement BFILE as native datatype
    private static async Task SetUpDbmsBFILE(EDBConnection conn)
    {
        await Execute(conn, "DROP PROCEDURE displaybfile_proc", true);
        await Execute(conn, "DROP PROCEDURE substringbfile_proc", true);
        await Execute(conn, "DROP TABLE table3_2235;", true);
        await Execute(conn, "DROP TABLE table2_2235;", true);
        await Execute(conn, "DROP TABLE table1_2235;", true);
        await Execute(conn, "DROP DIRECTORY tmp_2235;", true);

        await Execute(conn, "CREATE OR REPLACE DIRECTORY tmp_2235 AS '/tmp';", false);
        await Execute(conn, "CREATE TABLE table1_2235 (col1_2235 INT, col2_2235 BFILE);", false);
        await Execute(conn, "CREATE TABLE table2_2235 (col1_2235 INT, col2_2235 BLOB);", false);
        await Execute(conn, "CREATE TABLE table3_2235 (col1_2235 INT, col2_2235 CLOB);", false);
        await Execute(conn, "INSERT INTO table1_2235 VALUES (10, BFILENAME('tmp_2235','file1_2235.txt'));", false);
        await Execute(conn, "INSERT INTO table1_2235 VALUES (20, BFILENAME('wrongdir','file2_2235.txt'));", false);
        await Execute(conn, "INSERT INTO table2_2235 VALUES (10, UTL_RAW.CAST_TO_RAW('abcd'));", false);
        await Execute(conn, "INSERT INTO table3_2235 VALUES (10, 'abcd');", false);

        var procSql1 = "CREATE OR REPLACE PROCEDURE displaybfile_proc() AUTHID CURRENT_USER\n"
    + "IS\n"
    + "File_loc BFILE;\n"
    + "Buffer RAW(32767);\n"
    + "RetVal INTEGER;\n"
    + "Amount BINARY_INTEGER := 1024;\n"
    + "Position INTEGER := 1;\n"
    + "BEGIN\n"
    + "/* Select the BFILE */\n"
    + "SELECT col2_2235 INTO File_loc FROM table1_2235 WHERE col1_2235 = 10;\n"
    + "DBMS_LOB.FILEOPEN(File_loc, DBMS_LOB.FILE_READONLY);\n"
    + "DBMS_LOB.READ(File_loc, Amount, Position, Buffer);\n"
    + "DBMS_OUTPUT.PUT_LINE('File contents are' || Buffer );\n"
    + "DBMS_LOB.FILECLOSE(File_loc);\n"
    + "END;\n";
        await Execute(conn, procSql1, false);

        var procSql2 = "CREATE OR REPLACE PROCEDURE substringbfile_proc() AUTHID CURRENT_USER\n"
    + "IS\n"
    + "File_loc BFILE;\n"
    + "Position INTEGER := 10;\n"
    + "Buffer RAW(32767);\n"
    + "RetVal INTEGER;\n"
    + "BEGIN\n"
    + "Buffer := DBMS_LOB.SUBSTR(File_loc, 255, Position);\n"
    + "IF Buffer is NULL THEN\n"
    + "DBMS_OUTPUT.PUT_LINE('Buffer is NULL');\n"
    + "END IF;\n"
    + "DBMS_OUTPUT.PUT_LINE('file contents in BFILE are' || Buffer );\n"
    + "/* Select the BFILE: */\n"
    + "SELECT col2_2235 INTO File_loc FROM table1_2235 WHERE col1_2235 = 10;\n"
    + "DBMS_LOB.FILEOPEN(File_loc, DBMS_LOB.FILE_READONLY);\n"
    + "Buffer := DBMS_LOB.SUBSTR(File_loc, 255, Position);\n"
    + "DBMS_OUTPUT.PUT_LINE('file contents in BFILE are' || Buffer );\n"
    + "DBMS_LOB.FILECLOSE(File_loc);\n"
    + "END;\n";
        await Execute(conn, procSql2, false);
    }

    //--DB-2235 : Implement BFILE as native datatype
    //EC-3187: Cannot get BFILE column value
    [Test]
    public async Task DB_2235_SelectTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsBFILE(conn);

        //The actual test shared by the server team and its output is the following:
        //SELECT * FROM table1_2235;
        // col1_2235 |         col2_2235         
        //-----------+---------------------------
        //	10 | tmp_2235,"file1_2235.txt"
        //	20 | wrongdir,"file2_2235.txt"

        using var com = new EDBCommand("", conn);
        com.CommandType = CommandType.Text;

        com.CommandText = "SELECT col1_2235, col2_2235 FROM table1_2235";
        var reader = await com.ExecuteReaderAsync();

        Assert.IsTrue(reader.HasRows);

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(10, reader.GetInt32(0));
        Assert.AreEqual("tmp_2235,\"file1_2235.txt\"", reader.GetValue(1).ToString());

        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual(20, reader.GetInt32(0));
        Assert.AreEqual("wrongdir,\"file2_2235.txt\"", reader.GetValue(1).ToString());

        await reader.CloseAsync();
    }

    //--DB-2235 : Implement BFILE as native datatype
            //For this test to work, create a file /tmp/file1_2235.txt and add the following text in it
    //this is test
    [Test]
    public async Task DB_2235_DisplayBFILEProcTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsBFILE(conn);

        // File contents areThis is a test
        var expected = "\\x46696c6520636f6e74656e7473206172657468697320697320746573740a";
        await CreateTestFileAsync();

        var messages = await ExecuteProcNotice(conn, "displaybfile_proc");
        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(expected, messages[0]);
    }

    private static async Task CreateTestFileAsync()
    {
        if (!Directory.Exists("/tmp"))
        {
            Directory.CreateDirectory("/tmp");
        }
        using var f = File.CreateText("/tmp/file1_2235.txt");
        await f.WriteAsync("this is test\n");
    }


    //--DB-2235 : Implement BFILE as native datatype
    [Test]
    public async Task DB_2235_SubStringBFILEProcTest()
    {
        await using var conn = await OpenConnectionAsync();

        TestUtil.MinimumPgVersion(conn, "17.0.0");

        await SetUpDbmsBFILE(conn);

        string[] values =
        [
    "Buffer is NULL",
    "\\x66696c6520636f6e74656e747320696e204246494c45206172656573740a",
    ];

        var messages = await ExecuteProcNotice(conn, "substringbfile_proc");
        Assert.AreEqual(2, messages.Count);
        Assert.AreEqual(values[0], messages[0]);
        Assert.AreEqual(values[1], messages[1]);
    }
}
