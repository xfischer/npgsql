using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EDBTypes;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.Util;

namespace EnterpriseDB.EDBClient{
    class PostgresMinimalDatabaseInfoFactory : IEDBDatabaseInfoFactory
    {
        public Task<EDBDatabaseInfo?> Load(EDBConnection conn, EDBTimeout timeout, bool async)
            => Task.FromResult(
                new EDBConnectionStringBuilder(conn.ConnectionString).ServerCompatibilityMode == ServerCompatibilityMode.NoTypeLoading
                    ? (EDBDatabaseInfo)new PostgresMinimalDatabaseInfo(conn)
                    : null
            );
    }

    class PostgresMinimalDatabaseInfo : PostgresDatabaseInfo
    {
        static readonly PostgresBaseType[] Types = typeof(EDBDbType).GetFields()
            .Select(f => f.GetCustomAttribute<BuiltInPostgresType>())
            .OfType<BuiltInPostgresType>()
            .Select(a => new PostgresBaseType("pg_catalog", a.Name, a.OID))
            .ToArray();

        protected override IEnumerable<PostgresType> GetTypes() => Types;

        internal PostgresMinimalDatabaseInfo(EDBConnection conn)
            : base(conn)
        {
            HasIntegerDateTimes = !conn.PostgresParameters.TryGetValue("integer_datetimes", out var intDateTimes) ||
                                  intDateTimes == "on";
        }
    }
}
