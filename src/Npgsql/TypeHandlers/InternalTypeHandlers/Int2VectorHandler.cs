using System;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.InternalTypeHandlers
{
    [TypeMapping("int2vector", EDBDbType.Int2Vector)]
    class Int2VectorHandlerFactory : EDBTypeHandlerFactory
    {
        public override EDBTypeHandler CreateNonGeneric(PostgresType pgType, EDBConnection conn)
            => new Int2VectorHandler(pgType, conn.Connector!.TypeMapper.DatabaseInfo.ByName["smallint"]
                                             ?? throw new EDBException("Two types called 'smallint' defined in the database"));

        public override Type DefaultValueType => typeof(short[]);
    }

    /// <summary>
    /// An int2vector is simply a regular array of shorts, with the sole exception that its lower bound must
    /// be 0 (we send 1 for regular arrays).
    /// </summary>
    class Int2VectorHandler : ArrayHandler<short>
    {
        public Int2VectorHandler(PostgresType arrayPostgresType, PostgresType postgresShortType)
            : base(arrayPostgresType, new Int16Handler(postgresShortType), 0) { }

        public override ArrayHandler CreateArrayHandler(PostgresArrayType arrayBackendType)
            => new ArrayHandler<ArrayHandler<short>>(arrayBackendType, this);
    }
}
