using EnterpriseDB.EDBClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EDBSample
{
    internal static class TestUtil
    {
        internal static EDBDataSource BuildDataSource(string connectionString, ILoggerFactory loggerFactory)
        {
            var dataSourceBuilder = new EDBDataSourceBuilder(connectionString);
            dataSourceBuilder.UseLoggerFactory(loggerFactory);
            return dataSourceBuilder.Build();
        }

        public static string GetUniqueIdentifier(string prefix)
        => prefix + Interlocked.Increment(ref _counter);

        static int _counter;


        internal static IDisposable CreateTempPool(string origConnectionString, out string tempConnectionString)
        => CreateTempPool(new EDBConnectionStringBuilder(origConnectionString), out tempConnectionString);

        /// <summary>
        /// Creates a pool with a unique application name, usable for a single test, and returns an
        /// <see cref="IDisposable"/> to drop it at the end of the test.
        /// </summary>
        internal static IDisposable CreateTempPool(EDBConnectionStringBuilder builder, out string tempConnectionString)
        {
            builder.ApplicationName = (builder.ApplicationName ?? "TempPool") + Interlocked.Increment(ref _tempPoolCounter);
            tempConnectionString = builder.ConnectionString;
            return new PoolDisposer(tempConnectionString);
        }

        static volatile int _tempPoolCounter;

        readonly struct PoolDisposer : IDisposable
        {
            readonly string _connectionString;

            internal PoolDisposer(string connectionString) => _connectionString = connectionString;

            public void Dispose()
            {
                var conn = new EDBConnection(_connectionString);
                EDBConnection.ClearPool(conn);
            }
        }
    }
}
