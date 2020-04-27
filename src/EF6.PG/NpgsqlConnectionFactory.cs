using System.Data.Common;
using System.Data.Entity.Infrastructure;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Instances of this class are used to create DbConnection objects for Postgresql
    /// </summary>
    public class EDBConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// Creates a connection for Postgresql for the given connection string.
        /// </summary>
        /// <param name="nameOrConnectionString">The connection string.</param>
        /// <returns>An initialized DbConnection.</returns>
        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            return new EDBConnection(nameOrConnectionString);
        }
    }
}
