using Microsoft.CodeAnalysis;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class NpgsqlPrecompiledQueryTestHelpers : PrecompiledQueryTestHelpers
{
    public static NpgsqlPrecompiledQueryTestHelpers Instance = new();

    protected override IEnumerable<MetadataReference> BuildProviderMetadataReferences()
    {
        yield return MetadataReference.CreateFromFile(typeof(NpgsqlOptionsExtension).Assembly.Location);
        yield return MetadataReference.CreateFromFile(typeof(EDBConnection).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location);
    }
}
