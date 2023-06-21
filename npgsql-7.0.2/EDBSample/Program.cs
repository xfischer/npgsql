using EnterpriseDB.EDBClient;
using System.Data;

namespace EDBSample
{
    internal class Program
    {
        static string connectionString = "Server=localhost;Port=5444;User Id=enterprisedb;Password=password;Database=edb";

        static async Task Main(string[] args)
        {
            await Sample();
            //await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (i,_) =>
            //{
            //    await EC_2716_Sample();
            //});
        }

        static async Task Sample()
        {
            try
            {
                var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
                await using var dataSource = dataSourceBuilder.Build();

                await using var conn = await dataSource.OpenConnectionAsync();

                //Simple select statement using EDBCommand object
                await using var EDBSeletCommand = new EDBCommand("SELECT EMPNO,ENAME,JOB,MGR,HIREDATE FROM EMP", conn);
                await using var SelectResult = await EDBSeletCommand.ExecuteReaderAsync();
                while (await SelectResult.ReadAsync())
                {
                    Console.WriteLine("Emp No" + " " + SelectResult.GetInt32(0));
                    Console.WriteLine("Emp Name" + " " + SelectResult.GetString(1));
                    if (SelectResult.IsDBNull(2) == false)
                        Console.WriteLine("Job" + " " + SelectResult.GetString(2));
                    else
                        Console.WriteLine("Job" + " null ");
                    if (SelectResult.IsDBNull(3) == false)
                        Console.WriteLine("Mgr" + " " + SelectResult.GetInt32(3));
                    else
                        Console.WriteLine("Mgr" + "null");
                    if (SelectResult.IsDBNull(4) == false)
                        Console.WriteLine("Hire Date" + " " + SelectResult.GetDateTime(4));
                    else
                        Console.WriteLine("Hire Date" + " null");
                    Console.WriteLine("---------------------------------");
                }
                await SelectResult.CloseAsync();

                //Insert statement using EDBCommand Object
                await using var EDBInsertCommand = new EDBCommand("INSERT INTO EMP(EMPNO,ENAME) VALUES((SELECT COUNT(EMPNO) FROM EMP),'JACKSON')", conn);
                EDBInsertCommand.CommandType = CommandType.Text;
                await EDBInsertCommand.ExecuteScalarAsync();
                Console.WriteLine("Record inserted");

                //Update using EDBCommand Object
                await using var EDBUpdateCommand = new EDBCommand("UPDATE EMP SET ENAME ='DOTNET' WHERE EMPNO < 100", conn);
                EDBUpdateCommand.CommandType = CommandType.Text;
                await EDBUpdateCommand.ExecuteNonQueryAsync();
                Console.WriteLine("Record updated");

                //Delete using EDBCommand Object
                await using var EDBDeletCommand = new EDBCommand("DELETE FROM EMP WHERE EMPNO < 100", conn);
                EDBDeletCommand.CommandType = CommandType.Text;
                await EDBDeletCommand.ExecuteScalarAsync();
                Console.WriteLine("Record deleted");

                //procedure call example
                try
                {
                    await using var callable_command = new EDBCommand("emp_query(:p_deptno,:p_empno,:p_ename,:p_job,:p_hiredate,:p_sal)", conn);
                    callable_command.CommandType = CommandType.StoredProcedure;
                    callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));
                    callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 7369));
                    callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "SMITH"));
                    callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                    callable_command.Parameters.Add(new EDBParameter("p_hiredate", EDBTypes.EDBDbType.Date, 200, "p_hiredate", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                    callable_command.Parameters.Add(new EDBParameter("p_sal", EDBTypes.EDBDbType.Numeric, 200, "p_sal", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
                    await callable_command.PrepareAsync();

                    callable_command.Parameters[0].Value = 20;
                    callable_command.Parameters[1].Value = 7369;

                    await using var result = await callable_command.ExecuteReaderAsync();
                    var fc = result.FieldCount;
                    for (var i = 0; i < fc; i++)
                        Console.WriteLine($"RESULT[{i}]={Convert.ToString(callable_command.Parameters[i].Value)} ({callable_command.Parameters[i].Value.GetType().Name})");
                    result.Close();
                }
                catch (EDBException exp)
                {
                    if (exp.ErrorCode.Equals("01403"))
                        Console.WriteLine("No data found");
                    else if (exp.ErrorCode.Equals("01422"))
                        Console.WriteLine("More than one rows were returned by the query");
                    else
                        Console.WriteLine("There was an error Calling the procedure. \nRoot Cause:\n");
                    Console.WriteLine(exp.Message.ToString());
                }

                //Prepared statement
                var updateQuery = "update emp set ename = :Name where empno = :ID";

                await using var Prepared_command = new EDBCommand(updateQuery, conn);
                Prepared_command.CommandType = CommandType.Text;
                Prepared_command.Parameters.Add(new EDBParameter("ID", EDBTypes.EDBDbType.Integer));
                Prepared_command.Parameters.Add(new EDBParameter("Name", EDBTypes.EDBDbType.Text));
                await Prepared_command.PrepareAsync();

                Prepared_command.Parameters[0].Value = 7369;
                Prepared_command.Parameters[1].Value = "Mark";

                await Prepared_command.ExecuteNonQueryAsync();
                Console.WriteLine("Record Updated...");

                //Close the connection
                await conn.CloseAsync();
            }

            catch (EDBException exp)
            {
                Console.WriteLine(exp.ToString());
            }
        }

        private static async Task EC_2716_Sample()
        {
            var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
            await using var dataSource = dataSourceBuilder.Build();

            await using var conn = await dataSource.OpenConnectionAsync();
            await using EDBCommand callable_command = new EDBCommand("emp_query(:p_deptno,:p_empno,:p_ename,:p_job,:p_hiredate,:p_sal)", conn);
            callable_command.CommandType = CommandType.StoredProcedure;
            callable_command.Parameters.Add(new EDBParameter("p_deptno", EDBTypes.EDBDbType.Numeric, 10, "p_deptno", ParameterDirection.Input, false, 2, 2, System.Data.DataRowVersion.Current, 20));
            callable_command.Parameters.Add(new EDBParameter("p_empno", EDBTypes.EDBDbType.Numeric, 10, "p_empno", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, 7369));
            callable_command.Parameters.Add(new EDBParameter("p_ename", EDBTypes.EDBDbType.Varchar, 10, "p_ename", ParameterDirection.InputOutput, false, 2, 2, System.Data.DataRowVersion.Current, "SMITH"));
            callable_command.Parameters.Add(new EDBParameter("p_job", EDBTypes.EDBDbType.Varchar, 10, "p_job", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            callable_command.Parameters.Add(new EDBParameter("p_hiredate", EDBTypes.EDBDbType.Date, 200, "p_hiredate", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            callable_command.Parameters.Add(new EDBParameter("p_sal", EDBTypes.EDBDbType.Numeric, 200, "p_sal", ParameterDirection.Output, false, 2, 2, System.Data.DataRowVersion.Current, null));
            callable_command.Prepare();
            callable_command.Parameters[0].Value = 20;
            callable_command.Parameters[1].Value = 7369;
            await using EDBDataReader result = callable_command.ExecuteReader();
            int fc = result.FieldCount;
            Console.WriteLine("Count: " + fc);
            while (await result.ReadAsync())
            {
            }
        }
    }
}
