using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// Extension allowing adding the IS TABLE OF type support plugin to an EDB type mapper.
/// </summary>
public static class EDBTableOfExtensions
{
    /// <summary>
    /// Sets up ArrayList mappings for the IS TABLE OF types.
    /// </summary>
    /// <param name="mapper">The type mapper to set up (global or connection-specific)</param>
    /// <param name="knownTableOfTypes">Specifies the datatype names that should be resolved as IS TABLE OF types.</param>
    public static IEDBTypeMapper UseEDBIsTableOf(this IEDBTypeMapper mapper, params string[] knownTableOfTypes)
    {
        mapper.AddTypeInfoResolverFactory(new EDBTableOfResolverFactory(knownTableOfTypes));
        return mapper;
    }
}
