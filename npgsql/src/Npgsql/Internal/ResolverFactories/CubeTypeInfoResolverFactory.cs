using System;
using EnterpriseDB.EDBClient.Internal.Converters;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EnterpriseDB.EDBClient.Properties;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.ResolverFactories;

sealed class CubeTypeInfoResolverFactory : PgTypeInfoResolverFactory
{
    const string CubeTypeName = "cube";

    public override IPgTypeInfoResolver CreateResolver() => new Resolver();
    public override IPgTypeInfoResolver CreateArrayResolver() => new ArrayResolver();

    public static void ThrowIfUnsupported<TBuilder>(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
    {
        if (dataTypeName is { UnqualifiedNameSpan: "cube" or "_cube" } || type == typeof(EDBCube))
            throw new NotSupportedException(
                string.Format(EDBStrings.CubeNotEnabled, nameof(EDBSlimDataSourceBuilder.EnableCube),
                    typeof(TBuilder).Name));
    }

    class Resolver : IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            mappings.AddStructType<EDBCube>(CubeTypeName,
                static (options, mapping, _) => mapping.CreateInfo(options, new CubeConverter()), isDefault: true);

            return mappings;
        }
    }

    sealed class ArrayResolver : Resolver, IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        new TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new(base.Mappings));

        public new PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            mappings.AddStructArrayType<EDBCube>(CubeTypeName);

            return mappings;
        }
    }
}
