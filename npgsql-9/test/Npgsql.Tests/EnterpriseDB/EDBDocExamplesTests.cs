using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Xml.Linq;
using System.Numerics;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
    [NonParallelizable]
    public class EDBDocExamplesTests : EPASTestBase
    {
        //Doc link:
        //https://www.enterprisedb.com/docs/net_connector/latest/06_opening_a_database_connection/
        [Test]
        public async Task OpeningConnectionTest()
        {
            try
            {
                await using var dataSource = EDBDataSource.Create(ConnectionString);
                var connection = dataSource.OpenConnection();

                //We are here means connection is successful.

                connection.Close();
            }
            catch (Exception exp)
            {
                Assert.Fail(exp.ToString());
            }
        }

        //Doc link:
        //https://www.enterprisedb.com/docs/net_connector/latest/06_opening_a_database_connection/
        [Test]
        public async Task OpeningConnectionAsyncTest()
        {
            try
            {
                await using var dataSource = EDBDataSource.Create(ConnectionString);
                var connection = await dataSource.OpenConnectionAsync();

                //We are here means connection is successful.

                await connection.CloseAsync();
            }
            catch (Exception exp)
            {
                Assert.Fail(exp.ToString());
            }
        }

        //Doc link:
        //First example:
        //https://www.enterprisedb.com/docs/net_connector/latest/07_retrieving_database_records/
        [Test]
        public async Task RetrieveDatabaseRecordsTest()
        {
            await using var dataSource = EDBDataSource.Create(ConnectionString);
            var connection = await dataSource.OpenConnectionAsync();

            using var cmdSelect = new EDBCommand("SELECT * FROM dept", connection);
            cmdSelect.CommandType = CommandType.Text;
            using var drDept = await cmdSelect.ExecuteReaderAsync();
            var dNos = new List<int>();
            var dNames = new List<string>();
            var dLocs = new List<string>();
            while (await drDept.ReadAsync())
            {
                dNos.Add(drDept.GetInt32(0));
                dNames.Add(drDept.GetString(1));
                dLocs.Add(drDept.GetString(2));
            }
            Assert.AreEqual(4, dNos.Count);
            Assert.AreEqual(4, dNames.Count);
            Assert.AreEqual(4, dLocs.Count);

            Assert.True(dNos.Contains(10));
            Assert.True(dNos.Contains(20));
            Assert.True(dNos.Contains(30));
            Assert.True(dNos.Contains(40));

            Assert.True(dNames.Contains("ACCOUNTING"));
            Assert.True(dNames.Contains("RESEARCH"));
            Assert.True(dNames.Contains("SALES"));
            Assert.True(dNames.Contains("OPERATIONS"));

            Assert.True(dLocs.Contains("NEW YORK"));
            Assert.True(dLocs.Contains("DALLAS"));
            Assert.True(dLocs.Contains("CHICAGO"));
            Assert.True(dLocs.Contains("BOSTON"));

            await connection.CloseAsync();
        }

        //Doc link:
        //Second example
        //https://www.enterprisedb.com/docs/net_connector/latest/07_retrieving_database_records/
        [Test]
        public async Task RetrieveSingleDatabaseRecordsTest()
        {
            await using var dataSource = EDBDataSource.Create(ConnectionString);
            var connection = await dataSource.OpenConnectionAsync();

            using var cmd = new EDBCommand("SELECT MAX(sal) FROM emp", connection);
            cmd.CommandType = CommandType.Text;
            var maxSal = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            Assert.AreEqual(5000, maxSal);

            await connection.CloseAsync();
        }

        //Doc link:
        //https://www.enterprisedb.com/docs/net_connector/latest/08_parameterized_queries/
        [Test]
        public async Task ParameterizedQueriesTest()
        {
            await using var dataSource = EDBDataSource.Create(ConnectionString);
            var connection = await dataSource.OpenConnectionAsync();

            //Just verification, not for doc
            using var cmd = new EDBCommand("SELECT sal FROM emp WHERE empno=7788", connection);
            var sal = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            Assert.AreEqual(3000, sal);
            //End verifiation

            //For doc
            var updateQuery = "UPDATE emp SET sal = sal+500 where empno = :ID";

            using var cmdUpdate = new EDBCommand(updateQuery, connection);

            cmdUpdate.Parameters.Add(new EDBParameter(":ID", EDBTypes.EDBDbType.Integer));

            cmdUpdate.Parameters[0].Value = 7788;

            await cmdUpdate.ExecuteNonQueryAsync();
            //End for doc

            //Just verification, not for doc
            using var cmd2 = new EDBCommand("SELECT sal FROM emp WHERE empno=7788", connection);
            var salUpdated = Convert.ToInt32(await cmd2.ExecuteScalarAsync());
            Assert.AreEqual(sal + 500, salUpdated);
            //End verifiation

            //Just for reverting our change if other tests depend on this data, not for doc.
            var updateQuery2 = "UPDATE emp SET sal = sal-500 where empno = :ID";
            using var cmdUpdate2 = new EDBCommand(updateQuery2, connection);
            cmdUpdate2.Parameters.Add(new EDBParameter(":ID", EDBTypes.EDBDbType.Integer));
            cmdUpdate2.Parameters[0].Value = 7788;
            await cmdUpdate2.ExecuteNonQueryAsync();
            //End

            //Just verification, not for doc
            using var cmd3 = new EDBCommand("SELECT sal FROM emp WHERE empno=7788", connection);
            var sal3 = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            Assert.AreEqual(3000, sal3);
            //End verifiation

            await connection.CloseAsync();

        }

        //Doc Links: Insert and Delete
        //https://www.enterprisedb.com/docs/net_connector/latest/09_inserting_records_in_a_database/
        //https://www.enterprisedb.com/docs/net_connector/latest/10_deleting_records_in_a_database/
        [Test]
        public async Task InsertRecordInDatabaseTest()
        {

            await using var dataSource = EDBDataSource.Create(ConnectionString);
            var connection = await dataSource.OpenConnectionAsync();

            //Just verification, not for doc
            using var cmd = new EDBCommand("SELECT ename FROM emp WHERE empno=1234", connection);

            using var dr1 = await cmd.ExecuteReaderAsync();
            //Record should not exist so read will return false.
            Assert.False(await dr1.ReadAsync());

            await dr1.CloseAsync();
            //End verifiation

            //For doc
            var cmdQuery = "INSERT INTO emp(empno,ename) VALUES(:EmpNo, :EName)";

            using var cmdInsert = new EDBCommand(cmdQuery, connection);

            cmdInsert.Parameters.Add(new EDBParameter(":EmpNo", EDBTypes.EDBDbType.Integer));
            cmdInsert.Parameters[0].Value = 1234;

            cmdInsert.Parameters.Add(new EDBParameter(":EName", EDBTypes.EDBDbType.Text));
            cmdInsert.Parameters[1].Value = "Lola";

            await cmdInsert.ExecuteNonQueryAsync();
            //End for doc

            //Just verification, not for doc
            using var cmd2 = new EDBCommand("SELECT ename FROM emp WHERE empno=1234", connection);
            using var dr2 = await cmd2.ExecuteReaderAsync();
            //Record should exist so read will return true.
            Assert.True(await dr2.ReadAsync());
            Assert.AreEqual("Lola", dr2.GetString(0));
            await dr2.CloseAsync();
            //End verifiation

            //For doc
            var strDeleteQuery = "DELETE FROM emp WHERE empno = :ID";

            using var deleteCommand = new EDBCommand(strDeleteQuery, connection);

            deleteCommand.Parameters.Add(new EDBParameter(":ID", EDBTypes.EDBDbType.Integer));
            deleteCommand.Parameters[0].Value = 1234;

            await deleteCommand.ExecuteNonQueryAsync();
            //End for doc

            //Just verification, not for doc
            using var cmd3 = new EDBCommand("SELECT ename FROM emp WHERE empno=1234", connection);
            using var dr3 = await cmd3.ExecuteReaderAsync();
            //Record should not exist so read will return false.
            Assert.False(await dr3.ReadAsync());
            await dr3.CloseAsync();
            //End verifiation

            await connection.CloseAsync();

        }

        //Used for CREATE/DROP procedures.
        private async Task RunQuery(string query, EDBConnection connection, bool verifyStatus=false)
        {
            //We don't throw any error or fail test here because this is done in actual test.
            try
            {
                using var cmd = new EDBCommand(query, connection);
                await cmd.ExecuteNonQueryAsync();
            }
            catch(Exception ex)
            {
                if (verifyStatus)
                    Assert.Fail(ex.Message);
            }
        }

        //Doc link:
        //First example.
        //https://www.enterprisedb.com/docs/net_connector/latest/11_using_spl_stored_procedures_in_your_net_application/
        [Test]
        public async Task ExecSPWithoutParamsTest()
        {
            await using var dataSource = EDBDataSource.Create(ConnectionString);
            var connection = await dataSource.OpenConnectionAsync();

            //Drop procedure if it exists.
            await RunQuery("DROP PROCEDURE list_dept10", connection);

            //Crate procedure
            var procSql = "CREATE OR REPLACE PROCEDURE list_dept10"
                + " IS"
                + " v_deptname VARCHAR2(30);"
                + " BEGIN"
                + " DBMS_OUTPUT.PUT_LINE('Dept No: 10');"
                + " SELECT dname INTO v_deptname FROM dept WHERE deptno = 10;"
                + " DBMS_OUTPUT.PUT_LINE('Dept Name: ' || v_deptname);"
                + " END;";
            await RunQuery(procSql, connection);

            //Just verification, not for doc
            var mre = new ManualResetEvent(false);
            var notices = new ArrayList();
            NoticeEventHandler action = (sender, args) =>
            {
                notices.Add(args.Notice);
                mre.Set();
            };
            connection.Notice += action;
            try
            {
                //End verification

                //For doc
                using var cmdStoredProc = new EDBCommand("list_dept10", connection);
                cmdStoredProc.CommandType = CommandType.StoredProcedure;

                await cmdStoredProc.PrepareAsync();
                await cmdStoredProc.ExecuteNonQueryAsync();
                //End doc

                //Just verification, not fo doc
                mre.WaitOne(5000);
                Assert.AreEqual(2, notices.Count);
                var expectedMsgs = new List<string>()
                {
                    "Dept No: 10",
                    "Dept Name: ACCOUNTING"
                };
                for (var i = 0; i < notices.Count; i++)
                {
                    var notice = (PostgresNotice?)notices[i];
                    Assert.AreEqual(expectedMsgs[i], notice!.MessageText);
                }
            }
            finally
            {
                connection.Notice -= action;
            }
            mre.Close();
            //End verification.
        }

        //Doc link:
        //Second example.
        //https://www.enterprisedb.com/docs/net_connector/latest/11_using_spl_stored_procedures_in_your_net_application/
        [Test]
        public async Task ExecSPWithInParamsTest()
        {
            await using var dataSource = EDBDataSource.Create(ConnectionString);
            var connection = await dataSource.OpenConnectionAsync();

            //Drop procedure if it exists.
            await RunQuery("DROP PROCEDURE EMP_INSERT", connection);

            //Crate procedure
            var procSql = "CREATE OR REPLACE PROCEDURE"
                + "  EMP_INSERT"
                + "  ("
                + "     pENAME IN VARCHAR,"
                + "     pJOB IN VARCHAR,"
                + "     pSAL IN FLOAT4,"
                + "     pCOMM IN FLOAT4,"
                + "     pDEPTNO IN INTEGER,"
                + "     pMgr IN INTEGER"
                + "   )"
                + " AS"
                + " DECLARE"
                + "  CURSOR TESTCUR IS SELECT MAX(EMPNO) FROM EMP;"
                + "  MAX_EMPNO INTEGER := 10;"
                + " BEGIN"

                + "  OPEN TESTCUR;"
                + "  FETCH TESTCUR INTO MAX_EMPNO;"
                + "  INSERT INTO EMP(EMPNO,ENAME,JOB,SAL,COMM,DEPTNO,MGR)"
                + "    VALUES(MAX_EMPNO+1,pENAME,pJOB,pSAL,pCOMM,pDEPTNO,pMgr);"
                + "  CLOSE testcur;"
                + " END;";
            await RunQuery(procSql, connection, true);

            //Just verification, not for doc
            using var cmd1 = new EDBCommand("SELECT COUNT(*) FROM emp", connection);
            var count1 = Convert.ToInt32(await cmd1.ExecuteScalarAsync());
            //End verifiation

            //For doc
            var empName = "EDB";
            var empJob = "Manager";
            var salary = 1000.0;
            var commission = 0.0;
            var deptno = 20;
            var manager = 7839;
            using var cmdStoredProc =
                new EDBCommand("EMP_INSERT(:EmpName,:Job,:Salary,:Commission,:DeptNo,:Manager)", connection);
            cmdStoredProc.CommandType = CommandType.StoredProcedure;

            cmdStoredProc.Parameters.Add(new EDBParameter
            ("EmpName", EDBTypes.EDBDbType.Varchar));
            cmdStoredProc.Parameters[0].Value = empName;

            cmdStoredProc.Parameters.Add(new EDBParameter
            ("Job", EDBTypes.EDBDbType.Varchar));
            cmdStoredProc.Parameters[1].Value = empJob;

            cmdStoredProc.Parameters.Add(new EDBParameter
            ("Salary", EDBTypes.EDBDbType.Real));
            cmdStoredProc.Parameters[2].Value = salary;

            cmdStoredProc.Parameters.Add(new EDBParameter
            ("Commission", EDBTypes.EDBDbType.Real));
            cmdStoredProc.Parameters[3].Value = commission;

            cmdStoredProc.Parameters.Add(new EDBParameter
            ("DeptNo", EDBTypes.EDBDbType.Integer));
            cmdStoredProc.Parameters[4].Value = deptno;

            cmdStoredProc.Parameters.Add
            (new EDBParameter("Manager", EDBTypes.EDBDbType.Integer));
            cmdStoredProc.Parameters[5].Value = manager;

            await cmdStoredProc.PrepareAsync();
            await cmdStoredProc.ExecuteNonQueryAsync();
            //End doc

            //Just verification, not for doc
            using var cmd2 = new EDBCommand("SELECT COUNT(*) FROM emp", connection);
            var count2 = Convert.ToInt32(await cmd2.ExecuteScalarAsync());

            //One row should be inserted.
            Assert.AreEqual(count1 + 1, count2);

            //Verify the t\rwo inserted.
            await using var seletcmd = new EDBCommand("SELECT ENAME, JOB, SAL, COMM, DEPTNO, MGR FROM EMP WHERE ENAME='EDB'", connection);
            await using var dr = await seletcmd.ExecuteReaderAsync();
            Assert.True(await dr.ReadAsync());

            Assert.AreEqual(empName, dr.GetString(0));
            Assert.AreEqual(empJob, dr.GetString(1));
            Assert.AreEqual(salary, dr.GetDouble(2));
            Assert.AreEqual(commission, dr.GetDouble(3));
            Assert.AreEqual(deptno, dr.GetInt32(4));
            Assert.AreEqual(manager, dr.GetInt32(5));

            await dr.CloseAsync();
            //End verifiation

            //Just for reverting the change so that data is the same for next tests.
            var strDeleteQuery = "DELETE FROM emp WHERE ename='EDB'";
            using var deleteCommand = new EDBCommand(strDeleteQuery, connection);

            await deleteCommand.ExecuteNonQueryAsync();
            //End cleanup

            //Just verification, not for doc
            using var cmd3 = new EDBCommand("SELECT COUNT(*) FROM emp", connection);
            var count3 = Convert.ToInt32(await cmd3.ExecuteScalarAsync());

            //The insert should be reverted.
            Assert.AreEqual(count1, count3);
            //End verifiation
        }

        //Doc link:
        //Third example.
        //The following code shows using the ExecuteReader method to retrieve a result set:
        //https://www.enterprisedb.com/docs/net_connector/latest/11_using_spl_stored_procedures_in_your_net_application/
        [Test]
        public async Task ExecSPWithInAndOutParamsExecuteReaderTest()
        {
            await using var dataSource = EDBDataSource.Create(ConnectionString);
            var connection = await dataSource.OpenConnectionAsync();

            //Drop procedure if it exists.
            await RunQuery("DROP PROCEDURE DEPT_SELECT", connection);

            //Crate procedure
            var procSql = "CREATE OR REPLACE PROCEDURE"
                + "  DEPT_SELECT"
                + "  ("
                + "    pDEPTNO IN  INTEGER,"
                + "    pDNAME  OUT VARCHAR,"
                + "    pLOC    OUT VARCHAR"
                + "  )"
                + " AS"
                + " DECLARE"
                + "  CURSOR TESTCUR IS SELECT DNAME,LOC FROM DEPT;"
                + "  REC RECORD;"
                + " BEGIN"

                + "  OPEN TESTCUR;"
                + "  FETCH TESTCUR INTO REC;"

                + "  pDNAME  := REC.DNAME;"
                + "  pLOC    := REC.LOC;"

                + "  CLOSE testcur;"
                + " END;";
            await RunQuery(procSql, connection, true);

            //For doc
            using var command = new EDBCommand("DEPT_SELECT(:pDEPTNO,:pDNAME,:pLOC)", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("pDEPTNO",
                  EDBTypes.EDBDbType.Integer, 10, "pDEPTNO",
                  ParameterDirection.Input, false, 2, 2,
                  System.Data.DataRowVersion.Current, 1));

            command.Parameters.Add(new EDBParameter("pDNAME",
                  EDBTypes.EDBDbType.Varchar, 10, "pDNAME",
                  ParameterDirection.Output, false, 2, 2,
                  System.Data.DataRowVersion.Current, 1));

            command.Parameters.Add(new EDBParameter("pLOC",
                  EDBTypes.EDBDbType.Varchar, 10, "pLOC",
                  ParameterDirection.Output, false, 2, 2,
                  System.Data.DataRowVersion.Current, 1));

            await command.PrepareAsync();

            command.Parameters[0].Value = 10;
            await using var result = await command.ExecuteReaderAsync();

            var fc = result.FieldCount;

            //Just verification, not for doc
            Assert.AreEqual(2, fc);
            //End verification

            var props = new List<string>();
            for (var i = 0; i < 3; i++)
            {
                //For doc
                //Console.WriteLine("RESULT[" + i + "]=" + Convert.ToString(command.Parameters[i].Value));
                //Console.WriteLine("\n");
                //End doc

                //Just verification, not for doc
                props.Add(Convert.ToString(command.Parameters[i].Value));
                //End verification
            }
            await result.CloseAsync();
            //End doc

            //Just verification, not for doc
            Assert.AreEqual(3, props.Count);
            Assert.AreEqual("10", props[0]);
            Assert.AreEqual("ACCOUNTING", props[1]);
            Assert.AreEqual("NEW YORK", props[2]);
            //End verification
        }

        //Doc link:
        //Fourth example.
        //The following code shows using the ExecuteNonQuery method to retrieve a result set:
        //https://www.enterprisedb.com/docs/net_connector/latest/11_using_spl_stored_procedures_in_your_net_application/
        [Test]
        public async Task ExecSPWithInAndOutParamsExecuteNonQueryTest()
        {
            await using var dataSource = EDBDataSource.Create(ConnectionString);
            var connection = await dataSource.OpenConnectionAsync();

            //Drop procedure if it exists.
            await RunQuery("DROP PROCEDURE DEPT_SELECT", connection);

            //Crate procedure
            var procSql = "CREATE OR REPLACE PROCEDURE"
                + "  DEPT_SELECT"
                + "  ("
                + "    pDEPTNO IN  INTEGER,"
                + "    pDNAME  OUT VARCHAR,"
                + "    pLOC    OUT VARCHAR"
                + "  )"
                + " AS"
                + " DECLARE"
                + "  CURSOR TESTCUR IS SELECT DNAME,LOC FROM DEPT;"
                + "  REC RECORD;"
                + " BEGIN"

                + "  OPEN TESTCUR;"
                + "  FETCH TESTCUR INTO REC;"

                + "  pDNAME  := REC.DNAME;"
                + "  pLOC    := REC.LOC;"

                + "  CLOSE testcur;"
                + " END;";
            await RunQuery(procSql, connection, true);

            //For doc
            using var command = new EDBCommand("DEPT_SELECT(:pDEPTNO,:pDNAME,:pLOC)", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("pDEPTNO",
              EDBTypes.EDBDbType.Integer, 10, "pDEPTNO",
              ParameterDirection.Input, false, 2, 2,
              System.Data.DataRowVersion.Current, 1));

            command.Parameters.Add(new EDBParameter("pDNAME",
              EDBTypes.EDBDbType.Varchar, 10, "pDNAME",
              ParameterDirection.Output, false, 2, 2,
              System.Data.DataRowVersion.Current, 1));

            command.Parameters.Add(new EDBParameter("pLOC",
              EDBTypes.EDBDbType.Varchar, 10, "pLOC",
              ParameterDirection.Output, false, 2, 2,
              System.Data.DataRowVersion.Current, 1));

            await command.PrepareAsync();
            command.Parameters[0].Value = 10;
            await command.ExecuteNonQueryAsync();

            //Response.Write(command.Parameters["pDNAME"].Value.ToString());
            //Response.Write(command.Parameters["pLOC"].Value.ToString());
            //End doc

            //Just verification, not for doc
            Assert.AreEqual("ACCOUNTING", command.Parameters["pDNAME"].Value!.ToString());
            Assert.AreEqual("NEW YORK", command.Parameters["pLOC"].Value!.ToString());
            //End verification
        }

        //Doc link:
        //First example.
        //https://www.enterprisedb.com/docs/net_connector/latest/13_using_a_ref_cursor_in_a_net_application/
        [Test]
        public async Task UsingRefCursorTest()
        {
            await using var dataSource = EDBDataSource.Create(ConnectionString);
            var connection = await dataSource.OpenConnectionAsync();

            //Drop procedure if it exists.
            await RunQuery("DROP PROCEDURE refcur_out_callee", connection);

            //Crate procedure
            var procSql = "CREATE OR REPLACE PROCEDURE"
                + "  refcur_inout_callee(v_refcur OUT SYS_REFCURSOR)"
                + " IS"
                + " BEGIN"
                + "   OPEN v_refcur FOR SELECT ename FROM emp;"
                + " END;";
            await RunQuery(procSql, connection, true);

            //For doc
            var tran = connection.BeginTransaction();
            using var command = new EDBCommand("refcur_inout_callee", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Transaction = tran;
            command.Parameters.Add(new EDBParameter("refCursor",
                EDBTypes.EDBDbType.Refcursor, 10, "refCursor",
                ParameterDirection.Output, false, 2, 2,
                System.Data.DataRowVersion.Current, null!));

            await command.PrepareAsync();
            command.Parameters[0].Value = null;

            await command.ExecuteNonQueryAsync();
            var cursorName = command.Parameters[0].Value!.ToString();
            command.CommandText = "fetch all in \"" + cursorName + "\"";
            command.CommandType = CommandType.Text;

            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            var fc = reader.FieldCount;
            var expectedEnames = new List<string>()
            {
                "ALLEN", "WARD", "JONES", "MARTIN", "BLAKE", "CLARK", "KING",
                "TURNER", "ADAMS", "JAMES", "FORD", "MILLER", "SMITH", "SCOTT"
            };
            expectedEnames.Sort();
            var actualEnames = new List<string>();
            while (reader.Read())
            {
                for (var i = 0; i < fc; i++)
                {
                    //For doc
                    //Console.WriteLine(reader.GetString(i));
                    //End doc

                    //Just verification, not for doc
                    actualEnames.Add(reader.GetString(i));
                    //End verification
                }
            }
            reader.Close();
            tran.Commit();
            //End doc

            //Just verification, not for doc
            actualEnames.Sort();
            Assert.AreEqual(expectedEnames.Count, actualEnames.Count);
            for(var j=0; j< actualEnames.Count; j++)
                Assert.AreEqual(expectedEnames[j], actualEnames[j]);
            //End verification
        }
    }
}
