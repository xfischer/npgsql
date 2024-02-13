global using global::System;
global using global::System.Threading.Tasks;

using EnterpriseDB.EDBClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;
using System.Threading;

namespace EDBSample
{

    // Sample program to test "default" EDB samples and run tests outside of test
    // environment, easier debugging
    internal class Program
    {
        //static string connectionString = "Server=localhost;Port=5444;User Id=enterprisedb;Password=edb;Database=edb";
        static string connectionString = "port=5433;Server=localhost;Username=npgsql_tests;Password=npgsql_tests;Database=npgsql_tests;Timeout=0;Command Timeout=0;SSL Mode=Disable";

        static ILoggerFactory _loggerFactory;
        static ILogger _logger;

        static async Task Main(string[] args)
        {
            InitLogging();

            try
            {
                _logger.LogDebug("---- Starting");
                //await Timeout_async_soft();
                //Bad_database();
                //Invalid_constringParams();
                //await WaitAsync_CancellationSample();
                await Sample();            
                //await EC_2716_ExecuteNonQueryAsync();
                //await EC_2716_ExecuteReaderAsync();
                _logger.LogDebug("---- end");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error");
            }

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
                    for (var i = 0; i < (fc + 1); i++)
                        Console.WriteLine("RESULT[" + i + "]=" + Convert.ToString(callable_command.Parameters[i].Value));
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

        private static void Bad_database()
        {
            var builder = new EDBConnectionStringBuilder(connectionString)
            {
                Database = "does_not_exist"
            };
            using (TestUtil.CreateTempPool(builder, out var connectionString))
            using (var conn = new EDBConnection(connectionString))
            {

                try
                {
                    conn.Open();
                }
                catch (PostgresException pe)
                    when (pe.SqlState == PostgresErrorCodes.InvalidCatalogName)
                {
                    _logger.LogInformation("OK got PostgresException with InvalidCatalogName");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Unexpected exception : {e.GetType().Name}");
                }
            }

        }

        private static void Invalid_constringParams()
        {
            var conn = new EDBConnection("Server=127.0.0.1;User Id=EDB_tests;Password=j");

            var command = new EDBCommand("select * from tablea", conn);

            try
            {
                command.Connection.Open();
            }
            catch (PostgresException pe)
            {
                _logger.LogInformation("OK got PostgresException");
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception : {e.GetType().Name}");
            }

        }
        private static async Task WaitAsync_CancellationSample()
        {
            var notify = TestUtil.GetUniqueIdentifier(nameof(Program));
            await using var dataSource = TestUtil.BuildDataSource(connectionString, _loggerFactory);

            //using (var conn = await dataSource.OpenConnectionAsync())
            //{
            //    try
            //    {
            //        await conn.WaitAsync(new CancellationToken(true));
            //    }
            //    catch(OperationCanceledException ex)
            //    {
            //        // ok
            //       _logger.LogDebug("OK (got OperationCanceledException)");
            //    }
            //    catch(Exception ex)
            //    {
            //        // problem
            //        _logger.LogError("Bad (got other exception)");
            //    }
            //    await using var command = new EDBCommand("SELECT 1", conn);
            //    var result = await command.ExecuteScalarAsync();
            //    _logger.LogDebug("Result should be 1 and is {result}", result);
            //}

            using (var conn = await dataSource.OpenConnectionAsync())
            {
                await using var command = new EDBCommand($"LISTEN {notify}", conn);
                await command.ExecuteNonQueryAsync();
                var cts = new CancellationTokenSource(1000);
                cts.Token.Register(() =>
                {
                    _logger.LogDebug("CancellationTokenSource fired");
                });
                try
                {
                    await conn.WaitAsync(cts.Token);
                }
                catch (OperationCanceledException ex)
                {
                    // ok
                    _logger.LogDebug("OK (got OperationCanceledException)");
                }
                catch (Exception ex)
                {
                    // problem
                    _logger.LogError("Bad (got other exception)");
                }
                await using var command2 = new EDBCommand("SELECT 1", conn);
                var result = await command2.ExecuteScalarAsync();
                _logger.LogDebug("Result should be 1 and is {result}", result);
            }
        }

        public static async Task Timeout_async_soft()
        {
            var builder = new EDBConnectionStringBuilder(connectionString)
            {
                Pooling = false,
                Multiplexing = false,
                Database = "postgres",
                CommandTimeout = 1
            };
            using var conn = new EDBConnection(builder.ConnectionString);
            await conn.OpenAsync();
            using var cmd = CreateSleepCommand(conn, 10);

            try
            {
                var rows = await cmd.ExecuteNonQueryAsync();
            }
            catch (EDBException ex)
            {
                // ok
                if (ex.InnerException is TimeoutException)
                {
                    _logger.LogDebug("OK (got EDBException with inner TimeoutException)");
                }
                else
                {
                    _logger.LogError("Bad (got other exception)");
                }
            }
            catch (Exception ex)
            {
                // problem
                _logger.LogError("Bad (got other exception)");
            }

            if (conn.FullState == ConnectionState.Open)
            {
                _logger.LogDebug("OK ConnectionState is open");
            }
            else
            {
                _logger.LogError($"BAD ConnectionState is {conn.FullState}");
            }
        }
        protected static EDBCommand CreateSleepCommand(EDBConnection conn, int seconds = 1000)
       => new($"SELECT pg_sleep({seconds}){(conn.PostgreSqlVersion < new Version(9, 1, 0) ? "::TEXT" : "")}", conn);

        private static void InitLogging(LogLevel logLevel = LogLevel.Warning)
        {
            _loggerFactory = LoggerFactory.Create(builder => builder
               .SetMinimumLevel(logLevel)
               //.AddSimpleConsole(options =>
               //{
               //    options.SingleLine = true;
               //    options.TimestampFormat = "yyyy/MM/dd HH:mm:ss ";
               //})
               //.AddConsole(options =>
               //{
               //    options.MaxQueueLength = 1;
               //    options.QueueFullMode = Microsoft.Extensions.Logging.Console.ConsoleLoggerQueueFullMode.Wait;
               //})
               .AddSystemdConsole(options =>
               {
                   options.TimestampFormat = "yyyy/MM/dd HH:mm:ss ";
               }).AddDebug()
               );

            _logger = _loggerFactory.CreateLogger("Sample");

            EDBLoggingConfiguration.InitializeLogging(_loggerFactory);
        }


    }
}
