using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Json.NET.Internal
{
    public class JsonNetTypeHandlerResolver : TypeHandlerResolver
    {
        readonly EDBDatabaseInfo _databaseInfo;
        readonly JsonbHandler _jsonbHandler;
        readonly JsonHandler _jsonHandler;
        readonly Dictionary<Type, string> _dataTypeNamesByClrType;

        internal JsonNetTypeHandlerResolver(
            EDBConnector connector,
            Dictionary<Type, string> dataClrTypeNamesDataTypeNamesByClrClrType,
            JsonSerializerSettings settings)
        {
            _databaseInfo = connector.DatabaseInfo;

            _jsonbHandler = new JsonbHandler(PgType("jsonb"), connector, settings);
            _jsonHandler = new JsonHandler(PgType("json"), connector, settings);

            _dataTypeNamesByClrType = dataClrTypeNamesDataTypeNamesByClrClrType;
        }

        public EDBTypeHandler? ResolveEDBDbType(EDBDbType npgsqlDbType)
            => npgsqlDbType switch
            {
                EDBDbType.Jsonb => _jsonbHandler,
                EDBDbType.Json => _jsonHandler,
                _ => null
            };

        public override EDBTypeHandler? ResolveByDataTypeName(string typeName)
            => typeName switch
            {
                "jsonb" => _jsonbHandler,
                "json" => _jsonHandler,
                _ => null
            };

        public override EDBTypeHandler? ResolveByClrType(Type type)
            => ClrTypeToDataTypeName(type, _dataTypeNamesByClrType) is { } dataTypeName && ResolveByDataTypeName(dataTypeName) is { } handler
                ? handler
                : null;

        internal static string? ClrTypeToDataTypeName(Type type, Dictionary<Type, string> clrTypes)
            => clrTypes.TryGetValue(type, out var dataTypeName) ? dataTypeName : null;

        public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
            => DoGetMappingByDataTypeName(dataTypeName);

        internal static TypeMappingInfo? DoGetMappingByDataTypeName(string dataTypeName)
            => dataTypeName switch
            {
                "jsonb" => new(EDBDbType.Jsonb,   "jsonb"),
                "json"  => new(EDBDbType.Json,    "json"),
                _ => null
            };

        PostgresType PgType(string pgTypeName) => _databaseInfo.GetPostgresTypeByName(pgTypeName);
    }
}
