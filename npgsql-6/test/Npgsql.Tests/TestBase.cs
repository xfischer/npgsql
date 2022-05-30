using System;
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

        static SemaphoreSlim DatabaseCreationLock = new(1);

        #region Utilities for use by tests

        protected virtual EDBConnection CreateConnection(string? connectionString = null)
            => new(connectionString ?? ConnectionString);

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

        ValueTask<EDBConnection> OpenConnection(string? connectionString, bool async)
        {
            return OpenConnectionInternal(hasLock: false);

            async ValueTask<EDBConnection> OpenConnectionInternal(bool hasLock)
            {
                var conn = CreateConnection(connectionString);
                try
                {
                    if (async)
                        await conn.OpenAsync();
                    else
                        conn.Open();
                    return conn;
                }
                catch (PostgresException e)
                {
                    if (e.SqlState == PostgresErrorCodes.InvalidPassword && connectionString == TestUtil.DefaultConnectionString)
                        throw new Exception("Please create a user npgsql_tests as follows: CREATE USER npgsql_tests PASSWORD 'npgsql_tests' SUPERUSER");

                    if (e.SqlState == PostgresErrorCodes.InvalidCatalogName)
                    {
                        if (!hasLock)
                        {
                            DatabaseCreationLock.Wait();
                            try
                            {
                                return await OpenConnectionInternal(hasLock: true);
                            }
                            finally
                            {
                                DatabaseCreationLock.Release();
                            }
                        }

                        // Database does not exist and we have the lock, proceed to creation
                        var builder = new EDBConnectionStringBuilder(connectionString ?? ConnectionString)
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

                        if (async)
                            await conn.OpenAsync();
                        else
                            conn.Open();
                        return conn;
                    }

                    throw;
                }
            }
        }

        protected EDBConnection OpenConnection(EDBConnectionStringBuilder csb)
            => OpenConnection(csb.ToString());

        protected virtual ValueTask<EDBConnection> OpenConnectionAsync(EDBConnectionStringBuilder csb)
            => OpenConnectionAsync(csb.ToString());

        // In PG under 9.1 you can't do SELECT pg_sleep(2) in binary because that function returns void and PG doesn't know
        // how to transfer that. So cast to text server-side.
        protected static EDBCommand CreateSleepCommand(EDBConnection conn, int seconds = 1000)
            => new($"SELECT pg_sleep({seconds}){(conn.PostgreSqlVersion < new Version(9, 1, 0) ? "::TEXT" : "")}", conn);

        protected bool IsRedshift => new EDBConnectionStringBuilder(ConnectionString).ServerCompatibilityMode == ServerCompatibilityMode.Redshift;

        #endregion
    }
}
