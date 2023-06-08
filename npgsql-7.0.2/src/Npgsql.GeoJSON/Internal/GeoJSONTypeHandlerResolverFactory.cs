using System;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.GeoJSON.Internal;

public class GeoJSONTypeHandlerResolverFactory : TypeHandlerResolverFactory
{
    readonly GeoJSONOptions _options;
    readonly bool _geographyAsDefault;

    public GeoJSONTypeHandlerResolverFactory(GeoJSONOptions options, bool geographyAsDefault)
        => (_options, _geographyAsDefault) = (options, geographyAsDefault);

    public override TypeHandlerResolver Create(EDBConnector connector)
        => new GeoJSONTypeHandlerResolver(connector, _options, _geographyAsDefault);

    public override string? GetDataTypeNameByClrType(Type type)
        => GeoJSONTypeHandlerResolver.ClrTypeToDataTypeName(type, _geographyAsDefault);

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
        => GeoJSONTypeHandlerResolver.DoGetMappingByDataTypeName(dataTypeName);
}