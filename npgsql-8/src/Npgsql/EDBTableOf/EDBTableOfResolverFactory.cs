using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal;

internal class EDBTableOfResolverFactory : PgTypeInfoResolverFactory
{
    public override IPgTypeInfoResolver? CreateArrayResolver() => null;
    public override IPgTypeInfoResolver CreateResolver() => new EDBTableOfResolver();
}

internal class EDBTableOfResolver : IPgTypeInfoResolver
{
    TypeInfoMappingCollection? _mappings;
    protected TypeInfoMappingCollection Mappings => _mappings ??= new();

    public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
    {
        if (dataTypeName == null) return null;

        if (options.DatabaseInfo.TryGetPostgresTypeByName(dataTypeName, out var pgType)
            && pgType is EDBTableOfType)
        {
            // EnterpriseDB : parameter is seen as INOUT even if it's OUT (EPAS behaviour).
            // Setting supportsWriting to false will raise an exception as parameter Bind is called.
            Mappings.AddType<ArrayList>(dataTypeName,
                (options, mapping, _) => mapping.CreateInfo(options, new BackendTextToArrayConverter(options, dataTypeName), preferredFormat: DataFormat.Text, supportsWriting: true),
                MatchRequirement.DataTypeName);
        }

        return Mappings.Find(type, dataTypeName, options);
    }
}

