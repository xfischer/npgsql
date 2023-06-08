using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Tests
{
#pragma warning disable CS8602
    /// <summary>
    /// Tests around EC-1001
    /// </summary>
    /// 
    [TestFixture]
    [NonParallelizable]
    public class EDBAS14Tests : TestBase
    {

        

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

        private async Task ExecuteReader(string query, bool shouldHaveValues)
        {
            try
            {
                await using var conn = await OpenConnectionAsync();

                using (var com = new EDBCommand("", conn))
                {
                    com.CommandType = CommandType.Text;

                    com.CommandText = query;
                    EDBDataReader reader = await com.ExecuteReaderAsync();

                    if (shouldHaveValues)
                    {
                        Assert.AreEqual(true, reader.HasRows);

                        //Just for debugging.
                        //while (reader.Read())
                        //{
                            //Log("Value [" + 0 + "]: " + reader.GetString(0));
                            //Log("Value [" + 1 + "]: " + reader.GetInt32(1));
                            //for (var i = 0; i < reader.GetColumnSchema().Count; i++)
                            //{
                            //    var val = reader.GetValue(i);
                            //    Console.WriteLine("Value [" + i + "]: " + val);
                            //}
                        //}
                    }

                        reader.Close();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task SupportForAliasInInsertStatement()
        {
            await using var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore
            //Clean
            await  Execute("DROP TABLE db299_inventory", true);
            await  Execute("DROP TABLE db299_test_insert", true);
            await  Execute("DROP TYPE db299_inventory_item", true);
            await  Execute("DROP TABLE db299_demo_tab1", true);
            await  Execute("DROP TYPE db299_demo_typ2", true);
            await  Execute("DROP TYPE db299_demo_typ1", true);

            //Setup

            await  Execute("CREATE TYPE db299_inventory_item AS (name text, supplier_id int, price numeric);", false);
            await  Execute("CREATE TABLE db299_inventory(item db299_inventory_item);", false);
            await  Execute("CREATE TABLE db299_test_insert(col1 INT, col2 INT);", false);
            await  Execute("CREATE TYPE db299_demo_typ1 AS OBJECT (a1 NUMBER, a2 NUMBER);", false);
            await  Execute("CREATE TYPE db299_demo_typ2 AS OBJECT (a1 db299_demo_typ1, a2 NUMBER);", false);
            await  Execute("CREATE TABLE db299_demo_tab1 (b1 NUMBER, b2 db299_demo_typ2);", false);

            //Test alias in insert statement for simple column
            Assert.AreEqual(1, await Execute("INSERT INTO db299_test_insert VALUES (1, 1);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db299_test_insert (col1, col2) VALUES (2, 2);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db299_test_insert ti (col1, col2) VALUES (3, 3);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db299_test_insert ti (ti.col1, ti.col2) VALUES (4, 4);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db299_test_insert AS ti (ti.col1, ti.col2) VALUES (5, 5);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db299_test_insert AS ti (ti.col1, col2) VALUES (6, 6);", false));

            //Test alias in insert statement for composite column
            Assert.AreEqual(1, await Execute("INSERT INTO db299_inventory (item.name, item.supplier_id, item.price) VALUES ('it1', 1, 20.2);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db299_inventory x (x.item.name, x.item.supplier_id, x.item.price) VALUES ('it2', 2, 20.4);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db299_inventory as x (x.item.name, x.item.supplier_id, x.item.price) VALUES ('it3', 3, 20.5);", false));

            //Test alias in nested composite type
            Assert.AreEqual(1, await Execute("INSERT INTO db299_demo_tab1 a1 (b2.a1.a2, b1) VALUES (1, 2);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db299_demo_tab1 a1 (a1.b2.a1.a2, b1) VALUES (1, 2);", false));
        }

        [Test]
        public async Task AccessPartitionByPartitionOrSubName()
        {
            var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore


            //Clean
            await  Execute("DROP TABLE db93_bar", true);
            await  Execute("DROP TABLE db93_foo", true);

            await  Execute("CREATE TABLE db93_foo (empno INT, empsal INT)\n"
                 + "PARTITION BY LIST (empno) SUBPARTITION BY LIST (empsal)\n"
                 + "(\n"
                        + "PARTITION p1 VALUES (1, 2)\n"
                        + "(\n"
                                + "SUBPARTITION p1_s1 VALUES (10),\n"
                                + "SUBPARTITION p1_s2 VALUES (20)\n"
                    + "),\n"
                    + "PARTITION p2 VALUES (3, 4)\n"
                        + "(\n"
                                + "SUBPARTITION p2_s1 VALUES (30),\n"
                                + "SUBPARTITION p2_s2 VALUES (40)\n"
                        + ")\n"
                 + ");", false);

            await  Execute("CREATE TABLE db93_bar (empno INT, empsal INT)\n"
             + "PARTITION BY LIST (empno) SUBPARTITION BY LIST (empsal)\n"
             + "(\n"
                + "PARTITION p1 VALUES (5, 6)\n"
                + "(\n"
                   + "SUBPARTITION p1_s1 VALUES (50),\n"
                   + "SUBPARTITION p1_s2 VALUES (60)\n"
                + "),\n"
                + "PARTITION p2 VALUES (7, 8)\n"
                + "(\n"
                   + "SUBPARTITION p2_s1 VALUES (70),\n"
                   + "SUBPARTITION p2_s2 VALUES (80)\n"
                + ")\n"
             + ");", false);

            //-- Check support in INSERT operation
            //--
            //-- Insert data into the table
            Assert.AreEqual(1, await Execute("INSERT INTO db93_foo PARTITION (p1) VALUES (1, 10);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db93_foo SUBPARTITION (p1_s2) VALUES (2, 20);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db93_foo SUBPARTITION (p2_s1) VALUES (3, 30);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db93_foo PARTITION (p2) VALUES (4, 40);", false));

            Assert.AreEqual(1, await Execute("INSERT INTO db93_bar PARTITION (p1) VALUES (5, 50);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db93_bar PARTITION (p1) VALUES (6, 60);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db93_bar SUBPARTITION (p2_s1) VALUES (7, 70);", false));
            Assert.AreEqual(1, await Execute("INSERT INTO db93_bar SUBPARTITION (p2_s2) VALUES (8, 80);", false));
        }

        [Test]
        public async Task UTL_FILEPackageSupportReadWriteBinaryFile()
        {
            var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore

            await  Execute("CREATE OR REPLACE FUNCTION read_bin_file() return void as\n"
                + "DECLARE\n"
                        + "v_tempfile UTL_FILE.FILE_TYPE;\n"
                        + "v_filename  varchar(20) := 'small_test.png';\n"
                        + "v_temprec BYTEA;\n"
                        + "v_count INTEGER := 0;\n"
                + "BEGIN\n"
                        + "v_tempfile := UTL_FILE.FOPEN('edatadir', v_filename, 'rb');\n"
                        + "UTL_FILE.GET_RAW(v_tempfile,v_temprec);\n"
                        + "UTL_FILE.GET_RAW(v_tempfile,v_temprec);\n"
                        + "UTL_FILE.GET_RAW(v_tempfile,v_temprec);\n"
                  + "EXCEPTION WHEN no_data_found THEN\n"
                      + "DBMS_OUTPUT.PUT_LINE(SQLCODE||' : '||SQLERRM);\n"
                        + "UTL_FILE.FCLOSE(v_tempfile);\n"
                  + "WHEN others THEN\n"
                        + "RAISE notice 'exception, others : SQLERRM: %', sqlerrm;\n"
                        + "UTL_FILE.FCLOSE(v_tempfile);\n"
                + "END;", false);
            await ExecuteReader("select read_bin_file();", true);
        }

         [Test, Ignore("Similar to EC-1339 as this is anonymous block")]
        public async Task ChangeDBMS_SQLNUMBERInsteadOfINTEGER()
        {
            var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore

            await  Execute("DECLARE\n"
             + "CURSOR_ID INTEGER;\n"
             + "SQL_STATEMENT VARCHAR2(50):='SELECT 4*5 AS RESULT FROM DUAL';\n"
            + "BEGIN \n"
              + "CURSOR_ID := DBMS_SQL.OPEN_CURSOR;\n"
              + "DBMS_OUTPUT.PUT_LINE('CURSOR ID : '||' '||CURSOR_ID);\n"
              + "DBMS_SQL.PARSE(CURSOR_ID,SQL_STATEMENT,DBMS_SQL.NATIVE);\n"
              + "DBMS_OUTPUT.PUT_LINE('RESULT :'||' '||SQL_STATEMENT);\n"
              + "DBMS_SQL.CLOSE_CURSOR(CURSOR_ID);\n"
            + "END;", false);

            //--WITH NUMBER DATA TYPE 
            await  Execute("DECLARE\n"
             + "CURSOR_ID NUMBER(10);\n"
             + "SQL_STATEMENT VARCHAR2(50):='SELECT 4*5 AS RESULT FROM DUAL';\n"
            + "BEGIN \n"
              + "CURSOR_ID := DBMS_SQL.OPEN_CURSOR;\n"
              + "DBMS_OUTPUT.PUT_LINE('CURSOR ID : '||' '||CURSOR_ID);\n"
              + "DBMS_SQL.PARSE(CURSOR_ID,SQL_STATEMENT,DBMS_SQL.NATIVE);\n"
              + "DBMS_OUTPUT.PUT_LINE('RESULT :'||' '||SQL_STATEMENT);\n"
              + "DBMS_SQL.CLOSE_CURSOR(CURSOR_ID);\n"
            + "END;", false);
        }
        
          [Test]
        public async Task NVL2ToUseAnycompatibleRatherThanAnyelement()
        {
            var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore

            await  ExecuteReader("SELECT NVL2( NULL, 'a', 'b');", true);
            await  ExecuteReader("SELECT NVL2( 'a', 'b', 'c');", true);
            await  ExecuteReader("SELECT NVL2( 1, 'a', 'b');", true);
            await  ExecuteReader("SELECT NVL2( 'a', 1, 2);", true);
            await  ExecuteReader("SELECT NVL2( NULL, 1, 2);", true);
        }

         [Test]
        public async Task SupportFor_userenv_Function()
        {
            var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore

            await  Execute("DROP TABLE DB109_TBL_1", true);

            await  Execute("CREATE TABLE DB109_TBL_1\n"
                + "(\n"
                  + "DBA_STATUS 	 VARCHAR2(20),\n"
                  + "LANG_NAME	 VARCHAR2(20),\n"
                  + "LANGUAGE_NAME  VARCHAR2(200),\n"
                  + "TERMINAL_ID 	 VARCHAR2(20),\n"
                  + "SID_NO 	 VARCHAR2(20)\n"
                + ");", false);

            Assert.AreEqual(1, await Execute("INSERT INTO DB109_TBL_1 VALUES(USERENV('ISDBA'),USERENV('LANG'),USERENV('LANGUAGE'),USERENV('TERMINAL'),USERENV('SID'));", false));

            await  ExecuteReader("SELECT * FROM DB109_TBL_1;", true);
        }

         [Test]
        public async Task AddBITANDAndBITOROracleCompatibleFunctions()
        {
            await using var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore

            await  ExecuteReader("Select BITAND(0, 25984734923529780987243592490572349857) from dual;", true);
            await  ExecuteReader("Select BITAND(0, -25984734923529780987243592490572349857) from dual;", true);
            await  ExecuteReader("Select BITAND(1, 25984734923529780987243592490572349857) from dual;", true);
            await  ExecuteReader("Select BITAND(1, -25984734923529780987243592490572349857) from dual;", true);
            await  ExecuteReader("Select BITAND(1, 25984734923529780987243592490572349856) from dual;", true);
            await  ExecuteReader("Select BITAND(1, -25984734923529780987243592490572349856) from dual;", true);
            await  ExecuteReader("Select BITAND(-1, 25984734923529780987243592490572349857) from dual;", true);
            await  ExecuteReader("Select BITAND(-1, -25984734923529780987243592490572349857) from dual;", true);
            await  ExecuteReader("Select BITAND(25984734923529780987243592490572349856, -1) from dual;", true);
            await ExecuteReader("Select BITAND(-1, -25984734923529780987243592490572349856) from dual;", true);
            await ExecuteReader("Select BITOR(0, 25984734923529780987243592490572349857) from dual;", true);
            await ExecuteReader("Select BITOR(0, -25984734923529780987243592490572349857) from dual;", true);
            await ExecuteReader("Select BITOR(1, 25984734923529780987243592490572349857) from dual;", true);
            await ExecuteReader("Select BITOR(1, -25984734923529780987243592490572349857) from dual;", true);
            await ExecuteReader("Select BITOR(1, 25984734923529780987243592490572349856) from dual;", true);
            await ExecuteReader("Select BITOR(1, -25984734923529780987243592490572349856) from dual;", true);
            await ExecuteReader("Select BITOR(-1, 25984734923529780987243592490572349857) from dual;", true);
            await ExecuteReader("Select BITOR(-1, -25984734923529780987243592490572349857) from dual;", true);
        }

         [Test]
        public async Task ImplementationOfSUBPARTITIONTemplateInEPAS()
        {
            await using var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore

        await Execute("DROP TABLE db563_list_list", true);

        await Execute("CREATE TABLE db563_list_list (\n"
          + "col1        NUMBER,\n"
          + "col2        NUMBER\n"
        + ")\n"
        + "PARTITION BY LIST (col1)\n"
        + "SUBPARTITION BY LIST (col2)\n"
        + "SUBPARTITION TEMPLATE(\n"
          + "SUBPARTITION s1 VALUES (10),\n"
          + "SUBPARTITION s2 VALUES (20)\n"
        + ")\n"
        + "(\n"
          + "PARTITION p1 VALUES (100),\n"
          + "PARTITION p2 VALUES (200)\n"
        + ");", false);

        await ExecuteReader("SELECT partition_name, subpartition_name, high_value\n"
        + "FROM sys.all_tab_subpartitions\n"
        + "WHERE table_name LIKE 'DB563_LIST_LIST'\n"
        + "ORDER BY 1,2;", true);

        }

          [Test, Ignore("Similar to EC-1339 as this is anonymous block")]
        public async Task EnablingParameter_default_with_rowids()
        {
            await using var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore

        await Execute("DROP TABLE db171_tab_1", true);
        await Execute("DROP TABLE db171_tab", true);

        await Execute("CREATE OR REPLACE PACKAGE db171 IS\n"
            + "last_rowid bigint;\n"
        + "END;\n", false);
        await Execute("BEGIN\n"
          + "db171.last_rowid := nvl(pg_sequence_last_value(\'sys.rowid_global_seq\'), 0);\n"
        + "END;", false);

        await Execute("SET default_with_rowids TO ON;", true);

        await Execute("CREATE TABLE db171_tab (id INT) PARTITION BY RANGE(id)\n"
        + "(\n"
          + "PARTITION p1 VALUES LESS THAN (5),\n"
         + "PARTITION p2 VALUES LESS THAN (10)\n"
        + ");", false);

        await ExecuteReader("SELECT attname, attishidden\n"
        + "FROM pg_attribute\n"
        + "WHERE attrelid = 'db171_tab'::regclass AND attnum > 0\n"
        + "ORDER BY attnum;", true);

        await Execute("CREATE TABLE db171_tab_1 (id INT, name VARCHAR2(10)) WITH (ROWIDS = TRUE);", false);
        await Execute("INSERT INTO db171_tab_1 VALUES (1, 'One'), (2, 'Two'), (3, 'Three');", false);
        await ExecuteReader("SELECT rowid - db171.last_rowid AS rowid, * FROM db171_tab_1\n"
            + "ORDER BY rowid DESC;", true);

        }

          [Test, Ignore("")]
        public async Task SupportPRIORInTargetListForCONNECTBYQueries()
        {
            await using var conn = await OpenConnectionAsync();

#nullable disable
            TestUtil.MinimumPgVersion(conn, "14.0.0");
#nullable restore

        await Execute("DROP TABLE emp", true);
        await Execute("DROP TABLE dept", true);
        await Execute("DROP TABLE test_emp", true);

        await Execute("CREATE TABLE test_emp(empno INT NOT NULL, ename VARCHAR2(100), mgr INT, dept INT);", false);
        await Execute("INSERT INTO test_emp VALUES (1,'CHAIRMAN', NULL, 50);", false);
        await Execute("INSERT INTO test_emp VALUES (11,'DIRECTOR-1',1, 100);", false);
        await Execute("INSERT INTO test_emp VALUES (22,'DIRECTOR-2',1, 150);", false);
        await Execute("INSERT INTO test_emp VALUES (111,'MANAGER-11-1',11, 100);", false);
        await Execute("INSERT INTO test_emp VALUES (222,'MANAGER-11-2',11, 100);", false);
        await Execute("INSERT INTO test_emp VALUES (333,'MANAGER-22-1',22, 150);", false);
        await Execute("INSERT INTO test_emp VALUES (444,'MANAGER-22-2',22, 150);", false);
        await Execute("INSERT INTO test_emp VALUES (1111,'EMPLOYEE-11-1-1',111, 100);", false);
        await Execute("CREATE TABLE dept (\n"
            + "deptno          NUMBER(2) NOT NULL CONSTRAINT dept_pk PRIMARY KEY,\n"
            + "dname           VARCHAR2(14) CONSTRAINT dept_dname_uq UNIQUE,\n"
            + "loc             VARCHAR2(13)\n"
        + ");", false);

        await Execute("CREATE TABLE emp (\n"
            + "empno           NUMBER(4) NOT NULL CONSTRAINT emp_pk PRIMARY KEY,\n"
            + "ename           VARCHAR2(10),\n"
            + "job             VARCHAR2(9),\n"
            + "mgr             NUMBER(4),\n"
            + "hiredate        DATE,\n"
            + "sal             NUMBER(7,2) CONSTRAINT emp_sal_ck CHECK (sal > 0),\n"
            + "comm            NUMBER(7,2),\n"
            + "deptno          NUMBER(2) CONSTRAINT emp_ref_dept_fk\n"
                                + "REFERENCES dept(deptno)\n"
        + ");", false);

        await Execute("INSERT INTO dept VALUES (10,'ACCOUNTING','NEW YORK');", false);
        await Execute("INSERT INTO dept VALUES (20,'RESEARCH','DALLAS');", false);
        await Execute("INSERT INTO dept VALUES (30,'SALES','CHICAGO');", false);
        await Execute("INSERT INTO dept VALUES (40,'OPERATIONS','BOSTON');", false);

        await Execute("INSERT INTO emp VALUES (7369,'SMITH','CLERK',7902,'17-DEC-80',800,NULL,20);", false);
        await Execute("INSERT INTO emp VALUES (7499,'ALLEN','SALESMAN',7698,'20-FEB-81',1600,300,30);", false);
        await Execute("INSERT INTO emp VALUES (7521,'WARD','SALESMAN',7698,'22-FEB-81',1250,500,30);", false);
        await Execute("INSERT INTO emp VALUES (7566,'JONES','MANAGER',7839,'02-APR-81',2975,NULL,20);", false);
        await Execute("INSERT INTO emp VALUES (7654,'MARTIN','SALESMAN',7698,'28-SEP-81',1250,1400,30);", false);
        await Execute("INSERT INTO emp VALUES (7698,'BLAKE','MANAGER',7839,'01-MAY-81',2850,NULL,30);", false);
        await Execute("INSERT INTO emp VALUES (7782,'CLARK','MANAGER',7839,'09-JUN-81',2450,NULL,10);", false);
        await Execute("INSERT INTO emp VALUES (7788,'SCOTT','ANALYST',7566,'19-APR-87',3000,NULL,20);", false);
        await Execute("INSERT INTO emp VALUES (7839,'KING','PRESIDENT',NULL,'17-NOV-81',5000,NULL,10);", false);
        await Execute("INSERT INTO emp VALUES (7844,'TURNER','SALESMAN',7698,'08-SEP-81',1500,0,30);", false);
        await Execute("INSERT INTO emp VALUES (7876,'ADAMS','CLERK',7788,'23-MAY-87',1100,NULL,20);", false);
        await Execute("INSERT INTO emp VALUES (7900,'JAMES','CLERK',7698,'03-DEC-81',950,NULL,30);", false);
        await Execute("INSERT INTO emp VALUES (7902,'FORD','ANALYST',7566,'03-DEC-81',3000,NULL,20);", false);
        await Execute("INSERT INTO emp VALUES (7934,'MILLER','CLERK',7782,'23-JAN-82',1300,NULL,10);", false);


        await ExecuteReader("SELECT LPAD(ename,length(ename)+2*(level-1),' ') AS employee, PRIOR empno, mgr, level FROM test_emp CONNECT BY PRIOR empno = mgr ORDER BY level, ename;", true);
        await ExecuteReader("SELECT LPAD(ename,length(ename)+2*(level-1),' ') AS employee, PRIOR empno, mgr, level, PRIOR dept FROM test_emp CONNECT BY PRIOR empno = mgr AND PRIOR dept = dept ORDER BY level, ename;", true);

        await ExecuteReader("SELECT empno, PRIOR empno, CONNECT_BY_ISCYCLE FROM emp, dept WHERE emp.job LIKE 'MANAGER' START WITH mgr IS NULL CONNECT BY NOCYCLE PRIOR empno = mgr;", true);

        }
    }
}
