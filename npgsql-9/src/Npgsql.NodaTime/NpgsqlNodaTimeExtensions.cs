using EnterpriseDB.EDBClient.NodaTime.Internal;
using EnterpriseDB.EDBClient.TypeMapping;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient;

/// <summary>
/// Extension adding the NodaTime plugin to an EDB type mapper.
/// </summary>
public static class EDBNodaTimeExtensions
{
    // Note: defined for binary compatibility and EDBConnection.GlobalTypeMapper.
    /// <summary>
    /// Sets up NodaTime mappings for the PostgreSQL date/time types.
    /// </summary>
    /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
    public static IEDBTypeMapper UseNodaTime(this IEDBTypeMapper mapper)
    {
        mapper.AddTypeInfoResolverFactory(new NodaTimeTypeInfoResolverFactory());
        return mapper;
    }

    /// <summary>
    /// Sets up NodaTime mappings for the PostgreSQL date/time types.
    /// </summary>
    /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
    public static TMapper UseNodaTime<TMapper>(this TMapper mapper) where TMapper : IEDBTypeMapper
    {
        mapper.AddTypeInfoResolverFactory(new NodaTimeTypeInfoResolverFactory());
        return mapper;
    }
}