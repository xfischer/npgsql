using System;
using EnterpriseDB.EDBClient.Internal.Converters;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EnterpriseDB.EDBClient.Properties;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.ResolverFactories;

sealed class FullTextSearchTypeInfoResolverFactory : PgTypeInfoResolverFactory
{
    public override IPgTypeInfoResolver CreateResolver() => new Resolver();
    public override IPgTypeInfoResolver CreateArrayResolver() => new ArrayResolver();

    public static void CheckUnsupported<TBuilder>(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
    {
        if (type != typeof(object) && (dataTypeName == DataTypeNames.TsQuery || dataTypeName == DataTypeNames.TsVector))
            throw new NotSupportedException(
                string.Format(EDBStrings.FullTextSearchNotEnabled, nameof(EDBSlimDataSourceBuilder.EnableFullTextSearch), typeof(TBuilder).Name));

        if (type is null)
            return;

        if (TypeInfoMappingCollection.IsArrayLikeType(type, out var elementType))
            type = elementType;

        if (type is { IsConstructedGenericType: true } && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            type = type.GetGenericArguments()[0];

        if (type == typeof(EDBTsVector) || typeof(EDBTsQuery).IsAssignableFrom(type))
            throw new NotSupportedException(
                string.Format(EDBStrings.FullTextSearchNotEnabled, nameof(EDBSlimDataSourceBuilder.EnableFullTextSearch), typeof(TBuilder).Name));
    }

    class Resolver : IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            // tsvector
            mappings.AddType<EDBTsVector>(DataTypeNames.TsVector,
                static (options, mapping, _) => mapping.CreateInfo(options, new TsVectorConverter(options.TextEncoding)), isDefault: true);

            // tsquery
            mappings.AddType<EDBTsQuery>(DataTypeNames.TsQuery,
                static (options, mapping, _) => mapping.CreateInfo(options, new TsQueryConverter<EDBTsQuery>(options.TextEncoding)), isDefault: true);
            mappings.AddType<EDBTsQueryEmpty>(DataTypeNames.TsQuery,
                static (options, mapping, _) => mapping.CreateInfo(options, new TsQueryConverter<EDBTsQueryEmpty>(options.TextEncoding)));
            mappings.AddType<EDBTsQueryLexeme>(DataTypeNames.TsQuery,
                static (options, mapping, _) => mapping.CreateInfo(options, new TsQueryConverter<EDBTsQueryLexeme>(options.TextEncoding)));
            mappings.AddType<EDBTsQueryNot>(DataTypeNames.TsQuery,
                static (options, mapping, _) => mapping.CreateInfo(options, new TsQueryConverter<EDBTsQueryNot>(options.TextEncoding)));
            mappings.AddType<EDBTsQueryAnd>(DataTypeNames.TsQuery,
                static (options, mapping, _) => mapping.CreateInfo(options, new TsQueryConverter<EDBTsQueryAnd>(options.TextEncoding)));
            mappings.AddType<EDBTsQueryOr>(DataTypeNames.TsQuery,
                static (options, mapping, _) => mapping.CreateInfo(options, new TsQueryConverter<EDBTsQueryOr>(options.TextEncoding)));
            mappings.AddType<EDBTsQueryFollowedBy>(DataTypeNames.TsQuery,
                static (options, mapping, _) => mapping.CreateInfo(options, new TsQueryConverter<EDBTsQueryFollowedBy>(options.TextEncoding)));

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
            // tsvector
            mappings.AddArrayType<EDBTsVector>(DataTypeNames.TsVector);

            // tsquery
            mappings.AddArrayType<EDBTsQuery>(DataTypeNames.TsQuery);
            mappings.AddArrayType<EDBTsQueryEmpty>(DataTypeNames.TsQuery);
            mappings.AddArrayType<EDBTsQueryLexeme>(DataTypeNames.TsQuery);
            mappings.AddArrayType<EDBTsQueryNot>(DataTypeNames.TsQuery);
            mappings.AddArrayType<EDBTsQueryAnd>(DataTypeNames.TsQuery);
            mappings.AddArrayType<EDBTsQueryOr>(DataTypeNames.TsQuery);
            mappings.AddArrayType<EDBTsQueryFollowedBy>(DataTypeNames.TsQuery);

            return mappings;
        }
    }
}
