using System;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;
using Newtonsoft.Json;
using EnterpriseDB.EDBClient.Json.Net.Internal;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient;

/// <summary>
/// Extension allowing adding the Json.NET plugin to an EDB type mapper.
/// </summary>
public static class EDBJsonNetExtensions
{
    /// <summary>
    /// Sets up JSON.NET mappings for the PostgreSQL json and jsonb types.
    /// </summary>
    /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
    /// <param name="jsonbClrTypes">A list of CLR types to map to PostgreSQL jsonb (no need to specify EDBDbType.Jsonb)</param>
    /// <param name="jsonClrTypes">A list of CLR types to map to PostgreSQL json (no need to specify EDBDbType.Json)</param>
    /// <param name="settings">Optional settings to customize JSON serialization</param>
    public static IEDBTypeMapper UseJsonNet(
        this IEDBTypeMapper mapper,
        Type[]? jsonbClrTypes = null,
        Type[]? jsonClrTypes = null,
        JsonSerializerSettings? settings = null)
    {
        mapper.AddTypeResolverFactory(new JsonNetTypeHandlerResolverFactory(jsonbClrTypes, jsonClrTypes, settings));
        return mapper;
    }
}