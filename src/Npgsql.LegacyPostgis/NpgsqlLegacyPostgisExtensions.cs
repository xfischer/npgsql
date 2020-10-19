using System;
using System.Data;
using EnterpriseDB.EDBClient.LegacyPostgis;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Extension adding the legacy PostGIS types to an EnterpriseDB.EDBClient type mapper.
    /// </summary>
    public static class EDBLegacyPostgisExtensions
    {
        /// <summary>
        /// Sets up the legacy PostGIS types to an EnterpriseDB.EDBClient type mapper.
        /// </summary>
        /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
        public static IEDBTypeMapper UseLegacyPostgis(this IEDBTypeMapper mapper)
        {
            var typeHandlerFactory = new LegacyPostgisHandlerFactory();

            return mapper
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "geometry",
                    EDBDbType = EDBDbType.Geometry,
                    ClrTypes = new[]
                    {
                        typeof(PostgisGeometry),
                        typeof(PostgisPoint),
                        typeof(PostgisMultiPoint),
                        typeof(PostgisLineString),
                        typeof(PostgisMultiLineString),
                        typeof(PostgisPolygon),
                        typeof(PostgisMultiPolygon),
                        typeof(PostgisGeometryCollection),
                    },
                    TypeHandlerFactory = typeHandlerFactory
                }.Build())
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "geography",
                    EDBDbType = EDBDbType.Geography,
                    DbTypes = new DbType[0],
                    ClrTypes = new Type[0],
                    InferredDbType = DbType.Object,
                    TypeHandlerFactory = typeHandlerFactory
                }.Build());
        }
    }
}
