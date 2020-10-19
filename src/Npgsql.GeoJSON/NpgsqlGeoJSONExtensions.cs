using System;
using System.Data;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using EnterpriseDB.EDBClient.GeoJSON;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Extension allowing adding the GeoJSON plugin to an EnterpriseDB.EDBClient type mapper.
    /// </summary>
    public static class EDBGeoJSONExtensions
    {
        static readonly Type[] ClrTypes = new[]
        {
            typeof(GeoJSONObject), typeof(IGeoJSONObject), typeof(IGeometryObject),
            typeof(Point), typeof(LineString), typeof(Polygon),
            typeof(MultiPoint), typeof(MultiLineString), typeof(MultiPolygon),
            typeof(GeometryCollection)
        };

        /// <summary>
        /// Sets up GeoJSON mappings for the PostGIS types.
        /// </summary>
        /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
        /// <param name="options">Options to use when constructing objects.</param>
        /// <param name="geographyAsDefault">Specifies that the geography type is used for mapping by default.</param>
        public static IEDBTypeMapper UseGeoJson(this IEDBTypeMapper mapper, GeoJSONOptions options = GeoJSONOptions.None, bool geographyAsDefault = false)
        {
            var factory = new GeoJSONHandlerFactory(options);
            return mapper
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "geometry",
                    EDBDbType = EDBDbType.Geometry,
                    ClrTypes = geographyAsDefault ? Type.EmptyTypes : ClrTypes,
                    InferredDbType = DbType.Object,
                    TypeHandlerFactory = factory
                }.Build())
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "geography",
                    EDBDbType = EDBDbType.Geography,
                    ClrTypes = geographyAsDefault ? ClrTypes : Type.EmptyTypes,
                    InferredDbType = DbType.Object,
                    TypeHandlerFactory = factory
                }.Build());
        }
    }
}
