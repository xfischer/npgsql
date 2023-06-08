using System;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.NumericHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.InternalTypeHandlers;

/// <summary>
/// An OIDVector is simply a regular array of uints, with the sole exception that its lower bound must
/// be 0 (we send 1 for regular arrays).
/// </summary>
sealed class OIDVectorHandler : ArrayHandler<uint>
{
    public OIDVectorHandler(PostgresType oidvectorType, PostgresType oidType)
        : base(oidvectorType, new UInt32Handler(oidType), ArrayNullabilityMode.Never, 0) { }

    public override EDBTypeHandler CreateArrayHandler(PostgresArrayType pgArrayType, ArrayNullabilityMode arrayNullabilityMode)
        => new ArrayHandler<ArrayHandler<uint>>(pgArrayType, this, arrayNullabilityMode);
}