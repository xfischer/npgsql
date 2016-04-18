using System;
using System.Data;
using EnterpriseDB.EDBClient;

/*
 * This class provides a simple way to perform DML operation in EnterpriseDB Advanced Server 8.1
 * @revision 1.0
 */


namespace EDBClientTest
{

    class SAMPLE_TEST
	{
		
		static void Main(string[] args)
		{
            EDBConnection con = new EDBConnection("Server=172.16.20.143;Port=5444;User Id=enterprisedb;Password=edb;Database=edb;");			
			
			try
			{

                con.Open();
                //////prereq
                EDBCommand Command = new EDBCommand("", con);

                Command.CommandText = "CREATE OR REPLACE FUNCTION allOutMixedArgFunc_test(a OUT varchar, b OUT int, c OUT numeric, d OUT long) RETURN int\n"
                        + " AS \n"
                        + " BEGIN \n"
                        + "    a:= 'HELLO'; \n"
                        + "    b:= 10; \n"
                        + "    c:= 20.55; \n"
                        + "    d:= 'HELLO1'; \n"
                        + "    return 10; \n"
                        + " END; \n";
                Command.ExecuteNonQuery();



                Command = new EDBCommand("allOutMixedArgFunc_test", con);
                Command.CommandType = CommandType.StoredProcedure;
                Command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 10, "param2", ParameterDirection.ReturnValue, false, 2, 2, DataRowVersion.Current, "HI"));
                Command.Prepare();
                EDBDataReader result = Command.ExecuteReader();
              Console.WriteLine( Command.Parameters[0].Value.ToString());
                result.Close();
              
                Command = new EDBCommand();
                Command.Connection = con;
                Command.CommandText = "DROP FUNCTION allOutMixedArgFunc_test";
                Command.ExecuteNonQuery();


                 Console.WriteLine("yes");
              /*  EDBCommand command = new EDBCommand("", conn);
                command.CommandType = CommandType.Text;

                string strSql = "CREATE OR REPLACE FUNCTION FunctionWithBit2(p_in in Bit,p_inout inout Bit,p_out out Bit) return Bit  IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return true;  END;";
                command.CommandText = strSql;
                command.ExecuteNonQuery();

                //////////////code
                try
                {
                    command = new EDBCommand("FunctionWithBit2(:v_in,:v_inout,:v_out)", conn);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Bit, 4, "v_in", ParameterDirection.Input, false, 0, 0, DataRowVersion.Current, true));
                    command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Bit, 4, "v_inout", ParameterDirection.InputOutput, false, 0, 0, DataRowVersion.Current, false));
                    command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Bit, 4, "v_out", ParameterDirection.Output, false, 0, 0, DataRowVersion.Current, true));
                    command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Bit, 100, "v_ret", ParameterDirection.ReturnValue, false, 0, 0, System.Data.DataRowVersion.Current, false));
                    command.Prepare();
                    command.ExecuteNonQuery();

                   Console.WriteLine(command.Parameters[0].Value.ToString());
                    Console.WriteLine(command.Parameters[1].Value.ToString());
                    bool p_out = false;
                    Console.WriteLine(command.Parameters[2].Value);
                    Console.WriteLine( command.Parameters[3].Value.ToString());
                }
                catch (EDBException e)
                {
                    throw new Exception(e.ToString());
                }

                //////////tear down
                command.Dispose();
                command = new EDBCommand("", conn);
                command.CommandText = "DROP Function FunctionWithBit2( in Bit, inout Bit, out Bit);";
                command.ExecuteNonQuery();

                */



           /*  
                try
                {
                    EDBCommand command = new EDBCommand("public.funcThreeInArgq(:param1,:param2,:param3)", conn);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 0, "param1", ParameterDirection.Input, false, 0, 0, System.Data.DataRowVersion.Current, 1));
                    command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 0, "param2", ParameterDirection.Input, false, 0, 0, System.Data.DataRowVersion.Current, 1));
                    command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 0, "param3", ParameterDirection.Input, false, 0, 0, System.Data.DataRowVersion.Current, 1));
                    command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Varchar, 12, "param4", ParameterDirection.ReturnValue, false, 0, 0, System.Data.DataRowVersion.Current, 1));

                    command.Prepare();

                  //  command.ExecuteNonQuery();
                    EDBDataReader result = command.ExecuteReader();
                    while (result.Read())
                    {


                        Console.WriteLine(command.Parameters[0].Value.ToString());
                        Console.WriteLine(command.Parameters[1].Value.ToString());
                        Console.WriteLine(command.Parameters[2].Value.ToString());
                        Console.WriteLine(command.Parameters[3].Value.ToString());
                    }

                }
                catch (EDBException exp)
                {
                    Console.WriteLine(exp.Message);
                } 



                */

                /*






                EDBCommand command = new EDBCommand("", conn);
                command.CommandType = CommandType.Text;

                string strSql = "CREATE OR REPLACE FUNCTION FunctionWithMediumText(p_in in MediumText,p_inout inout MediumText,p_out out MediumText) return MediumText   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
                command.CommandText = strSql;
                command.ExecuteNonQuery();

                //////////////code
                try
                {
                    command = new EDBCommand("FunctionWithMediumText(:v_in,:v_inout,:v_out)", conn);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Text, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                    command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Text, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                    command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Text, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                    command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Text, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                    command.Prepare();
                    command.ExecuteNonQuery();

                    Console.WriteLine(command.Parameters[0].Value.ToString());
                    Console.WriteLine(command.Parameters[1].Value.ToString());
                    Console.WriteLine(command.Parameters[2].Value.ToString());
                    Console.WriteLine(command.Parameters[3].Value.ToString());
                }
                catch (EDBException e)
                {
                    throw new Exception(e.ToString());
                }

             

                try
                {
                    command = new EDBCommand("FunctionWithMediumText(:v_in,:v_inout,:v_out)", conn);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Text, 12, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                    command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Text, 12, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                    command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Text, 12, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                    command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Text, 12, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                    command.Prepare();
                   EDBDataReader reader =  command.ExecuteReader();
                    while(reader.Read()){
                    Console.WriteLine(command.Parameters[0].Value.ToString());
                    Console.WriteLine(command.Parameters[1].Value.ToString());
                    Console.WriteLine(command.Parameters[2].Value.ToString());
                    Console.WriteLine(command.Parameters[3].Value.ToString());
                    }
                    reader.Close();
                }
                catch (EDBException e)
                {
                    throw new Exception(e.ToString());
                }

                
                command.Dispose();

                 command = new EDBCommand("public.functionsanity_varchar2(:param1,:param2)", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 6, "param1", ParameterDirection.Output, false, 0, 0, System.Data.DataRowVersion.Current, "zahid"));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 6, "param2", ParameterDirection.Output, false, 0, 0, System.Data.DataRowVersion.Current, "zahid"));
                command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 12, "param5", ParameterDirection.ReturnValue, false, 0, 0, System.Data.DataRowVersion.Current, 10));

                command.Prepare();


                EDBDataReader result = command.ExecuteReader();
                while (result.Read())
                {

                    Console.WriteLine(result[0].ToString());
                    Console.WriteLine(result[1].ToString());
                    Console.WriteLine(result[2].ToString());

                }
                result.Close();
                
*/


/*
                EDBCommand command = new EDBCommand("public.functionsanity_varchar2(:param1,:param2)", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 6, "param1", ParameterDirection.Output, false, 0, 0, System.Data.DataRowVersion.Current, "zahid"));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Varchar, 6, "param2", ParameterDirection.Output, false, 0, 0, System.Data.DataRowVersion.Current, "zahid"));
                command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 12, "param5", ParameterDirection.ReturnValue, false, 0, 0, System.Data.DataRowVersion.Current, 10));

                command.Prepare();

          
                EDBDataReader result = command.ExecuteReader();
                while (result.Read())
                { }
 */



/*


                EDBCommand command = new EDBCommand("public.functionsanity(:param1,:param2,:param3,:param4)", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Numeric, 0, "param1", ParameterDirection.Output, false, 0, 0, System.Data.DataRowVersion.Current, 10));
                command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Numeric, 0, "param2", ParameterDirection.Output, false, 0, 0, System.Data.DataRowVersion.Current, 10));
                command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Numeric, 0, "param3", ParameterDirection.Input, false, 0, 0, System.Data.DataRowVersion.Current, 10));
                command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Numeric, 0, "param4", ParameterDirection.Output, false, 0, 0, System.Data.DataRowVersion.Current, 10));
                command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 0, "param5", ParameterDirection.ReturnValue, false, 0, 0, System.Data.DataRowVersion.Current, 10));

                command.Prepare();

                command.Parameters[0].Value = 1;
                command.Parameters[1].Value = 1;
                command.Parameters[2].Value = 3;
                command.Parameters[3].Value = 1;


                EDBDataReader result = command.ExecuteReader();
                while (result.Read())
                { }

              //  Assert.AreEqual(100, int.Parse(command.Parameters[0].Value.ToString()));
                //Assert.AreEqual(200, int.Parse(command.Parameters[1].Value.ToString()));
              //  Assert.AreEqual(3, int.Parse(command.Parameters[2].Value.ToString()));
              //  Assert.AreEqual(400, int.Parse(command.Parameters[3].Value.ToString()));
              //  Assert.AreEqual("EnterpriseDB", command.Parameters[4].Value.ToString());













                /*
              EDBCommand com = new EDBCommand("", conn);
                com.CommandType = CommandType.Text;

                string strSql = "CREATE OR REPLACE PACKAGE PKG_INVOKE_exec_pro IS PROCEDURE exec_pro(namein IN VARCHAR2,nameout OUT VARCHAR2); END PKG_INVOKE_exec_pro;";
                com.CommandText = strSql;
                com.ExecuteNonQuery();
                
                strSql = "CREATE OR REPLACE PACKAGE BODY PKG_INVOKE_exec_pro IS PROCEDURE local(namein VARCHAR2, nameout OUT VARCHAR2) IS BEGIN nameout := TRANSLATE(namein,'AEIOUaeiou','EIOUAeioua'); END local; PROCEDURE exec_pro(namein IN VARCHAR2,nameout OUT VARCHAR2)  IS  countX NUMBER; BEGIN local(namein, nameout); END exec_pro; END PKG_INVOKE_exec_pro;";
                com.CommandText = strSql;
                com.ExecuteNonQuery();
/*
                strSql = "CREATE OR REPLACE PACKAGE PKG_Variable_Test IS alpha   varchar(20); beta numeric; PROCEDURE proc(aa OUT varchar,bb OUT numeric) END PKG_Variable_Test;\n"
                            + "CREATE OR REPLACE PACKAGE BODY PKG_Variable_Test IS\n"
                            + "PROCEDURE proc(aa OUT varchar,bb OUT numeric) IS\n"
                            + "BEGIN\n"
                            + "alpha := 'alpha';\n"
                            + "aa := alpha;\n"
                            + "beta := 2;\n"
                            + "bb := beta;\n"
                            + "END proc;\n"
                            + "END PKG_Variable_Test;\n";
                com.CommandText = strSql;
                com.ExecuteNonQuery();
                */


/*

                EDBCommand command = new EDBCommand("", conn);
                command.CommandType = CommandType.Text;


                 strSql = "CREATE OR REPLACE PACKAGE check_package  IS  procedure get_c1(p_in in int,p_inout inout int,p_out out int) ;   END check_package; ";
                command.CommandText = strSql;
                command.ExecuteNonQuery();

                strSql = "CREATE OR REPLACE PACKAGE BODY check_package  IS procedure get_c1(p_in in int,p_inout inout int,p_out out int)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;END check_package;";
                command.CommandText = strSql;
                command.ExecuteNonQuery();


                //////////////code
                try
                {
                    command = new EDBCommand("check_package.get_c1(:v_in,:v_inout,:v_out)", conn);
                    command.CommandType = CommandType.StoredProcedure;


                    command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Integer, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, 1));
                    command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Integer, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 2));
                    command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Integer, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, 4));
                    command.Prepare();



                    command.ExecuteNonQuery();

                   Console.WriteLine(command.Parameters[0].Value.ToString());
                    Console.WriteLine(command.Parameters[1].Value.ToString());
                    Console.WriteLine(command.Parameters[2].Value.ToString());
                    
                }
                catch (EDBException e)
                {
                    throw new Exception(e.ToString());
                }

 */

/*
                com = new EDBCommand("", conn);
                com.CommandType = CommandType.Text;

                com.CommandText = "DROP PACKAGE PKG_INVOKE_exec_pro;";
                com.ExecuteNonQuery();

                com.CommandText = "DROP PACKAGE PKG_Variable_Test;";
                com.ExecuteNonQuery();

*/

				/*
                 conn.Open();
                EDBCommand command = new EDBCommand("",conn);
                string strSql = "CREATE OR REPLACE PROCEDURE ProcedureWithFloat(p_in in FLOAT,p_inout inout FLOAT,p_out out FLOAT)   IS   BEGIN  p_out:=p_inout; p_inout:=p_in;   END;";
                command.CommandText = strSql;
                command.ExecuteNonQuery();

                try
                {
                    command = new EDBCommand("ProcedureWithFloat(:v_in,:v_inout,:v_out)", conn);
                    command.CommandType = CommandType.StoredProcedure;


                    command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Numeric, 10, "v_in", ParameterDirection.Input, false, 8, 8, DataRowVersion.Current, 1.100001));
                    command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Numeric, 10, "v_inout", ParameterDirection.InputOutput, false, 8, 8, DataRowVersion.Current, -2.2131));
                    command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Numeric, 10, "v_out", ParameterDirection.InputOutput, false, 8, 8, DataRowVersion.Current, 4.4009));
                    command.Prepare();



                    command.ExecuteNonQuery();
                   // Assert.AreEqual(1.100001f, float.Parse(command.Parameters[0].Value.ToString()));
                 //   Assert.AreEqual(1.10000098f, float.Parse(command.Parameters[1].Value.ToString()));
                   // Assert.AreEqual(-2.2131f, float.Parse(command.Parameters[2].Value.ToString()));


                    Console.WriteLine(command.Parameters[0].Value.ToString());
                    Console.WriteLine(command.Parameters[1].Value.ToString());
                    Console.WriteLine(command.Parameters[2].Value.ToString());

                }
                catch (EDBException e)
                {
                    throw new Exception(e.ToString());
                }
			
                */


/*
                EDBCommand command;

                command = new EDBCommand("ProcedureWithChar(:v_in,:v_inout,:v_out)", conn);
                command.CommandType = CommandType.StoredProcedure;


                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Char, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Char, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Char, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                command.Prepare();

*/

             //   command.ExecuteNonQuery();
             

            // Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
            //Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
            //  Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());	



            //      Console.WriteLine( command.Parameters[0].Value.ToString());
            //Console.WriteLine( command.Parameters[1].Value.ToString());
            //Console.WriteLine(command.Parameters[2].Value.ToString());

            //command.Clone();


/*                EDBCommand com = new EDBCommand("", conn);


                com.CommandType = CommandType.Text;

                //	Testing procedure with Empty Argument list
                string strSqlEmptyArg = "CREATE OR REPLACE PROCEDURE emptyArg_test \n"
                    + " AS \n"
                    + "b        NUMBER(2);\n"
                    + " BEGIN \n"
                    + "    b := 6; \n"
                    + " END; \n";
                com.CommandText = strSqlEmptyArg;
                com.ExecuteNonQuery();

                //	Testing procedure with one IN Param
                string strSqlOneInArg = "CREATE OR REPLACE PROCEDURE oneInArg_test(a IN NUMERIC) \n"
                    + " AS \n"
                    + "b        NUMBER(2);\n"
                    + " BEGIN \n"
                    + "    b := a; \n"
                    + " END; \n";
                com.CommandText = strSqlOneInArg;
                com.ExecuteNonQuery();

                //	Testing procedure with three IN Param
                string strSqlThreeInArg = "CREATE OR REPLACE PROCEDURE threeInArg_test(a IN NUMERIC, b IN NUMERIC, c IN NUMERIC) \n"
                    + " AS \n"
                    + "d        NUMBER(2);\n"
                    + " BEGIN \n"
                    + "    d:=a; \n"
                    + "    d:=d+b; \n"
                    + "    d:=d+c; \n"
                    + " END; \n";
                com.CommandText = strSqlThreeInArg;
                com.ExecuteNonQuery();

                //	Testing procedure with one OUT Param
                string strSqlOneOutArg = "CREATE OR REPLACE PROCEDURE oneOutArg_test(a OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + " END; \n";
                com.CommandText = strSqlOneOutArg;
                com.ExecuteNonQuery();

                //	Testing procedure with three OUT Param
                string strSqlThreeOutArg = "CREATE OR REPLACE PROCEDURE threeOutArg_test(a OUT NUMERIC, b OUT NUMERIC, c OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    a:=5; \n"
                    + "    b:=15; \n"
                    + "    c:=25; \n"
                    + " END; \n";
                com.CommandText = strSqlThreeOutArg;
                com.ExecuteNonQuery();

                //	Testing procedure with one IN one OUT Param
                string strSqlInOutArg = "CREATE OR REPLACE PROCEDURE singleInOutArg_test(a IN NUMERIC, b OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    b:=a+2; \n"
                    + " END; \n";
                com.CommandText = strSqlInOutArg;
                com.ExecuteNonQuery();

                //	Testing procedure with multiple IN multiple OUT Param
                string strSqlMultInOutArg = "CREATE OR REPLACE PROCEDURE multipleInOutArg_test(a IN NUMERIC, b OUT NUMERIC, c IN NUMERIC, d OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    b:=a; \n"
                    + "    d:=c; \n"
                    + " END; \n";
                com.CommandText = strSqlMultInOutArg;
                com.ExecuteNonQuery();

                //	Testing procedure with IN, OUT and IN/OUT Param
                string strSqlMixArg = "CREATE OR REPLACE PROCEDURE mixArg_test(a INOUT NUMERIC, b OUT NUMERIC, c IN NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    b:=c; \n"
                    + "    a:=a+a; \n"
                    + " END; \n";
                com.CommandText = strSqlMixArg;
                com.ExecuteNonQuery();


                string strInOutArgs = "CREATE OR REPLACE PROCEDURE multipleInOutParameters(a IN NUMERIC, b OUT NUMERIC, c IN NUMERIC, d OUT NUMERIC) \n"
                    + " AS \n"
                    + " BEGIN \n"
                    + "    b:=a; \n"
                    + "    d:=c; \n"
                    + " END; \n";


                com.CommandText = strInOutArgs;
                com.ExecuteNonQuery();
                Console.WriteLine("Procedure Executed");

                string CreateTable = "CREATE  table InOutTestEmp(a numeric)";


                com.CommandText = CreateTable;
                com.CommandType = CommandType.Text;

                com.ExecuteNonQuery();
                Console.WriteLine("Table created");





                string strRefTwoArg = "CREATE OR REPLACE PROCEDURE public.cursortest2 (c_1 OUT    refcursor,c_2 OUT refcursor ) IS BEGIN open  c_1 for select * from emp order by empno; open  c_2 for select * from emp order by empno;END;";

                com.CommandText = strRefTwoArg;

                com.ExecuteNonQuery();



                string strRefThreeArg = "CREATE OR REPLACE PROCEDURE public.refcur_callee2 (c_1  OUT numeric, c_2 IN OUT refcursor,c_3 IN OUT refcursor ) IS BEGIN c_1 :=100; open  c_2 for select * from emp; open  c_3 for select ename from emp order by ename;END;";

                com.CommandText = strRefThreeArg;

                com.ExecuteNonQuery();
               
                EDBCommand command = new EDBCommand("multipleInOutArg_test(:a,:b,:c,:d)", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Numeric));
                command.Parameters[0].Value = 5;

                command.Parameters.Add(new EDBParameter("b",
                    EDBTypes.EDBDbType.Numeric, 0, "b",
                    ParameterDirection.Output, false, 0, 0,
                    System.Data.DataRowVersion.Current, 1));

                command.Parameters.Add(new EDBParameter("c", EDBTypes.EDBDbType.Numeric));
                command.Parameters[2].Value = 15;

                command.Parameters.Add(new EDBParameter("d",
                EDBTypes.EDBDbType.Numeric, 0, "d",
                ParameterDirection.Output, false, 0, 0,
                    System.Data.DataRowVersion.Current, 1));

                command.Prepare();
                EDBDataReader reader =  command.ExecuteReader();
                while (reader.Read())
                {

                 //   Console.WriteLine(command.Parameters[0].Value.ToString());
                    Console.WriteLine(command.Parameters[1].Value.ToString());
                   // Console.WriteLine(command.Parameters[2].Value.ToString());
                    Console.WriteLine(command.Parameters[3].Value.ToString());

                }
                reader.Close();


                EDBCommand deletecommand = new EDBCommand("",conn);
                deletecommand.CommandType = CommandType.Text;

                deletecommand.CommandText = "DROP PROCEDURE multipleInOutArg_test";
                deletecommand.ExecuteNonQuery();



                try
                {
                    EDBCommand callable_command = new EDBCommand("emp_query3(:p_deptno,:p_empno,:p_ename,:p_job)", conn);
                    callable_command.CommandType = CommandType.StoredProcedure;
                    callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                    callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                    callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 2, "p_ename", ParameterDirection.InputOutput, false, 0, 0, System.Data.DataRowVersion.Current,"zk"));
                    callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 2, "p_job", ParameterDirection.Output, false, 0, 0, System.Data.DataRowVersion.Current, "kh"));
           
                    callable_command.Prepare();

                    callable_command.Parameters[0].Value = 30;
                    callable_command.Parameters[1].Value = 7521;


                     EDBDataReader result3 = callable_command.ExecuteReader();
                    int fc22 = result3.FieldCount;

                    

                    	while(result3.Read())
                    {
                //        for (int i = 0; i < fc; i++)
                        Console.WriteLine("RESULT[ 1 ]=" + Convert.ToString(callable_command.Parameters[0].Value));
                        Console.WriteLine("RESULT[  2 ]=" + Convert.ToString(callable_command.Parameters[1].Value));
                        Console.WriteLine("RESULT[  3 ]=" + Convert.ToString(callable_command.Parameters[2].Value));
                        Console.WriteLine("RESULT[  4 ]=" + Convert.ToString(callable_command.Parameters[3].Value));
                    }
                    result3.Close();
                }
                catch (EDBException exp)
                {

                    if (exp.Code.Equals("01403"))
                        Console.WriteLine("No data found");
                    else if (exp.Code.Equals("01422"))
                        Console.WriteLine("More than one rows were returned by the query");
                    else
                        Console.WriteLine("There was an error Calling the procedure. \nRoot Cause:\n");
                    Console.WriteLine(exp.Message.ToString());
                }





                try
                {
                    EDBCommand callable_command = new EDBCommand("emp_query3(:p_deptno,:p_empno,:p_ename,:p_job)", conn);
                    callable_command.CommandType = CommandType.StoredProcedure;
                    callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                    callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                    callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "zk"));
                    callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                    callable_command.Prepare();

                    callable_command.Parameters[0].Value = 30;
                    callable_command.Parameters[1].Value = 7521;


                    EDBDataReader result3 = callable_command.ExecuteReader();
                    int fc22 = result3.FieldCount;



                    while (result3.Read())
                    {
                        //        for (int i = 0; i < fc; i++)
                        Console.WriteLine("RESULT[ 1 ]=" + Convert.ToString(callable_command.Parameters[0].Value));
                        Console.WriteLine("RESULT[  2 ]=" + Convert.ToString(callable_command.Parameters[1].Value));
                        Console.WriteLine("RESULT[  3 ]=" + Convert.ToString(callable_command.Parameters[2].Value));
                        Console.WriteLine("RESULT[  4 ]=" + Convert.ToString(callable_command.Parameters[3].Value));
                    }
                    result3.Close();
                }
                catch (EDBException exp)
                {

                    if (exp.Code.Equals("01403"))
                        Console.WriteLine("No data found");
                    else if (exp.Code.Equals("01422"))
                        Console.WriteLine("More than one rows were returned by the query");
                    else
                        Console.WriteLine("There was an error Calling the procedure. \nRoot Cause:\n");
                    Console.WriteLine(exp.Message.ToString());
                }

                try{
                EDBCommand cmd = new EDBCommand("select * from emp", conn);
                EDBDataReader rd = cmd.ExecuteReader();

                while (rd.Read()) { 
                    Console.WriteLine(rd[0].ToString());
                    Console.WriteLine(rd[1].ToString());
                
                
                      }
                rd.Close();
                 }
             catch (EDBException exp)
                {

                    
                    Console.WriteLine(exp.Message.ToString());
                }
               
                try
                {
                    EDBCommand callable_command = new EDBCommand("emp_query3(:p_deptno,:p_empno,:p_ename,:p_job)", conn);
                    callable_command.CommandType = CommandType.StoredProcedure;
                    callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                    callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                    callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "zk"));
                    callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                    callable_command.Prepare();

                    callable_command.Parameters[0].Value = 30;
                    callable_command.Parameters[1].Value = 7521;


                    EDBDataReader result3 = callable_command.ExecuteReader();
                    int fc22 = result3.FieldCount;



                    while (result3.Read())
                    {
                        //        for (int i = 0; i < fc; i++)
                        Console.WriteLine("RESULT[ 1 ]=" + Convert.ToString(callable_command.Parameters[0].Value));
                        Console.WriteLine("RESULT[  2 ]=" + Convert.ToString(callable_command.Parameters[1].Value));
                        Console.WriteLine("RESULT[  3 ]=" + Convert.ToString(callable_command.Parameters[2].Value));
                        Console.WriteLine("RESULT[  4 ]=" + Convert.ToString(callable_command.Parameters[3].Value));
                    }
                    result3.Close();
                }
                catch (EDBException exp)
                {

                    if (exp.Code.Equals("01403"))
                        Console.WriteLine("No data found");
                    else if (exp.Code.Equals("01422"))
                        Console.WriteLine("More than one rows were returned by the query");
                    else
                        Console.WriteLine("There was an error Calling the procedure. \nRoot Cause:\n");
                    Console.WriteLine(exp.Message.ToString());
                }

                try
                {
                    EDBCommand callable_command = new EDBCommand("emp_query3(:p_deptno,:p_empno,:p_ename,:p_job)", conn);
                    callable_command.CommandType = CommandType.StoredProcedure;
                    callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                    callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                    callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "zk"));
                    callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                    callable_command.Prepare();

                    callable_command.Parameters[0].Value = 30;
                    callable_command.Parameters[1].Value = 7521;


                    EDBDataReader result3 = callable_command.ExecuteReader();
                    int fc22 = result3.FieldCount;



                    while (result3.Read())
                    {
                        //        for (int i = 0; i < fc; i++)
                        Console.WriteLine("RESULT[ 1 ]=" + Convert.ToString(callable_command.Parameters[0].Value));
                        Console.WriteLine("RESULT[  2 ]=" + Convert.ToString(callable_command.Parameters[1].Value));
                        Console.WriteLine("RESULT[  3 ]=" + Convert.ToString(callable_command.Parameters[2].Value));
                        Console.WriteLine("RESULT[  4 ]=" + Convert.ToString(callable_command.Parameters[3].Value));
                    }
                    result3.Close();
                }
                catch (EDBException exp)
                {

                    if (exp.Code.Equals("01403"))
                        Console.WriteLine("No data found");
                    else if (exp.Code.Equals("01422"))
                        Console.WriteLine("More than one rows were returned by the query");
                    else
                        Console.WriteLine("There was an error Calling the procedure. \nRoot Cause:\n");
                    Console.WriteLine(exp.Message.ToString());
                }




                com.CommandText = "DROP PROCEDURE emptyArg_test";
                com.ExecuteNonQuery();

                com.CommandText = "DROP PROCEDURE oneInArg_test";
                com.ExecuteNonQuery();

                com.CommandText = "DROP PROCEDURE threeInArg_test";
                com.ExecuteNonQuery();

                com.CommandText = "DROP PROCEDURE oneOutArg_test";
                com.ExecuteNonQuery();

                com.CommandText = "DROP PROCEDURE threeOutArg_test";
                com.ExecuteNonQuery();

                com.CommandText = "DROP PROCEDURE singleInOutArg_test";
                com.ExecuteNonQuery();

                //com.CommandText = "DROP PROCEDURE multipleInOutArg_test";
                //com.ExecuteNonQuery();

                com.CommandText = "DROP PROCEDURE mixArg_test";
                com.ExecuteNonQuery();

                com.CommandText = "DROP PROCEDURE multipleInOutParameters";
                com.ExecuteNonQuery();

                com.CommandText = "Drop table InOutTestEmp";
                com.ExecuteNonQuery();



                com.CommandText = "DROP PROCEDURE cursortest2";

                com.ExecuteNonQuery();



                com.CommandText = "DROP PROCEDURE refcur_callee2";

                com.ExecuteNonQuery();
                */


			}
        	catch(EDBException exp)
			{
				Console.WriteLine(exp.ToString());
			}
			finally
			{
				con.Close();
			}
		
		}
	}
}
