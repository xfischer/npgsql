using EnterpriseDB.EDBClient.GeoJSON;
using EnterpriseDB.EDBClient.GeoJSON.Internal;
using EnterpriseDB.EDBClient.TypeMapping;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient;

/// <summary>
/// Extension allowing adding the GeoJSON plugin to an EDB type mapper.
/// </summary>
public static class EDBGeoJSONExtensions
{
    // Note: defined for binary compatibility and EDBConnection.GlobalTypeMapper.
    /// <summary>
    /// Sets up GeoJSON mappings for the PostGIS types.
    /// </summary>
    /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
    /// <param name="options">Options to use when constructing objects.</param>
    /// <param name="geographyAsDefault">Specifies that the geography type is used for mapping by default.</param>
    public static IEDBTypeMapper UseGeoJson(this IEDBTypeMapper mapper, GeoJSONOptions options = GeoJSONOptions.None, bool geographyAsDefault = false)
    {
        mapper.AddTypeInfoResolverFactory(new GeoJSONTypeInfoResolverFactory(options, geographyAsDefault, crsMap: null));
        return mapper;
    }

    // Note: defined for binary compatibility and EDBConnection.GlobalTypeMapper.
    /// <summary>
    /// Sets up GeoJSON mappings for the PostGIS types.
    /// </summary>
    /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
    /// <param name="crsMap">A custom crs map that might contain more or less entries than the default well-known crs map.</param>
    /// <param name="options">Options to use when constructing objects.</param>
    /// <param name="geographyAsDefault">Specifies that the geography type is used for mapping by default.</param>
    public static IEDBTypeMapper UseGeoJson(this IEDBTypeMapper mapper, CrsMap crsMap, GeoJSONOptions options = GeoJSONOptions.None, bool geographyAsDefault = false)
    {
        mapper.AddTypeInfoResolverFactory(new GeoJSONTypeInfoResolverFactory(options, geographyAsDefault, crsMap));
        return mapper;
    }

    /// <summary>
    /// Sets up GeoJSON mappings for the PostGIS types.
    /// </summary>
    /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
    /// <param name="options">Options to use when constructing objects.</param>
    /// <param name="geographyAsDefault">Specifies that the geography type is used for mapping by default.</param>
    public static TMapper UseGeoJson<TMapper>(this TMapper mapper, GeoJSONOptions options = GeoJSONOptions.None, bool geographyAsDefault = false)
        where TMapper : IEDBTypeMapper
    {
        mapper.AddTypeInfoResolverFactory(new GeoJSONTypeInfoResolverFactory(options, geographyAsDefault, crsMap: null));
        return mapper;
    }

    /// <summary>
    /// Sets up GeoJSON mappings for the PostGIS types.
    /// </summary>
    /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
    /// <param name="crsMap">A custom crs map that might contain more or less entries than the default well-known crs map.</param>
    /// <param name="options">Options to use when constructing objects.</param>
    /// <param name="geographyAsDefault">Specifies that the geography type is used for mapping by default.</param>
    public static TMapper UseGeoJson<TMapper>(this TMapper mapper, CrsMap crsMap, GeoJSONOptions options = GeoJSONOptions.None, bool geographyAsDefault = false)
        where TMapper : IEDBTypeMapper
    {
        mapper.AddTypeInfoResolverFactory(new GeoJSONTypeInfoResolverFactory(options, geographyAsDefault, crsMap));
        return mapper;
    }
}
