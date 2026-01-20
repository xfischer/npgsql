//#define EDB_DIAGNOSTICS

using Microsoft.Extensions.Logging;
using EnterpriseDB.EDBClient;
using EnterpriseDB.EDBClient.Tests;
using NUnit.Framework;
using System;
using System.Threading;

[SetUpFixture]
public class AssemblySetUp
{
    [OneTimeSetUp]
    public void Setup()
    {
        var loggerFactory = LoggerFactory.Create(c => c
            .AddProvider(new NUnitLoggerProvider())
#if DEBUG || EDB_DIAGNOSTICS
            .AddDebug()
            .SetMinimumLevel(LogLevel.Trace)
#else
            .SetMinimumLevel(LogLevel.Warning)
#endif
            );
        EDBLoggingConfiguration.InitializeLogging(loggerFactory);

        var connString = TestUtil.ConnectionString;
        using var conn = new EDBConnection(connString);
        try
        {
            conn.Open();
        }
        catch (PostgresException e)
        {
            if (e.SqlState == PostgresErrorCodes.InvalidPassword && connString == TestUtil.DefaultConnectionString)
                throw new Exception("Please create a user npgsql_tests as follows: CREATE USER npgsql_tests PASSWORD 'npgsql_tests' SUPERUSER");

            if (e.SqlState == PostgresErrorCodes.InvalidCatalogName)
            {
                var builder = new EDBConnectionStringBuilder(connString)
                {
                    Pooling = false,
                    Multiplexing = false,
                    Database = "postgres"
                };

                using var adminConn = new EDBConnection(builder.ConnectionString);
                adminConn.Open();
                adminConn.ExecuteNonQuery("CREATE DATABASE " + conn.Database);
                adminConn.Close();
                Thread.Sleep(1000);

                conn.Open();
                return;
            }

            throw;
        }
    }
}

public class NUnitLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => TestLogger.Create(categoryName);
    public void Dispose() => throw new NotImplementedException();
}
public static class TestLogger
{
    public static ILogger Create(string categoryName)
    {
        var logger = new NUnitLogger(categoryName);
        return logger;
    }

    class NUnitLogger : ILogger, IDisposable
    {
        private readonly Action<string> output = Console.WriteLine;
        private string categoryName;

        public NUnitLogger(string categoryName) => this.categoryName = categoryName;

        public void Dispose()
        {
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => output($"{categoryName}: {formatter(state, exception)}");

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this;
    }
}
