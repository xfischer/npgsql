using System;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;

namespace EnterpriseDB.EDBClient.NodaTime.Internal
{
    public class NodaTimeTypeHandlerResolverFactory : TypeHandlerResolverFactory
    {
        public override TypeHandlerResolver Create(EDBConnector connector)
            => new NodaTimeTypeHandlerResolver(connector);

        public override string? GetDataTypeNameByClrType(Type type)
            => NodaTimeTypeHandlerResolver.ClrTypeToDataTypeName(type);

        public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
            => NodaTimeTypeHandlerResolver.DoGetMappingByDataTypeName(dataTypeName);
    }
}
