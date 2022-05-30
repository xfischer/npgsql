using System;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.NumericHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.InternalTypeHandlers
{
    /// <summary>
    /// An int2vector is simply a regular array of shorts, with the sole exception that its lower bound must
    /// be 0 (we send 1 for regular arrays).
    /// </summary>
    class Int2VectorHandler : ArrayHandler<short>
    {
        public Int2VectorHandler(PostgresType arrayPostgresType, PostgresType postgresShortType)
            : base(arrayPostgresType, new Int16Handler(postgresShortType), ArrayNullabilityMode.Never, 0) { }

        public override EDBTypeHandler CreateArrayHandler(PostgresArrayType pgArrayType, ArrayNullabilityMode arrayNullabilityMode)
            => new ArrayHandler<ArrayHandler<short>>(pgArrayType, this, arrayNullabilityMode);
    }
}
