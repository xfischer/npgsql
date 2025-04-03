using System;
using System.Configuration;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
#pragma warning disable CS8602
    /// <summary>
    /// This Class contains functions for unit testing of .Net Driver.
    /// </summary>
    [TestFixture]
    [NonParallelizable]

    public class EDBDBFeaturesTest : EPASTestBase
    {
        EDBConnection? con = null;

        [SetUp]
        public void SetUp()
        {
            var connectionString = ConnectionString;
            con = new EDBConnection(connectionString);
        }

        [Test]
        public void TestExecImmediate()
        {
            //con.Open();
            //EDBTransaction tran = con.BeginTransaction();

            //EDBCommand command = new EDBCommand("SELECT imed_using", con);
            //command.CommandType = CommandType.Text;
            //command.Transaction = tran;

            //command.Parameters.Add(new EDBParameter("refCursor", EDBTypes.EDBDbType.Refcursor, 10, "refCursor", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null!));
            //command.Prepare();
            //command.Parameters[0].Value = null;
            //command.ExecuteNonQuery();
            //string cursorName = command.Parameters[0].Value.ToString();

            //command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
            //command.CommandType = CommandType.Text;
            //EDBDataReader result = command.ExecuteReader(CommandBehavior.SequentialAccess);
            //int funcReturn = -1;

            //if (result.Read())
            //{
            //    Console.WriteLine(result.GetInt32(0));
            //    funcReturn = result.GetInt32(0);
            //}
            ////Console.WriteLine(fc.ToString());

            //Assert.AreEqual(funcReturn, 0);
        }

        [Test]
        public void TestExecImmedWithParameters()
        {
            //con.Open();
            //EDBTransaction tran = con.BeginTransaction();

            //EDBCommand command = new EDBCommand("imed_proc(:param1, :param2)", con);
            //command.CommandType = CommandType.StoredProcedure;
            //command.Transaction = tran;

            //command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 25, "param1", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null!));
            //command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null!));

            //command.Prepare();
            //command.Parameters[0].Value = "SALES";
            //command.Parameters[1].Value = 1500;

            //EDBDataReader result = command.ExecuteReader(CommandBehavior.SequentialAccess);
            //bool hasResult = false;
            //hasResult = result.HasRows;

            //if (result.Read())
            //{
            //    Console.WriteLine(result.GetInt32(0));
            //    Console.WriteLine(result.GetInt32(1));
            //}
            ////Console.WriteLine(fc.ToString());

            //Assert.AreEqual(hasResult, false);
        }
        [Test]
        public void TestSynonyms()
        {
            con.Open();

            var sql = "CREATE OR REPLACE PUBLIC SYNONYM employee FOR emp;";
            var select = "SELECT * FROM employee";

            var cmd = new EDBCommand(sql, con);
            cmd.ExecuteNonQuery();

            var selectCmd = new EDBCommand(select, con);
            var reader = selectCmd.ExecuteReader();

            var fc = reader.FieldCount;
            if (fc <= 0)
                Assert.Fail();
            reader.Close();
        }

        [Test]
        public void TestSelectINTO()
        {
            con.Open();
            TestUtil.dropTable(con, "empTemp");
            var sql = "SELECT ENAME,EMPNO,DEPTNO INTO empTemp FROM emp WHERE deptno  IN (10,20)";
            var selectSql = "select * from empTemp";

            var cmd = new EDBCommand(sql, con);

            cmd.ExecuteNonQuery();

            var selectCmd = new EDBCommand(selectSql, con);
            var reader = selectCmd.ExecuteReader();

            var fc = reader.FieldCount;
            Console.WriteLine(fc);
            reader.Close();
            if (fc < 3)
                Assert.Fail();

            var deleteSql = "drop table empTemp";
            cmd.CommandText = deleteSql;
            cmd.ExecuteNonQuery();
        }

        [Test]
        public void TestDefaultwithOneParameter()
        {
            con.Open();

            var defaultProcedure = "create or replace procedure defaultProc(y varchar, x integer default 20 + 1) is " +
                                        " begin " +
                                        " dbms_output.put_line('value of y = ' || y ); " +
                                        " dbms_output.put_line('value of x = ' || x); " +
                                            " declare " +
                                            " res integer := x + 10/2; " +
                                            " begin " +
                                                " dbms_output.put_line('res = ' || res); " +
                                            " end; " +
                                        "end;";
            var cmd = new EDBCommand(defaultProcedure, con);
            cmd.ExecuteNonQuery();

            var cmdProc = new EDBCommand("defaultProc(:y)", con);
            cmdProc.CommandType = CommandType.StoredProcedure;

            cmdProc.Parameters.Add(new EDBParameter("y", EDBTypes.EDBDbType.Varchar, 10, "y", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, "test"));
            cmdProc.Prepare();
            cmdProc.Parameters[0].Value = "hello";
            try
            {
                cmdProc.ExecuteNonQuery();
            }
            catch (EDBException)
            {
                throw;
            }
            finally
            {
                if (con.State != ConnectionState.Closed)
                    con.Close();
            }
        }

        [Test]
        public void UserDefinedType()
        {
            con.Open();

            var sql = "CREATE OR REPLACE TYPE PERSONOBJ AS OBJECT (\n"
              + "  first_name  VARCHAR2(50),\n"
              + "  last_name   VARCHAR2(50),\n"
              + "  date_of_birth  DATE,\n"
              + "  MEMBER FUNCTION getAge RETURN NUMBER\n"
              + ");";
            var cmd = new EDBCommand(sql, con);
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            sql = "DROP TYPE PERSONOBJ;";
            cmd = new EDBCommand(sql, con);
            cmd.ExecuteNonQuery();
        }

        [TearDown]
        public void Dispose()
        {
            if (con != null && con.State != ConnectionState.Closed)
                //con.Clone();
                con.Close();
        }



    }
#pragma warning restore CS8602
}
