using System;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.InternalTypeHandlers
{
    [TypeMapping("oidvector", EDBDbType.Oidvector)]
    class OIDVectorHandlerFactory : EDBTypeHandlerFactory
    {
        public override EDBTypeHandler CreateNonGeneric(PostgresType pgType, EDBConnection conn)
            => new OIDVectorHandler(pgType, conn.Connector!.TypeMapper.DatabaseInfo.ByName["oid"]
                                    ?? throw new EDBException("Two types called 'oid' defined in the database"));

        public override Type DefaultValueType => typeof(uint[]);
    }

    /// <summary>
    /// An OIDVector is simply a regular array of uints, with the sole exception that its lower bound must
    /// be 0 (we send 1 for regular arrays).
    /// </summary>
    class OIDVectorHandler : ArrayHandler<uint>
    {
        public OIDVectorHandler(PostgresType oidvectorType, PostgresType oidType)
            : base(oidvectorType, new UInt32Handler(oidType), 0) { }

        public override ArrayHandler CreateArrayHandler(PostgresArrayType arrayBackendType)
            => new ArrayHandler<ArrayHandler<uint>>(arrayBackendType, this);
    }
}
