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
            EDBConnection conn = new EDBConnection("Server=localhost;Port=5444;User Id=enterprisedb;Password=eedb;Database=edb");			
			
			try
			{
				conn.Open();
				//Simple select statement using EDBCommand object

			    EDBCommand EDBCreateProc = new EDBCommand("CREATE OR REPLACE PROCEDURE emp_query (p_deptno IN NUMBER,\n"
                                                          + "p_empno  IN OUT NUMBER,\n"
                                                          + "p_ename         IN OUT VARCHAR2,\n"
                                                          + "p_job           OUT    VARCHAR2,\n"
                                                          + "p_hiredate      OUT    DATE,\n"
                                                          + "p_sal           OUT    NUMBER )\n"
                            
                                                          + "IS\n"
                                                          + "BEGIN\n"
                                                          + "SELECT empno, ename, job, hiredate, sal\n"
                                   + "INTO p_empno, p_ename, p_job, p_hiredate, p_sal\n"
                                   + "FROM emp  WHERE deptno = p_deptno  AND (empno = p_empno    OR  ename = UPPER(p_ename));\n"
                                   + " END;\n",conn);

                EDBCreateProc.ExecuteNonQuery();

                EDBCommand command = new EDBCommand("", conn);
                command.CommandType = CommandType.Text;

              /*  string strSql = "CREATE OR REPLACE FUNCTION FunctionWithVarchar(p_in in Varchar,p_inout inout Varchar,p_out out Varchar) return Varchar   IS   BEGIN  p_out:=p_inout; p_inout:=p_in; return p_out;  END;";
                command.CommandText = strSql;
                command.ExecuteNonQuery();

                //////////////code
                try
                {
                    command = new EDBCommand("FunctionWithVarchar(:v_in,:v_inout,:v_out)", conn);
                    command.CommandType = CommandType.StoredProcedure;
                 
//   callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, null));
			
                    command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Varchar, 10, "v_in", ParameterDirection.Input, false, 2, 2, DataRowVersion.Current, "Hashim"));
                    command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Varchar, 10, "v_inout", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "EnterpriseDB"));
                    command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Varchar, 10, "v_out", ParameterDirection.InputOutput, false, 2, 2, DataRowVersion.Current, "4"));
                    command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Varchar, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, "Hashim"));
                    command.Prepare();
                    command.ExecuteNonQuery();

                 //   Assert.AreEqual("Hashim", command.Parameters[0].Value.ToString());
                  //  Assert.AreEqual("Hashim", command.Parameters[1].Value.ToString());
                  //  Assert.AreEqual("EnterpriseDB", command.Parameters[2].Value.ToString());
                  //  Assert.AreEqual("EnterpriseDB", command.Parameters[3].Value.ToString());


                    Console.WriteLine( command.Parameters[0].Value.ToString());
                    Console.WriteLine(command.Parameters[1].Value.ToString());
                    Console.WriteLine(command.Parameters[2].Value.ToString());
                    Console.WriteLine(command.Parameters[3].Value.ToString());

                }
                catch (EDBException e)
                {
                    throw new Exception(e.ToString());
                }

                //////////tear down
                command.Dispose();
                command = new EDBCommand("", conn);
                command.CommandText = "DROP FUNCTION FunctionWithVarchar( in Varchar, inout Varchar, out Varchar);";
                command.ExecuteNonQuery();

                */

               /*
                try
                {
                    EDBCommand command11 = new EDBCommand("funcC()", conn);
                    command11.CommandType = CommandType.StoredProcedure;

                    Object result = command11.ExecuteScalar();
                    Console.WriteLine(result.ToString());

                    //                Assert.AreEqual(6, result);           
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message.ToString());
                }
                */
    
                           


                
         /*       EDBCommand EDBSeletCommand = new EDBCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP",conn);
				EDBDataReader SelectResult =  EDBSeletCommand.ExecuteReader();
				while(SelectResult.Read())
				{
				    Console.WriteLine("Emp No" + " " +	SelectResult.GetDecimal(0));
					Console.WriteLine("Emp Name" + " " + SelectResult.GetString(1));
					Console.WriteLine("Job" + " " +  SelectResult.GetString(2));
					Console.WriteLine("Mgr" + " " + SelectResult.GetString(3));
					Console.WriteLine("Hire Date" + " " + SelectResult.GetDateTime(4));
					Console.WriteLine("---------------------------------");
				}
				//Insert statement using EDBCommand Object

				EDBCommand EDBInsertCommand = new EDBCommand("INSERT INTO EMP(EMPNO,ENAME) VALUES((SELECT COUNT(EMPNO) FROM EMP),'JACKSON')",conn);
				EDBInsertCommand.ExecuteScalar();	
				Console.WriteLine("Record inserted");
				
				//Update  using EDBCommand Object

				EDBCommand  EDBUpdateCommand = new EDBCommand("UPDATE EMP SET ENAME ='DOTNET' WHERE EMPNO < 100",conn);
				EDBUpdateCommand.ExecuteNonQuery();
				Console.WriteLine("Record has been updated");
			
			
				//Delete  using EDBCommand Object

				EDBCommand EDBDeletCommand = new EDBCommand("DELETE FROM EMP WHERE EMPNO < 100",conn);
				EDBDeletCommand.CommandType= CommandType.Text;
				EDBDeletCommand.ExecuteScalar();
				Console.WriteLine("Record deleted");
				*/
				/*
				 //procedure call example
				try
				{
				EDBCommand callable_command = new EDBCommand("emp_query(:p_deptno,:p_empno,:p_ename,:p_job,:p_hiredate,:p_sal)", conn);
				callable_command.CommandType = CommandType.StoredProcedure;
				callable_command.Parameters.Add(new EDBParameter("p_deptno",EDBTypes.EDBDbType.Numeric,10,"p_deptno",ParameterDirection.Input,false ,2,2,System.Data.DataRowVersion.Current,10));
				callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric,10,"p_empno",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,10));
				callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar,10,"p_ename",ParameterDirection.InputOutput,false ,2,2,System.Data.DataRowVersion.Current,null));
				callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar,10,"p_job",ParameterDirection.Output,false ,2,2,System.Data.DataRowVersion.Current,null));
				callable_command.Parameters.Add(new EDBParameter("p_hiredate", EDBTypes.EDBDbType.Date,200));
                callable_command.Parameters.Add(new EDBParameter("p_sal", EDBTypes.EDBDbType.Numeric, 200, "p_sal", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 10));
                callable_command.Parameters[4].Direction = ParameterDirection.Output;

				callable_command.Prepare();
				callable_command.Parameters[0].Value = 30;
				callable_command.Parameters[1].Value = 7521;
				
				
					EDBDataReader result = callable_command.ExecuteReader();								
					int fc	=	result.FieldCount;
					
				
					
				//	while(result.Read())
					{
						for(int i=0;i<fc;i++)
							Console.WriteLine("RESULT["+i+"]="+ Convert.ToString(callable_command.Parameters[i].Value));
						  
					}
					result.Close();
				}
				catch(EDBException exp)
				{
					
					if(exp.Code.Equals("01403"))
						Console.WriteLine("No data found");
					else if(exp.Code.Equals("01422"))
						Console.WriteLine("More than one rows were returned by the query");
					else 
						Console.WriteLine("There was an error Calling the procedure. \nRoot Cause:\n");
						Console.WriteLine(exp.Message.ToString());
				}
				
			//Prepared statement
                conn.Close();
                conn.Open();
                
                */
				string updateQuery  = "update emp set ename = :Name where empno = :ID";
				EDBCommand Prepared_command = new EDBCommand(updateQuery, conn);
                
				Prepared_command.CommandType = CommandType.Text;

				Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer))  ;
				Prepared_command.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Text));
                 
				Prepared_command.Prepare();

				Prepared_command.Parameters[0].Value = 7369;
				Prepared_command.Parameters[1].Value = "Mark";				

				Prepared_command.ExecuteNonQuery();
				Console.WriteLine("Record Updated...");
                
			}
			
			catch(EDBException exp)
			{
				Console.WriteLine(exp.ToString() );
			}
			finally
			{
				conn.Close();
			}
		
		}
	}
}
