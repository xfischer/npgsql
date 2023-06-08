using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.Json.Net.Internal;

public class JsonNetTypeHandlerResolverFactory : TypeHandlerResolverFactory
{
    readonly Type[] _jsonbClrTypes;
    readonly Type[] _jsonClrTypes;
    readonly JsonSerializerSettings _settings;
    readonly Dictionary<Type, string> _byType;

    public JsonNetTypeHandlerResolverFactory(
        Type[]? jsonbClrTypes,
        Type[]? jsonClrTypes,
        JsonSerializerSettings? settings)
    {
        _jsonbClrTypes = jsonbClrTypes ?? Array.Empty<Type>();
        _jsonClrTypes = jsonClrTypes ?? Array.Empty<Type>();
        _settings = settings ?? new JsonSerializerSettings();

        _byType = new();

        if (jsonbClrTypes is not null)
            foreach (var type in jsonbClrTypes)
                _byType[type] = "jsonb";

        if (jsonClrTypes is not null)
            foreach (var type in jsonClrTypes)
                _byType[type] = "json";
    }

    public override TypeHandlerResolver Create(EDBConnector connector)
        => new JsonNetTypeHandlerResolver(connector, _byType, _settings);

    public override string? GetDataTypeNameByClrType(Type type)
        => JsonNetTypeHandlerResolver.ClrTypeToDataTypeName(type, _byType);

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
        => JsonNetTypeHandlerResolver.DoGetMappingByDataTypeName(dataTypeName);

}