using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
#pragma warning disable CS8604
#pragma warning disable CS8602
#nullable disable
    /// <summary>
    /// Testing Procedures with Different combination of parameters
    /// </summary>
    [TestFixture]
	public class EDBProcedureTest : TestBase
	{

		[OneTimeSetUp]
		public async Task Init()
		{
            await using var con = await OpenConnectionAsync();

            var com = new EDBCommand("",con);
			com.CommandType = CommandType.Text;

			//	Testing procedure with one IN Param

            //var CreateTable="CREATE  table InOutTestEmp(a numeric)";

            //com.CommandText=CreateTable;
            //com.CommandType=CommandType.Text;

            //await com.ExecuteNonQueryAsync();
            //Console.WriteLine("Table created");

            var strRefTwoArg = "CREATE OR REPLACE PROCEDURE public.cursortest2 (c_1 OUT refcursor,c_2 OUT refcursor ) \n"
                + "IS \n"
                + "BEGIN \n"
                + "open  c_1 for select * from emp order by empno; \n"
                + "open  c_2 for select * from emp order by empno; \n"
                + "END;";

            com.CommandText = strRefTwoArg;

            await com.ExecuteNonQueryAsync();

            var strRefThreeArg = "CREATE OR REPLACE PROCEDURE public.refcur_callee2 (c_1  OUT numeric, c_2 IN OUT refcursor,c_3 IN OUT refcursor )" +
                " IS BEGIN" +
                " c_1 :=100;" +
                " open  c_2 for select * from emp order by empno;" +
                " open  c_3 for select ename from emp order by ename;" +
                "END;";

            com.CommandText = strRefThreeArg;

            await com.ExecuteNonQueryAsync();

		}

		[OneTimeTearDown] 
		public async Task DisposeAsync()
		{
            //await using var con = await OpenConnectionAsync();

            //// Following extra Close() open sequence will make sure pending transactions are rolled back.
            //if (con.State != ConnectionState.Closed)
            //    con.Close();
            ////con.Open();
            await using var con = await OpenConnectionAsync();

            var com = new EDBCommand("",con);
            com.CommandType = CommandType.Text;


            //com.CommandText="DROP TABLE IF EXIST InOutTestEmp";
            //await com.ExecuteNonQueryAsync();



            //com.CommandText = "DROP PROCEDURE cursortest2";

            //await com.ExecuteNonQueryAsync();



            //com.CommandText = "DROP PROCEDURE refcur_callee2";

            //await com.ExecuteNonQueryAsync();


            //com.CommandText = "DROP PROCEDURE oneOutArg_test";
            //await com.ExecuteNonQueryAsync();
            
			TestUtil.closeDB(con);
        }


		[Test]
		public async Task testEmptyArg()
		{
			try 
			{
                await using var con = await OpenConnectionAsync();

                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;

                //	Testing procedure with Empty Argument list
                var strSqlEmptyArg = "CREATE OR REPLACE PROCEDURE emptyArg_test \n"
                    + " AS \n"
                    + "b        NUMBER(2);\n"
                    + " BEGIN \n"
                    + "    b := 6; \n"
                    + " END; \n";
                com.CommandText = strSqlEmptyArg;
                await com.ExecuteNonQueryAsync();

                var command = new EDBCommand("emptyArg_test",con);
				command.CommandType = CommandType.StoredProcedure;
				command.Prepare();
				await command.ExecuteNonQueryAsync();

                var com2 = new EDBCommand("", con);
                com2.CommandType = CommandType.Text;

                com2.CommandText = "DROP PROCEDURE emptyArg_test";
                await com2.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);

            }
            catch (EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}

		[Test]
		public async Task testOneInArg()
		{
			try 
			{
                await using var con = await OpenConnectionAsync();

                var com2 = new EDBCommand("", con);
                com2.CommandType = CommandType.Text;

                var strSqlOneInArg = "CREATE OR REPLACE PROCEDURE oneInArg_test(a IN NUMERIC) \n"
                    + " AS \n"
                    + "b        NUMBER(2);\n"
                    + " BEGIN \n"
                    + "    b := a; \n"
                    + " END; \n";
                com2.CommandText = strSqlOneInArg;
                await com2.ExecuteNonQueryAsync();

				var command = new EDBCommand("oneInArg_test(@1)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("",EDBTypes.EDBDbType.Numeric));
				command.Parameters[0].Value = 5;

				command.Prepare();
				await command.ExecuteNonQueryAsync();

                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;


                com.CommandText = "DROP PROCEDURE oneInArg_test";
                await com.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);

            }
            catch (EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}

		[Test]
		public async Task testThreeInArg()
		{
			try 
			{
                await using var con = await OpenConnectionAsync();

                var com1 = new EDBCommand("", con);
                com1.CommandType = CommandType.Text;

                //	Testing procedure with three IN Param
                var strSqlThreeInArg = "CREATE OR REPLACE PROCEDURE threeInArg_test(a IN NUMERIC, b IN NUMERIC, c IN NUMERIC) \n"
                    + " AS \n"
                    + "d        NUMBER(2);\n"
                    + " BEGIN \n"
                    + "    d:=a; \n"
                    + "    d:=d+b; \n"
                    + "    d:=d+c; \n"
                    + " END; \n";
                com1.CommandText = strSqlThreeInArg;
                await com1.ExecuteNonQueryAsync();


				var command = new EDBCommand("threeInArg_test(:a,:b,:c)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Numeric));
				command.Parameters[0].Value = 5;

				command.Parameters.Add(new EDBParameter("b",EDBTypes.EDBDbType.Numeric));
				command.Parameters[1].Value = 15;

				command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Numeric));
				command.Parameters[2].Value = 25;

				command.Prepare();
				await command.ExecuteNonQueryAsync();
                command.Dispose();

                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;

                com.CommandText = "DROP PROCEDURE threeInArg_test";
                await com.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);

            }
            catch (EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}
        [Test]
        public async Task testOneOutArg()
        {
            try
            {
                await using var con = await OpenConnectionAsync();

                var com1 = new EDBCommand("", con);
                com1.CommandType = CommandType.Text;

                //	Testing procedure with one OUT Param
                var strSqlOneOutArg = "CREATE OR REPLACE PROCEDURE oneOutArg_test(a OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " END; \n";
                com1.CommandText = strSqlOneOutArg;
                await com1.ExecuteNonQueryAsync();

                //	Testing procedure with three OUT Param

                var command = new EDBCommand("oneOutArg_test(:a)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("a",
                    EDBTypes.EDBDbType.Integer, 10, "a",
                    ParameterDirection.Output, false, 2, 2,
                    System.Data.DataRowVersion.Current, 1));

                command.Prepare();
                await command.ExecuteNonQueryAsync();
            //    Assert.AreEqual(5, int.Parse(command.Parameters[0].Value.ToString()));

                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;

                com.CommandText = "DROP PROCEDURE oneOutArg_test";
                await com.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);

            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }
        }
        [Test]
		public async Task testThreeOutArg()
		{
			try
            {
                await using var con = await OpenConnectionAsync();

                var com2 = new EDBCommand("", con);
                com2.CommandType = CommandType.Text;

                var strSqlThreeOutArg = "CREATE OR REPLACE PROCEDURE threeOutArg_test(a OUT NUMERIC, b OUT NUMERIC, c OUT NUMERIC) \n"
                + " AS \n"
                + " BEGIN \n"
                + "    a:=5; \n"
                + "    b:=15; \n"
                + "    c:=25; \n"
                + " END; \n";
                com2.CommandText = strSqlThreeOutArg;
                await com2.ExecuteNonQueryAsync();

				var command = new EDBCommand("threeOutArg_test(:a,:b,:c)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("a", 
					EDBTypes.EDBDbType.Integer,10,"a",
					ParameterDirection.Output,false ,2,2,
					System.Data.DataRowVersion.Current,1));

				command.Parameters.Add(new EDBParameter("b", 
					EDBTypes.EDBDbType.Integer,10,"b",
					ParameterDirection.Output,false ,2,2,
					System.Data.DataRowVersion.Current,1));

				command.Parameters.Add(new EDBParameter("c", 
					EDBTypes.EDBDbType.Integer,10,"b",
					ParameterDirection.Output,false ,2,2,
					System.Data.DataRowVersion.Current,1));

				command.Prepare();
				await command.ExecuteNonQueryAsync();
				Assert.AreEqual(5,int.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(15,int.Parse(command.Parameters[1].Value.ToString()));
				Assert.AreEqual(25,int.Parse(command.Parameters[2].Value.ToString()));

                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;
                com.CommandText = "DROP PROCEDURE threeOutArg_test";
                await com.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);

            }
            catch (EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}
		[Test]
		public async Task testSingleInOutArg()
		{
			try
            {
                await using var con = await OpenConnectionAsync();

                var com1 = new EDBCommand("", con);
                com1.CommandType = CommandType.Text;
                //	Testing procedure with one IN one OUT Param
                var strSqlInOutArg = "CREATE OR REPLACE PROCEDURE singleInOutArg_test(a IN NUMERIC, b OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    b:=a+2; \n"
                    + " END; \n";
                com1.CommandText = strSqlInOutArg;
                await com1.ExecuteNonQueryAsync();

				var command = new EDBCommand("singleInOutArg_test(:a,:b)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Numeric));
				command.Parameters[0].Value = 5;

				command.Parameters.Add(new EDBParameter("b", 
					EDBTypes.EDBDbType.Integer,10,"b",
					ParameterDirection.Output,false ,2,2,
					System.Data.DataRowVersion.Current,1));

				command.Prepare();
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual(7,int.Parse(command.Parameters[1].Value.ToString()));

                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;

                com.CommandText = "DROP PROCEDURE singleInOutArg_test";
                await com.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);

            }
            catch (EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}
		[Test]
		public async Task testMixInOutArg()
		{
			try
            {
                await using var con = await OpenConnectionAsync();


                var com1 = new EDBCommand("", con);
                com1.CommandType = CommandType.Text;

                //	Testing procedure with IN, OUT and IN/OUT Param
                var strSqlMixArg = "CREATE OR REPLACE PROCEDURE mixArg_test(a INOUT NUMERIC, b OUT NUMERIC, c IN NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    b:=c; \n"
                    + "    a:=a+a; \n"
                    + " END; \n";
                com1.CommandText = strSqlMixArg;
                await com1.ExecuteNonQueryAsync();

                var command = new EDBCommand("mixArg_test(:a,:b,:c)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("a", 
					EDBTypes.EDBDbType.Numeric,0,"a",
					ParameterDirection.InputOutput,false ,0,0,
					System.Data.DataRowVersion.Current,5));

				command.Parameters.Add(new EDBParameter("b", 
					EDBTypes.EDBDbType.Numeric,0,"b",
					ParameterDirection.Output,false ,0,0,
					System.Data.DataRowVersion.Current,1));

				command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Numeric));
				command.Parameters[2].Value = 15;

				command.Prepare();
				await command.ExecuteNonQueryAsync();
				Assert.AreEqual(10,int.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(15,int.Parse(command.Parameters[1].Value.ToString()));

                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;

                com.CommandText = "DROP PROCEDURE mixArg_test";
                await com.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);

            }
            catch (EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}

		[Test]
		public async Task testMultipleInOutArg()
		{
			try
            {
                await using var con = await OpenConnectionAsync();

                var com2 = new EDBCommand("", con);
                com2.CommandType = CommandType.Text;
                //	Testing procedure with multiple IN multiple OUT Param
                var strSqlMultInOutArg = "CREATE OR REPLACE PROCEDURE multipleInOutArg_test(a IN NUMERIC, b OUT NUMERIC, c IN NUMERIC, d OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    b:=a; \n"
                    + "    d:=c; \n"
                    + " END; \n";
                com2.CommandText = strSqlMultInOutArg;
                await com2.ExecuteNonQueryAsync();


				var command = new EDBCommand("multipleInOutArg_test(:a,:b,:c,:d)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Numeric));
				command.Parameters[0].Value = 5;

				command.Parameters.Add(new EDBParameter("b", 
					EDBTypes.EDBDbType.Integer,10,"b",
					ParameterDirection.Output,false ,2,2,
					System.Data.DataRowVersion.Current,1));
				
				command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Numeric));
				command.Parameters[2].Value = 15;

				command.Parameters.Add(new EDBParameter("d", 
				EDBTypes.EDBDbType.Integer,10,"d",
				ParameterDirection.Output,false ,2,2,
					System.Data.DataRowVersion.Current,1));

				command.Prepare();
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual(5,int.Parse(command.Parameters[1].Value.ToString()));
				Assert.AreEqual(15,int.Parse(command.Parameters[3].Value.ToString()));

                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;
                com.CommandText = "DROP PROCEDURE multipleInOutArg_test";
                await com.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);


            }
            catch (EDBException e)
			{
				throw new Exception(e.ToString());
			}
		}
		[Test]
		public async Task TestCursor()
		{
			var i = 0;
            await using var con = await OpenConnectionAsync();

            EDBTransaction tran = con.BeginTransaction();
			var command = new EDBCommand("declare te cursor for select * from tablea;", con);
			await command.ExecuteNonQueryAsync();

			command.CommandText = "fetch forward 3 in te;";
			EDBDataReader dr = command.ExecuteReader();

			while (dr.Read())
			{
				i++;
			}
            
			Assert.AreEqual(3, i);
            dr.Close();
			command.CommandText = "close te;";
			await command.ExecuteNonQueryAsync();
			tran.Commit();
            TestUtil.closeDB(con);

        }

        [Test]
		public async Task TestOneProcCallingOtherProc()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq Sarim
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			var strSql = "create or replace procedure check_proc(a inout int) as  BEGIN check_proc1(a);     END; ";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();
				
			strSql ="create or replace procedure check_proc1(a inout int) as BEGIN  a:= 2;       END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();


			//////////////code
			try
			{
				command = new EDBCommand("check_proc(:v_inout)",con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Integer,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,1));
				command.Prepare();
	

				
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual(2,int.Parse(command.Parameters[0].Value.ToString()));

			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP Procedure check_proc;";
			await command.ExecuteNonQueryAsync();
			command.CommandText = "DROP Procedure check_proc1;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);

        }
        [Test]
		public async Task TestTwoProcCallingEachOtherRecursively()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq Sarim
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			var strSql = "create or replace procedure check_proc(a inout int) as  BEGIN  IF a > 0 then check_proc1(a);  END IF;   END;";//////////////
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();
				
			strSql ="create or replace procedure check_proc1(a inout int) as  BEGIN  a:= a-1;  check_proc(a);     END;";//////////////creating a procedure

			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();



            //////////////code
            try
            {
				command = new EDBCommand("check_proc(:v_inout)",con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Integer,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,3));
				command.Prepare();

				await command.ExecuteNonQueryAsync();

				Assert.AreEqual(0,int.Parse(command.Parameters[0].Value.ToString()));

			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP Procedure check_proc;";
			await command.ExecuteNonQueryAsync();
			command.CommandText = "DROP Procedure check_proc1;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);

        }

        /// <summary>
        /// ///////////Test Procedure calling itself /Sarim
        /// //////////Parameter type INOUT
        /// </summary>
        [Test, Ignore("check after all")]
		public async Task TestProcCallingItself()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			var strSql = "create or replace procedure check_proc(a inout int) as  BEGIN   IF a > 0 then  a:= a-1; check_proc(a);  END IF;   END;";//////////////creating a procedure

			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();

			//////////////code
			try
			{
				var command2 = new EDBCommand("check_proc(:v_inout)",con);
				command2.CommandType = CommandType.StoredProcedure;

			
				command2.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Integer,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,3));
				command2.Prepare();
	

				
				await command2.ExecuteNonQueryAsync();

				Assert.AreEqual(0,int.Parse(command2.Parameters[0].Value.ToString()));
                TestUtil.closeDB(con);


            }
            catch (EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			//command.Dispose();
		//	var command1 = new EDBCommand("",con);
			//command1.CommandText = "DROP Procedure check_proc;";
			//await command1.ExecuteNonQueryAsync();
		}

		//////////////////////////////////////////Procedures with in Packages
		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument INT type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
		[Test]
		public async Task testProcedureWithINTAsInInoutOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			var strSql ="CREATE OR REPLACE PROCEDURE ProcedureWithINT(p_in in int,p_inout inout int,p_out out int)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();

			//////////////code
			try
			{
				command = new EDBCommand("ProcedureWithINT(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Integer,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,100));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Integer,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2000));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Integer,10,"v_out",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,40));
				command.Prepare();
				
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual(100,int.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(100,int.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(2000,int.Parse(command.Parameters[2].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP PROCEDURE ProcedureWithINT;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);


        }

        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument INT4 type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test]
		public async Task testProcedureWithINT4AsInInoutOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			var strSql ="CREATE OR REPLACE PROCEDURE ProcedureWithInt4(p_in in int4,p_inout inout int4,p_out out int4)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();


			//////////////code
			try
			{
				command = new EDBCommand("ProcedureWithInt4(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Integer,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1000));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Integer,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,2000));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Integer,10,"v_out",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,4000));
				command.Prepare();
	

				
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual(1000,int.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1000,int.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(2000,int.Parse(command.Parameters[2].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP procedure ProcedureWithInt4;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);


        }
        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument INT8 type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test]
		public async Task testProcedureWithINT8AsInInoutOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			var strSql ="CREATE OR REPLACE PROCEDURE ProcedureWithInt8(p_in in int8,p_inout inout int8,p_out out int8)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();

			//////////////code
			try
			{
				command = new EDBCommand("ProcedureWithInt8(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

			
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Bigint,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,1000));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Bigint,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,20000));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Bigint,10,"v_out",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,400));
				command.Prepare();
	

				
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual(1000,int.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1000,int.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(20000,int.Parse(command.Parameters[2].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP Procedure ProcedureWithInt8;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);


        }
        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument NUMERIC type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test]
		public async Task testProcedureWithNUMERICASInInoutOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			var strSql ="CREATE OR REPLACE PROCEDURE ProcedureWithNumeric(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();


			//////////////code
			try
			{
				command = new EDBCommand("ProcedureWithNumeric(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;
				
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Numeric,10,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,10000));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Numeric,10,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,-2));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Numeric,10,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,40000));
				command.Prepare();
			
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual(10000,int.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(10000,int.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(-2,int.Parse(command.Parameters[2].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP procedure ProcedureWithNumeric;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);


        }

        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument FLOAT type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test]
		public async Task testProcedureWithFLOATAsInInoutOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			var strSql ="CREATE OR REPLACE PROCEDURE ProcedureWithFloat(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();


			//////////////code
			try
			{
				command = new EDBCommand("ProcedureWithFloat(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;

				
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Double,0,"v_in",ParameterDirection.Input,false, 8, 8,DataRowVersion.Current,1.100001));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Double, 0, "v_inout", ParameterDirection.InputOutput, false, 8, 8, DataRowVersion.Current, -2.2131));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Double, 0, "v_out", ParameterDirection.InputOutput, false, 8, 8, DataRowVersion.Current, 4.4009));
				command.Prepare();
	

				
				await command.ExecuteNonQueryAsync();
             	Assert.AreEqual(1.100001f,float.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1.10000098f,float.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(-2.2131f,float.Parse(command.Parameters[2].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP Procedure ProcedureWithFloat;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);


        }
        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument REAL type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test]
		public async Task testProcedureWithREALAsInInoutOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			var strSql ="CREATE OR REPLACE procedure procedureWithReal(p_in in REAL,p_inout inout REAL,p_out out REAL)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();


			//////////////code
			try
			{
				command = new EDBCommand("procedureWithReal(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;


                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Real, 0, "v_in", ParameterDirection.Input, false, 0, 0, DataRowVersion.Current, 1.1));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Real, 0, "v_inout", ParameterDirection.InputOutput, false, 0, 0, DataRowVersion.Current, 2.2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Real, 0, "v_out", ParameterDirection.InputOutput, false, 0, 0, DataRowVersion.Current, 4.4));
				command.Prepare();
	

				
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual(1.1f,float.Parse(command.Parameters[0].Value.ToString()));
				Assert.AreEqual(1.1f,float.Parse(command.Parameters[1].Value.ToString()));	
				Assert.AreEqual(2.2f,float.Parse(command.Parameters[2].Value.ToString()));	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP PROCEDURE procedureWithReal;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);


        }

        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument CHAR type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test]
		public async Task testProcedureWithCHARAsInInoutOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;

			var strSql ="CREATE OR REPLACE PROCEDURE ProcedureWithChar(p_in in CHAR(30),p_inout inout CHAR(30),p_out out CHAR(30))   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();

			//////////////code
			try
			{
				command = new EDBCommand("ProcedureWithChar(:v_in,:v_inout,:v_out)",con);
				command.CommandType = CommandType.StoredProcedure;
		
				command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Char,12,"v_in",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,"Hashim"));
				command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Char,12,"v_inout",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"EnterpriseDB"));
				command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Char,12,"v_out",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"4"));

                command.Prepare();
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual("Hashim",command.Parameters[0].Value.ToString());
				Assert.AreEqual("Hashim",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("EnterpriseDB",command.Parameters[2].Value.ToString());	
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP PROCEDURE ProcedureWithChar;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);


        }

        /*
		        To verify that maximum 128 OUT parameters are supported in .NET Connector.
        */
        [Test]
		public async Task testMaxParametersSupportInProcedureWithNumericAsOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			var strSql ="CREATE OR REPLACE PROCEDURE MaxProcNumeric(param1 out Numeric, param2 out Numeric,param3 out Numeric ,param4 out Numeric, param5 out Numeric,param6 out Numeric,param7 out Numeric, param8 out Numeric,param9 out Numeric,param10 out Numeric, param11 out Numeric,param12 out Numeric,param13 out Numeric, param14 out Numeric,param15 out Numeric,param16 out Numeric, param17 out Numeric,param18 out Numeric,param19 out Numeric, param20 out Numeric,param21 out Numeric,param22 out Numeric, param23 out Numeric,param24 out Numeric,param25 out Numeric, param26 out Numeric,param27 out Numeric,param28 out Numeric, param29 out Numeric,param30 out Numeric,param31 out Numeric, param32 out Numeric,param33 out Numeric,param34 out Numeric, param35 out Numeric,param36 out Numeric,param37 out Numeric, param38 out Numeric,param39 out Numeric,param40 out Numeric, param41 out Numeric,param42 out Numeric,param43 out Numeric, param44 out Numeric,param45 out Numeric,param46 out Numeric, param47 out Numeric,param48 out Numeric,param49 out Numeric, param50 out Numeric,param51 out Numeric,param52 out Numeric, param53 out Numeric,param54 out Numeric,param55 out Numeric, param56 out Numeric,param57 out Numeric,param58 out Numeric, param59 out Numeric,param60 out Numeric,param61 out Numeric, param62 out Numeric,param63 out Numeric,param64 out Numeric, param65 out Numeric,param66 out Numeric,param67 out Numeric, param68 out Numeric,param69 out Numeric,param70 out Numeric, param71 out Numeric,param72 out Numeric,param73 out Numeric, param74 out Numeric,param75 out Numeric,param76 out Numeric, param77 out Numeric,param78 out Numeric,param79 out Numeric, param80 out Numeric,param81 out Numeric,param82 out Numeric, param83 out Numeric,param84 out Numeric,param85 out Numeric, param86 out Numeric,param87 out Numeric,param88 out Numeric, param89 out Numeric,param90 out Numeric,param91 out Numeric, param92 out Numeric,param93 out Numeric,param94 out Numeric, param95 out Numeric,param96 out Numeric,param97 out Numeric,"
							+" param98 out Numeric,param99 out Numeric,param100 out Numeric, param101 out Numeric,param102 out Numeric,param103 out Numeric, param104 out Numeric,param105 out Numeric,param106 out Numeric, param107 out Numeric,param108 out Numeric,param109 out Numeric, param110 out Numeric,param111 out Numeric,param112 out Numeric, param113 out Numeric,param114 out Numeric,param115 out Numeric, param116 out Numeric,param117 out Numeric,param118 out Numeric, param119 out Numeric,param120 out Numeric,param121 out Numeric, param122 out Numeric,param123 out Numeric,param124 out Numeric, param125 out Numeric,param126 out Numeric,param127 out Numeric, param128 out Numeric)"
							+" IS \n"
							+" BEGIN \n"
							+"param1 := 1; param2 := 2; param3 := 3; param4 := 4; param5 := 5; param6 := 6; param7 := 7; param8 := 8; param9 := 9; param10 := 10; param11 := 11; param12 := 12; param13 := 13; param14 := 14; param15 := 15; param16 := 16; param17 := 17; param18 := 18; param19 := 19; param20 := 20; param21 := 21; param22 := 22; param23 := 23; param24 := 24; param25 := 25; param26 := 26; param27 := 27; param28 := 28; param29 := 29; param30 := 30; param31 := 31; param32 := 32; param33 := 33; param34 := 34; param35 := 35; param36 := 36; param37 := 37; param38 := 38; param39 := 39; param40 := 40; param41 := 41; param42 := 42; param43 := 43; param44 := 44; param45 := 45; param46 := 46; param47 := 47; param48 := 48; param49 := 49; param50 := 50; param51 := 51; param52 := 52; param53 := 53; param54 := 54; param55 := 55; param56 := 56; param57 := 57; param58 := 58; param59 := 59; param60 := 60; param61 := 61; param62 := 62; param63 := 63; param64 := 64; param65 := 65; param66 := 66; param67 := 67; param68 := 68; param69 := 69; param70 := 70; param71 := 71; param72 := 72; param73 := 73; param74 := 74; param75 := 75; param76 := 76; param77 := 77; param78 := 78; param79 := 79; param80 := 80; param81 := 81; param82 := 82; param83 := 83; param84 := 84; param85 := 85; param86 := 86; param87 := 87; param88 := 88; param89 := 89; param90 := 90; param91 := 91; param92 := 92; param93 := 93; param94 := 94; param95 := 95; param96 := 96; param97 := 97; param98 := 98; param99 := 99; param100 := 100; param101 := 101; param102 := 102; param103 := 103; param104 := 104; param105 := 105; param106 := 106; param107 := 107; param108 := 108; param109 := 109; param110 := 110; param111 := 111; param112 := 112; param113 := 113; param114 := 114; param115 := 115; param116 := 116; param117 := 117; param118 := 118; param119 := 119; param120 := 120; param121 := 121; param122 := 122; param123 := 123; param124 := 124; param125 := 125; param126 := 126; param127 := 127; param128 := 128; END;";
 			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();


			//////////////code
			try
			{
				command = new EDBCommand("MaxProcNumeric(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10,:param11,:param12,:param13,:param14,:param15,:param16,:param17,:param18,:param19,:param20,:param21,:param22,:param23,:param24,:param25,:param26,:param27,:param28,:param29,:param30,:param31,:param32,:param33,:param34,:param35,:param36,:param37,:param38,:param39,:param40,:param41,:param42,:param43,:param44,:param45,:param46,:param47,:param48,:param49,:param50,:param51,:param52,:param53,:param54,:param55,:param56,:param57,:param58,:param59,:param60,:param61,:param62,:param63,:param64,:param65,:param66,:param67,:param68,:param69,:param70,:param71,:param72,:param73,:param74,:param75,:param76,:param77,:param78,:param79,:param80,:param81,:param82,:param83,:param84,:param85,:param86,:param87,:param88,:param89,:param90,:param91,:param92,:param93,:param94,:param95,:param96,:param97,:param98,:param99,:param100,:param101,:param102,:param103,:param104,:param105,:param106,:param107,:param108,:param109,:param110,:param111,:param112,:param113,:param114,:param115,:param116,:param117,:param118,:param119,:param120,:param121,:param122,:param123,:param124,:param125,:param126,:param127,:param128)",con);
				command.CommandType = CommandType.StoredProcedure;

                for ( var i=0; i<128; i++)
                {
                    var paramValue = 128 - i;
                    var paramName = "param" + (i + 1).ToString();
                    command.Parameters.Add(new EDBParameter(paramName, EDBTypes.EDBDbType.Numeric, 10,paramName, ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, paramValue));
                }
				command.Prepare();				
				await command.ExecuteNonQueryAsync();

                for ( var i = 0; i< 128; i++)
                    Assert.AreEqual((i+1).ToString(), command.Parameters[i].Value.ToString());

			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP PROCEDURE MaxProcNumeric;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);


        }

        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument CHAR type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test]
		public async Task testMaxParametersSupportInProcedureWithNumericAsInAndOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			var strSql ="CREATE OR REPLACE PROCEDURE MaxProcNumericInOut(param1 out Numeric, param2 inout Numeric,param3 in Numeric ,param4 out Numeric, param5 inout Numeric,param6 in Numeric,param7 out Numeric, param8 inout Numeric,param9 in Numeric,param10 out Numeric, param11 inout Numeric,param12 in Numeric,param13 out Numeric, param14 inout Numeric,param15 in Numeric,param16 out Numeric, param17 inout Numeric,param18 in Numeric,param19 out Numeric, param20 inout Numeric,param21 in Numeric,param22 out Numeric, param23 inout Numeric,param24 in Numeric,param25 out Numeric, param26 inout Numeric,param27 in Numeric,param28 out Numeric, param29 inout Numeric,param30 in Numeric,param31 out Numeric, param32 inout Numeric,param33 in Numeric,param34 out Numeric, param35 inout Numeric,param36 in Numeric,param37 out Numeric, param38 inout Numeric,param39 in Numeric,param40 out Numeric, param41 inout Numeric,param42 in Numeric,param43 out Numeric, param44 inout Numeric,param45 in Numeric,param46 out Numeric, param47 inout Numeric,param48 in Numeric,param49 out Numeric, param50 inout Numeric,param51 in Numeric,param52 out Numeric, param53 inout Numeric,param54 in Numeric,param55 out Numeric, param56 inout Numeric,param57 in Numeric,param58 out Numeric, param59 inout Numeric,param60 in Numeric,param61 out Numeric, param62 inout Numeric,param63 in Numeric,param64 out Numeric, param65 inout Numeric,param66 in Numeric,param67 out Numeric, param68 inout Numeric,param69 in Numeric,param70 out Numeric, param71 inout Numeric,param72 in Numeric,param73 out Numeric, param74 inout Numeric,param75 in Numeric,param76 out Numeric, param77 inout Numeric,param78 in Numeric,param79 out Numeric, param80 inout Numeric,param81 in Numeric,param82 out Numeric, param83 inout Numeric,param84 in Numeric,param85 out Numeric, param86 inout Numeric,param87 in Numeric,param88 out Numeric, param89 inout Numeric,param90 in Numeric,param91 out Numeric, param92 inout Numeric,param93 in Numeric,param94 out Numeric, param95 inout Numeric,param96 in Numeric"
				+" ,param97 out Numeric, param98 inout Numeric,param99 in Numeric,param100 out Numeric, param101 inout Numeric,param102 in Numeric,param103 out Numeric, param104 inout Numeric,param105 in Numeric,param106 out Numeric, param107 inout Numeric,param108 in Numeric,param109 out Numeric, param110 inout Numeric,param111 in Numeric,param112 out Numeric, param113 inout Numeric,param114 in Numeric,param115 out Numeric, param116 inout Numeric,param117 in Numeric,param118 out Numeric, param119 inout Numeric,param120 in Numeric,param121 out Numeric, param122 inout Numeric,param123 in Numeric,param124 out Numeric, param125 inout Numeric,param126 in Numeric,param127 out Numeric, param128 inout Numeric)"
				+" IS \n"
				+" BEGIN \n"
				+"param1 := param2; param2 := param3; param4 := param5; param5 := param6; param7 := param8; param8 := param9; param10 := param11; param11 := param12; param13 := param14; param14 := param15; param16 := param17; param17 := param18; param19 := param20; param20 := param21; param22 := param23; param23 := param24; param25 := param26; param26 := param27; param28 := param29; param29 := param30; param31 := param32; param32 := param33; param34 := param35; param35 := param36; param37 := param38; param38 := param39; param40 := param41; param41 := param42; param43 := param44; param44 := param45; param46 := param47; param47 := param48; param49 := param50; param50 := param51; param52 := param53; param53 := param54; param55 := param56; param56 := param57; param58 := param59; param59 := param60; param61 := param62; param62 := param63; param64 := param65; param65 := param66; param67 := param68; param68 := param69; param70 := param71; param71 := param72; param73 := param74; param74 := param75; param76 := param77; param77 := param78; param79 := param80; param80 := param81; param82 := param83; param83 := param84; param85 := param86; param86 := param87; param88 := param89; param89 := param90; param91 := param92; param92 := param93; param94 := param95; param95 := param96; param97 := param98; param98 := param99; param100 := param101; param101 := param102; param103 := param104; param104 := param105; param106 := param107; param107 := param108; param109 := param110; param110 := param111; param112 := param113; param113 := param114; param115 := param116; param116 := param117; param118 := param119; param119 := param120; param121 := param122; param122 := param123; param124 := param125; param125 := param126; param127 := param128; param128 := 200; END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();


			//////////////code
			try
			{
				command = new EDBCommand("MaxProcNumericInOut(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10,:param11,:param12,:param13,:param14,:param15,:param16,:param17,:param18,:param19,:param20,:param21,:param22,:param23,:param24,:param25,:param26,:param27,:param28,:param29,:param30,:param31,:param32,:param33,:param34,:param35,:param36,:param37,:param38,:param39,:param40,:param41,:param42,:param43,:param44,:param45,:param46,:param47,:param48,:param49,:param50,:param51,:param52,:param53,:param54,:param55,:param56,:param57,:param58,:param59,:param60,:param61,:param62,:param63,:param64,:param65,:param66,:param67,:param68,:param69,:param70,:param71,:param72,:param73,:param74,:param75,:param76,:param77,:param78,:param79,:param80,:param81,:param82,:param83,:param84,:param85,:param86,:param87,:param88,:param89,:param90,:param91,:param92,:param93,:param94,:param95,:param96,:param97,:param98,:param99,:param100,:param101,:param102,:param103,:param104,:param105,:param106,:param107,:param108,:param109,:param110,:param111,:param112,:param113,:param114,:param115,:param116,:param117,:param118,:param119,:param120,:param121,:param122,:param123,:param124,:param125,:param126,:param127,:param128)", con);
				command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1",	EDBTypes.EDBDbType.Numeric,10,"param1",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param2",	EDBTypes.EDBDbType.Numeric,10,"param2",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param3",	EDBTypes.EDBDbType.Numeric,10,"param3",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param4",	EDBTypes.EDBDbType.Numeric,10,"param4",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param5",	EDBTypes.EDBDbType.Numeric,10,"param5",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param6",	EDBTypes.EDBDbType.Numeric,10,"param6",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param7",	EDBTypes.EDBDbType.Numeric,10,"param7",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param8",	EDBTypes.EDBDbType.Numeric,10,"param8",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param9",	EDBTypes.EDBDbType.Numeric,10,"param9",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param10",	EDBTypes.EDBDbType.Numeric,10,"param10",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param11",	EDBTypes.EDBDbType.Numeric,10,"param11",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param12",	EDBTypes.EDBDbType.Numeric,10,"param12",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param13",	EDBTypes.EDBDbType.Numeric,10,"param13",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param14",	EDBTypes.EDBDbType.Numeric,10,"param14",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param15",	EDBTypes.EDBDbType.Numeric,10,"param15",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param16",	EDBTypes.EDBDbType.Numeric,10,"param16",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param17",	EDBTypes.EDBDbType.Numeric,10,"param17",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param18",	EDBTypes.EDBDbType.Numeric,10,"param18",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param19",	EDBTypes.EDBDbType.Numeric,10,"param19",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param20",	EDBTypes.EDBDbType.Numeric,10,"param20",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param21",	EDBTypes.EDBDbType.Numeric,10,"param21",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param22",	EDBTypes.EDBDbType.Numeric,10,"param22",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param23",	EDBTypes.EDBDbType.Numeric,10,"param23",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param24",	EDBTypes.EDBDbType.Numeric,10,"param24",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param25",	EDBTypes.EDBDbType.Numeric,10,"param25",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param26",	EDBTypes.EDBDbType.Numeric,10,"param26",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param27",	EDBTypes.EDBDbType.Numeric,10,"param27",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param28",	EDBTypes.EDBDbType.Numeric,10,"param28",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param29",	EDBTypes.EDBDbType.Numeric,10,"param29",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param30",	EDBTypes.EDBDbType.Numeric,10,"param30",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param31",	EDBTypes.EDBDbType.Numeric,10,"param31",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param32",	EDBTypes.EDBDbType.Numeric,10,"param32",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param33",	EDBTypes.EDBDbType.Numeric,10,"param33",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param34",	EDBTypes.EDBDbType.Numeric,10,"param34",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param35",	EDBTypes.EDBDbType.Numeric,10,"param35",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param36",	EDBTypes.EDBDbType.Numeric,10,"param36",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param37",	EDBTypes.EDBDbType.Numeric,10,"param37",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param38",	EDBTypes.EDBDbType.Numeric,10,"param38",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param39",	EDBTypes.EDBDbType.Numeric,10,"param39",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param40",	EDBTypes.EDBDbType.Numeric,10,"param40",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param41",	EDBTypes.EDBDbType.Numeric,10,"param41",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param42",	EDBTypes.EDBDbType.Numeric,10,"param42",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param43",	EDBTypes.EDBDbType.Numeric,10,"param43",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param44",	EDBTypes.EDBDbType.Numeric,10,"param44",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param45",	EDBTypes.EDBDbType.Numeric,10,"param45",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param46",	EDBTypes.EDBDbType.Numeric,10,"param46",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param47",	EDBTypes.EDBDbType.Numeric,10,"param47",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param48",	EDBTypes.EDBDbType.Numeric,10,"param48",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param49",	EDBTypes.EDBDbType.Numeric,10,"param49",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param50",	EDBTypes.EDBDbType.Numeric,10,"param50",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param51",	EDBTypes.EDBDbType.Numeric,10,"param51",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param52",	EDBTypes.EDBDbType.Numeric,10,"param52",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param53",	EDBTypes.EDBDbType.Numeric,10,"param53",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param54",	EDBTypes.EDBDbType.Numeric,10,"param54",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param55",	EDBTypes.EDBDbType.Numeric,10,"param55",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param56",	EDBTypes.EDBDbType.Numeric,10,"param56",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param57",	EDBTypes.EDBDbType.Numeric,10,"param57",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param58",	EDBTypes.EDBDbType.Numeric,10,"param58",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param59",	EDBTypes.EDBDbType.Numeric,10,"param59",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param60",	EDBTypes.EDBDbType.Numeric,10,"param60",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param61",	EDBTypes.EDBDbType.Numeric,10,"param61",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param62",	EDBTypes.EDBDbType.Numeric,10,"param62",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param63",	EDBTypes.EDBDbType.Numeric,10,"param63",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param64",	EDBTypes.EDBDbType.Numeric,10,"param64",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param65",	EDBTypes.EDBDbType.Numeric,10,"param65",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param66",	EDBTypes.EDBDbType.Numeric,10,"param66",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param67",	EDBTypes.EDBDbType.Numeric,10,"param67",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param68",	EDBTypes.EDBDbType.Numeric,10,"param68",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param69",	EDBTypes.EDBDbType.Numeric,10,"param69",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param70",	EDBTypes.EDBDbType.Numeric,10,"param70",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param71",	EDBTypes.EDBDbType.Numeric,10,"param71",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param72",	EDBTypes.EDBDbType.Numeric,10,"param72",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param73",	EDBTypes.EDBDbType.Numeric,10,"param73",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param74",	EDBTypes.EDBDbType.Numeric,10,"param74",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param75",	EDBTypes.EDBDbType.Numeric,10,"param75",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param76",	EDBTypes.EDBDbType.Numeric,10,"param76",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param77",	EDBTypes.EDBDbType.Numeric,10,"param77",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param78",	EDBTypes.EDBDbType.Numeric,10,"param78",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param79",	EDBTypes.EDBDbType.Numeric,10,"param79",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param80",	EDBTypes.EDBDbType.Numeric,10,"param80",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param81",	EDBTypes.EDBDbType.Numeric,10,"param81",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param82",	EDBTypes.EDBDbType.Numeric,10,"param82",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param83",	EDBTypes.EDBDbType.Numeric,10,"param83",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param84",	EDBTypes.EDBDbType.Numeric,10,"param84",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param85",	EDBTypes.EDBDbType.Numeric,10,"param85",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param86",	EDBTypes.EDBDbType.Numeric,10,"param86",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param87",	EDBTypes.EDBDbType.Numeric,10,"param87",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param88",	EDBTypes.EDBDbType.Numeric,10,"param88",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param89",	EDBTypes.EDBDbType.Numeric,10,"param89",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param90",	EDBTypes.EDBDbType.Numeric,10,"param90",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,119));			

				command.Parameters.Add(new EDBParameter("param91",	EDBTypes.EDBDbType.Numeric,10,"param91",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param92",	EDBTypes.EDBDbType.Numeric,10,"param92",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param93",	EDBTypes.EDBDbType.Numeric,10,"param93",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param94",	EDBTypes.EDBDbType.Numeric,10,"param94",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param95",	EDBTypes.EDBDbType.Numeric,10,"param95",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param96",	EDBTypes.EDBDbType.Numeric,10,"param96",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param97",	EDBTypes.EDBDbType.Numeric,10,"param97",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param98",	EDBTypes.EDBDbType.Numeric,10,"param98",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param99",	EDBTypes.EDBDbType.Numeric,10,"param99",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param100",	EDBTypes.EDBDbType.Numeric,10,"param100",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param101",	EDBTypes.EDBDbType.Numeric,10,"param101",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param102",	EDBTypes.EDBDbType.Numeric,10,"param102",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param103",	EDBTypes.EDBDbType.Numeric,10,"param103",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param104",	EDBTypes.EDBDbType.Numeric,10,"param104",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param105",	EDBTypes.EDBDbType.Numeric,10,"param105",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param106",	EDBTypes.EDBDbType.Numeric,10,"param106",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param107",	EDBTypes.EDBDbType.Numeric,10,"param107",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param108",	EDBTypes.EDBDbType.Numeric,10,"param108",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param109",	EDBTypes.EDBDbType.Numeric,10,"param109",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param110",	EDBTypes.EDBDbType.Numeric,10,"param110",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param111",	EDBTypes.EDBDbType.Numeric,10,"param111",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param112",	EDBTypes.EDBDbType.Numeric,10,"param112",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param113",	EDBTypes.EDBDbType.Numeric,10,"param113",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param114",	EDBTypes.EDBDbType.Numeric,10,"param114",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param115",	EDBTypes.EDBDbType.Numeric,10,"param115",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param116",	EDBTypes.EDBDbType.Numeric,10,"param116",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param117",	EDBTypes.EDBDbType.Numeric,10,"param117",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param118",	EDBTypes.EDBDbType.Numeric,10,"param118",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param119",	EDBTypes.EDBDbType.Numeric,10,"param119",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param120",	EDBTypes.EDBDbType.Numeric,10,"param120",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param121",	EDBTypes.EDBDbType.Numeric,10,"param121",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param122",	EDBTypes.EDBDbType.Numeric,10,"param122",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param123",	EDBTypes.EDBDbType.Numeric,10,"param123",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param124",	EDBTypes.EDBDbType.Numeric,10,"param124",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param125",	EDBTypes.EDBDbType.Numeric,10,"param125",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param126",	EDBTypes.EDBDbType.Numeric,10,"param126",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param127",	EDBTypes.EDBDbType.Numeric,10,"param127",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param128",	EDBTypes.EDBDbType.Numeric,10,"param128",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,121));				

				command.Prepare();
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual("127",command.Parameters[0].Value.ToString());
				Assert.AreEqual("126",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[2].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[3].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[4].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[5].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[6].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[7].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[8].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[9].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[10].Value.ToString());
				Assert.AreEqual("127",command.Parameters[11].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[12].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[13].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[14].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[15].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[16].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[17].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[18].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[19].Value.ToString());	

				Assert.AreEqual("128",command.Parameters[20].Value.ToString());
				Assert.AreEqual("126",command.Parameters[21].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[22].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[23].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[24].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[25].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[26].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[27].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[28].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[29].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[30].Value.ToString());
				Assert.AreEqual("126",command.Parameters[31].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[32].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[33].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[34].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[35].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[36].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[37].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[38].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[39].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[40].Value.ToString());
				Assert.AreEqual("127",command.Parameters[41].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[42].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[43].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[44].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[45].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[46].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[47].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[48].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[49].Value.ToString());	

				Assert.AreEqual("128",command.Parameters[50].Value.ToString());
				Assert.AreEqual("126",command.Parameters[51].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[52].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[53].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[54].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[55].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[56].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[57].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[58].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[59].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[60].Value.ToString());
				Assert.AreEqual("126",command.Parameters[61].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[62].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[63].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[64].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[65].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[66].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[67].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[68].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[69].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[70].Value.ToString());
				Assert.AreEqual("127",command.Parameters[71].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[72].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[73].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[74].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[75].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[76].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[77].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[78].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[79].Value.ToString());	

				Assert.AreEqual("128",command.Parameters[80].Value.ToString());
				Assert.AreEqual("126",command.Parameters[81].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[82].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[83].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[84].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[85].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[86].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[87].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[88].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[89].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[90].Value.ToString());
				Assert.AreEqual("126",command.Parameters[91].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[92].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[93].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[94].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[95].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[96].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[97].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[98].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[99].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[100].Value.ToString());
				Assert.AreEqual("127",command.Parameters[101].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[102].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[103].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[104].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[105].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[106].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[107].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[108].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[109].Value.ToString());	

				Assert.AreEqual("128",command.Parameters[110].Value.ToString());
				Assert.AreEqual("126",command.Parameters[111].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[112].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[113].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[114].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[115].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[116].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[117].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[118].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[119].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[120].Value.ToString());
				Assert.AreEqual("126",command.Parameters[121].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[122].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[123].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[124].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[125].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[126].Value.ToString());	
				Assert.AreEqual("200",command.Parameters[127].Value.ToString());	

			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}

			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP PROCEDURE MaxProcNumericInOut;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);

        }

        /*
		To verify that maximum 128 OUT parameters are supported in .NET Connector.
*/
        //	[Test]
        public async Task testMaxParametersSupportInProcedureWithTextAsOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			var strSql ="CREATE OR REPLACE PROCEDURE MaxProcText(param1 out Text, param2 out Text,param3 out Text ,param4 out Text, param5 out Text,param6 out Text,param7 out Text, param8 out Text,param9 out Text,param10 out Text, param11 out Text,param12 out Text,param13 out Text, param14 out Text,param15 out Text,param16 out Text, param17 out Text,param18 out Text,param19 out Text, param20 out Text,param21 out Text,param22 out Text, param23 out Text,param24 out Text,param25 out Text, param26 out Text,param27 out Text,param28 out Text, param29 out Text,param30 out Text,param31 out Text, param32 out Text,param33 out Text,param34 out Text, param35 out Text,param36 out Text,param37 out Text, param38 out Text,param39 out Text,param40 out Text, param41 out Text,param42 out Text,param43 out Text, param44 out Text,param45 out Text,param46 out Text, param47 out Text,param48 out Text,param49 out Text, param50 out Text,param51 out Text,param52 out Text, param53 out Text,param54 out Text,param55 out Text, param56 out Text,param57 out Text,param58 out Text, param59 out Text,param60 out Text,param61 out Text, param62 out Text,param63 out Text,param64 out Text, param65 out Text,param66 out Text,param67 out Text, param68 out Text,param69 out Text,param70 out Text, param71 out Text,param72 out Text,param73 out Text, param74 out Text,param75 out Text,param76 out Text, param77 out Text,param78 out Text,param79 out Text, param80 out Text,param81 out Text,param82 out Text, param83 out Text,param84 out Text,param85 out Text, param86 out Text,param87 out Text,param88 out Text, param89 out Text,param90 out Text,param91 out Text, param92 out Text,param93 out Text,param94 out Text, param95 out Text,param96 out Text,param97 out Text,"
				+" param98 out Text,param99 out Text,param100 out Text, param101 out Text,param102 out Text,param103 out Text, param104 out Text,param105 out Text,param106 out Text, param107 out Text,param108 out Text,param109 out Text, param110 out Text,param111 out Text,param112 out Text, param113 out Text,param114 out Text,param115 out Text, param116 out Text,param117 out Text,param118 out Text, param119 out Text,param120 out Text,param121 out Text, param122 out Text,param123 out Text,param124 out Text, param125 out Text,param126 out Text,param127 out Text, param128 out Text)"
				+" IS \n"
				+" BEGIN \n"
				+"param1 := '1'; param2 := '2'; param3 := '3'; param4 := '4'; param5 := '5'; param6 := '6'; param7 := '7'; param8 := '8'; param9 := '9'; param10 := '10'; param11 := '11'; param12 := '12'; param13 := '13'; param14 := '14'; param15 := '15'; param16 := '16'; param17 := '17'; param18 := '18'; param19 := '19'; param20 := '20'; param21 := '21'; param22 := '22'; param23 := '23'; param24 := '24'; param25 := '25'; param26 := '26'; param27 := '27'; param28 := '28'; param29 := '29'; param30 := '30'; param31 := '31'; param32 := '32'; param33 := '33'; param34 := '34'; param35 := '35'; param36 := '36'; param37 := '37'; param38 := '38'; param39 := '39'; param40 := '40'; param41 := '41'; param42 := '42'; param43 := '43'; param44 := '44'; param45 := '45'; param46 := '46'; param47 := '47'; param48 := '48'; param49 := '49'; param50 := '50'; param51 := '51'; param52 := '52'; param53 := '53'; param54 := '54'; param55 := '55'; param56 := '56'; param57 := '57'; param58 := '58'; param59 := '59'; param60 := '60'; param61 := '61'; param62 := '62'; param63 := '63'; param64 := '64'; param65 := '65'; param66 := '66'; param67 := '67'; param68 := '68'; param69 := '69'; param70 := '70'; param71 := '71'; param72 := '72'; param73 := '73'; param74 := '74'; param75 := '75'; param76 := '76'; param77 := '77'; param78 := '78'; param79 := '79'; param80 := '80'; param81 := '81'; param82 := '82'; param83 := '83'; param84 := '84'; param85 := '85'; param86 := '86'; param87 := '87'; param88 := '88'; param89 := '89'; param90 := '90'; param91 := '91'; param92 := '92'; param93 := '93'; param94 := '94'; param95 := '95'; param96 := '96'; param97 := '97'; param98 := '98'; param99 := '99'; param100 := '100'; param101 := '101'; param102 := '102'; param103 := '103'; param104 := '104'; param105 := '105'; param106 := '106'; param107 := '107'; param108 := '108'; param109 := '109'; param110 := '110'; param111 := '111'; param112 := '112'; param113 := '113'; param114 := '114'; param115 := '115'; param116 := '116'; "
				+"param117 := '117'; param118 := '118'; param119 := '119'; param120 := '120'; param121 := '121'; param122 := '122'; param123 := '123'; param124 := '124'; param125 := '125'; param126 := '126'; param127 := '127'; param128 := '128'; END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();


			//////////////code
			try
			{
				command = new EDBCommand("MaxProcText(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10,:param11,:param12,:param13,:param14,:param15,:param16,:param17,:param18,:param19,:param20,:param21,:param22,:param23,:param24,:param25,:param26,:param27,:param28,:param29,:param30,:param31,:param32,:param33,:param34,:param35,:param36,:param37,:param38,:param39,:param40,:param41,:param42,:param43,:param44,:param45,:param46,:param47,:param48,:param49,:param50,:param51,:param52,:param53,:param54,:param55,:param56,:param57,:param58,:param59,:param60,:param61,:param62,:param63,:param64,:param65,:param66,:param67,:param68,:param69,:param70,:param71,:param72,:param73,:param74,:param75,:param76,:param77,:param78,:param79,:param80,:param81,:param82,:param83,:param84,:param85,:param86,:param87,:param88,:param89,:param90,:param91,:param92,:param93,:param94,:param95,:param96,:param97,:param98,:param99,:param100,:param101,:param102,:param103,:param104,:param105,:param106,:param107,:param108,:param109,:param110,:param111,:param112,:param113,:param114,:param115,:param116,:param117,:param118,:param119,:param120,:param121,:param122,:param123,:param124,:param125,:param126,:param127,:param128)",con);
				command.CommandType = CommandType.StoredProcedure;

                for (var i = 0; i < 128; i++)
                {
                    var paramValue = 128 - i;
                    var paramName = "param" + (i + 1).ToString();
                    command.Parameters.Add(new EDBParameter(paramName, EDBTypes.EDBDbType.Text, 10, paramName, ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, paramValue));
                }

				command.Prepare();
				await command.ExecuteNonQueryAsync();

                for (var i = 0; i < 128; i++)
                    Assert.AreEqual((i + 1).ToString(), command.Parameters[i].Value.ToString());
			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			
			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP PROCEDURE MaxProcText;";
			await command.ExecuteNonQueryAsync();

		}

		/// <summary>
		/// ////////////////////////Calling a procedure within a package with argument CHAR type
		/// ////////////////////////and with Parameter types IN, INOUT, OUT
		/// ////////////////////////DB feature used = Procedure
		/// </summary>
	//	[Test]
		public async Task testMaxParametersSupportInProcedureWithTextAsInAndOut()
        {
            await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("",con);
			command.CommandType = CommandType.Text;


			var strSql ="CREATE OR REPLACE PROCEDURE MaxProcText(param1 out Text, param2 inout Text,param3 in Text ,param4 out Text, param5 inout Text,param6 in Text,param7 out Text, param8 inout Text,param9 in Text,param10 out Text, param11 inout Text,param12 in Text,param13 out Text, param14 inout Text,param15 in Text,param16 out Text, param17 inout Text,param18 in Text,param19 out Text, param20 inout Text,param21 in Text,param22 out Text, param23 inout Text,param24 in Text,param25 out Text, param26 inout Text,param27 in Text,param28 out Text, param29 inout Text,param30 in Text,param31 out Text, param32 inout Text,param33 in Text,param34 out Text, param35 inout Text,param36 in Text,param37 out Text, param38 inout Text,param39 in Text,param40 out Text, param41 inout Text,param42 in Text,param43 out Text, param44 inout Text,param45 in Text,param46 out Text, param47 inout Text,param48 in Text,param49 out Text, param50 inout Text,param51 in Text,param52 out Text, param53 inout Text,param54 in Text,param55 out Text, param56 inout Text,param57 in Text,param58 out Text, param59 inout Text,param60 in Text,param61 out Text, param62 inout Text,param63 in Text,param64 out Text, param65 inout Text,param66 in Text,param67 out Text, param68 inout Text,param69 in Text,param70 out Text, param71 inout Text,param72 in Text,param73 out Text, param74 inout Text,param75 in Text,param76 out Text, param77 inout Text,param78 in Text,param79 out Text, param80 inout Text,param81 in Text,param82 out Text, param83 inout Text,param84 in Text,param85 out Text, param86 inout Text,param87 in Text,param88 out Text, param89 inout Text,param90 in Text,param91 out Text, param92 inout Text,param93 in Text,param94 out Text, param95 inout Text,param96 in Text"
				+" ,param97 out Text, param98 inout Text,param99 in Text,param100 out Text, param101 inout Text,param102 in Text,param103 out Text, param104 inout Text,param105 in Text,param106 out Text, param107 inout Text,param108 in Text,param109 out Text, param110 inout Text,param111 in Text,param112 out Text, param113 inout Text,param114 in Text,param115 out Text, param116 inout Text,param117 in Text,param118 out Text, param119 inout Text,param120 in Text,param121 out Text, param122 inout Text,param123 in Text,param124 out Text, param125 inout Text,param126 in Text,param127 out Text, param128 inout Text)"
				+" IS \n"
				+" BEGIN \n"
				+"param1 := param2; param2 := param3; param4 := param5; param5 := param6; param7 := param8; param8 := param9; param10 := param11; param11 := param12; param13 := param14; param14 := param15; param16 := param17; param17 := param18; param19 := param20; param20 := param21; param22 := param23; param23 := param24; param25 := param26; param26 := param27; param28 := param29; param29 := param30; param31 := param32; param32 := param33; param34 := param35; param35 := param36; param37 := param38; param38 := param39; param40 := param41; param41 := param42; param43 := param44; param44 := param45; param46 := param47; param47 := param48; param49 := param50; param50 := param51; param52 := param53; param53 := param54; param55 := param56; param56 := param57; param58 := param59; param59 := param60; param61 := param62; param62 := param63; param64 := param65; param65 := param66; param67 := param68; param68 := param69; param70 := param71; param71 := param72; param73 := param74; param74 := param75; param76 := param77; param77 := param78; param79 := param80; param80 := param81; param82 := param83; param83 := param84; param85 := param86; param86 := param87; param88 := param89; param89 := param90; param91 := param92; param92 := param93; param94 := param95; param95 := param96; param97 := param98; param98 := param99; param100 := param101; param101 := param102; param103 := param104; param104 := param105; param106 := param107; param107 := param108; param109 := param110; param110 := param111; param112 := param113; param113 := param114; param115 := param116; param116 := param117; param118 := param119; param119 := param120; param121 := param122; param122 := param123; param124 := param125; param125 := param126; param127 := param128; param128 := 'Hashim'; END;";
			command.CommandText = strSql;
			await command.ExecuteNonQueryAsync();


			//////////////code
			try
			{
				command = new EDBCommand("MaxProcText(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10,:param11,:param12,:param13,:param14,:param15,:param16,:param17,:param18,:param19,:param20,:param21,:param22,:param23,:param24,:param25,:param26,:param27,:param28,:param29,:param30,:param31,:param32,:param33,:param34,:param35,:param36,:param37,:param38,:param39,:param40,:param41,:param42,:param43,:param44,:param45,:param46,:param47,:param48,:param49,:param50,:param51,:param52,:param53,:param54,:param55,:param56,:param57,:param58,:param59,:param60,:param61,:param62,:param63,:param64,:param65,:param66,:param67,:param68,:param69,:param70,:param71,:param72,:param73,:param74,:param75,:param76,:param77,:param78,:param79,:param80,:param81,:param82,:param83,:param84,:param85,:param86,:param87,:param88,:param89,:param90,:param91,:param92,:param93,:param94,:param95,:param96,:param97,:param98,:param99,:param100,:param101,:param102,:param103,:param104,:param105,:param106,:param107,:param108,:param109,:param110,:param111,:param112,:param113,:param114,:param115,:param116,:param117,:param118,:param119,:param120,:param121,:param122,:param123,:param124,:param125,:param126,:param127,:param128)",con);
				command.CommandType = CommandType.StoredProcedure;

				command.Parameters.Add(new EDBParameter("param1",	EDBTypes.EDBDbType.Text,10,"param1",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,"128"));
				command.Parameters.Add(new EDBParameter("param2",	EDBTypes.EDBDbType.Text,10,"param2",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"127"));
				command.Parameters.Add(new EDBParameter("param3",	EDBTypes.EDBDbType.Text,10,"param3",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param4",	EDBTypes.EDBDbType.Text,10,"param4",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param5",	EDBTypes.EDBDbType.Text,10,"param5",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param6",	EDBTypes.EDBDbType.Text,10,"param6",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param7",	EDBTypes.EDBDbType.Text,10,"param7",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param8",	EDBTypes.EDBDbType.Text,10,"param8",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param9",	EDBTypes.EDBDbType.Text,10,"param9",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param10",	EDBTypes.EDBDbType.Text,10,"param10",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param11",	EDBTypes.EDBDbType.Text,10,"param11",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param12",	EDBTypes.EDBDbType.Text,10,"param12",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param13",	EDBTypes.EDBDbType.Text,10,"param13",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param14",	EDBTypes.EDBDbType.Text,10,"param14",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param15",	EDBTypes.EDBDbType.Text,10,"param15",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param16",	EDBTypes.EDBDbType.Text,10,"param16",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param17",	EDBTypes.EDBDbType.Text,10,"param17",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param18",	EDBTypes.EDBDbType.Text,10,"param18",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param19",	EDBTypes.EDBDbType.Text,10,"param19",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param20",	EDBTypes.EDBDbType.Text,10,"param20",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param21",	EDBTypes.EDBDbType.Text,10,"param21",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param22",	EDBTypes.EDBDbType.Text,10,"param22",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param23",	EDBTypes.EDBDbType.Text,10,"param23",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param24",	EDBTypes.EDBDbType.Text,10,"param24",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param25",	EDBTypes.EDBDbType.Text,10,"param25",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param26",	EDBTypes.EDBDbType.Text,10,"param26",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param27",	EDBTypes.EDBDbType.Text,10,"param27",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param28",	EDBTypes.EDBDbType.Text,10,"param28",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param29",	EDBTypes.EDBDbType.Text,10,"param29",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param30",	EDBTypes.EDBDbType.Text,10,"param30",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param31",	EDBTypes.EDBDbType.Text,10,"param31",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param32",	EDBTypes.EDBDbType.Text,10,"param32",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param33",	EDBTypes.EDBDbType.Text,10,"param33",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param34",	EDBTypes.EDBDbType.Text,10,"param34",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param35",	EDBTypes.EDBDbType.Text,10,"param35",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param36",	EDBTypes.EDBDbType.Text,10,"param36",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param37",	EDBTypes.EDBDbType.Text,10,"param37",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param38",	EDBTypes.EDBDbType.Text,10,"param38",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param39",	EDBTypes.EDBDbType.Text,10,"param39",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param40",	EDBTypes.EDBDbType.Text,10,"param40",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param41",	EDBTypes.EDBDbType.Text,10,"param41",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param42",	EDBTypes.EDBDbType.Text,10,"param42",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param43",	EDBTypes.EDBDbType.Text,10,"param43",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param44",	EDBTypes.EDBDbType.Text,10,"param44",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param45",	EDBTypes.EDBDbType.Text,10,"param45",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param46",	EDBTypes.EDBDbType.Text,10,"param46",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param47",	EDBTypes.EDBDbType.Text,10,"param47",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param48",	EDBTypes.EDBDbType.Text,10,"param48",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param49",	EDBTypes.EDBDbType.Text,10,"param49",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param50",	EDBTypes.EDBDbType.Text,10,"param50",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param51",	EDBTypes.EDBDbType.Text,10,"param51",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param52",	EDBTypes.EDBDbType.Text,10,"param52",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param53",	EDBTypes.EDBDbType.Text,10,"param53",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param54",	EDBTypes.EDBDbType.Text,10,"param54",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param55",	EDBTypes.EDBDbType.Text,10,"param55",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param56",	EDBTypes.EDBDbType.Text,10,"param56",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param57",	EDBTypes.EDBDbType.Text,10,"param57",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param58",	EDBTypes.EDBDbType.Text,10,"param58",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param59",	EDBTypes.EDBDbType.Text,10,"param59",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param60",	EDBTypes.EDBDbType.Text,10,"param60",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param61",	EDBTypes.EDBDbType.Text,10,"param61",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param62",	EDBTypes.EDBDbType.Text,10,"param62",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param63",	EDBTypes.EDBDbType.Text,10,"param63",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param64",	EDBTypes.EDBDbType.Text,10,"param64",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param65",	EDBTypes.EDBDbType.Text,10,"param65",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param66",	EDBTypes.EDBDbType.Text,10,"param66",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param67",	EDBTypes.EDBDbType.Text,10,"param67",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param68",	EDBTypes.EDBDbType.Text,10,"param68",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param69",	EDBTypes.EDBDbType.Text,10,"param69",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param70",	EDBTypes.EDBDbType.Text,10,"param70",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param71",	EDBTypes.EDBDbType.Text,10,"param71",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param72",	EDBTypes.EDBDbType.Text,10,"param72",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param73",	EDBTypes.EDBDbType.Text,10,"param73",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param74",	EDBTypes.EDBDbType.Text,10,"param74",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param75",	EDBTypes.EDBDbType.Text,10,"param75",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param76",	EDBTypes.EDBDbType.Text,10,"param76",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param77",	EDBTypes.EDBDbType.Text,10,"param77",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param78",	EDBTypes.EDBDbType.Text,10,"param78",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param79",	EDBTypes.EDBDbType.Text,10,"param79",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param80",	EDBTypes.EDBDbType.Text,10,"param80",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param81",	EDBTypes.EDBDbType.Text,10,"param81",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param82",	EDBTypes.EDBDbType.Text,10,"param82",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param83",	EDBTypes.EDBDbType.Text,10,"param83",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param84",	EDBTypes.EDBDbType.Text,10,"param84",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param85",	EDBTypes.EDBDbType.Text,10,"param85",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param86",	EDBTypes.EDBDbType.Text,10,"param86",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param87",	EDBTypes.EDBDbType.Text,10,"param87",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param88",	EDBTypes.EDBDbType.Text,10,"param88",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param89",	EDBTypes.EDBDbType.Text,10,"param89",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param90",	EDBTypes.EDBDbType.Text,10,"param90",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,119));			

				command.Parameters.Add(new EDBParameter("param91",	EDBTypes.EDBDbType.Text,10,"param91",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param92",	EDBTypes.EDBDbType.Text,10,"param92",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param93",	EDBTypes.EDBDbType.Text,10,"param93",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param94",	EDBTypes.EDBDbType.Text,10,"param94",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param95",	EDBTypes.EDBDbType.Text,10,"param95",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param96",	EDBTypes.EDBDbType.Text,10,"param96",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param97",	EDBTypes.EDBDbType.Text,10,"param97",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param98",	EDBTypes.EDBDbType.Text,10,"param98",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param99",	EDBTypes.EDBDbType.Text,10,"param99",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param100",	EDBTypes.EDBDbType.Text,10,"param100",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param101",	EDBTypes.EDBDbType.Text,10,"param101",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param102",	EDBTypes.EDBDbType.Text,10,"param102",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param103",	EDBTypes.EDBDbType.Text,10,"param103",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param104",	EDBTypes.EDBDbType.Text,10,"param104",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param105",	EDBTypes.EDBDbType.Text,10,"param105",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param106",	EDBTypes.EDBDbType.Text,10,"param106",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param107",	EDBTypes.EDBDbType.Text,10,"param107",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param108",	EDBTypes.EDBDbType.Text,10,"param108",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param109",	EDBTypes.EDBDbType.Text,10,"param109",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param110",	EDBTypes.EDBDbType.Text,10,"param110",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param111",	EDBTypes.EDBDbType.Text,10,"param111",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param112",	EDBTypes.EDBDbType.Text,10,"param112",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param113",	EDBTypes.EDBDbType.Text,10,"param113",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param114",	EDBTypes.EDBDbType.Text,10,"param114",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param115",	EDBTypes.EDBDbType.Text,10,"param115",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,124));				
				command.Parameters.Add(new EDBParameter("param116",	EDBTypes.EDBDbType.Text,10,"param116",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,123));				
				command.Parameters.Add(new EDBParameter("param117",	EDBTypes.EDBDbType.Text,10,"param117",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,122));				
				command.Parameters.Add(new EDBParameter("param118",	EDBTypes.EDBDbType.Text,10,"param118",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,121));				
				command.Parameters.Add(new EDBParameter("param119",	EDBTypes.EDBDbType.Text,10,"param119",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,120));				
				command.Parameters.Add(new EDBParameter("param120",	EDBTypes.EDBDbType.Text,10,"param120",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,119));				

				command.Parameters.Add(new EDBParameter("param121",	EDBTypes.EDBDbType.Text,10,"param121",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,128));
				command.Parameters.Add(new EDBParameter("param122",	EDBTypes.EDBDbType.Text,10,"param122",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,127));
				command.Parameters.Add(new EDBParameter("param123",	EDBTypes.EDBDbType.Text,10,"param123",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,126));
				command.Parameters.Add(new EDBParameter("param124",	EDBTypes.EDBDbType.Text,10,"param124",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,125));				
				command.Parameters.Add(new EDBParameter("param125",	EDBTypes.EDBDbType.Text,10,"param125",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"124"));				
				command.Parameters.Add(new EDBParameter("param126",	EDBTypes.EDBDbType.Text,10,"param126",ParameterDirection.Input,false, 2, 2,DataRowVersion.Current,"123"));				
				command.Parameters.Add(new EDBParameter("param127",	EDBTypes.EDBDbType.Text,10,"param127",ParameterDirection.Output,false, 2, 2,DataRowVersion.Current,"122"));				
				command.Parameters.Add(new EDBParameter("param128",	EDBTypes.EDBDbType.Text,10,"param128",ParameterDirection.InputOutput,false, 2, 2,DataRowVersion.Current,"121"));				

				command.Prepare();
	

				
				await command.ExecuteNonQueryAsync();

				Assert.AreEqual("127",command.Parameters[0].Value.ToString());
				Assert.AreEqual("126",command.Parameters[1].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[2].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[3].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[4].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[5].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[6].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[7].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[8].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[9].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[10].Value.ToString());
				Assert.AreEqual("127",command.Parameters[11].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[12].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[13].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[14].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[15].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[16].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[17].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[18].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[19].Value.ToString());	

				Assert.AreEqual("128",command.Parameters[20].Value.ToString());
				Assert.AreEqual("126",command.Parameters[21].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[22].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[23].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[24].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[25].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[26].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[27].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[28].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[29].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[30].Value.ToString());
				Assert.AreEqual("126",command.Parameters[31].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[32].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[33].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[34].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[35].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[36].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[37].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[38].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[39].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[40].Value.ToString());
				Assert.AreEqual("127",command.Parameters[41].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[42].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[43].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[44].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[45].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[46].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[47].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[48].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[49].Value.ToString());	

				Assert.AreEqual("128",command.Parameters[50].Value.ToString());
				Assert.AreEqual("126",command.Parameters[51].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[52].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[53].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[54].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[55].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[56].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[57].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[58].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[59].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[60].Value.ToString());
				Assert.AreEqual("126",command.Parameters[61].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[62].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[63].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[64].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[65].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[66].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[67].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[68].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[69].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[70].Value.ToString());
				Assert.AreEqual("127",command.Parameters[71].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[72].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[73].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[74].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[75].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[76].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[77].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[78].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[79].Value.ToString());	

				Assert.AreEqual("128",command.Parameters[80].Value.ToString());
				Assert.AreEqual("126",command.Parameters[81].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[82].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[83].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[84].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[85].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[86].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[87].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[88].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[89].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[90].Value.ToString());
				Assert.AreEqual("126",command.Parameters[91].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[92].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[93].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[94].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[95].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[96].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[97].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[98].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[99].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[100].Value.ToString());
				Assert.AreEqual("127",command.Parameters[101].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[102].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[103].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[104].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[105].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[106].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[107].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[108].Value.ToString());	
				Assert.AreEqual("128",command.Parameters[109].Value.ToString());	

				Assert.AreEqual("128",command.Parameters[110].Value.ToString());
				Assert.AreEqual("126",command.Parameters[111].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[112].Value.ToString());	
				Assert.AreEqual("125",command.Parameters[113].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[114].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[115].Value.ToString());	
				Assert.AreEqual("122",command.Parameters[116].Value.ToString());	
				Assert.AreEqual("120",command.Parameters[117].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[118].Value.ToString());	
				Assert.AreEqual("119",command.Parameters[119].Value.ToString());	

				Assert.AreEqual("127",command.Parameters[120].Value.ToString());
				Assert.AreEqual("126",command.Parameters[121].Value.ToString());	
				Assert.AreEqual("126",command.Parameters[122].Value.ToString());	
				Assert.AreEqual("124",command.Parameters[123].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[124].Value.ToString());	
				Assert.AreEqual("123",command.Parameters[125].Value.ToString());	
				Assert.AreEqual("121",command.Parameters[126].Value.ToString());	
				Assert.AreEqual("Hashim",command.Parameters[127].Value.ToString());	

			}
			catch(EDBException e)
			{			
				throw new Exception(e.ToString());
			}
			


			//////////tear down
			///
			command.Dispose();
			command = new EDBCommand("",con);
			command.CommandText = "DROP PROCEDURE MaxProcText;";
			await command.ExecuteNonQueryAsync();
            TestUtil.closeDB(con);


        }

        [Test]
		public void TestProcedureINOUT()
		{
//			var command = new EDBCommand("DEPT_SELECT(:pDEPTNO,:pDNAME,:pLOC)", con);
//			command.CommandType = CommandType.StoredProcedure;
//
//			command.Parameters.Add(new EDBParameter("pDEPTNO", EDBTypes.EDBDbType.Integer, 10, "pDEPTNO", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
//			command.Parameters.Add(new EDBParameter("pDNAME", EDBTypes.EDBDbType.Varchar, 10, "pDNAME", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
//			command.Parameters.Add(new EDBParameter("pLOC", EDBTypes.EDBDbType.Varchar, 10, "pLOC", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
//
//			command.Prepare();
//			command.Parameters[0].Value = 10;
//			await command.ExecuteNonQueryAsync();
//
//			Console.WriteLine(command.Parameters["pDNAME"].Value.ToString());
//			Console.WriteLine(command.Parameters["pLOC"].Value.ToString());    
//
//			Assert.AreEqual("accounting", command.Parameters["pDNAME"].Value.ToString().ToLower());
//			Assert.AreEqual("new york", command.Parameters["pLOC"].Value.ToString().ToLower() );          
            
		}
		

		[Test]
		public async Task TestMultipleInOutParameters()
		{
			try
            {
                await using var con = await OpenConnectionAsync();

                var com1 = new EDBCommand("", con);
                com1.CommandType = CommandType.Text;

                var strInOutArgs = "CREATE OR REPLACE PROCEDURE multipleInOutParameters(a IN NUMERIC, b OUT NUMERIC, c IN NUMERIC, d OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    b:=a; \n"
                    + "    d:=c; \n"
                    + " END; \n";


                com1.CommandText = strInOutArgs;
                await com1.ExecuteNonQueryAsync();
                com1.Dispose();

				var Command = new EDBCommand();
				Command=new EDBCommand("multipleInOutParameters(:a,:b,:c,:d)",con);
				Command.CommandType=CommandType.StoredProcedure;

				Command.Parameters.Add(new EDBParameter("a",EDBTypes.EDBDbType.Numeric));
				Command.Parameters[0].Value=50;

				Command.Parameters.Add(new EDBParameter("b",EDBTypes.EDBDbType.Integer,10,"b",ParameterDirection.Output,false,2,2,DataRowVersion.Current,1));
			
				Command.Parameters.Add(new EDBParameter("c",EDBTypes.EDBDbType.Numeric));
				Command.Parameters[2].Value=200;

				Command.Parameters.Add(new EDBParameter("d",EDBTypes.EDBDbType.Integer,10,"b",ParameterDirection.Output,false,2,2,DataRowVersion.Current,1));

				Command.Prepare();
				await Command.ExecuteNonQueryAsync();

				Assert.AreEqual(50,int.Parse(Command.Parameters[1].Value.ToString()));
				Assert.AreEqual(200,int.Parse(Command.Parameters[3].Value.ToString()));
                
                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;
                com.CommandText = "DROP PROCEDURE multipleInOutParameters";
                await com.ExecuteNonQueryAsync();
                com.Dispose();
                TestUtil.closeDB(con);

            }
            catch (EDBException exp)
			{
                Console.WriteLine(exp.Message);
                throw new Exception(exp.ToString());
            }
        }


        [Test, /*Ignore("MERGE_NEED_TO_EXPLORE")*/]
        public async Task TERSE_PROC_NATIVE_INPUT_TYPES()
        {
            try
            {
                await using var con = await OpenConnectionAsync();

                var Command = new EDBCommand();
                Command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                await Command.ExecuteNonQueryAsync();
                Command.Dispose();

                Command = new EDBCommand("BEGIN;", con);
                await Command.ExecuteNonQueryAsync();
                Command.Dispose();

                try
                {
                    Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    await Command.ExecuteNonQueryAsync();
                    Command.Dispose();
                }
                catch (EDBException)
                {
                }

                Command = new EDBCommand("create or replace procedure terse_p1( a integer, b integer ) is " +
                                         "begin " +
                                         "  dbms_output.put_line('a = ' || a); " +
                                         "  dbms_output.put_line('b = ' || b); " +
                                         "end; ", con);

                await Command.ExecuteNonQueryAsync();
                Command.Dispose();

                Command = new EDBCommand("terse_p1(:a,:b)", con);
                Command.CommandType = CommandType.StoredProcedure;
                Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
                Command.Parameters[0].Value = 50;

                Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer));
                Command.Parameters[1].Value = 51;

                Command.Prepare();
                await Command.ExecuteNonQueryAsync();
                Command.Dispose();

                Command = new EDBCommand("DROP PROCEDURE terse_p1", con);
                await Command.ExecuteNonQueryAsync();
                Command.Dispose();

                Command = new EDBCommand("END;", con);
                await Command.ExecuteNonQueryAsync();
                Command.Dispose();
                TestUtil.closeDB(con);

            }
            catch (EDBException exp)
            {
                throw new Exception(exp.ToString());
            }

        }
        
        //[Test]
        public async Task TERSE_PROC_NATIVE_OUTPUT_TYPES()
        {
            try
            {
                await using var con = await OpenConnectionAsync();

                var Command = new EDBCommand();
                Command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                await Command.ExecuteNonQueryAsync();
                Command.Dispose();

                Command = new EDBCommand("BEGIN;", con);
                await Command.ExecuteNonQueryAsync();
                Command.Dispose();

                try
                {
                    Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    await Command.ExecuteNonQueryAsync();
                    Command.Dispose();
                }
                catch (EDBException)
                {
                }

                Command = new EDBCommand("create or replace procedure terse_p1( a out integer, b out integer ) is " +
                                         "begin " +
                                         "  a := 10; " +
                                         "  b := 20; " +
                                         "end; ", con);

                await Command.ExecuteNonQueryAsync();
                Command.Dispose();

                Command = new EDBCommand("terse_p1(:a,:b)", con);
                Command.CommandType = CommandType.StoredProcedure;

                Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
                Command.Parameters[0].Direction = ParameterDirection.Output;
                Command.Parameters[0].Value = 10;

                Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer));
                Command.Parameters[1].Direction = ParameterDirection.Output;
                Command.Parameters[1].Value = 11;

                Command.Prepare();
                await Command.ExecuteNonQueryAsync();

                Assert.AreEqual(10, int.Parse(Command.Parameters[0].Value.ToString()));
                Assert.AreEqual(20, int.Parse(Command.Parameters[1].Value.ToString()));

                Command.Dispose();

                Command = new EDBCommand("DROP PROCEDURE terse_p1", con);
                await Command.ExecuteNonQueryAsync();
                Command.Dispose();

                Command = new EDBCommand("END;", con);
                await Command.ExecuteNonQueryAsync();
                Command.Dispose();
                TestUtil.closeDB(con);

            }
            catch (EDBException exp)
            {
                throw new Exception(exp.ToString());
            }
        }

        //[Test]
        public async Task TERSE_PROC_MIXED_NATIVE_TYPES()
        {
            try
            {
                await using var con = await OpenConnectionAsync();

                var command = new EDBCommand();

                command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                await command.ExecuteNonQueryAsync();

                command.Dispose();

                command = new EDBCommand("BEGIN;", con);

                await command.ExecuteNonQueryAsync();

                command.Dispose();

                try
                {

                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    await command.ExecuteNonQueryAsync();

                    command.Dispose();

                }
                catch (EDBException )
                {
                }



                command = new EDBCommand("multipleInOutArg_test(:a,:b,:c,:d)", con);

                command.CommandType = CommandType.StoredProcedure;



                command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Numeric));

                command.Parameters[0].Value = 5;



                command.Parameters.Add(new EDBParameter("b",

                    EDBTypes.EDBDbType.Integer, 10, "b",

                    ParameterDirection.Output, false, 2, 2,

                    System.Data.DataRowVersion.Current, 1));



                command.Parameters.Add(new EDBParameter("c", EDBTypes.EDBDbType.Numeric));

                command.Parameters[2].Value = 15;



                command.Parameters.Add(new EDBParameter("d",

                EDBTypes.EDBDbType.Integer, 10, "d",

                ParameterDirection.Output, false, 2, 2,

                    System.Data.DataRowVersion.Current, 1));



                command.Prepare();

                await command.ExecuteNonQueryAsync();



                Assert.AreEqual(5, int.Parse(command.Parameters[1].Value.ToString()));

                Assert.AreEqual(15, int.Parse(command.Parameters[3].Value.ToString()));

                command.Dispose();

                command = new EDBCommand("END;", con);

                await command.ExecuteNonQueryAsync();

                command.Dispose();

            }

            catch (EDBException e)

            {

                throw new Exception(e.ToString());

            }

        }

		[Test]
        public async Task TERSE_PROC_CURSOR_TYPES()

        {
            try
            {
                await using var con = await OpenConnectionAsync();

                var command = new EDBCommand();
                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                await command.ExecuteNonQueryAsync();
                command.Dispose();
                EDBTransaction tran = con.BeginTransaction();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    await command.ExecuteNonQueryAsync();
                    command.Dispose();
                }

                catch (EDBException)
                {
                }

                command = new EDBCommand("cursortest2(:cur1,:cur2)", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;

                //REFCUSOR CommandBehavior.SequentialAccess

                command.Parameters.Add(new EDBParameter("cur1", EDBTypes.EDBDbType.Refcursor, 10, "cur1", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("cur2", EDBTypes.EDBDbType.Refcursor, 10, "cur2", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Prepare();
                await command.ExecuteNonQueryAsync();

                var cursorName1 = command.Parameters[0].Value.ToString();
                var cursorName2 = command.Parameters[1].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader rst = command.ExecuteReader(CommandBehavior.SequentialAccess);

                rst.Read();

                Assert.AreEqual("7369", Convert.ToString(rst[0].ToString()));
                Assert.AreEqual("SMITH", Convert.ToString(rst.GetString(1)));
                Assert.AreEqual("CLERK", Convert.ToString(rst.GetString(2)));
                Assert.AreEqual("7902", Convert.ToString(rst[3].ToString()));
                Assert.AreEqual("800.00", Convert.ToString(rst[5].ToString()));

                rst.Close();
                
                command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                command.CommandType = CommandType.Text;
                rst = command.ExecuteReader(CommandBehavior.SequentialAccess);
                
                rst.Read();

                rst.Read();

                rst.Read();

                Assert.AreEqual("7521", Convert.ToString(rst[0].ToString()));

                Assert.AreEqual("WARD", Convert.ToString(rst.GetString(1)));

                Assert.AreEqual("SALESMAN", Convert.ToString(rst.GetString(2)));

                Assert.AreEqual("7698", Convert.ToString(rst[3].ToString()));

                Assert.AreEqual("1250.00", Convert.ToString(rst[5].ToString()));

                rst.Close();
                tran.Commit();
                TestUtil.closeDB(con);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

        }

        [Test]
        public async Task TERSE_PROC_MIXED_NATIVE_CURSOR_TYPES()
        {
            try
            {
                await using var con = await OpenConnectionAsync();

                var command = new EDBCommand();
                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                await command.ExecuteNonQueryAsync();
                command.Dispose();

                EDBTransaction tran = con.BeginTransaction();
                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    await command.ExecuteNonQueryAsync();
                    command.Dispose();
                }
                catch (EDBException )
                {
                }

                command = new EDBCommand("refcur_callee2(:b,:a,:c)", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;

                command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Numeric, 10, "b", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Refcursor, 10, "a", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("c", EDBTypes.EDBDbType.Refcursor, 10, "c", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null));

                command.Prepare();
                command.Parameters[0].Value = 7369;
                
                await command.ExecuteNonQueryAsync();
                Assert.AreEqual("100", Convert.ToString(command.Parameters[0].Value.ToString()));

                var cursorName1 = command.Parameters[1].Value.ToString();
                var cursorName2 = command.Parameters[2].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                reader.Read();
                reader.Read();

                Assert.AreEqual("7499", Convert.ToString(reader[0].ToString()));
                Assert.AreEqual("ALLEN", Convert.ToString(reader[1].ToString()));
                Assert.AreEqual("SALESMAN", Convert.ToString(reader[2].ToString()));
                Assert.AreEqual("7698", Convert.ToString(reader[3].ToString()));
                Assert.AreEqual("1600.00", Convert.ToString(reader[5].ToString()));

                reader.Close();

                command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                command.CommandType = CommandType.Text;
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                reader.Read();
                
                Assert.AreEqual("ADAMS", Convert.ToString(reader[0].ToString()));

                reader.Close();

                tran.Commit();
                TestUtil.closeDB(con);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }

        }

        [Test]
        public async Task TestProcWithNumericOutputParamOnly_ShouldBeClrType()
        {
            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            await using var dataSource = dataSourceBuilder.Build();

            await using var con = await dataSource.OpenConnectionAsync();
            //await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;


            var strSql = """
                CREATE OR REPLACE PROCEDURE public.TestProcWithNumericOutputParamOnly_ShouldBeClrType(
                    OUT p_out numeric
                )
                LANGUAGE 'edbspl'
                    SECURITY DEFINER VOLATILE PARALLEL UNSAFE 
                    COST 100
                AS $BODY$
                   BEGIN  
                   	SELECT 123.45
                        INTO p_out
                        FROM DUAL;   
                   END
                $BODY$;
                """;
            command.CommandText = strSql;
            await command.ExecuteNonQueryAsync();


            //////////////code
            try
            {
                command = new EDBCommand("TestProcWithNumericOutputParamOnly_ShouldBeClrType(:p_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("p_out", EDBTypes.EDBDbType.Numeric, 20, "p_out", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                await command.PrepareAsync();
                await command.ExecuteNonQueryAsync();

                var paramValue = command.Parameters["p_out"].Value;

                //////////tear down
                ///
                command.Dispose();
                command = new EDBCommand("", con);
                command.CommandText = "DROP procedure TestProcWithNumericOutputParamOnly_ShouldBeClrType;";
                await command.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);

                Assert.IsNotNull(paramValue);
                Assert.IsInstanceOf(typeof(decimal), paramValue);
                Assert.AreEqual(paramValue, 123.45m);
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }
        }

        [Test]

        public async Task TestProcWithNumericOutputParamAndOther_ShouldBeClrType()
        {
            var dataSourceBuilder = new EDBDataSourceBuilder(ConnectionString);
            await using var dataSource = dataSourceBuilder.Build();

            await using var con = await dataSource.OpenConnectionAsync();
            //await using var con = await OpenConnectionAsync();

            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;


            var strSql = """
                CREATE OR REPLACE PROCEDURE public.TestProcWithNumericOutputParamAndOther_ShouldBeClrType(
                	IN p_empno numeric,
                    OUT p_out numeric
                )
                LANGUAGE 'edbspl'
                    SECURITY DEFINER VOLATILE PARALLEL UNSAFE 
                    COST 100
                AS $BODY$
                   BEGIN  
                   	SELECT 123.45
                        INTO p_out
                        FROM DUAL;   
                   END
                $BODY$;
                """;
            command.CommandText = strSql;
            await command.ExecuteNonQueryAsync();


            //////////////code
            try
            {
                command = new EDBCommand("TestProcWithNumericOutputParamAndOther_ShouldBeClrType(:p_empno,:p_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                //command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));
                command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 7369));
                //command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "SMITH"));
                command.Parameters.Add(new EDBParameter("p_out", EDBTypes.EDBDbType.Numeric, 20, "p_out", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                //Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Date, 200, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, null));

                await command.PrepareAsync();
                await command.ExecuteNonQueryAsync();

                var paramValue = command.Parameters["p_out"].Value;

                //////////tear down
                ///
                command.Dispose();
                command = new EDBCommand("", con);
                command.CommandText = "DROP procedure TestProcWithNumericOutputParamAndOther_ShouldBeClrType;";
                await command.ExecuteNonQueryAsync();
                TestUtil.closeDB(con);

                Assert.IsNotNull(paramValue);
                Assert.IsInstanceOf(typeof(decimal), paramValue);
                Assert.AreEqual(paramValue, 123.45m);
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }
        }



        /*
				[Test]
				public async Task TERSE_PROC_DEFAULT_TYPES()
				{

					try

					{
						var Command = new EDBCommand();

						Command = new EDBCommand("set edb_stmt_level_tx to on;", con);

						await Command.ExecuteNonQueryAsync();

						Command.Dispose();

						Command = new EDBCommand("BEGIN;", con);

						await Command.ExecuteNonQueryAsync();

						Command.Dispose();

						try
						{
							Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

							await Command.ExecuteNonQueryAsync();

							Command.Dispose();

						}
						catch (EDBException exp)
						{
						}

						Command = new EDBCommand("create or replace procedure terse_p2( a integer, b integer default 10) is " +

												 "begin " +

												 "  dbms_output.put_line('a = ' || a); " +

												 "  dbms_output.put_line('b = ' || b); " +

												 "end; ", con);

						await Command.ExecuteNonQueryAsync();

						Command.Dispose();

						Command = new EDBCommand("terse_p2(:a)", con);

						Command.CommandType = CommandType.StoredProcedure;



						Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));

						Command.Parameters[0].Value = 50;

						Command.Prepare();

						await Command.ExecuteNonQueryAsync();

						Command.Dispose();

						Command = new EDBCommand("END;", con);

						await Command.ExecuteNonQueryAsync();

						Command.Dispose();

					}
					catch (EDBException exp)
					{
						throw new Exception(exp.ToString());
					}
				}
			*/

	}
#pragma warning restore CS8604
#pragma warning restore CS8602
#nullable restore
}
