using EnterpriseDB.EDBClient.RawPostgis;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient{
    /// <summary>
    /// Extension adding the legacy PostGIS types to an EDB type mapper.
    /// </summary>
    public static class EDBRawPostgisExtensions
    {
        /// <summary>
        /// Sets up the legacy PostGIS types to an EDB type mapper.
        /// </summary>
        /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
        public static IEDBTypeMapper UseRawPostgis(this IEDBTypeMapper mapper)
            => mapper
                .AddMapping(new EDBTypeMappingBuilder
                {
                    PgTypeName = "geometry",
                    EDBDbType = EDBDbType.Geometry,
                    TypeHandlerFactory = new PostgisRawHandlerFactory()
                }.Build());
    }
}
