using System;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeMapping
{
    public interface IUserTypeMapping
    {
        public string PgTypeName { get; }
        public Type ClrType { get; }

        public EDBTypeHandler CreateHandler(PostgresType pgType, EDBConnector connector);
    }
}
