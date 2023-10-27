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
                await Timeout_async_soft();
                //Bad_database();
                //Invalid_constringParams();
                //await WaitAsync_CancellationSample();
                //await Sample();            
                //await EC_2716_ExecuteNonQueryAsync();
                //await EC_2716_ExecuteReaderAsync();
                _logger.LogDebug("---- end");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error");
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
