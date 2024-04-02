using System;
using NUnit.Framework;
using EnterpriseDB.EDBClient;
using System.Data;

namespace EnterpriseDB.EDBClient.Tests.EnterpriseDB
{
#pragma warning disable CS8604
#pragma warning disable CS8602
#nullable disable
    /// <summary>
    /// Testing Functions with Different combination of parameters
    /// </summary>
    [TestFixture]
    //[NonParallelizable] // Manipulates the EnableStoredProcedureCompatMode global flag

    public class EDBFunctionTests : EPASTestBase
    {
        #region Setup / Teardown

        [OneTimeSetUp]
        public void Init()
        {
            using var con = OpenConnection();

            var com = new EDBCommand("", con);
            com.CommandType = CommandType.Text;

            //	Testing Functions without parameters
            var strSqlemptyfunction = "CREATE OR REPLACE Function emptyfunction_test return Varchar\n"
                + " IS \n"
                + " BEGIN \n"
                + "    RETURN 'EnterpriseDB'; \n"
                + " END; \n";
            com.CommandText = strSqlemptyfunction;
            com.ExecuteNonQuery();

            //	Testing Functions sanity
            var strSqlfunctionsanity = "CREATE OR REPLACE Function functionsanity(a1 OUT NUMERIC, a2 OUT NUMERIC, a3 IN NUMERIC,a4 OUT NUMERIC) return Varchar\n"
                + " IS \n"
                + " BEGIN \n"
                + "    a1:= 100; \n"
                + "    a2:= 200; \n"
                + "    a4:= 400; \n"
                + "    RETURN 'EnterpriseDB'; \n"
                + " END; \n";
            com.CommandText = strSqlfunctionsanity;
            com.ExecuteNonQuery();

            //	Testing Functions with one IN Param
            var strSqlFuncOneInArg = "CREATE OR REPLACE Function FunconeInArg_test(a IN NUMERIC) return varchar\n"
                + " AS \n"
                + "b        NUMBER(2);\n"
                + " BEGIN \n"
                + "    b := a; \n"
                + "    RETURN 'EnterpriseDB'; \n"
                + " END; \n";
            com.CommandText = strSqlFuncOneInArg;
            com.ExecuteNonQuery();

            //	Testing procedure with three IN Param
            var strSqlFuncThreeInArg = "CREATE OR REPLACE FUNCTION funcThreeInArg(a IN NUMERIC, b IN NUMERIC, c IN NUMERIC) return varchar\n"
                + " AS \n"
                + "d        NUMBER(2);\n"
                + " BEGIN \n"
                + "    d:=a; \n"
                + "    d:=d+b; \n"
                + "    d:=d+c; \n"
                + "    RETURN 'EnterpriseDB'; \n"
                + " END; \n";
            com.CommandText = strSqlFuncThreeInArg;
            com.ExecuteNonQuery();

            //	Testing function with args mix and return value
            var strSqlFuncMixArgsRetVal = """
                CREATE OR REPLACE FUNCTION mixArgFunc_test(a INOUT NUMERIC, b OUT NUMERIC, c IN NUMERIC)
                    RETURN int
                AS
                BEGIN
                    b:=c;
                    a:=a+a;
                    return b-1;
                END;
                """;
            com.CommandText = strSqlFuncMixArgsRetVal;
            com.ExecuteNonQuery();

            //	Testing parameterless function with a return value
            var strretValFunc_test = """
                CREATE OR REPLACE FUNCTION retValFunc_test()
                    RETURN int
                AS
                BEGIN
                    return 10;
                END;
                """;
            com.CommandText = strretValFunc_test;
            com.ExecuteNonQuery();

        }

        [OneTimeTearDown]
        public void Dispose()
        {
            using var con = OpenConnection();

            var com = new EDBCommand("", con);
            com.CommandType = CommandType.Text;

            com.CommandText = "DROP Function IF EXISTS emptyfunction_test;";
            com.ExecuteNonQuery();

            com.CommandText = "DROP Function IF EXISTS  functionsanity( OUT NUMERIC,  OUT NUMERIC, IN NUMERIC,OUT NUMERIC)";
            com.ExecuteNonQuery();

            com.CommandText = "DROP Function IF EXISTS FunconeInArg_test(IN NUMERIC)";
            com.ExecuteNonQuery();

            com.CommandText = "DROP Function IF EXISTS funcThreeInArg(IN NUMERIC, IN NUMERIC, IN NUMERIC)";
            com.ExecuteNonQuery();

            com.CommandText = "DROP Function IF EXISTS mixArgFunc_test(INOUT NUMERIC, OUT NUMERIC, IN NUMERIC)";
            com.ExecuteNonQuery();

            com.CommandText = "DROP Function IF EXISTS retValFunc_test()";
            com.ExecuteNonQuery();

            TestUtil.closeDB(con);
        }

        #endregion

        /* To verify the sanity of functions */
        [Test]
        public void testfunctionsanity()
        {
            try
            {
                using var con = OpenConnection();
                var command = new EDBCommand("public.functionsanity(:param1,:param2,:param3,:param4)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer, 10, "param3", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 10, "param5", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

                command.Prepare();

                command.Parameters[0].Value = 1;
                command.Parameters[1].Value = null;
                command.Parameters[2].Value = 3;
                command.Parameters[3].Value = null;

                command.ExecuteNonQuery();

                Assert.AreEqual(100, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(200, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(3, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(400, int.Parse(command.Parameters[3].Value.ToString()));
                Assert.AreEqual("EnterpriseDB", command.Parameters[4].Value.ToString());

            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        /* To verify the sanity of functions without parameters*/
        [Test]//, Ignore("Investigate Prompt")]
        public void testemptyfunction()
        {
            try
            {
                using var con = OpenConnection();
                var command = new EDBCommand("public.emptyfunction_test", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

                command.Prepare();
                command.ExecuteNonQuery();
                Assert.AreEqual("EnterpriseDB", command.Parameters[0].Value.ToString());

            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        /* To verify the sanity of functions with one IN parameters*/
        [Test]
        public void testOneInArg()
        {
            using var con = OpenConnection();
            var command = new EDBCommand("public.FunconeInArg_test(:a)", con);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Numeric, 10, "a", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
            command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

            command.Prepare();

            command.Parameters[0].Value = 3;

            command.ExecuteNonQuery();

            Assert.AreEqual(3, int.Parse(command.Parameters[0].Value.ToString()));
            Assert.AreEqual("EnterpriseDB", command.Parameters[1].Value.ToString());
        }

        /* To verify the sanity of functions with three IN parameters*/
        [Test, /*Ignore("Investigate Prompt")*/]
        public void testThreeInArg()
        {
            try
            {
                using var con = OpenConnection();
                //var command = new EDBCommand("public.funcThreeInArg(:param1,:param2,:param3)", con); 
                var command = new EDBCommand("public.funcThreeInArg(:param1, :param2, :param3)", con);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1", ParameterDirection.Input, false, 4, 4, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2", ParameterDirection.Input, false, 4, 4, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3", ParameterDirection.Input, false, 4, 4, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 10, "param4", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

                command.Prepare();

                command.Parameters[0].Value = 10;
                command.Parameters[1].Value = 20;
                command.Parameters[2].Value = 30;


                EDBDataReader result = command.ExecuteReader();
                while (result.Read())
                { }

                Assert.AreEqual(10, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(20, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(30, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        [Test]
        public void testmixArgRetValFunc()
        {

            using var con = OpenConnection();
            var command = new EDBCommand("public.mixArgFunc_test(:paramInOut, :paramOut, :paramIn)", con);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new EDBParameter("paramInOut", 10m) { EDBDbType = EDBTypes.EDBDbType.Numeric, Size = 10, Direction = ParameterDirection.InputOutput });
            command.Parameters.Add(new EDBParameter("paramOut", 10m) { EDBDbType = EDBTypes.EDBDbType.Numeric, Size = 10, Direction = ParameterDirection.Output });
            command.Parameters.Add(new EDBParameter("paramIn", 10m) { EDBDbType = EDBTypes.EDBDbType.Numeric, Size = 10, Direction = ParameterDirection.Input });
            command.Parameters.Add(new EDBParameter("paramRetVal", 4) { Direction = ParameterDirection.ReturnValue });
            //command.Parameters.Add(new EDBParameter("paramInOut", EDBTypes.EDBDbType.Numeric, 10, "paramInOut", ParameterDirection.InputOutput, false, 4, 4, System.Data.DataRowVersion.Current, 1));
            //command.Parameters.Add(new EDBParameter("paramOut", EDBTypes.EDBDbType.Numeric, 10, "paramOut", ParameterDirection.Output, false, 4, 4, System.Data.DataRowVersion.Current, 1));
            //command.Parameters.Add(new EDBParameter("paramIn", EDBTypes.EDBDbType.Numeric, 10, "paramIn", ParameterDirection.Input, false, 4, 4, System.Data.DataRowVersion.Current, 1));
            //command.Parameters.Add(new EDBParameter("paramRetVal", EDBTypes.EDBDbType.Integer, 4, "paramRetVal", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

            command.Prepare();

            command.Parameters["paramInOut"].Value = 10;
            command.Parameters["paramIn"].Value = 25;

            EDBDataReader reader = command.ExecuteReader();
            Assert.IsTrue(reader.HasRows);
            Assert.AreEqual(20m, command.Parameters["paramInOut"].Value);
            Assert.AreEqual(25m, command.Parameters["paramOut"].Value);
            Assert.AreEqual(24, command.Parameters["paramRetVal"].Value);

            Assert.AreEqual(3, reader.FieldCount);

            Assert.IsTrue(reader.Read());


            object[] values = new object[reader.FieldCount];
            reader.GetValues(values);

            int[] expected = [20, 25, 24];
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], values[i]);
            }

            Assert.IsFalse(reader.Read());


            Assert.AreEqual(20, int.Parse(command.Parameters["paramInOut"].Value.ToString()));
            Assert.AreEqual(25, int.Parse(command.Parameters["paramOut"].Value.ToString()));
            Assert.AreEqual(24, int.Parse(command.Parameters["paramRetVal"].Value.ToString()));

            reader.Close();
            Assert.DoesNotThrowAsync(async () => await reader.DisposeAsync());

        }

        [Test]
        public void testretValFunc_test()
        {
            try
            {
                using var con = OpenConnection();
                var command = new EDBCommand("public.retValFunc_test()", con);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("paramRetVal", null) { Direction = ParameterDirection.ReturnValue });

                command.Prepare();

                EDBDataReader reader = command.ExecuteReader();
                Assert.IsTrue(reader.HasRows);
                Assert.AreEqual(10, int.Parse(command.Parameters["paramRetVal"].Value.ToString()));

                Assert.AreEqual(1, reader.FieldCount);

                Assert.IsTrue(reader.Read());


                object[] values = new object[reader.FieldCount];
                reader.GetValues(values);

                int[] expected = [10];
                for (int i = 0; i < expected.Length; i++)
                {
                    Assert.AreEqual(expected[i], values[i]);
                }

                Assert.IsFalse(reader.Read());

                Assert.AreEqual(10, int.Parse(command.Parameters["paramRetVal"].Value.ToString()));
            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }
        }


        #region Numeric data type

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with INT datatype */
        [Test]
        public void testFunctionWithINTAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithINT(p_in in int,p_inout inout int,p_out out int) return int  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 12000;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithINT(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;


                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Integer, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 100));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Integer, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 2000));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Integer, 10, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 40));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Integer, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(100, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(100, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(2000, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(12000, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithINT( in int, inout int,out int) ";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with INT4 datatype */
        [Test]
        public void testFunctionWithINT4AsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithInt4(p_in in int4,p_inout inout int4,p_out out int4) return int4  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 12000;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithInt4(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Integer, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Integer, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 2000));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Integer, 10, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 4000));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Integer, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(1000, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(1000, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(2000, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(12000, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithInt4(in int4, inout int4, out int4);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with INT8 datatype */
        [Test]
        public void testFunctionWithINT8AsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithInt8(p_in in int8,p_inout inout int8,p_out out int8) return int8  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1010;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithInt8(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Bigint, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Bigint, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 20000));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Bigint, 10, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 400));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Bigint, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(1000, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(1000, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(20000, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(1010, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithInt8( in int8, inout int8, out int8);";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with INTEGER datatype */
        [Test]
        public void testFunctionWithINTEGERAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithInteger(p_in in Integer,p_inout inout Integer,p_out out Integer) return Integer  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1010;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithInteger(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Integer, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Integer, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 20000));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Integer, 10, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 400));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Integer, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(1000, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(1000, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(20000, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(1010, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithInteger( in Integer, inout Integer, out Integer);";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with SmallInt datatype */
        [Test]
        public void testFunctionWithSmallINTAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithSmallInt(p_in in SmallInt,p_inout inout SmallInt,p_out out SmallInt) return SmallInt  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1010;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithSmallInt(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Smallint, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Smallint, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 20000));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Smallint, 10, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 400));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Smallint, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(1000, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(1000, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(20000, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(1010, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithSmallInt( in SmallInt, inout SmallInt, out SmallInt)";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with BigInt datatype */
        [Test, Ignore("EDB: Bigvar type does not exist")]
        //[Test]
        public void testFunctionWithBigIntAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithBigInt(p_in in Bigint,p_inout inout Bigint,p_out out Bigint) return Bigvar  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 101000;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithBigint(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Bigint, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 100906));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Bigint, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 200906));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Bigint, 10, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 220905));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Bigint, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 180902));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(100906, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(100906, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(200906, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(101000, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithBigInt( in Bigint, inout Bigint, out Bigint);";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with Numeric datatype */
        [Test]
        public void testFunctionWithNUMERICASInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithNumeric(p_in in NUMERIC,p_inout inout NUMERIC,p_out out NUMERIC) return NUMERIC  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1234;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithNumeric(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Numeric, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 10000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Numeric, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, -2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Numeric, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 40000));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Numeric, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(10000, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(10000, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(-2, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(1234, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithNumeric( in NUMERIC, inout NUMERIC, out NUMERIC) ;";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with NUMBER datatype */
        [Test]
        public void testFunctionWithNUMBERASInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithNumber(p_in in NUMBER,p_inout inout NUMBER,p_out out NUMBER) return NUMBER  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1234;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithNumber(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Numeric, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 10000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Numeric, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, -2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Numeric, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 40000));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Numeric, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(10000, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(10000, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(-2, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(1234, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            ///
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithNumber( in NUMBER, inout NUMBER, out NUMBER);";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with DEC datatype */
        [Test]
        public void testFunctionWithDecASInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithDec(p_in in Dec,p_inout inout Dec,p_out out Dec) return Dec  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1234;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithDec(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Numeric, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 10000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Numeric, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, -2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Numeric, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 40000));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Numeric, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(10000, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(10000, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(-2, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(1234, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithDec( in Dec, inout Dec, out Dec);";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with DECIMAL datatype */
        [Test]
        public void testFunctionWithDecimalASInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithDecimal(p_in in Decimal,p_inout inout Decimal,p_out out Decimal) return Decimal  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1234;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithDecimal(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Numeric, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 10000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Numeric, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, -2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Numeric, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 40000));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Numeric, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(10000, int.Parse(s: command.Parameters[0].Value.ToString()));
                Assert.AreEqual(10000, int.Parse(s: command.Parameters[1].Value.ToString()));
                Assert.AreEqual(-2, int.Parse(s: command.Parameters[2].Value.ToString()));
                Assert.AreEqual(1234, int.Parse(s: command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION  FunctionWithDecimal( in Decimal, inout Decimal, out Decimal);";
            command.ExecuteNonQuery();

        }

        #endregion

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with FLOAT datatype */
        [Test]
        public void testFunctionWithFLOATAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithFloat(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT) return FLOAT  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return -0.999;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////codef
            try
            {
                command = new EDBCommand("FunctionWithFloat(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Double, 10, "v_in", ParameterDirection.Input, false, 8, 8, DataRowVersion.Current, 1.10001));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Double, 10, "v_inout", ParameterDirection.InputOutput, false, 8, 8, DataRowVersion.Current, -2.2131));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Double, 10, "v_out", ParameterDirection.InputOutput, false, 8, 8, DataRowVersion.Current, 4.4009));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Double, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 8.8));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(1.10001f, float.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(1.10001f, float.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(-2.2131f, float.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(-0.999f, float.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithFloat( in FLOAT, inout FLOAT, out FLOAT);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with DOUBLE PRECISION datatype */
        [Test]
        public void testFunctionWithDoublePrecisionAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithDoublePrecision(p_in in Double Precision,p_inout inout Double Precision,p_out out Double Precision) return Double Precision  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return -0.999;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithDoublePrecision(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Double, 10, "v_in", ParameterDirection.Input, false, 8, 8, DataRowVersion.Current, 1.10001));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Double, 10, "v_inout", ParameterDirection.InputOutput, false, 8, 8, DataRowVersion.Current, -2.2131));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Double, 10, "v_out", ParameterDirection.Output, false, 8, 8, DataRowVersion.Current, 4.4009));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Double, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 8.8));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(1.10001f, float.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(1.10001f, float.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(-2.2131f, float.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(-0.999f, float.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithDoublePrecision( in Double Precision, inout Double Precision, out Double Precision) ;";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with REAL datatype */
        [Test]
        public void testFunctionWithREALAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithReal(p_in in REAL,p_inout inout REAL,p_out out REAL) return REAL  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 10.1111;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithReal(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Real, 0, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1.1));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Real, 0, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 2.2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Real, 0, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 4.4));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Real, 0, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 8.8));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(1.1f, float.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(1.1f, float.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(2.2f, float.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(10.1111f, float.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithReal( in REAL, inout REAL, out REAL);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with CHAR datatype */
        [Test]
        public void testFunctionWithCHARAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithChar(p_in in CHAR(30),p_inout inout CHAR(30),p_out out CHAR(30)) return CHAR(30)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithChar(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Char, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Char, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Char, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Char, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithChar( in CHAR(30), inout CHAR(30), out CHAR(30));";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with CHARACTER datatype */
        [Test]
        public void testFunctionWithCHARACTERAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithCharacter(p_in in CHARACTER(30),p_inout inout CHARACTER(30),p_out out CHARACTER(30)) return CHARACTER(30)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithCharacter(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Char, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Char, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Char, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Char, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithCharacter( in CHARACTER(30), inout CHARACTER(30), out CHARACTER(30));";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with CHARACTER VARYING datatype */
        [Test]
        public void testFunctionWithCHARACTERVARYINGAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithCharacterVarying(p_in in CHARACTER Varying,p_inout inout CHARACTER Varying,p_out out CHARACTER Varying) return CHARACTER Varying   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithCharacterVarying(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Varchar, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Varchar, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Varchar, 12, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithCharacterVarying( in CHARACTER Varying, inout CHARACTER Varying, out CHARACTER Varying);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with CHAR VARYING datatype */
        [Test]
        public void testFunctionWithCHARVARYINGAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithCharVarying(p_in in CHAR Varying,p_inout inout CHAR Varying,p_out out CHAR Varying) return CHAR Varying   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithCharVarying(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Varchar, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Varchar, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Varchar, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithCharVarying( in CHAR Varying, inout CHAR Varying, out CHAR Varying);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with CLOB datatype */
        [Test]
        public void testFunctionWithCLOBAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithClob(p_in in CLOB,p_inout inout CLOB,p_out out CLOB) return CLOB   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithCLOB(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Varchar, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Varchar, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Varchar, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithClob( in CLOB, inout CLOB, out CLOB);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with LONG TEXT datatype */
        [Test]
        public void testFunctionWithLongTextAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithLongText1(p_in in Text,p_inout inout Text,p_out out Text) return Text   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithLongText1(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Varchar, 6, "v_in", ParameterDirection.Input, false, 0, 0, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Varchar, 12, "v_inout", ParameterDirection.InputOutput, false, 0, 0, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Varchar, 1, "v_out", ParameterDirection.Output, false, 0, 0, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 6, "v_ret", ParameterDirection.ReturnValue, false, 0, 0, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION  FunctionWithLongText1( in LongText, inout LongText, out LongText)";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with LONG datatype */
        [Test]
        public void testFunctionWithLongAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithLong(p_in in Long,p_inout inout Long,p_out out Long) return Long   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithLong(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Text, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Text, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Text, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Text, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithLong( in Long, inout Long, out Long);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with TEXT datatype */
        [Test]
        public void testFunctionWithTextAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithText(p_in in Text,p_inout inout Text,p_out out Text) return Text   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithText(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Text, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Text, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Text, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Text, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION  FunctionWithText( in Text, inout Text, out Text) ;";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with LONG VARCHAR datatype */
        [Test]
        public void testFunctionWithLongVarcharAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithLongVarchar(p_in in Long Varchar,p_inout inout Long Varchar,p_out out Long Varchar) return Long Varchar   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithLongVarchar(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Text, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Text, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Text, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Text, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithLongVarchar( in Long Varchar, inout Long Varchar, out Long Varchar);";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with LVARCHAR datatype */
        [Test]
        public void testFunctionWithLVarcharAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithLVarchar(p_in in LVarchar,p_inout inout LVarchar,p_out out LVarchar) return LVarchar   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithLVarchar(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Varchar, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Varchar, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Varchar, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithLVarchar( in LVarchar, inout LVarchar, out LVarchar)";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with MEDIUM TEXT datatype */
        [Test]
        public void testFunctionWithMediumTextAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithMediumText(p_in in MediumText,p_inout inout MediumText,p_out out MediumText) return MediumText   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithMediumText(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Text, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Text, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Text, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Text, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION  FunctionWithMediumText( in MediumText, inout MediumText, out MediumText) ;";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with TINY TEXT datatype */
        [Test]
        public void testFunctionWithTinyTextAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithtinyText(p_in in TinyText,p_inout inout TinyText,p_out out TinyText) return TinyText   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithTinyText(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Text, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Text, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Text, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Text, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithtinyText( in TinyText, inout TinyText, out TinyText);";
            command.ExecuteNonQuery();

        }


        /* To verify the sanity of IN, INOUT and OUT parameters in functions with MONEY datatype */
        [Test, /*Ignore("Investigate Prompt")*/]
        public void testFunctionWithMONEYASInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithMONEY(p_in in MONEY,p_inout inout MONEY,p_out out MONEY) return MONEY  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1234;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithMONEY(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Money, 10, "v_in", ParameterDirection.Input, false, 0, 0, DataRowVersion.Current, 10000m));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Money, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, -2.0m));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Money, 10, "v_out", ParameterDirection.Output, false, 0, 0, DataRowVersion.Current, 40000m));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Money, 10, "v_ret", ParameterDirection.ReturnValue, false, 0, 0, System.Data.DataRowVersion.Current, 100m));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(10000, (int)(float.Parse(command.Parameters[0].Value.ToString())));
                Console.WriteLine(command.Parameters[0].Value.ToString());

                Assert.AreEqual(10000, (int)(float.Parse(command.Parameters[1].Value.ToString())));
                Console.WriteLine(command.Parameters[1].Value.ToString());
                var val = command.Parameters[2].Value.ToString();
                //Not sure which AS version, but it returns ($2.00) for -2.
                var expected = -2;
                if (val.StartsWith('(') && val.EndsWith(')'))
                {
                    expected = 2;
                    val = val.Trim('(', ')', '$');
                }
                Assert.AreEqual(expected, (int)(float.Parse(val)));
                Console.WriteLine(command.Parameters[2].Value.ToString());
                Assert.AreEqual(1234, (int)(float.Parse(command.Parameters[3].Value.ToString())));
                Console.WriteLine(command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithMONEY( in MONEY, inout MONEY, out MONEY);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with SMALLMONEY datatype */
        [Test, /*Ignore("Investigate Prompt")*/]
        public void testFunctionWithSmallMoneyASInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithSmallMoney(p_in in SmallMoney,p_inout inout SmallMoney,p_out out SmallMoney) return SmallMoney  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1234;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithSmallMoney(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Numeric, 0, "v_in", ParameterDirection.Input, false, 0, 0, DataRowVersion.Current, 10000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Numeric, 0, "v_inout", ParameterDirection.InputOutput, false, 0, 0, DataRowVersion.Current, -2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Numeric, 0, "v_out", ParameterDirection.InputOutput, false, 0, 0, DataRowVersion.Current, 40000));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Numeric, 0, "v_ret", ParameterDirection.ReturnValue, false, 0, 0, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual("10000", command.Parameters[0].Value.ToString());
                Assert.AreEqual("10000", command.Parameters[1].Value.ToString());
                Assert.AreEqual("-2", command.Parameters[2].Value.ToString());
                Assert.AreEqual("1234", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithSmallMoney( in SmallMoney, inout SmallMoney, out SmallMoney);";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with VARCHAR datatype */
        [Test]
        public void testFunctionWithVarcharAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithVarchar(p_in in Varchar,p_inout inout Varchar,p_out out Varchar) return Varchar   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithVarchar(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Varchar, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Varchar, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Varchar, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithVarchar( in Varchar, inout Varchar, out Varchar);";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with VARCHAR2 datatype */
        [Test]
        public void testFunctionWithVarchar2AsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithVarchar2(p_in in Varchar2,p_inout inout Varchar2,p_out out Varchar2) return Varchar2   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithVarchar2(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Varchar, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Varchar, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Varchar, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithVarchar2( in Varchar2, inout Varchar2, out Varchar2)";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with BOOLEAN datatype */
        [Test]
        public void testFunctionWithBooleanAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithBoolean(p_in in Boolean,p_inout inout Boolean,p_out out Boolean) return Boolean  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithBoolean(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;


                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Boolean, 10, "v_in", ParameterDirection.Input, false, 8, 8, DataRowVersion.Current, true));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Boolean, 10, "v_inout", ParameterDirection.InputOutput, false, 8, 8, DataRowVersion.Current, false));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Boolean, 10, "v_out", ParameterDirection.Output, false, 8, 8, DataRowVersion.Current, true));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Boolean, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, true));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(true, command.Parameters["v_in"].Value);
                Assert.AreEqual(true, command.Parameters["v_inout"].Value);
                Assert.AreEqual(false, command.Parameters["v_out"].Value);
                Assert.AreEqual(true, command.Parameters["v_ret"].Value);
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithBoolean( in Boolean, inout Boolean, out Boolean);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with BIT datatype */
        //	[Test]
        public void testFunctionWithBitAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithBit(p_in in Bit,p_inout inout Bit,p_out out Bit) return Bit  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return true;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithBit(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Boolean, 0, "v_in", ParameterDirection.Input, false, 0, 0, DataRowVersion.Current, true));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Boolean, 0, "v_inout", ParameterDirection.InputOutput, false, 0, 0, DataRowVersion.Current, false));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Boolean, 0, "v_out", ParameterDirection.Output, false, 0, 0, DataRowVersion.Current, true));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Boolean, 0, "v_ret", ParameterDirection.ReturnValue, false, 0, 0, System.Data.DataRowVersion.Current, true));
                command.Prepare();
                command.ExecuteNonQuery();

                Assert.AreEqual(true, bool.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual("True", command.Parameters[1].Value.ToString());
                var p_out = false;
                Assert.AreEqual(p_out, command.Parameters[2].Value);
                Assert.AreEqual("True", command.Parameters[3].Value.ToString());
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithBit( in Bit, inout Bit, out Bit);";
            command.ExecuteNonQuery();

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with SMALL FLOAT datatype */
        [Test]
        public void testFunctionWithSMALLFLOATAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithSmallFloat(p_in in Float,p_inout inout Float,p_out out Float) return Float  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 10.1111;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithSmallFloat(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Double, 0, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1.1));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Double, 0, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 2.2));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Double, 0, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 4.4));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Double, 0, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 8.8));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(1.1f, float.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(1.1f, float.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(2.2f, float.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(10.1111f, float.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP FUNCTION FunctionWithSmallFloat( in Float, inout Float, out Float);";
            command.ExecuteNonQuery();
        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with TinyInt datatype */
        [Test]
        public void testFunctionWithTinyIntAsInInoutOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;

            var strSql = "CREATE OR REPLACE FUNCTION FunctionWithTinyInt(p_in in TinyInt,p_inout inout TinyInt,p_out out TinyInt) return TinyInt  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return 1010;  END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();

            //////////////code
            try
            {
                command = new EDBCommand("FunctionWithTinyInt(:v_in,:v_inout,:v_out)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Smallint, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1000));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Smallint, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 20000));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Smallint, 10, "v_out", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 400));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Smallint, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();

                Assert.AreEqual(1000, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(1000, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(20000, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(1010, int.Parse(command.Parameters[3].Value.ToString()));
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

            //////////tear down
            command.Dispose();
            command = new EDBCommand("", con);
            command.CommandText = "DROP Function FunctionWithTinyInt( in TinyInt, inout TinyInt, out TinyInt) ;";
            command.ExecuteNonQuery();

        }

        /*
		To verify that maximum 128 OUT parameters are supported in .NET Connector.
*/
        [Test]
        public void testMaxParametersSupportInFunctionWithNumericAsOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;


            var strSql = "CREATE OR REPLACE FUNCTION MaxFuncNumeric(param1 out Numeric, param2 out Numeric,param3 out Numeric ,param4 out Numeric, param5 out Numeric,param6 out Numeric,param7 out Numeric, param8 out Numeric,param9 out Numeric,param10 out Numeric, param11 out Numeric,param12 out Numeric,param13 out Numeric, param14 out Numeric,param15 out Numeric,param16 out Numeric, param17 out Numeric,param18 out Numeric,param19 out Numeric, param20 out Numeric,param21 out Numeric,param22 out Numeric, param23 out Numeric,param24 out Numeric,param25 out Numeric, param26 out Numeric,param27 out Numeric,param28 out Numeric, param29 out Numeric,param30 out Numeric,param31 out Numeric, param32 out Numeric,param33 out Numeric,param34 out Numeric, param35 out Numeric,param36 out Numeric,param37 out Numeric, param38 out Numeric,param39 out Numeric,param40 out Numeric, param41 out Numeric,param42 out Numeric,param43 out Numeric, param44 out Numeric,param45 out Numeric,param46 out Numeric, param47 out Numeric,param48 out Numeric,param49 out Numeric, param50 out Numeric,param51 out Numeric,param52 out Numeric, param53 out Numeric,param54 out Numeric,param55 out Numeric, param56 out Numeric,param57 out Numeric,param58 out Numeric, param59 out Numeric,param60 out Numeric,param61 out Numeric, param62 out Numeric,param63 out Numeric,param64 out Numeric, param65 out Numeric,param66 out Numeric,param67 out Numeric, param68 out Numeric,param69 out Numeric,param70 out Numeric, param71 out Numeric,param72 out Numeric,param73 out Numeric, param74 out Numeric,param75 out Numeric,param76 out Numeric, param77 out Numeric,param78 out Numeric,param79 out Numeric, param80 out Numeric,param81 out Numeric,param82 out Numeric, param83 out Numeric,param84 out Numeric,param85 out Numeric, param86 out Numeric,param87 out Numeric,param88 out Numeric, param89 out Numeric,param90 out Numeric,param91 out Numeric, param92 out Numeric,param93 out Numeric,param94 out Numeric, param95 out Numeric,param96 out Numeric,param97 out Numeric,"
                + " param98 out Numeric,param99 out Numeric,param100 out Numeric, param101 out Numeric,param102 out Numeric,param103 out Numeric, param104 out Numeric,param105 out Numeric,param106 out Numeric, param107 out Numeric,param108 out Numeric,param109 out Numeric, param110 out Numeric,param111 out Numeric,param112 out Numeric, param113 out Numeric,param114 out Numeric,param115 out Numeric, param116 out Numeric,param117 out Numeric,param118 out Numeric, param119 out Numeric,param120 out Numeric,param121 out Numeric, param122 out Numeric,param123 out Numeric,param124 out Numeric, param125 out Numeric,param126 out Numeric,param127 out Numeric, param128 out Numeric) return Numeric"
                + " IS \n"
                + " BEGIN \n"
                + "param1 := 1; param2 := 2; param3 := 3; param4 := 4; param5 := 5; param6 := 6; param7 := 7; param8 := 8; param9 := 9; param10 := 10; param11 := 11; param12 := 12; param13 := 13; param14 := 14; param15 := 15; param16 := 16; param17 := 17; param18 := 18; param19 := 19; param20 := 20; param21 := 21; param22 := 22; param23 := 23; param24 := 24; param25 := 25; param26 := 26; param27 := 27; param28 := 28; param29 := 29; param30 := 30; param31 := 31; param32 := 32; param33 := 33; param34 := 34; param35 := 35; param36 := 36; param37 := 37; param38 := 38; param39 := 39; param40 := 40; param41 := 41; param42 := 42; param43 := 43; param44 := 44; param45 := 45; param46 := 46; param47 := 47; param48 := 48; param49 := 49; param50 := 50; param51 := 51; param52 := 52; param53 := 53; param54 := 54; param55 := 55; param56 := 56; param57 := 57; param58 := 58; param59 := 59; param60 := 60; param61 := 61; param62 := 62; param63 := 63; param64 := 64; param65 := 65; param66 := 66; param67 := 67; param68 := 68; param69 := 69; param70 := 70; param71 := 71; param72 := 72; param73 := 73; param74 := 74; param75 := 75; param76 := 76; param77 := 77; param78 := 78; param79 := 79; param80 := 80; param81 := 81; param82 := 82; param83 := 83; param84 := 84; param85 := 85; param86 := 86; param87 := 87; param88 := 88; param89 := 89; param90 := 90; param91 := 91; param92 := 92; param93 := 93; param94 := 94; param95 := 95; param96 := 96; param97 := 97; param98 := 98; param99 := 99; param100 := 100; param101 := 101; param102 := 102; param103 := 103; param104 := 104; param105 := 105; param106 := 106; param107 := 107; param108 := 108; param109 := 109; param110 := 110; param111 := 111; param112 := 112; param113 := 113; param114 := 114; param115 := 115; param116 := 116; param117 := 117; param118 := 118; param119 := 119; param120 := 120; param121 := 121; param122 := 122; param123 := 123; param124 := 124; param125 := 125; param126 := 126; param127 := 127; param128 := 128; return 203; END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();


            //////////////code
            try
            {
                command = new EDBCommand("MaxFuncNumeric(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10,:param11,:param12,:param13,:param14,:param15,:param16,:param17,:param18,:param19,:param20,:param21,:param22,:param23,:param24,:param25,:param26,:param27,:param28,:param29,:param30,:param31,:param32,:param33,:param34,:param35,:param36,:param37,:param38,:param39,:param40,:param41,:param42,:param43,:param44,:param45,:param46,:param47,:param48,:param49,:param50,:param51,:param52,:param53,:param54,:param55,:param56,:param57,:param58,:param59,:param60,:param61,:param62,:param63,:param64,:param65,:param66,:param67,:param68,:param69,:param70,:param71,:param72,:param73,:param74,:param75,:param76,:param77,:param78,:param79,:param80,:param81,:param82,:param83,:param84,:param85,:param86,:param87,:param88,:param89,:param90,:param91,:param92,:param93,:param94,:param95,:param96,:param97,:param98,:param99,:param100,:param101,:param102,:param103,:param104,:param105,:param106,:param107,:param108,:param109,:param110,:param111,:param112,:param113,:param114,:param115,:param116,:param117,:param118,:param119,:param120,:param121,:param122,:param123,:param124,:param125,:param126,:param127,:param128)", con);
                command.CommandType = CommandType.StoredProcedure;
                for (var i = 0; i < 128; i++)
                {
                    var paramValue = 128 - i;
                    var paramName = "param" + (i + 1).ToString();

                    command.Parameters.Add(new EDBParameter(paramName, EDBTypes.EDBDbType.Numeric, 10, paramName, ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, paramValue));
                }
                command.Parameters.Add(new EDBParameter("param129", EDBTypes.EDBDbType.Numeric, 10, "param129", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 121));

                command.Prepare();
                command.ExecuteNonQuery();
                for (var i = 0; i < 128; i++)
                    Assert.AreEqual((i + 1).ToString(), command.Parameters[i].Value.ToString());
                Assert.AreEqual("203", command.Parameters[128].Value.ToString());

            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

        }

        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument CHAR type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
        [Test]
        public void testMaxParametersSupportInFunctionWithNumericAsInAndOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;


            var strSql = "CREATE OR REPLACE Function MaxFuncNumericInOUT(param1 out Numeric, param2 inout Numeric,param3 in Numeric ,param4 out Numeric, param5 inout Numeric,param6 in Numeric,param7 out Numeric, param8 inout Numeric,param9 in Numeric,param10 out Numeric, param11 inout Numeric,param12 in Numeric,param13 out Numeric, param14 inout Numeric,param15 in Numeric,param16 out Numeric, param17 inout Numeric,param18 in Numeric,param19 out Numeric, param20 inout Numeric,param21 in Numeric,param22 out Numeric, param23 inout Numeric,param24 in Numeric,param25 out Numeric, param26 inout Numeric,param27 in Numeric,param28 out Numeric, param29 inout Numeric,param30 in Numeric,param31 out Numeric, param32 inout Numeric,param33 in Numeric,param34 out Numeric, param35 inout Numeric,param36 in Numeric,param37 out Numeric, param38 inout Numeric,param39 in Numeric,param40 out Numeric, param41 inout Numeric,param42 in Numeric,param43 out Numeric, param44 inout Numeric,param45 in Numeric,param46 out Numeric, param47 inout Numeric,param48 in Numeric,param49 out Numeric, param50 inout Numeric,param51 in Numeric,param52 out Numeric, param53 inout Numeric,param54 in Numeric,param55 out Numeric, param56 inout Numeric,param57 in Numeric,param58 out Numeric, param59 inout Numeric,param60 in Numeric,param61 out Numeric, param62 inout Numeric,param63 in Numeric,param64 out Numeric, param65 inout Numeric,param66 in Numeric,param67 out Numeric, param68 inout Numeric,param69 in Numeric,param70 out Numeric, param71 inout Numeric,param72 in Numeric,param73 out Numeric, param74 inout Numeric,param75 in Numeric,param76 out Numeric, param77 inout Numeric,param78 in Numeric,param79 out Numeric, param80 inout Numeric,param81 in Numeric,param82 out Numeric, param83 inout Numeric,param84 in Numeric,param85 out Numeric, param86 inout Numeric,param87 in Numeric,param88 out Numeric, param89 inout Numeric,param90 in Numeric,param91 out Numeric, param92 inout Numeric,param93 in Numeric,param94 out Numeric, param95 inout Numeric,param96 in Numeric"
                + " ,param97 out Numeric, param98 inout Numeric,param99 in Numeric,param100 out Numeric, param101 inout Numeric,param102 in Numeric,param103 out Numeric, param104 inout Numeric,param105 in Numeric,param106 out Numeric, param107 inout Numeric,param108 in Numeric,param109 out Numeric, param110 inout Numeric,param111 in Numeric,param112 out Numeric, param113 inout Numeric,param114 in Numeric,param115 out Numeric, param116 inout Numeric,param117 in Numeric,param118 out Numeric, param119 inout Numeric,param120 in Numeric,param121 out Numeric, param122 inout Numeric,param123 in Numeric,param124 out Numeric, param125 inout Numeric,param126 in Numeric,param127 out Numeric, param128 inout Numeric) return Numeric"
                + " IS \n"
                + " BEGIN \n"
                + "param1 := param2; param2 := param3; param4 := param5; param5 := param6; param7 := param8; param8 := param9; param10 := param11; param11 := param12; param13 := param14; param14 := param15; param16 := param17; param17 := param18; param19 := param20; param20 := param21; param22 := param23; param23 := param24; param25 := param26; param26 := param27; param28 := param29; param29 := param30; param31 := param32; param32 := param33; param34 := param35; param35 := param36; param37 := param38; param38 := param39; param40 := param41; param41 := param42; param43 := param44; param44 := param45; param46 := param47; param47 := param48; param49 := param50; param50 := param51; param52 := param53; param53 := param54; param55 := param56; param56 := param57; param58 := param59; param59 := param60; param61 := param62; param62 := param63; param64 := param65; param65 := param66; param67 := param68; param68 := param69; param70 := param71; param71 := param72; param73 := param74; param74 := param75; param76 := param77; param77 := param78; param79 := param80; param80 := param81; param82 := param83; param83 := param84; param85 := param86; param86 := param87; param88 := param89; param89 := param90; param91 := param92; param92 := param93; param94 := param95; param95 := param96; param97 := param98; param98 := param99; param100 := param101; param101 := param102; param103 := param104; param104 := param105; param106 := param107; param107 := param108; param109 := param110; param110 := param111; param112 := param113; param113 := param114; param115 := param116; param116 := param117; param118 := param119; param119 := param120; param121 := param122; param122 := param123; param124 := param125; param125 := param126; param127 := param128; param128 := 200; return 300; END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();


            //////////////code
            try
            {
                command = new EDBCommand("MaxFuncNumericInOUT(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10,:param11,:param12,:param13,:param14,:param15,:param16,:param17,:param18,:param19,:param20,:param21,:param22,:param23,:param24,:param25,:param26,:param27,:param28,:param29,:param30,:param31,:param32,:param33,:param34,:param35,:param36,:param37,:param38,:param39,:param40,:param41,:param42,:param43,:param44,:param45,:param46,:param47,:param48,:param49,:param50,:param51,:param52,:param53,:param54,:param55,:param56,:param57,:param58,:param59,:param60,:param61,:param62,:param63,:param64,:param65,:param66,:param67,:param68,:param69,:param70,:param71,:param72,:param73,:param74,:param75,:param76,:param77,:param78,:param79,:param80,:param81,:param82,:param83,:param84,:param85,:param86,:param87,:param88,:param89,:param90,:param91,:param92,:param93,:param94,:param95,:param96,:param97,:param98,:param99,:param100,:param101,:param102,:param103,:param104,:param105,:param106,:param107,:param108,:param109,:param110,:param111,:param112,:param113,:param114,:param115,:param116,:param117,:param118,:param119,:param120,:param121,:param122,:param123,:param124,:param125,:param126,:param127,:param128)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 10, "param1", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 10, "param2", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 10, "param3", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Numeric, 10, "param4", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Numeric, 10, "param5", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param6", EDBTypes.EDBDbType.Numeric, 10, "param6", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param7", EDBTypes.EDBDbType.Numeric, 10, "param7", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param8", EDBTypes.EDBDbType.Numeric, 10, "param8", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param9", EDBTypes.EDBDbType.Numeric, 10, "param9", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param10", EDBTypes.EDBDbType.Numeric, 10, "param10", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param11", EDBTypes.EDBDbType.Numeric, 10, "param11", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param12", EDBTypes.EDBDbType.Numeric, 10, "param12", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param13", EDBTypes.EDBDbType.Numeric, 10, "param13", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param14", EDBTypes.EDBDbType.Numeric, 10, "param14", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param15", EDBTypes.EDBDbType.Numeric, 10, "param15", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param16", EDBTypes.EDBDbType.Numeric, 10, "param16", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param17", EDBTypes.EDBDbType.Numeric, 10, "param17", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param18", EDBTypes.EDBDbType.Numeric, 10, "param18", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param19", EDBTypes.EDBDbType.Numeric, 10, "param19", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param20", EDBTypes.EDBDbType.Numeric, 10, "param20", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param21", EDBTypes.EDBDbType.Numeric, 10, "param21", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param22", EDBTypes.EDBDbType.Numeric, 10, "param22", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param23", EDBTypes.EDBDbType.Numeric, 10, "param23", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param24", EDBTypes.EDBDbType.Numeric, 10, "param24", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param25", EDBTypes.EDBDbType.Numeric, 10, "param25", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param26", EDBTypes.EDBDbType.Numeric, 10, "param26", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param27", EDBTypes.EDBDbType.Numeric, 10, "param27", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param28", EDBTypes.EDBDbType.Numeric, 10, "param28", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param29", EDBTypes.EDBDbType.Numeric, 10, "param29", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param30", EDBTypes.EDBDbType.Numeric, 10, "param30", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param31", EDBTypes.EDBDbType.Numeric, 10, "param31", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param32", EDBTypes.EDBDbType.Numeric, 10, "param32", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param33", EDBTypes.EDBDbType.Numeric, 10, "param33", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param34", EDBTypes.EDBDbType.Numeric, 10, "param34", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param35", EDBTypes.EDBDbType.Numeric, 10, "param35", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param36", EDBTypes.EDBDbType.Numeric, 10, "param36", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param37", EDBTypes.EDBDbType.Numeric, 10, "param37", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param38", EDBTypes.EDBDbType.Numeric, 10, "param38", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param39", EDBTypes.EDBDbType.Numeric, 10, "param39", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param40", EDBTypes.EDBDbType.Numeric, 10, "param40", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param41", EDBTypes.EDBDbType.Numeric, 10, "param41", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param42", EDBTypes.EDBDbType.Numeric, 10, "param42", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param43", EDBTypes.EDBDbType.Numeric, 10, "param43", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param44", EDBTypes.EDBDbType.Numeric, 10, "param44", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param45", EDBTypes.EDBDbType.Numeric, 10, "param45", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param46", EDBTypes.EDBDbType.Numeric, 10, "param46", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param47", EDBTypes.EDBDbType.Numeric, 10, "param47", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param48", EDBTypes.EDBDbType.Numeric, 10, "param48", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param49", EDBTypes.EDBDbType.Numeric, 10, "param49", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param50", EDBTypes.EDBDbType.Numeric, 10, "param50", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param51", EDBTypes.EDBDbType.Numeric, 10, "param51", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param52", EDBTypes.EDBDbType.Numeric, 10, "param52", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param53", EDBTypes.EDBDbType.Numeric, 10, "param53", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param54", EDBTypes.EDBDbType.Numeric, 10, "param54", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param55", EDBTypes.EDBDbType.Numeric, 10, "param55", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param56", EDBTypes.EDBDbType.Numeric, 10, "param56", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param57", EDBTypes.EDBDbType.Numeric, 10, "param57", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param58", EDBTypes.EDBDbType.Numeric, 10, "param58", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param59", EDBTypes.EDBDbType.Numeric, 10, "param59", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param60", EDBTypes.EDBDbType.Numeric, 10, "param60", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param61", EDBTypes.EDBDbType.Numeric, 10, "param61", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param62", EDBTypes.EDBDbType.Numeric, 10, "param62", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param63", EDBTypes.EDBDbType.Numeric, 10, "param63", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param64", EDBTypes.EDBDbType.Numeric, 10, "param64", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param65", EDBTypes.EDBDbType.Numeric, 10, "param65", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param66", EDBTypes.EDBDbType.Numeric, 10, "param66", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param67", EDBTypes.EDBDbType.Numeric, 10, "param67", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param68", EDBTypes.EDBDbType.Numeric, 10, "param68", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param69", EDBTypes.EDBDbType.Numeric, 10, "param69", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param70", EDBTypes.EDBDbType.Numeric, 10, "param70", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param71", EDBTypes.EDBDbType.Numeric, 10, "param71", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param72", EDBTypes.EDBDbType.Numeric, 10, "param72", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param73", EDBTypes.EDBDbType.Numeric, 10, "param73", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param74", EDBTypes.EDBDbType.Numeric, 10, "param74", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param75", EDBTypes.EDBDbType.Numeric, 10, "param75", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param76", EDBTypes.EDBDbType.Numeric, 10, "param76", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param77", EDBTypes.EDBDbType.Numeric, 10, "param77", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param78", EDBTypes.EDBDbType.Numeric, 10, "param78", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param79", EDBTypes.EDBDbType.Numeric, 10, "param79", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param80", EDBTypes.EDBDbType.Numeric, 10, "param80", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param81", EDBTypes.EDBDbType.Numeric, 10, "param81", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param82", EDBTypes.EDBDbType.Numeric, 10, "param82", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param83", EDBTypes.EDBDbType.Numeric, 10, "param83", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param84", EDBTypes.EDBDbType.Numeric, 10, "param84", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param85", EDBTypes.EDBDbType.Numeric, 10, "param85", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param86", EDBTypes.EDBDbType.Numeric, 10, "param86", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param87", EDBTypes.EDBDbType.Numeric, 10, "param87", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param88", EDBTypes.EDBDbType.Numeric, 10, "param88", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param89", EDBTypes.EDBDbType.Numeric, 10, "param89", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param90", EDBTypes.EDBDbType.Numeric, 10, "param90", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param91", EDBTypes.EDBDbType.Numeric, 10, "param91", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param92", EDBTypes.EDBDbType.Numeric, 10, "param92", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param93", EDBTypes.EDBDbType.Numeric, 10, "param93", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param94", EDBTypes.EDBDbType.Numeric, 10, "param94", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param95", EDBTypes.EDBDbType.Numeric, 10, "param95", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param96", EDBTypes.EDBDbType.Numeric, 10, "param96", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param97", EDBTypes.EDBDbType.Numeric, 10, "param97", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param98", EDBTypes.EDBDbType.Numeric, 10, "param98", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param99", EDBTypes.EDBDbType.Numeric, 10, "param99", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param100", EDBTypes.EDBDbType.Numeric, 10, "param100", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param101", EDBTypes.EDBDbType.Numeric, 10, "param101", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param102", EDBTypes.EDBDbType.Numeric, 10, "param102", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param103", EDBTypes.EDBDbType.Numeric, 10, "param103", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param104", EDBTypes.EDBDbType.Numeric, 10, "param104", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param105", EDBTypes.EDBDbType.Numeric, 10, "param105", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param106", EDBTypes.EDBDbType.Numeric, 10, "param106", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param107", EDBTypes.EDBDbType.Numeric, 10, "param107", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param108", EDBTypes.EDBDbType.Numeric, 10, "param108", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param109", EDBTypes.EDBDbType.Numeric, 10, "param109", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param110", EDBTypes.EDBDbType.Numeric, 10, "param110", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param111", EDBTypes.EDBDbType.Numeric, 10, "param111", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param112", EDBTypes.EDBDbType.Numeric, 10, "param112", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param113", EDBTypes.EDBDbType.Numeric, 10, "param113", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param114", EDBTypes.EDBDbType.Numeric, 10, "param114", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param115", EDBTypes.EDBDbType.Numeric, 10, "param115", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param116", EDBTypes.EDBDbType.Numeric, 10, "param116", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param117", EDBTypes.EDBDbType.Numeric, 10, "param117", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param118", EDBTypes.EDBDbType.Numeric, 10, "param118", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param119", EDBTypes.EDBDbType.Numeric, 10, "param119", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 120));
                command.Parameters.Add(new EDBParameter("param120", EDBTypes.EDBDbType.Numeric, 10, "param120", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 119));

                command.Parameters.Add(new EDBParameter("param121", EDBTypes.EDBDbType.Numeric, 10, "param121", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 128));
                command.Parameters.Add(new EDBParameter("param122", EDBTypes.EDBDbType.Numeric, 10, "param122", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 127));
                command.Parameters.Add(new EDBParameter("param123", EDBTypes.EDBDbType.Numeric, 10, "param123", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 126));
                command.Parameters.Add(new EDBParameter("param124", EDBTypes.EDBDbType.Numeric, 10, "param124", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 125));
                command.Parameters.Add(new EDBParameter("param125", EDBTypes.EDBDbType.Numeric, 10, "param125", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 124));
                command.Parameters.Add(new EDBParameter("param126", EDBTypes.EDBDbType.Numeric, 10, "param126", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 123));
                command.Parameters.Add(new EDBParameter("param127", EDBTypes.EDBDbType.Numeric, 10, "param127", ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, 122));
                command.Parameters.Add(new EDBParameter("param128", EDBTypes.EDBDbType.Numeric, 10, "param128", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 121));
                command.Parameters.Add(new EDBParameter("param129", EDBTypes.EDBDbType.Numeric, 10, "param129", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 121));

                command.Prepare();



                command.ExecuteNonQuery();

                Assert.AreEqual("127", command.Parameters[0].Value.ToString());
                Assert.AreEqual("126", command.Parameters[1].Value.ToString());
                Assert.AreEqual("126", command.Parameters[2].Value.ToString());
                Assert.AreEqual("124", command.Parameters[3].Value.ToString());
                Assert.AreEqual("123", command.Parameters[4].Value.ToString());
                Assert.AreEqual("123", command.Parameters[5].Value.ToString());
                Assert.AreEqual("121", command.Parameters[6].Value.ToString());
                Assert.AreEqual("120", command.Parameters[7].Value.ToString());
                Assert.AreEqual("120", command.Parameters[8].Value.ToString());
                Assert.AreEqual("128", command.Parameters[9].Value.ToString());

                Assert.AreEqual("127", command.Parameters[10].Value.ToString());
                Assert.AreEqual("127", command.Parameters[11].Value.ToString());
                Assert.AreEqual("125", command.Parameters[12].Value.ToString());
                Assert.AreEqual("124", command.Parameters[13].Value.ToString());
                Assert.AreEqual("124", command.Parameters[14].Value.ToString());
                Assert.AreEqual("122", command.Parameters[15].Value.ToString());
                Assert.AreEqual("121", command.Parameters[16].Value.ToString());
                Assert.AreEqual("121", command.Parameters[17].Value.ToString());
                Assert.AreEqual("119", command.Parameters[18].Value.ToString());
                Assert.AreEqual("128", command.Parameters[19].Value.ToString());

                Assert.AreEqual("128", command.Parameters[20].Value.ToString());
                Assert.AreEqual("126", command.Parameters[21].Value.ToString());
                Assert.AreEqual("125", command.Parameters[22].Value.ToString());
                Assert.AreEqual("125", command.Parameters[23].Value.ToString());
                Assert.AreEqual("123", command.Parameters[24].Value.ToString());
                Assert.AreEqual("122", command.Parameters[25].Value.ToString());
                Assert.AreEqual("122", command.Parameters[26].Value.ToString());
                Assert.AreEqual("120", command.Parameters[27].Value.ToString());
                Assert.AreEqual("119", command.Parameters[28].Value.ToString());
                Assert.AreEqual("119", command.Parameters[29].Value.ToString());

                Assert.AreEqual("127", command.Parameters[30].Value.ToString());
                Assert.AreEqual("126", command.Parameters[31].Value.ToString());
                Assert.AreEqual("126", command.Parameters[32].Value.ToString());
                Assert.AreEqual("124", command.Parameters[33].Value.ToString());
                Assert.AreEqual("123", command.Parameters[34].Value.ToString());
                Assert.AreEqual("123", command.Parameters[35].Value.ToString());
                Assert.AreEqual("121", command.Parameters[36].Value.ToString());
                Assert.AreEqual("120", command.Parameters[37].Value.ToString());
                Assert.AreEqual("120", command.Parameters[38].Value.ToString());
                Assert.AreEqual("128", command.Parameters[39].Value.ToString());

                Assert.AreEqual("127", command.Parameters[40].Value.ToString());
                Assert.AreEqual("127", command.Parameters[41].Value.ToString());
                Assert.AreEqual("125", command.Parameters[42].Value.ToString());
                Assert.AreEqual("124", command.Parameters[43].Value.ToString());
                Assert.AreEqual("124", command.Parameters[44].Value.ToString());
                Assert.AreEqual("122", command.Parameters[45].Value.ToString());
                Assert.AreEqual("121", command.Parameters[46].Value.ToString());
                Assert.AreEqual("121", command.Parameters[47].Value.ToString());
                Assert.AreEqual("119", command.Parameters[48].Value.ToString());
                Assert.AreEqual("128", command.Parameters[49].Value.ToString());

                Assert.AreEqual("128", command.Parameters[50].Value.ToString());
                Assert.AreEqual("126", command.Parameters[51].Value.ToString());
                Assert.AreEqual("125", command.Parameters[52].Value.ToString());
                Assert.AreEqual("125", command.Parameters[53].Value.ToString());
                Assert.AreEqual("123", command.Parameters[54].Value.ToString());
                Assert.AreEqual("122", command.Parameters[55].Value.ToString());
                Assert.AreEqual("122", command.Parameters[56].Value.ToString());
                Assert.AreEqual("120", command.Parameters[57].Value.ToString());
                Assert.AreEqual("119", command.Parameters[58].Value.ToString());
                Assert.AreEqual("119", command.Parameters[59].Value.ToString());

                Assert.AreEqual("127", command.Parameters[60].Value.ToString());
                Assert.AreEqual("126", command.Parameters[61].Value.ToString());
                Assert.AreEqual("126", command.Parameters[62].Value.ToString());
                Assert.AreEqual("124", command.Parameters[63].Value.ToString());
                Assert.AreEqual("123", command.Parameters[64].Value.ToString());
                Assert.AreEqual("123", command.Parameters[65].Value.ToString());
                Assert.AreEqual("121", command.Parameters[66].Value.ToString());
                Assert.AreEqual("120", command.Parameters[67].Value.ToString());
                Assert.AreEqual("120", command.Parameters[68].Value.ToString());
                Assert.AreEqual("128", command.Parameters[69].Value.ToString());

                Assert.AreEqual("127", command.Parameters[70].Value.ToString());
                Assert.AreEqual("127", command.Parameters[71].Value.ToString());
                Assert.AreEqual("125", command.Parameters[72].Value.ToString());
                Assert.AreEqual("124", command.Parameters[73].Value.ToString());
                Assert.AreEqual("124", command.Parameters[74].Value.ToString());
                Assert.AreEqual("122", command.Parameters[75].Value.ToString());
                Assert.AreEqual("121", command.Parameters[76].Value.ToString());
                Assert.AreEqual("121", command.Parameters[77].Value.ToString());
                Assert.AreEqual("119", command.Parameters[78].Value.ToString());
                Assert.AreEqual("128", command.Parameters[79].Value.ToString());

                Assert.AreEqual("128", command.Parameters[80].Value.ToString());
                Assert.AreEqual("126", command.Parameters[81].Value.ToString());
                Assert.AreEqual("125", command.Parameters[82].Value.ToString());
                Assert.AreEqual("125", command.Parameters[83].Value.ToString());
                Assert.AreEqual("123", command.Parameters[84].Value.ToString());
                Assert.AreEqual("122", command.Parameters[85].Value.ToString());
                Assert.AreEqual("122", command.Parameters[86].Value.ToString());
                Assert.AreEqual("120", command.Parameters[87].Value.ToString());
                Assert.AreEqual("119", command.Parameters[88].Value.ToString());
                Assert.AreEqual("119", command.Parameters[89].Value.ToString());

                Assert.AreEqual("127", command.Parameters[90].Value.ToString());
                Assert.AreEqual("126", command.Parameters[91].Value.ToString());
                Assert.AreEqual("126", command.Parameters[92].Value.ToString());
                Assert.AreEqual("124", command.Parameters[93].Value.ToString());
                Assert.AreEqual("123", command.Parameters[94].Value.ToString());
                Assert.AreEqual("123", command.Parameters[95].Value.ToString());
                Assert.AreEqual("121", command.Parameters[96].Value.ToString());
                Assert.AreEqual("120", command.Parameters[97].Value.ToString());
                Assert.AreEqual("120", command.Parameters[98].Value.ToString());
                Assert.AreEqual("128", command.Parameters[99].Value.ToString());

                Assert.AreEqual("127", command.Parameters[100].Value.ToString());
                Assert.AreEqual("127", command.Parameters[101].Value.ToString());
                Assert.AreEqual("125", command.Parameters[102].Value.ToString());
                Assert.AreEqual("124", command.Parameters[103].Value.ToString());
                Assert.AreEqual("124", command.Parameters[104].Value.ToString());
                Assert.AreEqual("122", command.Parameters[105].Value.ToString());
                Assert.AreEqual("121", command.Parameters[106].Value.ToString());
                Assert.AreEqual("121", command.Parameters[107].Value.ToString());
                Assert.AreEqual("119", command.Parameters[108].Value.ToString());
                Assert.AreEqual("128", command.Parameters[109].Value.ToString());

                Assert.AreEqual("128", command.Parameters[110].Value.ToString());
                Assert.AreEqual("126", command.Parameters[111].Value.ToString());
                Assert.AreEqual("125", command.Parameters[112].Value.ToString());
                Assert.AreEqual("125", command.Parameters[113].Value.ToString());
                Assert.AreEqual("123", command.Parameters[114].Value.ToString());
                Assert.AreEqual("122", command.Parameters[115].Value.ToString());
                Assert.AreEqual("122", command.Parameters[116].Value.ToString());
                Assert.AreEqual("120", command.Parameters[117].Value.ToString());
                Assert.AreEqual("119", command.Parameters[118].Value.ToString());
                Assert.AreEqual("119", command.Parameters[119].Value.ToString());

                Assert.AreEqual("127", command.Parameters[120].Value.ToString());
                Assert.AreEqual("126", command.Parameters[121].Value.ToString());
                Assert.AreEqual("126", command.Parameters[122].Value.ToString());
                Assert.AreEqual("124", command.Parameters[123].Value.ToString());
                Assert.AreEqual("123", command.Parameters[124].Value.ToString());
                Assert.AreEqual("123", command.Parameters[125].Value.ToString());
                Assert.AreEqual("121", command.Parameters[126].Value.ToString());
                Assert.AreEqual("200", command.Parameters[127].Value.ToString());
                Assert.AreEqual("300", command.Parameters[128].Value.ToString());

            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }
        }

        /*
		To verify that maximum 128 OUT parameters are supported in .NET Connector.
*/
        //	[Test]
        public void testMaxParametersSupportInFunctionsWithTextAsOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;


            var strSql = "CREATE OR REPLACE FUNCTION MaxFuncText(param1 out Text, param2 out Text,param3 out Text ,param4 out Text, param5 out Text,param6 out Text,param7 out Text, param8 out Text,param9 out Text,param10 out Text, param11 out Text,param12 out Text,param13 out Text, param14 out Text,param15 out Text,param16 out Text, param17 out Text,param18 out Text,param19 out Text, param20 out Text,param21 out Text,param22 out Text, param23 out Text,param24 out Text,param25 out Text, param26 out Text,param27 out Text,param28 out Text, param29 out Text,param30 out Text,param31 out Text, param32 out Text,param33 out Text,param34 out Text, param35 out Text,param36 out Text,param37 out Text, param38 out Text,param39 out Text,param40 out Text, param41 out Text,param42 out Text,param43 out Text, param44 out Text,param45 out Text,param46 out Text, param47 out Text,param48 out Text,param49 out Text, param50 out Text,param51 out Text,param52 out Text, param53 out Text,param54 out Text,param55 out Text, param56 out Text,param57 out Text,param58 out Text, param59 out Text,param60 out Text,param61 out Text, param62 out Text,param63 out Text,param64 out Text, param65 out Text,param66 out Text,param67 out Text, param68 out Text,param69 out Text,param70 out Text, param71 out Text,param72 out Text,param73 out Text, param74 out Text,param75 out Text,param76 out Text, param77 out Text,param78 out Text,param79 out Text, param80 out Text,param81 out Text,param82 out Text, param83 out Text,param84 out Text,param85 out Text, param86 out Text,param87 out Text,param88 out Text, param89 out Text,param90 out Text,param91 out Text, param92 out Text,param93 out Text,param94 out Text, param95 out Text,param96 out Text,param97 out Text,"
                + " param98 out Text,param99 out Text,param100 out Text, param101 out Text,param102 out Text,param103 out Text, param104 out Text,param105 out Text,param106 out Text, param107 out Text,param108 out Text,param109 out Text, param110 out Text,param111 out Text,param112 out Text, param113 out Text,param114 out Text,param115 out Text, param116 out Text,param117 out Text,param118 out Text, param119 out Text,param120 out Text,param121 out Text, param122 out Text,param123 out Text,param124 out Text, param125 out Text,param126 out Text,param127 out Text, param128 out Text) return Text"
                + " IS \n"
                + " BEGIN \n"
                + "param1 := '1'; param2 := '2'; param3 := '3'; param4 := '4'; param5 := '5'; param6 := '6'; param7 := '7'; param8 := '8'; param9 := '9'; param10 := '10'; param11 := '11'; param12 := '12'; param13 := '13'; param14 := '14'; param15 := '15'; param16 := '16'; param17 := '17'; param18 := '18'; param19 := '19'; param20 := '20'; param21 := '21'; param22 := '22'; param23 := '23'; param24 := '24'; param25 := '25'; param26 := '26'; param27 := '27'; param28 := '28'; param29 := '29'; param30 := '30'; param31 := '31'; param32 := '32'; param33 := '33'; param34 := '34'; param35 := '35'; param36 := '36'; param37 := '37'; param38 := '38'; param39 := '39'; param40 := '40'; param41 := '41'; param42 := '42'; param43 := '43'; param44 := '44'; param45 := '45'; param46 := '46'; param47 := '47'; param48 := '48'; param49 := '49'; param50 := '50'; param51 := '51'; param52 := '52'; param53 := '53'; param54 := '54'; param55 := '55'; param56 := '56'; param57 := '57'; param58 := '58'; param59 := '59'; param60 := '60'; param61 := '61'; param62 := '62'; param63 := '63'; param64 := '64'; param65 := '65'; param66 := '66'; param67 := '67'; param68 := '68'; param69 := '69'; param70 := '70'; param71 := '71'; param72 := '72'; param73 := '73'; param74 := '74'; param75 := '75'; param76 := '76'; param77 := '77'; param78 := '78'; param79 := '79'; param80 := '80'; param81 := '81'; param82 := '82'; param83 := '83'; param84 := '84'; param85 := '85'; param86 := '86'; param87 := '87'; param88 := '88'; param89 := '89'; param90 := '90'; param91 := '91'; param92 := '92'; param93 := '93'; param94 := '94'; param95 := '95'; param96 := '96'; param97 := '97'; param98 := '98'; param99 := '99'; param100 := '100'; param101 := '101'; param102 := '102'; param103 := '103'; param104 := '104'; param105 := '105'; param106 := '106'; param107 := '107'; param108 := '108'; param109 := '109'; param110 := '110'; param111 := '111'; param112 := '112'; param113 := '113'; param114 := '114'; param115 := '115'; param116 := '116'; "
                + "param117 := '117'; param118 := '118'; param119 := '119'; param120 := '120'; param121 := '121'; param122 := '122'; param123 := '123'; param124 := '124'; param125 := '125'; param126 := '126'; param127 := '127'; param128 := '128'; return 'Hashim'; END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();


            //////////////code
            try
            {
                command = new EDBCommand("MaxFuncText(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10,:param11,:param12,:param13,:param14,:param15,:param16,:param17,:param18,:param19,:param20,:param21,:param22,:param23,:param24,:param25,:param26,:param27,:param28,:param29,:param30,:param31,:param32,:param33,:param34,:param35,:param36,:param37,:param38,:param39,:param40,:param41,:param42,:param43,:param44,:param45,:param46,:param47,:param48,:param49,:param50,:param51,:param52,:param53,:param54,:param55,:param56,:param57,:param58,:param59,:param60,:param61,:param62,:param63,:param64,:param65,:param66,:param67,:param68,:param69,:param70,:param71,:param72,:param73,:param74,:param75,:param76,:param77,:param78,:param79,:param80,:param81,:param82,:param83,:param84,:param85,:param86,:param87,:param88,:param89,:param90,:param91,:param92,:param93,:param94,:param95,:param96,:param97,:param98,:param99,:param100,:param101,:param102,:param103,:param104,:param105,:param106,:param107,:param108,:param109,:param110,:param111,:param112,:param113,:param114,:param115,:param116,:param117,:param118,:param119,:param120,:param121,:param122,:param123,:param124,:param125,:param126,:param127,:param128)", con);
                command.CommandType = CommandType.StoredProcedure;
                for (var i = 0; i < 128; i++)
                {
                    var paramValue = 128 - i;
                    var paramName = "param" + (i + 1).ToString();
                    command.Parameters.Add(new EDBParameter(paramName, EDBTypes.EDBDbType.Text, 10, paramName, ParameterDirection.Output, false, 2, 2, DataRowVersion.Current, paramValue));
                }
                command.Parameters.Add(new EDBParameter("param129", EDBTypes.EDBDbType.Text, 10, "param129", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, 121));

                command.Prepare();
                command.ExecuteNonQuery();

                for (var i = 0; i < 128; i++)
                    Assert.AreEqual((i + 1).ToString(), command.Parameters[i].Value.ToString());
                Assert.AreEqual("Hashim", command.Parameters[128].Value.ToString());

            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        /// ////////////////////////Calling a procedure within a package with argument CHAR type
        /// ////////////////////////and with Parameter types IN, INOUT, OUT
        /// ////////////////////////DB feature used = Procedure
        /// </summary>
		[Test, Ignore("Umar: Temp")]
        public void testMaxParametersSupportInFunctionWithTextAsInAndOut()
        {
            using var con = OpenConnection();
            //////prereq
            var command = new EDBCommand("", con);
            command.CommandType = CommandType.Text;


            var strSql = "CREATE OR REPLACE FUNCTION MaxFuncTextInOut(param1 out Text, param2 inout Text,param3 in Text ,param4 out Text, param5 inout Text,param6 in Text,param7 out Text, param8 inout Text,param9 in Text,param10 out Text, param11 inout Text,param12 in Text,param13 out Text, param14 inout Text,param15 in Text,param16 out Text, param17 inout Text,param18 in Text,param19 out Text, param20 inout Text,param21 in Text,param22 out Text, param23 inout Text,param24 in Text,param25 out Text, param26 inout Text,param27 in Text,param28 out Text, param29 inout Text,param30 in Text,param31 out Text, param32 inout Text,param33 in Text,param34 out Text, param35 inout Text,param36 in Text,param37 out Text, param38 inout Text,param39 in Text,param40 out Text, param41 inout Text,param42 in Text,param43 out Text, param44 inout Text,param45 in Text,param46 out Text, param47 inout Text,param48 in Text,param49 out Text, param50 inout Text,param51 in Text,param52 out Text, param53 inout Text,param54 in Text,param55 out Text, param56 inout Text,param57 in Text,param58 out Text, param59 inout Text,param60 in Text,param61 out Text, param62 inout Text,param63 in Text,param64 out Text, param65 inout Text,param66 in Text,param67 out Text, param68 inout Text,param69 in Text,param70 out Text, param71 inout Text,param72 in Text,param73 out Text, param74 inout Text,param75 in Text,param76 out Text, param77 inout Text,param78 in Text,param79 out Text, param80 inout Text,param81 in Text,param82 out Text, param83 inout Text,param84 in Text,param85 out Text, param86 inout Text,param87 in Text,param88 out Text, param89 inout Text,param90 in Text,param91 out Text, param92 inout Text,param93 in Text,param94 out Text, param95 inout Text,param96 in Text"
                + " ,param97 out Text, param98 inout Text,param99 in Text,param100 out Text, param101 inout Text,param102 in Text,param103 out Text, param104 inout Text,param105 in Text,param106 out Text, param107 inout Text,param108 in Text,param109 out Text, param110 inout Text,param111 in Text,param112 out Text, param113 inout Text,param114 in Text,param115 out Text, param116 inout Text,param117 in Text,param118 out Text, param119 inout Text,param120 in Text,param121 out Text, param122 inout Text,param123 in Text,param124 out Text, param125 inout Text,param126 in Text,param127 out Text, param128 inout Text) return Text"
                + " IS \n"
                + " BEGIN \n"
                + "param1 := param2; param2 := param3; param4 := param5; param5 := param6; param7 := param8; param8 := param9; param10 := param11; param11 := param12; param13 := param14; param14 := param15; param16 := param17; param17 := param18; param19 := param20; param20 := param21; param22 := param23; param23 := param24; param25 := param26; param26 := param27; param28 := param29; param29 := param30; param31 := param32; param32 := param33; param34 := param35; param35 := param36; param37 := param38; param38 := param39; param40 := param41; param41 := param42; param43 := param44; param44 := param45; param46 := param47; param47 := param48; param49 := param50; param50 := param51; param52 := param53; param53 := param54; param55 := param56; param56 := param57; param58 := param59; param59 := param60; param61 := param62; param62 := param63; param64 := param65; param65 := param66; param67 := param68; param68 := param69; param70 := param71; param71 := param72; param73 := param74; param74 := param75; param76 := param77; param77 := param78; param79 := param80; param80 := param81; param82 := param83; param83 := param84; param85 := param86; param86 := param87; param88 := param89; param89 := param90; param91 := param92; param92 := param93; param94 := param95; param95 := param96; param97 := param98; param98 := param99; param100 := param101; param101 := param102; param103 := param104; param104 := param105; param106 := param107; param107 := param108; param109 := param110; param110 := param111; param112 := param113; param113 := param114; param115 := param116; param116 := param117; param118 := param119; param119 := param120; param121 := param122; param122 := param123; param124 := param125; param125 := param126; param127 := param128; param128 := 'Hashim'; return 'Ran Away'; END;";
            command.CommandText = strSql;
            command.ExecuteNonQuery();


            //////////////code
            try
            {
                command = new EDBCommand("MaxFuncTextInOut(:param1,:param2,:param3,:param4,:param5,:param6,:param7,:param8,:param9,:param10,:param11,:param12,:param13,:param14,:param15,:param16,:param17,:param18,:param19,:param20,:param21,:param22,:param23,:param24,:param25,:param26,:param27,:param28,:param29,:param30,:param31,:param32,:param33,:param34,:param35,:param36,:param37,:param38,:param39,:param40,:param41,:param42,:param43,:param44,:param45,:param46,:param47,:param48,:param49,:param50,:param51,:param52,:param53,:param54,:param55,:param56,:param57,:param58,:param59,:param60,:param61,:param62,:param63,:param64,:param65,:param66,:param67,:param68,:param69,:param70,:param71,:param72,:param73,:param74,:param75,:param76,:param77,:param78,:param79,:param80,:param81,:param82,:param83,:param84,:param85,:param86,:param87,:param88,:param89,:param90,:param91,:param92,:param93,:param94,:param95,:param96,:param97,:param98,:param99,:param100,:param101,:param102,:param103,:param104,:param105,:param106,:param107,:param108,:param109,:param110,:param111,:param112,:param113,:param114,:param115,:param116,:param117,:param118,:param119,:param120,:param121,:param122,:param123,:param124,:param125,:param126,:param127,:param128)", con);
                command.CommandType = CommandType.StoredProcedure;
                for (var i = 0; i < 128; i++)
                {
                    var paramValue = i.ToString();
                    var paramName = "param" + (i + 1).ToString();
                    ParameterDirection direction = ParameterDirection.Output;
                    if (i % 3 == 2)
                        direction = ParameterDirection.InputOutput;
                    if (i % 3 == 2)
                        direction = ParameterDirection.Input;
                    command.Parameters.Add(new EDBParameter(paramName, EDBTypes.EDBDbType.Text, 10, paramName, direction, false, 2, 2, DataRowVersion.Current, paramValue));
                }
                command.Parameters.Add(new EDBParameter("param128", EDBTypes.EDBDbType.Text, 10, "param128", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, ""));

                command.Prepare();
                command.ExecuteNonQuery();
                for (var i = 0; i < 127; i++)
                {
                    var expectedValue = (i + 1).ToString();
                    if (i % 3 == 2)
                        expectedValue = (i).ToString();
                    Assert.AreEqual(expectedValue, command.Parameters[i].Value.ToString());
                }
                Assert.AreEqual("Hashim", command.Parameters[127].Value.ToString());
                Assert.AreEqual("Ran Away", command.Parameters[128].Value.ToString());

            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

        }

        /* To verify the sanity of IN, INOUT and OUT parameters in functions with Date datatype */
        /*		[Test]
                public void testFunctionWithDateAsInInoutOut()
                {
                    //////prereq
                    var command = new EDBCommand("",con);
                    command.CommandType = CommandType.Text;

                    var strSql ="CREATE OR REPLACE FUNCTION FunctionWithDate(p_in in Date,p_inout inout Date,p_out out Date) return Date  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
                    command.CommandText = strSql;
                    command.ExecuteNonQuery();

                    DateTime v_in=DateTime.Parse("Dec 30, 1200 12:00:00 PM").ToUniversalTime();
                    DateTime v_inout=DateTime.Parse("Jan 31, 2006 10:01:50 PM").ToUniversalTime();
                    DateTime v_out=DateTime.Parse("Sep 06, 1100 11:50:25 PM").ToUniversalTime();
                    DateTime v_ret=DateTime.Parse("Sep 21, 2008 10:58:20 PM").ToUniversalTime();

                    //////////////code
                    try
                    {
                        command = new EDBCommand("FunctionWithDate(:v_in,:v_inout,:v_out)",con);
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Date,100,"v_in",ParameterDirection.Input,false, 12, 12,DataRowVersion.Current,v_in));
                        command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Date,100,"v_inout",ParameterDirection.InputOutput,false, 12, 12,DataRowVersion.Current,v_inout));
                        command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Date,100,"v_out",ParameterDirection.Output,false, 12, 12,DataRowVersion.Current,v_out));
                        command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Date,100,"v_ret",ParameterDirection.ReturnValue,false,12,12,System.Data.DataRowVersion.Current,v_ret)); 
                        command.Prepare();

                        command.ExecuteNonQuery();

                        Assert.AreEqual(1000,DateTime.Parse(command.Parameters[0].Value.ToString()));
                        Assert.AreEqual(1000,int.Parse(command.Parameters[1].Value.ToString()));	
                        Assert.AreEqual(20000,int.Parse(command.Parameters[2].Value.ToString()));	
                        Assert.AreEqual(1010,int.Parse(command.Parameters[3].Value.ToString()));	
                    }
                    catch(EDBException e)
                    {			
                        throw new Exception(e.ToString());
                    }

                    //////////tear down
                    command.Dispose();
                    command = new EDBCommand("",con);
                    command.CommandText = "DROP Function FunctionWithDate(Date,Date,Date);";
                    command.ExecuteNonQuery();

                }

                /* To verify the sanity of IN, INOUT and OUT parameters in functions with Binary datatype */
        /*		[Test]
                public void testFunctionWithBinaryAsInInoutOut()
                {
                    //////prereq
                    var command = new EDBCommand("",con);
                    command.CommandType = CommandType.Text;

                    var strSql ="CREATE OR REPLACE FUNCTION FunctionWithBinary(p_in in Binary,p_inout inout Binary,p_out out Binary) return Binary  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
                    command.CommandText = strSql;
                    command.ExecuteNonQuery();

                    //////////////code
                    try
                    {
                        command = new EDBCommand("FunctionWithBinary(:v_in,:v_inout,:v_out)",con);
                        command.CommandType = CommandType.StoredProcedure;

                        Byte a = Byte.Parse("4");
                        Byte b = Byte.Parse("3");
                        Byte c = Byte.Parse("2");
                        Byte d = Byte.Parse("1");

                        command.Parameters.Add(new EDBParameter("v_in",	EDBTypes.EDBDbType.Bytea,10,"v_in",ParameterDirection.Input,false, 12, 12,DataRowVersion.Current,a));
                        command.Parameters.Add(new EDBParameter("v_inout",	EDBTypes.EDBDbType.Bytea,10,"v_inout",ParameterDirection.InputOutput,false, 12, 12,DataRowVersion.Current,b));
                        command.Parameters.Add(new EDBParameter("v_out",	EDBTypes.EDBDbType.Bytea,10,"v_out",ParameterDirection.Output,false, 12, 12,DataRowVersion.Current,c));
                        command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Bytea,10,"v_ret",ParameterDirection.ReturnValue,false,12,12,System.Data.DataRowVersion.Current,d)); 
                        command.Prepare();

                        command.ExecuteNonQuery();
                        Console.WriteLine("Alpha");
                        Assert.AreEqual(1000,command.Parameters[0].Value);
                        Assert.AreEqual(1000,command.Parameters[1].Value);	
                        Assert.AreEqual(20000,command.Parameters[2].Value);	
                        Assert.AreEqual(1010,command.Parameters[3].Value);	
                    }
                    catch(EDBException e)
                    {			
                        Console.WriteLine(e.Message); 
                    }

                    //////////tear down
                    command.Dispose();
                    command = new EDBCommand("",con);
                    command.CommandText = "DROP Function FunctionWithBinary(Binary,Binary,Binary);";
                    command.ExecuteNonQuery();

                }
                */



        #region TERSE

        [Test, /*Ignore("Investigate")*/]

        public void TERSE_FUNC_NATIVE_INPUT_TYPES()
        {
            using var con = OpenConnection();
            try
            {
                EDBCommand command;
                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("BEGIN;", con);

                command.ExecuteNonQuery();
                command.Dispose();
                try
                {

                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);

                    command.ExecuteNonQuery();

                    command.Dispose();

                }
                catch (EDBException)
                {
                }

                command = new EDBCommand("public.FunconeInArg_test(:param1)", con);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));

                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

                command.Prepare();

                command.Parameters[0].Value = 3;

                EDBDataReader result = command.ExecuteReader();

                while (result.Read())
                { }

                Assert.AreEqual(3, int.Parse(command.Parameters[0].Value.ToString()));

                Assert.AreEqual("EnterpriseDB", command.Parameters[1].Value.ToString());

                result.Close();
                command.Dispose();

                command = new EDBCommand("END;", con);

                command.ExecuteNonQuery();

                command.Dispose();

            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }

        }

        [Test, /*Ignore("Temp")*/]
        public void TERSE_FUNC_NATIVE_OUTPUT_TYPES()
        {
            using var con = OpenConnection();
            try
            {

                EDBCommand Command;
                Command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("BEGIN;", con);
                Command.ExecuteNonQuery();
                Command.Dispose();

                try
                {
                    Command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    Command.ExecuteNonQuery();
                    Command.Dispose();
                }
                catch (EDBException)
                {
                }

                Command = new EDBCommand("create or replace function terse_f1( a out integer, b out integer ) return integer is " +
                                         "begin " +
                                         "  a := 10; " +
                                         "  b := 20; " +
                                         "  return 30; " +
                                         "end; ", con);

                Command.ExecuteNonQuery();
                Command.Dispose();

                Command = new EDBCommand("terse_f1(:a,:b)", con);
                Command.CommandType = CommandType.StoredProcedure;

                Command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Integer));
                Command.Parameters[0].Direction = ParameterDirection.Output;
                Command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Integer));
                Command.Parameters[1].Direction = ParameterDirection.Output;
                Command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Integer, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                Command.Prepare();

                Command.ExecuteNonQuery();
                Assert.AreEqual(10, int.Parse(Command.Parameters[0].Value.ToString()));
                Assert.AreEqual(20, int.Parse(Command.Parameters[1].Value.ToString()));
                // Assert.AreEqual(30, int.Parse(Command.Parameters[2].Value.ToString()));
                Command.Dispose();

                Command = new EDBCommand("END;", con);
                Command.ExecuteNonQuery();
                Command.Dispose();

            }
            catch (EDBException exp)
            {
                throw new Exception(exp.ToString());
            }

        }

        [Test]
        public void TERSE_FUNC_MIXED_NATIVE_TYPES()
        {
            using var con = OpenConnection();

            try
            {
                EDBCommand command;
                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("BEGIN;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (EDBException)
                {
                }

                command = new EDBCommand("public.functionsanity(:param1,:param2,:param3,:param4)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer, 10, "param3", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 10, "param5", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

                command.Prepare();
                command.Parameters[0].Value = 1;
                command.Parameters[1].Value = null;
                command.Parameters[2].Value = 3;
                command.Parameters[3].Value = null;

                EDBDataReader result = command.ExecuteReader();
                while (result.Read())
                { }

                Assert.AreEqual(100, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual(200, int.Parse(command.Parameters[1].Value.ToString()));
                Assert.AreEqual(3, int.Parse(command.Parameters[2].Value.ToString()));
                Assert.AreEqual(400, int.Parse(command.Parameters[3].Value.ToString()));
                Assert.AreEqual("EnterpriseDB", command.Parameters[4].Value.ToString());
                result.Close();

                command.Dispose();

                command = new EDBCommand("END;", con);
                command.ExecuteNonQuery();
                command.Dispose();

            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        [Test]
        public void TERSE_FUNC_CURSOR_TYPES()
        {
            try
            {
                using var con = OpenConnection();
                var com = new EDBCommand("", con);
                com.CommandType = CommandType.Text;

                var CursorTable = "CREATE TABLE IF NOT EXISTS TestCursorTable (c1 BIGINT,c2 BOOLEAN,c3 BYTEA,c4 CHAR,c5 DATE,c6 DOUBLE PRECISION,c7 INTEGER,c8 NUMERIC,c9 NUMERIC(10,2),c10 REAL,c11 SMALLINT,c12 TEXT,c13 TIMESTAMP,c14 VARCHAR(10));";
                com.CommandText = CursorTable;
                com.ExecuteNonQuery();

                CursorTable = "CREATE OR REPLACE Function RefCursorsOUT(Test_RefCursor OUT SYS_REFCURSOR) return NUMERIC IS " +
                              "BEGIN " +
                              "  OPEN Test_RefCursor FOR SELECT * FROM TestCursorTable; " +
                              "  return 10; " +
                              "END;";

                com.CommandText = CursorTable;

                com.ExecuteNonQuery();

                var CursorInsert1 = "INSERT INTO TestCursorTable VALUES(1, false, '\\001', 'a', '2006-01-01', 1.1, 1,1, 2.2, 2.2, 1, 'Shehzad', '2006-01-01', 'Hashim');";
                com.CommandText = CursorInsert1;
                com.ExecuteNonQuery();

                var CursorInsert2 = "INSERT INTO TestCursorTable VALUES(2, TRUE, '\\004', 'b', '2007-10-10', 1.2, 2,2, 3.3, 3.3, 2, 'EnterpriseDB', '2005-02-03', 'Great');";
                com.CommandText = CursorInsert2;
                com.ExecuteNonQuery();

                var CursorInsert3 = "INSERT INTO TestCursorTable VALUES(3, TRUE, '\\005', 'c', '2007-11-1', 1.3, 3,3, 2.1, 2.2, 1, 'Islamabad', '2006-01-01', 'Sirsyed');";
                com.CommandText = CursorInsert3;
                com.ExecuteNonQuery();

                var CursorInsert4 = "INSERT INTO TestCursorTable VALUES(4, false, '\\003', 'd', '1997-02-03', 1.4, 4,5, 2.2, 2.2, 1, 'Pakistan', '2006-01-01', 'Endnews');";
                com.CommandText = CursorInsert4;
                com.ExecuteNonQuery();

                com = new EDBCommand("set edb_stmt_level_tx to on;", con);
                com.ExecuteNonQuery();
                com.Dispose();

                EDBTransaction tran = con.BeginTransaction();
                try
                {
                    com = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    com.ExecuteNonQuery();
                    com.Dispose();
                }
                catch (EDBException)
                {
                }

                var command = new EDBCommand("RefCursorsOUT(:v_id)", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;
                command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.Refcursor, 0, "v_id", ParameterDirection.Output, false, 10, 10, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Numeric, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 100));
                command.Prepare();

                command.ExecuteNonQuery();
                var cursorName = command.Parameters[0].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader cur = command.ExecuteReader(CommandBehavior.SequentialAccess);

                cur.Read();
                Assert.AreEqual(1, cur[0]);
                Assert.AreEqual(false, cur[1]);
                Assert.IsInstanceOf(typeof(byte[]), cur[2]);
                Assert.AreEqual("a", cur[3]);
                Assert.AreEqual(new DateTime(2006, 1, 1), cur[4]);
                Assert.AreEqual(1.1, cur[5]);
                Assert.AreEqual(1, cur[6]);
                Assert.AreEqual(1, cur[7]);
                Assert.AreEqual(2.20, cur[8]);
                Assert.AreEqual(2.2f, cur[9]);
                Assert.AreEqual(1, cur[10]);
                Assert.AreEqual("Shehzad", cur[11]);
                Assert.AreEqual(new DateTime(2006, 1, 1), cur[12]);
                Assert.AreEqual("Hashim", cur[13]);

                cur.Read();
                Assert.AreEqual(2, cur[0]);
                Assert.AreEqual(true, cur[1]);
                Assert.IsInstanceOf(typeof(byte[]), cur[2]);
                Assert.AreEqual("b", cur[3]);
                Assert.AreEqual(new DateTime(2007, 10, 10), cur[4]);
                Assert.AreEqual(1.2, cur[5]);
                Assert.AreEqual(2, cur[6]);
                Assert.AreEqual(2, cur[7]);
                Assert.AreEqual(3.30, cur[8]);
                Assert.AreEqual(3.3f, cur[9]);
                Assert.AreEqual(2, cur[10]);
                Assert.AreEqual("EnterpriseDB", cur[11]);
                Assert.AreEqual(new DateTime(2005, 2, 3), cur[12]);
                Assert.AreEqual("Great", cur.GetString(13));

                cur.Read();
                Assert.AreEqual(3, cur[0]);
                Assert.AreEqual(true, cur[1]);
                Assert.IsInstanceOf(typeof(byte[]), cur[2]);
                Assert.AreEqual("c", cur[3]);
                Assert.AreEqual(new DateTime(2007, 11, 1), cur[4]);
                Assert.AreEqual(1.3, cur[5]);
                Assert.AreEqual(3, cur[6]);
                Assert.AreEqual(3, cur[7]);
                Assert.AreEqual(2.10, cur[8]);
                Assert.AreEqual(2.2f, cur[9]);
                Assert.AreEqual(1, cur[10]);
                Assert.AreEqual("Islamabad", cur[11]);
                Assert.AreEqual(new DateTime(2006, 1, 1), cur[12]);
                Assert.AreEqual("Sirsyed", cur[13]);

                cur.Read();
                Assert.AreEqual(4, cur[0]);
                Assert.AreEqual(false, cur[1]);
                Assert.IsInstanceOf(typeof(byte[]), cur[2]);
                Assert.AreEqual("d", cur[3]);
                Assert.AreEqual(new DateTime(1997, 2, 3), cur[4]);
                Assert.AreEqual(1.4, cur[5]);
                Assert.AreEqual(4, cur[6]);
                Assert.AreEqual(5, cur[7]);
                Assert.AreEqual(2.20, cur[8]);
                Assert.AreEqual(2.2f, cur[9]);
                Assert.AreEqual(1, cur[10]);
                Assert.AreEqual("Pakistan", cur[11]);
                Assert.AreEqual(new DateTime(2006, 1, 1), cur[12]);
                Assert.AreEqual("Endnews", cur[13]);

                cur.Close();

                tran.Commit();

                com.CommandText = "DROP TABLE TestCursorTable;";
                com.ExecuteNonQuery();
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }

        }

        [Test, /*Ignore("Investigate")*/]





        public void TERSE_FUNC_MIXED_NATIVE_CURSOR_TYPES()
        {
            EDBCommand command = null;
            try
            {
                using var con = OpenConnection();
                command = new EDBCommand("set edb_stmt_level_tx to on;", con);

                command.ExecuteNonQuery();
                command.Dispose();
                EDBTransaction tran = con.BeginTransaction();

                command = new EDBCommand("CREATE OR REPLACE function refcur_callee2_func( c_1 OUT numeric, " +
                                        "                                           c_2 IN OUT refcursor, " +
                                        "                                           c_3 IN OUT refcursor ) return numeric IS " +
                                        "BEGIN " +
                                        "   c_1 := 100; " +
                                        "   open c_2 for select * from emp; " +
                                        "   open c_3 for select ename from emp; " +
                                        "   return c_1; " +
                                        "END;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (EDBException)
                {
                }
                command = new EDBCommand("refcur_callee2_func(:b,:a,:c)", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Transaction = tran;
                command.Parameters.Add(new EDBParameter("b", EDBTypes.EDBDbType.Numeric, 10, "b", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Refcursor, 10, "a", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("c", EDBTypes.EDBDbType.Refcursor, 10, "c", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Parameters.Add(new EDBParameter("ret", EDBTypes.EDBDbType.Numeric, 10, "ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, null));
                command.Prepare();
                command.Parameters[0].Value = 7369;
                command.ExecuteNonQuery();


                Assert.AreEqual("100", Convert.ToString(command.Parameters[0].Value.ToString()));
                Assert.AreEqual("100", Convert.ToString(command.Parameters[3].Value.ToString()));

                var cursorName1 = command.Parameters[1].Value.ToString();
                var cursorName2 = command.Parameters[2].Value.ToString();

                command.CommandText = "FETCH ALL IN \"" + cursorName1 + "\"";
                command.CommandType = CommandType.Text;
                EDBDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                reader.Read();
                reader.Read();

                Assert.AreEqual("7499", Convert.ToString(reader.GetString(0)));
                Assert.AreEqual("ALLEN", Convert.ToString(reader.GetString(1)));
                Assert.AreEqual("SALESMAN", Convert.ToString(reader.GetString(2)));
                Assert.AreEqual("7698", Convert.ToString(reader.GetString(3)));
                Assert.AreEqual("1600", Convert.ToString(reader.GetString(5)));
                reader.Close();

                command.CommandText = "FETCH ALL IN \"" + cursorName2 + "\"";
                command.CommandType = CommandType.Text;
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

                reader.Read();
                Assert.AreEqual("SMITH", Convert.ToString(reader.GetString(0)));
                reader.Close();
                tran.Commit();

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                if (command != null)
                    command.Dispose();
            }

        }

        [Test, /*Ignore("Investigate")*/]
        public void TERSE_FUNC_DEFAULT_TYPES()
        {
            try
            {
                using var con = OpenConnection();
                EDBCommand command;
                command = new EDBCommand("set edb_stmt_level_tx to on;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("BEGIN;", con);
                command.ExecuteNonQuery();
                command.Dispose();

                command = new EDBCommand("create or replace function terse_func_defvals( param1 integer, param2 integer default 10 ) return varchar2 is " +
                                         "begin " +
                                         "  return 'EnterpriseDB'; " +
                                         "end;", con);

                command.ExecuteNonQuery();
                command.Dispose();

                try
                {
                    command = new EDBCommand("INSERT INTO SOME_GARBAGE VALUES( 10, 20 );", con);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (EDBException)
                {
                }

                command = new EDBCommand("terse_func_defvals(:param1)", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));
                command.Prepare();

                command.Parameters[0].Value = 3;
                EDBDataReader result = command.ExecuteReader();
                while (result.Read())
                { }

                Assert.AreEqual(3, int.Parse(command.Parameters[0].Value.ToString()));
                Assert.AreEqual("EnterpriseDB", command.Parameters[1].Value.ToString());

                result.Close();
                command.Dispose();

                command = new EDBCommand("END;", con);
                command.ExecuteNonQuery();
                command.Dispose();

            }
            catch (EDBException exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        #endregion
    }
#pragma warning restore CS8604
#pragma warning restore CS8602
#nullable restore
}

