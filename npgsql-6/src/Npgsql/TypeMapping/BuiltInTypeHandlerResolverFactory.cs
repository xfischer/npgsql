using System;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;

namespace EnterpriseDB.EDBClient.TypeMapping
{
    class BuiltInTypeHandlerResolverFactory : TypeHandlerResolverFactory
    {
        public override TypeHandlerResolver Create(EDBConnector connector)
            => new BuiltInTypeHandlerResolver(connector);

        public override string? GetDataTypeNameByClrType(Type clrType)
            => BuiltInTypeHandlerResolver.ClrTypeToDataTypeName(clrType);

        public override string? GetDataTypeNameByValueDependentValue(object value)
            => BuiltInTypeHandlerResolver.ValueDependentValueToDataTypeName(value);

        public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
            => BuiltInTypeHandlerResolver.DoGetMappingByDataTypeName(dataTypeName);
    }
}
