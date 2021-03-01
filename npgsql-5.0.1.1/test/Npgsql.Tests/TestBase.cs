using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.Tests
{
    public abstract class TestBase
    {
        /// <summary>
        /// The connection string that will be used when opening the connection to the tests database.
        /// May be overridden in fixtures, e.g. to set special connection parameters
        /// </summary>
        public virtual string ConnectionString => TestUtil.ConnectionString;

        #region Utilities for use by tests

        protected virtual EDBConnection CreateConnection(string? connectionString = null)
            => new EDBConnection(connectionString ?? ConnectionString);

        protected virtual EDBConnection CreateConnection(Action<EDBConnectionStringBuilder> builderAction)
        {
            var builder = new EDBConnectionStringBuilder(ConnectionString);
            builderAction(builder);
            return new EDBConnection(builder.ConnectionString);
        }

        protected virtual EDBConnection OpenConnection(string? connectionString = null)
            => OpenConnection(connectionString, async: false).GetAwaiter().GetResult();

        protected virtual EDBConnection OpenConnection(Action<EDBConnectionStringBuilder> builderAction)
        {
            var builder = new EDBConnectionStringBuilder(ConnectionString);
            builderAction(builder);
            return OpenConnection(builder.ConnectionString, async: false).GetAwaiter().GetResult();
        }

        protected virtual ValueTask<EDBConnection> OpenConnectionAsync(string? connectionString = null)
            => OpenConnection(connectionString, async: true);

        protected virtual ValueTask<EDBConnection> OpenConnectionAsync(
            Action<EDBConnectionStringBuilder> builderAction)
        {
            var builder = new EDBConnectionStringBuilder(ConnectionString);
            builderAction(builder);
            return OpenConnection(builder.ConnectionString, async: true);
        }

        async ValueTask<EDBConnection> OpenConnection(string? connectionString, bool async)
        {
            var conn = CreateConnection(connectionString);
            try
            {
                if (async)
                    await conn.OpenAsync();
                else
                    conn.Open();
            }
            catch (PostgresException e)
            {
                if (e.SqlState == PostgresErrorCodes.InvalidCatalogName)
                    TestUtil.IgnoreExceptOnBuildServer("Please create a database EDB_tests, owned by user EDB_tests");
                else if (e.SqlState == PostgresErrorCodes.InvalidPassword && connectionString == TestUtil.DefaultConnectionString)
                    TestUtil.IgnoreExceptOnBuildServer("Please create a user EDB_tests as follows: create user EDB_tests with password 'EDB_tests'");
                else
                    throw;
            }

            return conn;
        }

        protected EDBConnection OpenConnection(EDBConnectionStringBuilder csb)
            => OpenConnection(csb.ToString());

        protected virtual ValueTask<EDBConnection> OpenConnectionAsync(EDBConnectionStringBuilder csb)
            => OpenConnectionAsync(csb.ToString());

        // In PG under 9.1 you can't do SELECT pg_sleep(2) in binary because that function returns void and PG doesn't know
        // how to transfer that. So cast to text server-side.
        protected static EDBCommand CreateSleepCommand(EDBConnection conn, int seconds = 1000)
            => new EDBCommand($"SELECT pg_sleep({seconds}){(conn.PostgreSqlVersion < new Version(9, 1, 0) ? "::TEXT" : "")}", conn);

        protected bool IsRedshift => new EDBConnectionStringBuilder(ConnectionString).ServerCompatibilityMode == ServerCompatibilityMode.Redshift;

        #endregion
    }
}
