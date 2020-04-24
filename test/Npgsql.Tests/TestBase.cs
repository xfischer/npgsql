using System;

namespace EnterpriseDB.EDBClient.Tests
{
    public abstract class TestBase
    {
        /// <summary>
        /// The connection string that will be used when opening the connection to the tests database.
        /// May be overridden in fixtures, e.g. to set special connection parameters
        /// </summary>
        public static string ConnectionString =>
            Environment.GetEnvironmentVariable("EDB_TEST_DB") ?? DefaultConnectionString;

        /// <summary>
        /// Unless the EDB_TEST_DB environment variable is defined, this is used as the connection string for the
        /// test database.
        /// </summary>
        const string DefaultConnectionString = "Server=192.168.182.131;port=5444;Username=zahidk;Password=edb;Database=edb;Timeout=0;Command Timeout=0";

        #region Utilities for use by tests

        protected virtual EDBConnection OpenConnection(string? connectionString = null)
        {
            if (connectionString == null)
                connectionString = ConnectionString;
            var conn = new EDBConnection(connectionString);
            try
            {
                conn.Open();
            }
            catch (PostgresException e)
            {
                if (e.SqlState == PostgresErrorCodes.InvalidCatalogName)
                    TestUtil.IgnoreExceptOnBuildServer("Please create a database EDB_tests, owned by user EDB_tests");
                else if (e.SqlState == PostgresErrorCodes.InvalidPassword && connectionString == DefaultConnectionString)
                    TestUtil.IgnoreExceptOnBuildServer("Please create a user EDB_tests as follows: create user EDB_tests with password 'EDB_tests'");
                else
                    throw;
            }

            return conn;
        }

        protected EDBConnection OpenConnection(EDBConnectionStringBuilder csb)
            => OpenConnection(csb.ToString());

        // In PG under 9.1 you can't do SELECT pg_sleep(2) in binary because that function returns void and PG doesn't know
        // how to transfer that. So cast to text server-side.
        protected static EDBCommand CreateSleepCommand(EDBConnection conn, int seconds = 1000)
            => new EDBCommand($"SELECT pg_sleep({seconds}){(conn.PostgreSqlVersion < new Version(9, 1, 0) ? "::TEXT" : "")}", conn);

        protected bool IsRedshift => new EDBConnectionStringBuilder(ConnectionString).ServerCompatibilityMode == ServerCompatibilityMode.Redshift;

        #endregion

        public static EDBConnection openDBwithoutPooling()
        {
            try
            {
                EDBConnection con = new EDBConnection(ConnectionString);
                con.Open();
                return con;
            }
            catch (EDBException e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}
