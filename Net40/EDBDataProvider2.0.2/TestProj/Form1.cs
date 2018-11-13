using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EnterpriseDB.EDBClient;
namespace TestProj
{
    public partial class Form1 : Form
    {
        EDBConnection conn = new EDBConnection("Server=localhost;Port=5444;User Id=enterprisedb;Password=edb;Database=test");
        
        public Form1()
        {
            InitializeComponent();
        }

        public IEnumerable<String> test()
        {
            for (int i=0;  i< 5; i++ )
            {
                if(i!=4)
                yield return "test" + i;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
              
          conn.Open();
          
            try
            {
              EDBCommand command = new EDBCommand("singleInOutArg_test(:a,:b)", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("a", EDBTypes.EDBDbType.Numeric));
                command.Parameters[0].Value = 100;
                command.Parameters.Add(new EDBParameter("b",
                    EDBTypes.EDBDbType.Numeric, 10, "b",
                    ParameterDirection.Output, false, 2, 2,
                    System.Data.DataRowVersion.Current, 1));

                command.Prepare();
                EDBDataReader result = command.ExecuteReader();
                //while (result.Read())
                 //   Console.WriteLine("in var"+result.GetName(0));
                result.Close();
                Console.WriteLine(command.Parameters[1].Value.ToString());
            }
            catch (Exception exp)
            {
                MessageBox.Show("exp is "+exp.Message);
            }
            finally
            {
                conn.Close();
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
               
          conn.Open();

          try
          {
              EDBCommand command = new EDBCommand("public.functionsanity(:param1,:param2,:param3,:param4)", conn);
              command.CommandType = CommandType.StoredProcedure;

              command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Integer, 10, "param1", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
              command.Parameters.Add(new EDBParameter("param2", EDBTypes.EDBDbType.Integer, 10, "param2", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
              command.Parameters.Add(new EDBParameter("param3", EDBTypes.EDBDbType.Integer, 10, "param3", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 1));
              command.Parameters.Add(new EDBParameter("param4", EDBTypes.EDBDbType.Integer, 10, "param4", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, 1));
              command.Parameters.Add(new EDBParameter("param5", EDBTypes.EDBDbType.Varchar, 10, "param5", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

              command.Prepare();

              //command.Parameters[0].Value = nu;
              //command.Parameters[1].Value = 15;
              command.Parameters[2].Value = 20;
              //command.Parameters[3].Value = 50;


              //NpgsqlDataReader result = command.ExecuteReader(); 
              EDBDataReader result = command.ExecuteReader();
              //command1.ExecuteNonQuery();
              /*while (result.Read())
              {
                  Console.WriteLine("in var" + result.GetName(0));
                  Console.WriteLine("in var" + result.GetName(1));
                  Console.WriteLine("in var" + result.GetName(2));
                  //Console.WriteLine("in var" + result1.GetName(0));
              }*/

              Console.WriteLine(command.Parameters[0].Value.ToString());
              Console.WriteLine(command.Parameters[1].Value.ToString());
              Console.WriteLine(command.Parameters[3].Value.ToString());
              Console.WriteLine(command.Parameters[4].Value.ToString());

              result.Close();
          }
          catch (Exception exp)
          {
              MessageBox.Show("exp is " + exp.Message);
          }
          finally
          {
              conn.Close();
          }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            conn.Open();
            try
            {
                EDBCommand command = new EDBCommand("emptyArg_test", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Prepare();
                command.ExecuteNonQuery();
            }
            catch (Exception exp)
            {
                MessageBox.Show("exp is " + exp.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            conn.Open();
            try
            {
                EDBCommand command = new EDBCommand("public.emptyfunction_test()", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("param1", EDBTypes.EDBDbType.Varchar, 10, "param1", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 1));

                command.Prepare();

                EDBDataReader result = command.ExecuteReader();
                Console.WriteLine(command.Parameters[0].Value.ToString());
            }
            catch (Exception exp)
            {
                MessageBox.Show("exp is " + exp.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            conn.Open();
            try
            {
                EDBTransaction tran = conn.BeginTransaction();
                EDBCommand command = new EDBCommand("RefCurProc(:v_id)", conn);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = tran;
                command.Parameters.Add(new EDBParameter("v_id", EDBTypes.EDBDbType.RefCursor, 0, "v_id", ParameterDirection.Output, false, 10, 10, System.Data.DataRowVersion.Current, null));
                command.Prepare();
                command.ExecuteNonQuery();
                Console.WriteLine(command.Parameters[0].Value);

                EDBDataReader cur = (EDBDataReader)command.Parameters[0].Value;

                while(cur.Read())
                Console.WriteLine(cur.GetInt64(0));
                //cur.Close();

                tran.Commit();

                /*EDBCommand command = new EDBCommand("cursortest2(:cur1,:cur2)", conn);

                command.CommandType = CommandType.StoredProcedure;

                command.Transaction = tran;

                //REFCUSOR CommandBehavior.SequentialAccess

                command.Parameters.Add(new EDBParameter("cur1", EDBTypes.EDBDbType.RefCursor, 10, "cur1", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                command.Parameters.Add(new EDBParameter("cur2", EDBTypes.EDBDbType.RefCursor, 10, "cur2", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));

                command.Prepare();

                EDBDataReader result = command.ExecuteReader(CommandBehavior.SequentialAccess);

                int fc = result.FieldCount;
                Console.WriteLine(fc);



                EDBDataReader rst = (EDBDataReader)command.Parameters[0].Value;
                rst.Read();
                Console.WriteLine(rst.GetDecimal(0));

                int fc1 = result.FieldCount;

                rst.Read();*/

                

            }
            catch (Exception exp)
            {
                MessageBox.Show("exp is " + exp.Message);
            }
            finally
            {
                conn.Close();
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
             
            conn.Open();
            try
            {
                string updateQuery = "update public.emp set ename = :Name where empno = :ID";

                EDBCommand Prepared_command = new EDBCommand(updateQuery, conn);
                Prepared_command.CommandType = CommandType.Text;

                Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
                Prepared_command.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Text));

                

                Prepared_command.Parameters[0].Value = 7369;
                Prepared_command.Parameters[1].Value = "Mark";

                Prepared_command.Prepare();

                Prepared_command.ExecuteNonQuery();
                
                
            }
            catch (Exception exp)
            {
                MessageBox.Show("exp is " + exp.Message);
            }
            finally
            {
                conn.Close();
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            conn.Open();
            try
            {
                string updateQuery = "select ename from emp where  empno = :ID";

                EDBCommand Prepared_command = new EDBCommand(updateQuery, conn);
                Prepared_command.CommandType = CommandType.Text;

                Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
                Prepared_command.Prepare();

                Prepared_command.Parameters[0].Value = 7369;
                EDBDataReader reader = Prepared_command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine(reader.GetValue(0).ToString().ToUpper());

                }
                reader.Close();
            }
            catch (Exception exp)
            {
                MessageBox.Show("exp is " + exp.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            conn.Open();
            try
            {
                //EDBEventLog.Level = LogLevel.Debug;
               // EDBEventLog.EchoMessages = true;
                /*EDBCommand Command = new EDBCommand("", conn);
                Command.CommandText = "CREATE TABLE NumericTAB(A Numeric(3,2))";
                Command.ExecuteNonQuery();

                Command.CommandText = "insert into NumericTAB values(4.15)";
                Command.ExecuteNonQuery();
                for (int i = 0; i < 2; i++)
                {
                    EDBCommand Command = new EDBCommand("select 1 from dual", conn);
                    
                    EDBDataReader r = Command.ExecuteReader();
                    //r.Close();
                }*/


                EDBCommand command = new EDBCommand("FunctionWithDoublePrecision(:v_in,:v_inout,:v_out)", conn);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new EDBParameter("v_in", EDBTypes.EDBDbType.Float, 10, "v_in", ParameterDirection.Input, false, 8, 8, DataRowVersion.Current, 1.10001));
                command.Parameters.Add(new EDBParameter("v_inout", EDBTypes.EDBDbType.Float, 10, "v_inout", ParameterDirection.InputOutput, false, 8, 8, DataRowVersion.Current, -2.2131));
                command.Parameters.Add(new EDBParameter("v_out", EDBTypes.EDBDbType.Float, 10, "v_out", ParameterDirection.Output, false, 8, 8, DataRowVersion.Current, 4.4009));
                command.Parameters.Add(new EDBParameter("v_ret", EDBTypes.EDBDbType.Float, 10, "v_ret", ParameterDirection.ReturnValue, false, 2, 2, System.Data.DataRowVersion.Current, 8.8));
                command.Prepare();

                command.ExecuteNonQuery();
                Console.WriteLine(float.Parse(command.Parameters[0].Value.ToString()));

                //Assert.AreEqual(1.10001, float.Parse(command.Parameters[0].Value.ToString()));
                //Assert.AreEqual(1.10001, float.Parse(command.Parameters[1].Value.ToString()));
                //Assert.AreEqual(-2.2131, float.Parse(command.Parameters[2].Value.ToString()));
                //Assert.AreEqual(-0.999, float.Parse(command.Parameters[3].Value.ToString()));

            }
            catch (Exception exp)
            {
                MessageBox.Show("exp is " + exp.Message);
                Console.WriteLine(exp.StackTrace);
            }
            finally
            {
                conn.Close();
            }

		
        }
        
    }
    
}
