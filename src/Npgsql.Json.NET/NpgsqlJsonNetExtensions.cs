using System;
using EnterpriseDB.EDBClient.Json.NET;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient{
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
            JsonSerializerSettings? settings = null
        )
        {
            mapper.AddMapping(new EDBTypeMappingBuilder
            {
                PgTypeName = "jsonb",
                EDBDbType = EDBDbType.Jsonb,
                ClrTypes = jsonbClrTypes,
                TypeHandlerFactory = new JsonbHandlerFactory(settings)
            }.Build());

            mapper.AddMapping(new EDBTypeMappingBuilder
            {
                PgTypeName = "json",
                EDBDbType = EDBDbType.Json,
                ClrTypes = jsonClrTypes,
                TypeHandlerFactory = new JsonHandlerFactory(settings)
            }.Build());

            return mapper;
        }
    }
}
