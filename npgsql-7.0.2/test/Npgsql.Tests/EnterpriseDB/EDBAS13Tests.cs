using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections.Generic;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
#pragma warning disable CS8602
    /// <summary>
    /// Tests around EC-1001
    /// </summary>
    /// 
    [TestFixture]
    public class EDBAS13Tests : TestBase
    {
        EDBConnection? conn = null;

        [SetUp]
        public void Init()
        {
            conn = OpenConnection();
            TestUtil.EnsureEDBAdvancedServer(conn);
        }

        [TearDown]
        public void Dispose()
        {
            TestUtil.closeDB(conn);
        }

        [Test, Ignore("EC-1339")]
        public void CompatibleSYSDATE_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropProc("DROP FUNCTION rm44158_fun;");
            DropTable("DROP TABLE rm44158_tab;");

            String function = "create function rm44158_fun return timestamp is "
                      + " begin "
                          + " perform dbms_lock.sleep(1);"
                              + " return sysdate;"
                      + " end;";

            CreateProc(function);

            String anony1 = "declare "
                          + " a timestamp; b timestamp; c timestamp;"
                      + " begin "
                          + " select sysdate, rm44158_fun, sysdate into a, b, c;"
                          + " if a <> c or a = b then"
                              + " dbms_output.put_line('OOPS: ' || a || ' ' || b || ' ' || c);"
                          + " end if;"
                              + " perform dbms_lock.sleep(1);"
                              + " if a = sysdate then"
                                      + " dbms_output.put_line('OOPS: sysdate did not advance');"
                              + " end if;"
                      + " end;";
            CreateAnonymousBlock(anony1);

            String createTable = "create table rm44158_tab(n timestamp);";
            CreateTable(createTable);
            String insert1 = "insert into rm44158_tab values('12-dec-18:12:12:12');";
            String insert2 = "insert into rm44158_tab values('12-may-2010:12:12:33');";
            InsertIntoTable(insert1);
            InsertIntoTable(insert2);

            String anony2 = "DECLARE"
                         + " v_value1   timestamp;"
                         + " v_value2   timestamp;"
                         + " result  integer;"
                      + " BEGIN"
                         + " v_value1= sysdate;"
                         + " perform pg_sleep(1);"
                         + " v_value2= sysdate;"
                      + " insert into rm44158_tab values (v_value1);"
                      + " insert into rm44158_tab values (v_value2);"
                      + " select count(*) into result from rm44158_tab where n=v_value1;--should return only 1"
                      + " DBMS_OUTPUT.PUT_LINE('Should be equal to one '|| result);"
                      + " END;";
            CreateAnonymousBlock(anony2);
        }

        public void CreateAnonymousBlock(String query)
        {
            CreateObject(query);

            Log("AnonymousBlock created successfully");
        }

        [Test]
        public void Explicit_INDEX__PRIMARY_KEY_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropTable("DROP TABLE rm43851_products1;");
            DropTable("DROP TABLE rm43851_products2;");

            //--Test1: Explicit INDEX specified on PRIMARY KEY column along with create
            //--table, server will create the explicit index specified.
            string createTable1 = "CREATE TABLE rm43851_products1("
                    + " product_no INTEGER,"
                    + " name VARCHAR(50),"
                    + " price NUMERIC,"
                    + " CONSTRAINT constraint_rm43851_products1 PRIMARY KEY(product_no)"
                        + " USING INDEX(CREATE INDEX idx1_rm43851_products1 ON rm43851_products1(product_no))"
                    + " );";

            CreateTable(createTable1);

            string select1 = "SELECT conname FROM pg_constraint WHERE conrelid = (SELECT oid FROM pg_class"
                    + " WHERE relname = 'rm43851_products1');";
            string select2 = "SELECT* FROM pg_indexes WHERE tablename = 'rm43851_products1' ORDER BY indexname;";

            SelectFromTable(select1);
            SelectFromTable(select2);
            //\d + rm43851_products1;

            string insert1 = "INSERT INTO rm43851_products1 VALUES(1, 'product 1', 1.1);";
            //string insert2 = "INSERT INTO rm43851_products1 VALUES(1, 'product 1', 1.1);";

            InsertIntoTable(insert1);
            //InsertIntoTable(insert2);

            //--Test2: Explicit INDEX specified on PRIMARY KEY column along with create
            //--table but the order of keys is different.
            string createTable2 = "CREATE TABLE rm43851_products2("
               + " product_no INTEGER,"
               + " name VARCHAR(50),"
               + " price NUMERIC,"
               + " CONSTRAINT constraint_rm43851_products2 PRIMARY KEY(product_no, name)"

                   + " USING INDEX(CREATE INDEX idx1_rm43851_products2 ON"

                                    + " rm43851_products2(name, product_no))"
            + " );";
            CreateTable(createTable2);

            string select3 = "SELECT conname FROM pg_constraint WHERE conrelid = (SELECT oid FROM pg_class"
                    + " WHERE relname = 'rm43851_products2');";
            string select4 = "SELECT* FROM pg_indexes WHERE tablename = 'rm43851_products2' ORDER BY indexname;";

            SelectFromTable(select3);
            SelectFromTable(select4);
            //\d + rm43851_products2;

            string insert3 = "INSERT INTO rm43851_products2 VALUES(1, 'product 1', 1.1);";
            //string insert4 = "INSERT INTO rm43851_products2 VALUES(1, 'product 1', 1.1);";
            InsertIntoTable(insert3);
            //InsertIntoTable(insert4);
        }

        [Test, Ignore("EC-1338")]
        public void UTL_HTTPExceptionHandling_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            string proc1 = "CREATE OR REPLACE PROCEDURE utl_proc1() IS"
                        + " req   UTL_HTTP.REQ;"
                        + " resp UTL_HTTP.RESP;"
                        + " value VARCHAR2(32768);"
                        + " BEGIN"
                + " req := UTL_HTTP.BEGIN_REQUEST('https://www.google.com/');"
        + " resp:= UTL_HTTP.GET_RESPONSE(req);"
            + " LOOP"

        + " UTL_HTTP.READ_LINE(resp, value, TRUE);"
            + " END LOOP;"
            + " UTL_HTTP.END_RESPONSE(resp);"
            + " EXCEPTION"
                + " WHEN UTL_HTTP.END_OF_BODY THEN"

    + " DBMS_OUTPUT.PUT_LINE('Exception caught');"
            + " UTL_HTTP.END_RESPONSE(resp);"
            + " END;";

            string proc2 = "CREATE OR REPLACE PROCEDURE utl_proc2() IS"
                + " req   UTL_HTTP.REQ;"
            + " resp UTL_HTTP.RESP;"
            + " value VARCHAR2(32768);"
            + " BEGIN"
                + " req := UTL_HTTP.BEGIN_REQUEST('https://www.google.com/');"
        + " resp:= UTL_HTTP.GET_RESPONSE(req);"
            + " LOOP"

        + " UTL_HTTP.READ_TEXT(resp, value, 32768);"
            + " END LOOP;"
            + " UTL_HTTP.END_RESPONSE(resp);"
            + " EXCEPTION"
                + " WHEN UTL_HTTP.END_OF_BODY THEN"

    + " DBMS_OUTPUT.PUT_LINE('Exception caught');"
            + " UTL_HTTP.END_RESPONSE(resp);"
            + " END;";

            string proc3 = "CREATE OR REPLACE PROCEDURE utl_proc3() IS"
                + " req   UTL_HTTP.REQ;"
            + " resp UTL_HTTP.RESP;"
            + " value raw(32767);"
            + " BEGIN"
                + " req := UTL_HTTP.BEGIN_REQUEST('https://www.google.com/');"
        + " resp:= UTL_HTTP.GET_RESPONSE(req);"
            + " LOOP"

        + " UTL_HTTP.READ_RAW(resp, value, 32767);"
            + " END LOOP;"
            + " UTL_HTTP.END_RESPONSE(resp);"
            + " EXCEPTION"
                + " WHEN UTL_HTTP.END_OF_BODY THEN"

    + " DBMS_OUTPUT.PUT_LINE('Exception caught');"
            + " UTL_HTTP.END_RESPONSE(resp);"
            + " END;";

            CreateProc(proc1);
            CreateProc(proc2);
            CreateProc(proc3);

            CallProc("utl_proc1();");
            CallProc("utl_proc2(); ");
            CallProc("utl_proc3(); ");
        }

        [Test, Ignore("Requires directories")]
        public void AlterDirectoryOwner_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropDirectory("DROP DIRECTORY dir_Test;");
            DropUser("DROP USER u1 CASCADE;");

            try { System.IO.Directory.Delete("C:\\TestDir"); } catch { }
            CreateUser("create user u1 superuser;");
            try { System.IO.Directory.CreateDirectory("C:\\TestDir"); } catch { }

            Mkdir("create directory dir_Test as 'C:\\TestDir';");
            SelectFromTable("select * from all_directories;");
            AlterDirectory("ALTER DIRECTORY dir_Test OWNER TO u1;");
            SelectFromTable("select * from all_directories;");
        }

        [Test, Ignore("Requires directories for tablespace, Works fine when run against server on Linux, Problem on Windows.")]
        public void PartitionSubPartitionNumberTable_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropTable("DROP TABLE tbl01 CASCADSE;");
            DropTable("DROP TABLE tbl02 CASCADSE;");
            DropTable("DROP TABLE tbl03 CASCADSE;");
            DropTable("DROP TABLE tbl04 CASCADSE;");
            DropTable("DROP TABLE tbl05 CASCADSE;");
            DropTable("DROP TABLE tbl06 CASCADSE;");
            DropTable("DROP TABLE tbl07 CASCADSE;");
            DropTable("DROP TABLE tbl08 CASCADSE;");
            DropTable("DROP TABLE tbl09 CASCADSE;");
            DropTable("DROP TABLE tbl10 CASCADSE;");
            DropTable("DROP TABLE tbl11 CASCADSE;");
            DropTable("DROP TABLE tbl12 CASCADSE;");
            DropTable("DROP TABLE tbl13 CASCADSE;");

            //DropDirectory("DROP DIRECTORY dir_1;");
            //DropDirectory("DROP DIRECTORY dir_2;");

            //Mkdir("\\! mkdir C:/edbtesting/tbsp1");
            //Mkdir("\\! mkdir C:/edbtesting/tbsp2");

            //CreateUser("create user Admin superuser;");

            //try { Directory.CreateDirectory("C:\\tbsp1"); } catch { }
            //try { Directory.CreateDirectory("C:\\tbsp2"); } catch { }
            //try { Directory.CreateDirectory("C:\\tbsp1"); } catch { }
            //try { Directory.CreateDirectory("C:\\tbsp2"); } catch { }

            //Mkdir("CREATE DIRECTORY dir_0 AS 'C:\\';");
            //Mkdir("CREATE DIRECTORY dir_1 AS 'C:\\tbsp1';");
            //Mkdir("CREATE DIRECTORY dir_2 AS 'C:\\tbsp2';");

            //Mkdir("ALTER DIRECTORY dir_0 OWNER TO Admin;");
            //Mkdir("ALTER DIRECTORY dir_1 OWNER TO Admin;");
            //Mkdir("ALTER DIRECTORY dir_2 OWNER TO Admin;");

            CreateTablespace("create tablespace tbsp1 location '/tmp/tbsp1';");
            CreateTablespace("create tablespace tbsp2 location '/tmp/tbsp2';");

            //--PARTITIONS number
            CreateTable("CREATE TABLE tbl01(c1 INT, c2 INT) PARTITION BY HASH(c1) PARTITIONS 2; ");
            SelectFromTable("SELECT table_name, partition_name FROM user_tab_partitions WHERE table_name = 'TBL01' ORDER BY 1,2; ");

            //--PARTITIONS number STORE IN(tablespace names)
            CreateTable("CREATE TABLE tbl02(c1 INT, c2 INT) PARTITION BY HASH(c1) PARTITIONS 3 STORE IN(tbsp1, tbsp2); ");
            SelectFromTable("SELECT table_name, partition_name, tablespace_name FROM user_tab_partitions WHERE table_name = 'TBL02' ORDER BY 1,2; ");

            //--PARTITIONS numbers with HASH - *partitioning
            CreateTable("CREATE TABLE tbl03(c1 INT, c2 INT) PARTITION BY HASH(c1) SUBPARTITION BY RANGE(c2) PARTITIONS 2; ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL03' ORDER BY 1,2; ");

            //--SUBPARTITIONS number with *-HASH partitioning
            CreateTable("CREATE TABLE tbl04(c1 INT, c2 INT) PARTITION BY LIST(c1) SUBPARTITION BY HASH(c2) SUBPARTITIONS 2(PARTITION p1 VALUES(10)); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL04' ORDER BY 1,2; ");

            //--PARTITIONS number SUBPARTITIONS number
            CreateTable("CREATE TABLE tbl05(c1 INT, c2 INT) PARTITION BY HASH(c1) SUBPARTITION BY HASH(c2) SUBPARTITIONS 3 PARTITIONS 2; ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL05' ORDER BY 1,2; ");

            //--SUBPARTITIONS number STORE IN(tablespace names)
            CreateTable("CREATE TABLE tbl06(c1 INT, c2 INT) PARTITION BY HASH(c1) SUBPARTITION BY HASH(c2) SUBPARTITIONS 3 PARTITIONS 2 STORE IN(tbsp1, tbsp2); ");
            SelectFromTable("SELECT table_name, partition_name, tablespace_name FROM user_tab_partitions WHERE table_name = 'TBL06' ORDER BY 1,2; ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name, tablespace_name FROM user_tab_subpartitions WHERE table_name = 'TBL06' ORDER BY 1,2; ");

            //--PARTITIONS number STORE IN(tablespace names) SUBPARTITIONS number STORE IN(tablespace names)
            CreateTable("CREATE TABLE tbl07(c1 INT, c2 INT) PARTITION BY HASH(c1) SUBPARTITION BY HASH(c2) SUBPARTITIONS 3 STORE IN(tbsp1) PARTITIONS 2 STORE IN(tbsp2, tbsp1); ");
            SelectFromTable("SELECT table_name, partition_name, tablespace_name FROM user_tab_partitions WHERE table_name = 'TBL07' ORDER BY 1,2; ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name, tablespace_name FROM user_tab_subpartitions WHERE table_name = 'TBL07' ORDER BY 1,2; ");

            //--SUBPARTITIONS number and SUBPARTITION descriptions
            CreateTable("CREATE TABLE tbl08(c1 INT, c2 INT) PARTITION BY RANGE(c1) SUBPARTITION BY HASH(c2) SUBPARTITIONS 2(PARTITION p1 VALUES LESS THAN(1)(SUBPARTITION s1), PARTITION p2 VALUES LESS THAN(2)); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL08' ORDER BY 1,2; ");

            //--SUBPARTITIONS number IN partition description
            CreateTable("CREATE TABLE tbl09(c1 INT, c2 INT) PARTITION BY RANGE(c1) SUBPARTITION BY HASH(c2) SUBPARTITIONS 2(PARTITION p1 VALUES LESS THAN(10) SUBPARTITIONS 3, PARTITION p2 VALUES LESS THAN(20)); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL09' ORDER BY 1,2; ");

            //--SUBPARTITIONS number STORE IN(tablespace names) IN partition description
            CreateTable("CREATE TABLE tbl10(c1 INT, c2 INT) PARTITION BY LIST(c1) SUBPARTITION BY HASH(c2) SUBPARTITIONS 3 STORE IN(tbsp1) (PARTITION p1 VALUES(10), PARTITION p2 VALUES(20) SUBPARTITIONS 2 STORE IN(tbsp2)); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name, tablespace_name FROM user_tab_subpartitions WHERE table_name = 'TBL10' ORDER BY 1,2; ");

            //--STORE IN(tablespace names) and explicit tablespaces
            CreateTable("CREATE TABLE tbl11 (c1 INT, c2 INT) PARTITION BY LIST (c1) SUBPARTITION BY HASH(c2) SUBPARTITIONS 3 STORE IN (tbsp1) (PARTITION p1 VALUES (20) TABLESPACE tbsp2); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name, tablespace_name FROM user_tab_subpartitions WHERE table_name = 'TBL11' ORDER BY 1,2; ");

            //-- ADD PARTITION 
            //-- will create number of subpartitions specified in partition defination.
            CreateTable("CREATE TABLE tbl12 (c1 INT, c2 INT) PARTITION BY LIST (c1) SUBPARTITION BY HASH (c2) SUBPARTITIONS 2 (PARTITION p1 VALUES (10)); ");
            AlterTable("ALTER TABLE tbl12 ADD PARTITION p2 VALUES(15,16,19,20); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL12' ORDER BY 1,2; ");

            //-- ADD PARTITION with SUBPARTITIONS number
            //-- will create number of subpartitions specified in add partition.
            AlterTable("ALTER TABLE tbl12 ADD PARTITION p3 VALUES (30) SUBPARTITIONS 3; ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL12' and partition_name = 'P3' ORDER BY 1,2; ");

            //-- ADD PARTITION SUBPARTITIONS number STORE IN(tablespace names)
            //-- will create number of subpartitions within tablespaces specified in add partition.
            AlterTable("ALTER TABLE tbl12 ADD PARTITION p4 VALUES (50) SUBPARTITIONS 4 STORE IN(tbsp1); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name, tablespace_name FROM user_tab_subpartitions WHERE table_name = 'TBL12' and partition_name = 'P4' ORDER BY 1,2; ");

            //-- SPLIT PARTITION with SUBPARTITIONS number
            //-- will split partition with subpartitions numbers given in split command.
            AlterTable("ALTER TABLE tbl12 SPLIT PARTITION p2 VALUES (15,16) INTO(PARTITION p2_1 SUBPARTITIONS 5, PARTITION p2_2); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name, tablespace_name FROM user_tab_subpartitions WHERE table_name = 'TBL12' and partition_name like 'P2%' ORDER BY 1,2; ");

            //-- ALTER TABLE..SET SUBPARTITION TEMPLATE
            CreateTable("CREATE TABLE tbl13 (c1 INT, c2 INT) PARTITION BY RANGE (c1) SUBPARTITION BY HASH (c2) SUBPARTITIONS 2 (PARTITION p1 VALUES LESS THAN (10)); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL13' ORDER BY 1,2; ");

            //-- adding new partition will add number of subpartition specified in partition defination. (2 here)
            AlterTable("ALTER TABLE tbl13 ADD PARTITION p2 VALUES LESS THAN(20);");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL13' and partition_name = 'P2' ORDER BY 1,2; ");

            //--by using template subpartitions number is modified(5 here).
            AlterTable("ALTER TABLE tbl13 SET SUBPARTITION TEMPLATE 5; ");

            //--now adding new partition will add number of subpartition modified by template. (5 here)
            AlterTable("ALTER TABLE tbl13 ADD PARTITION p3 VALUES LESS THAN(30); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL13' and partition_name = 'P3' ORDER BY 1,2; ");

            //--by using template () reset subpartitions number to default i.e. 1.
            AlterTable("ALTER TABLE tbl13 SET SUBPARTITION TEMPLATE(); ");

            //--now adding new partition will add number of subpartition as default (1 here)
            AlterTable("ALTER TABLE tbl13 ADD PARTITION p4 VALUES LESS THAN(40); ");
            SelectFromTable("SELECT table_name, partition_name, subpartition_name FROM user_tab_subpartitions WHERE table_name = 'TBL13' and partition_name = 'P4' ORDER BY 1,2; ");
        }

        [Test]
        public void Parallel_NoParallel_Create_Table_Index_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropTable("DROP TABLE rm43833 CASCADE;");
            DropTable("DROP TABLE rm43833_t1 CASCADE;");
            //--Create table should accept the PARALLEL clause with some degree and set the
            //--parallel_worker = degree value.
            CreateTable("CREATE TABLE rm43833(c1 int) PARALLEL 6 WITH(FILLFACTOR = 66);");
            SelectFromTable("SELECT reloptions FROM pg_class WHERE relname = 'rm43833';");

            //--Create table should accept the NOPARALLEL clause and set the
            //-- paraller_workers = 0
            CreateTable("CREATE TABLE rm43833_t1(c1 int) NOPARALLEL WITH(FILLFACTOR= 66);");
            SelectFromTable("SELECT reloptions FROM pg_class WHERE relname = 'rm43833_t1';");

            //--Alter table should accept the PARALLEL / NOPARALLEL clause.If NOPARALLEL
            //--option is provided set the parallel_workers = 0
            AlterTable("ALTER TABLE rm43833 NOPARALLEL;");
            SelectFromTable("SELECT reloptions FROM pg_class WHERE relname = 'rm43833';");

            //--Alter table should accept the PARALLEL clause with some degree and set the
            //--parallel_worker = degree value
            AlterTable("ALTER TABLE rm43833 PARALLEL(DEGREE 3);");
            SelectFromTable("SELECT reloptions FROM pg_class WHERE relname = 'rm43833';");

            //--Alter table should accept the PARALLEL clause without degree which will
            //--remove the parallel_worker option if exists otherwise will ignore.
            AlterTable("ALTER TABLE rm43833 PARALLEL;");
            SelectFromTable("SELECT reloptions FROM pg_class WHERE relname = 'rm43833';");

            CreateIndex("CREATE UNIQUE INDEX rm43833_idx1 ON rm43833(c1) PARALLEL 7;");
            SelectFromTable("SELECT reloptions FROM pg_class WHERE relname = 'rm43833_idx1';");

            //--Alter table should accept the PARALLEL / NOPARALLEL clause.If NOPARALLEL
            //--option is provided set the parallel_workers = 0, also set
            //-- max_parallel_maintenance_workers = 0 for that statement.
            AlterIndex("ALTER INDEX rm43833_idx1 NOPARALLEL;");
            SelectFromTable("SELECT reloptions FROM pg_class WHERE relname = 'rm43833_idx1';");
        }

        [Test]
        public void DefaultBehaviour_dbms_output_compatible_Redwood_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropProc("DROP PROCEDURE dbms_output_proc;");
            string createProc = "CREATE OR REPLACE PROCEDURE dbms_output_proc() IS"
            + " BEGIN"
            + " SET dbms_output.serveroutput = OFF;"
            + " dbms_output.put_line('when \"set dbms_output.serveroutput OFF\" is used.');"
            + " SET dbms_output.serveroutput = ON;"
            + " dbms_output.put_line('when \"set dbms_output.serveroutput ON\" is used.');"
            + " SET dbms_output.serveroutput = OFF;"
            + " dbms_output.put_line('when \"set dbms_output.serveroutput OFF\" is used.');"
            + " END;";

            CreateProc(createProc);

            CallProc("dbms_output_proc()");
        }

        [Test]
        public void Stats_Mode_Function_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropTable("DROP TABLE smt1 CASCADE;");
            CreateTable("create table smt1(a int, b int, c varchar2(10));");
            InsertIntoTable("insert into smt1 values(1, 10, 'str1');");
            InsertIntoTable("insert into smt1 values(2, 20, 'str2');");
            InsertIntoTable("insert into smt1 values(3, 30, 'str3');");
            InsertIntoTable("insert into smt1 values(1, 10, 'str1');");
            InsertIntoTable("insert into smt1 values(2, 20, 'str2');");
            InsertIntoTable("insert into smt1 values(3, 30, 'str3');");
            InsertIntoTable("insert into smt1 values(3, 30, 'str3');");

            //--Testcase 1: stats_mode as a normal aggregate function.
            CreateView("create view sm_vw1 as select stats_mode(a) sm_a from smt1;");

            //--Testcase 2: stats_mode as a normal aggregate on textual column.
            CreateView("create view sm_vw2 as select stats_mode(c) sm_c from smt1;");

            //--Testcase 3: stats_mode with ORDER BY clause but not GROUP BY clause.
            CreateView("create view sm_vw3 as select stats_mode(a) sm_a from smt1 group by b order by 1;");

            //--Testcase 4: stats_mode with GROUP BY clause.
            CreateView("create view sm_vw4 as select stats_mode(a) sm_a from smt1 group by b;");

            //--Testcase 5: stats_mode as an ordered - set aggregate function.
            CreateView("create view sm_vw5 as select stats_mode(a) sm_a from smt1 group by a order by 1;");

            //--Testcase 6: stats_mode with GROUPING SETS.
            CreateView("create view sm_vw6 as select stats_mode(a) sm_a from smt1 group by grouping sets((a), (b));");

            SelectFromTable("select * from sm_vw1; ");
            SelectFromTable("select * from sm_vw2; ");
            SelectFromTable("select * from sm_vw3; ");
            SelectFromTable("select * from sm_vw4; ");
            SelectFromTable("select * from sm_vw5; ");
            SelectFromTable("select * from sm_vw6; ");
        }

        [Test]
        public void DBMS_SQL_Func_Proc_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropProc("DROP PROCEDURE dbmssql_proc1;");
            DropProc("DROP PROCEDURE dbmssql_proc2;");

            DropTable("DROP TABLE rm44142_dtypes;");
            DropTable("DROP TABLE rm44142_tbl;");

            string createTable1 = "CREATE TABLE rm44142_dtypes(col_long LONG, col_int  INTEGER);";
            CreateTable(createTable1);

            string insert1 = "INSERT INTO rm44142_dtypes VALUES('TestingForDefineColumnValueLong', 1);";
            InsertIntoTable(insert1);

            string createProc1 = "CREATE OR REPLACE PROCEDURE dbmssql_proc1() IS"
               + " cur_id               INTEGER;"
            + " v_long VARCHAR2(20);"
            + " sql_stmt VARCHAR2(50) := 'SELECT col_int, col_long ' || ' FROM rm44142_dtypes';"
            + " status INTEGER;"
            + " length INTEGER;"

            + " BEGIN"
               + " cur_id := DBMS_SQL.OPEN_CURSOR;"
            + " DBMS_SQL.PARSE(cur_id, sql_stmt, DBMS_SQL.native);"
            + " DBMS_SQL.DEFINE_COLUMN_LONG(cur_id, 2);"
            + " status:= DBMS_SQL.EXECUTE(cur_id);"
            + " status:= DBMS_SQL.FETCH_ROWS(cur_id);"
            + " DBMS_SQL.COLUMN_VALUE_LONG(cur_id, 2, 12, 10, v_long, length);"
            + " DBMS_OUTPUT.PUT_LINE('col_long: ' || v_long);"
            + " DBMS_OUTPUT.PUT_LINE('value_length: ' || length);"
            + " DBMS_SQL.CLOSE_CURSOR(cur_id);"
            + " END;";
            CreateProc(createProc1);

            CallProc("dbmssql_proc1()");

            string createTable2 = "CREATE TABLE rm44142_tbl(col_int INTEGER PRIMARY KEY);";
            CreateTable(createTable2);

            string insert2 = "INSERT INTO rm44142_tbl VALUES(1);";
            InsertIntoTable(insert2);

            string createProc2 = "CREATE OR REPLACE PROCEDURE dbmssql_proc2() IS"
               + " cur_id               INTEGER;"
            + " sql_stmt VARCHAR2(50) := 'SELECT col_int FROM rm44142';"
            + " position INTEGER;"
            + " BEGIN"
               + " cur_id := DBMS_SQL.OPEN_CURSOR;"
            + " DBMS_SQL.PARSE(cur_id, sql_stmt, DBMS_SQL.native);"
            + " EXCEPTION"
            + " WHEN OTHERS THEN"
            + " position:= DBMS_SQL.LAST_ERROR_POSITION;"
            + " DBMS_OUTPUT.PUT_LINE('error position = ' || position);"
            + " DBMS_SQL.CLOSE_CURSOR(cur_id);"
            + " END;";
            CreateProc(createProc2);

            CallProc("dbmssql_proc2()");
        }

        [Test, Ignore("EC-1337")]
        public void Function_to_timestamp_tz_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropView("DROP VIEW tstz_vw;");
            string createView = "CREATE VIEW tstz_vw as SELECT"
            + " TO_TIMESTAMP_TZ('12-jan-2010', 'dd-month-yyyy') tz1, "
            + " TO_TIMESTAMP_TZ('12-january-2010', 'dd-mon-yyyy') tz2, "
            + " TO_TIMESTAMP_TZ('', 'DD/MM/YYYY HH24:MI:SS') tz3, "
            + " TO_TIMESTAMP_TZ('15-JUL-84', 'ddth-fmmonth-YYYY') tz4, "
            + " TO_TIMESTAMP_TZ('10-Sep-02 14:10:10.123000', 'DD-Mon-RR HH24:MI:SS.FF') tz5, "
            + " TO_TIMESTAMP_TZ('03-APR-07 09:12:21 P.M', 'DD-MON-YY HH12:MI:SS A.M') tz6, "
            + " TO_CHAR(TO_TIMESTAMP_TZ('210955', 'HH24:SS:MI'), 'HH:MI:SS') tz7, "
            + " TO_TIMESTAMP_TZ('20-MAR-20 04:30:00 +08:00', 'DD-MON-YY HH:MI:SS TZH:TZM') tz8, "
            + " TO_TIMESTAMP_TZ('20-MAR-20 04:30:00 +00:00', 'DD-MON-YY HH:MI:SS TZH:TZM') tz9, "
            + " TO_TIMESTAMP_TZ('20-MAR-20 04:30:00 +05:30', 'DD-MON-YY HH:MI:SS TZH:TZM') tz10, "
            + " TO_TIMESTAMP_TZ('01-Jan-4711 BC', 'DD-Mon-YYYY BC') tz11"
            + " FROM DUAL;";

            CreateView(createView);

            string select = "select* from tstz_vw;";
            SelectFromTable(select);
        }

        [Test]
        public void FuncProcInsidePkgBody_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropPackageBody("DROP PACKAGE BODY test_func_spec_pkg");
            DropPackage("DROP PACKAGE test_func_spec_pkg");
            string pkg = "create or replace package test_func_spec_pkg as"
                            + " function test1(col1 number) return number;"
                            + " procedure test2(col1 IN number, col2 OUT number);"
                         + " end test_func_spec_pkg;";

            CreatePackage(pkg);

            string pkgbody = "create or replace package body test_func_spec_pkg as"

                            + " function test1(col1 number) return number is"
                            + " begin"
                            + " return 1;"
                            + " end test1;"

                            + " procedure test2(col1 IN number, col2 OUT number) as"
                            + " begin"
                            + " null;"
                            + " end test2;"

                            + " function test3(col4 number) return number;"
                            + " function test3(col4 number) return number is"
                            + " begin"
                            + " return 1;"
                            + " end test3;"

                            + " end test_func_spec_pkg;";

            CreatePackageBody(pkgbody);
        }

        [Test]
        public void FMFormatIn_to_number_Function_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropTable("DROP TABLE fm_tab CASCADE");
            string createTable = "create table fm_tab(c1 varchar2(50));";
            CreateTable(createTable);

            string insert = "insert into fm_tab values ('0101.010'), ('00915.12300');";
            InsertIntoTable(insert);

            string createView = "create view fm_vw as select to_number(c1,'FM99999999.99999') from fm_tab;";
            CreateView(createView);

            string selectView1 = "select * from all_views where view_name = 'FM_VW';";
            SelectFromTable(selectView1);

            string selectView2 = "select * from fm_vw ;";
            SelectFromTable(selectView2);
        }

        [Test, Ignore("EC-1336")]
        public void AutomaticListPartitionaing_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            string dropTable = "drop table alp_tab";
            DropTable(dropTable);

            string createTable = "create table alp_tab (a int) partition by list(a) automatic (partition p1 values (1), partition p2 values (2));";
            CreateTable(createTable);

            string insert = "insert into alp_tab values(1), (2), (3), (4);";
            InsertIntoTable(insert);

            //\d+ alp_tab

            string select = "select tableoid::regclass, * from alp_tab;";
            SelectFromTable(select);
        }

        [Test]
        public void AutomaticListPartitionaing_Workaround_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            string dropTable = "drop table alp_tab";
            DropTable(dropTable);

            string createTable = "create table alp_tab (a int) partition by list(a) automatic (partition p1 values (1), partition p2 values (2));";
            CreateTable(createTable);

            string insert = "insert into alp_tab values(1), (2), (3), (4);";
            InsertIntoTable(insert);

            //\d+ alp_tab

            string select = "select tableoid::text, * from alp_tab;";
            SelectFromTable(select);
        }

        [Test]
        public void AggregateFuncMedian_IndexNumber_Test()
        {
#nullable disable
            TestUtil.MinimumPgVersion(conn, "13.0.0");
#nullable restore 
            DropTable("DROP TABLE median_test CASCADE");
            //-- Additional smallint, int, bigint, and numeric variants in aggregate function MEDIAN.
            string createTable = "create table median_test(c1 smallint, c2 integer, c3 bigint, c4 decimal, c5 numeric, c6 real, c7 double precision, c8 NUMERIC(3, 2),c9 NUMERIC(3),c10 float, c11 float(2),c12 float8, c13 money,c14 interval, c15 long, c16 smallserial,c17 serial, c18 bigserial);";
            CreateTable(createTable);

            string insert1 = "INSERT INTO median_test VALUES (32766,2147483646,9223372036854775806,131071.16383,131071.16383,131071.16383,131071.16383,1.1,1.0,131.16,131,131,131,'5 days 4 hours',1111);";
            string insert2 = "INSERT INTO median_test VALUES (12766,1147483646,1223372036854775806,31071.16383,31071.16383,31071.16383,31071.16383,2.1,2.0,31.16,31,31,31,'2 days 4 hours',111);";
            InsertIntoTable(insert1);
            InsertIntoTable(insert2);

            string createView = "CREATE VIEW median_vw AS select median(c1) mc1, median(c2) mc2, median(c3) mc3, median(c4) mc4, median(c5) mc5, median(c6) mc6, median(c7) mc7, median(c8) mc8, median(c9) mc9, median(c10) mc10, median(c11) mc11, median(c12) mc12, median(c13::numeric) mc13, median(c14) mc14, median(c15::numeric) mc15, median(c16) mc16, median(c17) mc17, median(c18) mc18 from median_test;";
            CreateView(createView);

            string selectView = "SELECT * FROM median_vw;";
            SelectFromTable(selectView);
            //-- Added support for CREATE INDEX syntax that contains a column name and number i.e. (col_name,1)

            string createIndex = "create index idx1_median_test on median_test(c1,5);";
            CreateIndex(createIndex);
            //\d+ median_test

        }

        void CallProc(string proc)
        {
            //Log("Calling Procedure...");
            try
            {
                using (EDBCommand com = new EDBCommand("", conn))
                {
                    com.CommandType = CommandType.StoredProcedure;

                    com.CommandText = proc;
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
                //Log("Exception: " + ex.Message);
            }
            Log("Procedure called successfully.");
        }

        void CreateObject(string create)
        {
            try
            {
                using (EDBCommand com = new EDBCommand("", conn))
                {
                    com.CommandType = CommandType.Text;

                    com.CommandText = create;
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
                //Log("Exception: " + ex.Message);
            }
        }

        void AlterTable(string alterTable)
        {
            Log("Altering Table...");

            CreateObject(alterTable);

            Log("Table Alterred successfully");
        }

        void Mkdir(string query)
        {
            //Log("Creating Directory...");

            CreateObject(query);

            Log("Directory created successfully");
        }

        void CreateUser(string query)
        {
            //Log("Creating User...");

            CreateObject(query);

            Log("User created successfully");
        }

        void DropUser(string query)
        {
            //Log("Dropping User...");

            CreateObject(query);

            Log("User dropped successfully");
        }

        void AlterDirectory(string query)
        {
            //Log("Alterring Directory...");

            CreateObject(query);

            Log("Directory alterred successfully");
        }

        void DropDirectory(string query)
        {
            //Log("Dropping Directory...");

            CreateObject(query);

            Log("Directory dropped successfully");
        }

        void CreateTablespace(string createTableSpace)
        {
            //Log("Creating TableSpace...");

            CreateObject(createTableSpace);

            Log("TableSpace created successfully");
        }

        void AlterIndex(string alterIndex)
        {
            //Log("Altering Index...");

            CreateObject(alterIndex);

            Log("Index Alterred successfully");
        }

        void CreateTable(string createTable)
        {
            //Log("Creating Table...");

            CreateObject(createTable);

            Log("Table created successfully");
        }

        void CreateProc(string createProc)
        {
            //Log("Creating Procedure...");

            CreateObject(createProc);

            Log("Procedure created successfully");
        }

        void CreatePackage(string createPkg)
        {
            //Log("Creating Package...");

            CreateObject(createPkg);

            Log("Package created successfully");
        }

        void CreatePackageBody(string createPkgBody)
        {
            //Log("Creating Package Body...");

            CreateObject(createPkgBody);

            Log("Package Body created successfully");
        }

        void CreateView(string createView)
        {
            //Log("Creating View...");

            CreateObject(createView);

            Log("View created successfully");
        }

        void CreateIndex(string createIndex)
        {
            //Log("Creating Index...");

            CreateObject(createIndex);

            Log("Index created successfully");
        }

        void DropObject(string query)
        {
            try
            {
                using (EDBCommand com = new EDBCommand("", conn))
                {
                    com.CommandType = CommandType.Text;

                    com.CommandText = query;
                    com.ExecuteNonQuery();
                }
            }
            catch  //We want to ignore error in case of drop.
            {

            }
        }

        void DropProc(string dropProc)
        {
            //Log("Dropping Procedure if exists...");

            DropObject(dropProc);

            Log("Procedure dropped successfully");
        }

        void DropPackage(string dropPkg)
        {
            //Log("Dropping Package if exists...");

            DropObject(dropPkg);

            Log("Package dropped successfully");
        }

        void DropPackageBody(string dropPkgBody)
        {
            //Log("Dropping Package Body if exists...");

            DropObject(dropPkgBody);

            Log("Package Body dropped successfully");
        }

        void DropView(string dropView)
        {
            //Log("Dropping View if exists...");

            DropObject(dropView);

            Log("View dropped successfully");
        }

        void DropTable(string dropTable)
        {
            //Log("Dropping Table if exists...");

            DropObject(dropTable);

            Log("Table dropped successfully");
        }

        void InsertIntoTable(string insert)
        {
            //Log("Inserting into Table...");

            try
            {
                using (EDBCommand command = new EDBCommand(insert, conn))
                {
                    Int32 rowsAdded = command.ExecuteNonQuery();
                    Log("Inserted rows: " + rowsAdded);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
                //Log("Exception: " + ex.Message);
            }

            Log("Inserted into Table successfully.");
        }

        void SelectFromTable(string select)
        {
            //Log("Selecting from table...");

            try
            {
                using (EDBCommand cmdSelect = new EDBCommand(select, conn))
                {
                    cmdSelect.CommandType = CommandType.Text;
                    using (EDBDataReader reader = cmdSelect.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Log("Value [" + 0 + "]: " + reader.GetString(0));
                            //Log("Value [" + 1 + "]: " + reader.GetInt32(1));
                            for (int i = 0; i < reader.GetColumnSchema().Count; i++)
                                Log("Value [" + i + "]: " + reader.GetValue(i));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
                Log("Exception in SelectFromTable: " + ex.Message);
            }

            Log("Selected from table successfully.");
        }

        void DescribeTable(string query)
        {
            //Log("Selecting from table...");

            try
            {
                using (EDBCommand cmdSelect = new EDBCommand(query, conn))
                {
                    cmdSelect.CommandType = CommandType.Text;
                    using (EDBDataReader reader = cmdSelect.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Log("Value [" + 0 + "]: " + reader.GetString(0));
                            //Log("Value [" + 1 + "]: " + reader.GetInt32(1));
                            for (int i = 0; i < reader.GetColumnSchema().Count; i++)
                                Log("Value [" + i + "]: " + reader.GetValue(i));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
                //Log("Exception: " + ex.Message);
            }

            Log("Selected from table successfully.");
        }

        void Log(string msg)
        {
            Console.WriteLine("[=======] " + msg);
        }
    }
}
