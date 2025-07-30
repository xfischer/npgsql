using System;
using System.Collections.Generic;
using EnterpriseDB.EDBClient.Internal.Converters;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EnterpriseDB.EDBClient.Util;
using EDBTypes;
using static EnterpriseDB.EDBClient.Internal.PgConverterFactory;

namespace EnterpriseDB.EDBClient.Internal.ResolverFactories;

sealed partial class AdoTypeInfoResolverFactory
{
    public override IPgTypeInfoResolver CreateMultirangeResolver() => new MultirangeResolver();
    public override IPgTypeInfoResolver CreateMultirangeArrayResolver() => new MultirangeArrayResolver();

    class MultirangeResolver : IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => options.DatabaseInfo.SupportsMultirangeTypes ? Mappings.Find(type, dataTypeName, options) : null;

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            // int4multirange
            mappings.AddType<EDBRange<int>[]>(DataTypeNames.Int4Multirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(CreateRangeConverter(new Int4Converter<int>(), options), options)),
                isDefault: true);
            mappings.AddType<List<EDBRange<int>>>(DataTypeNames.Int4Multirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(CreateRangeConverter(new Int4Converter<int>(), options), options)));

            // int8multirange
            mappings.AddType<EDBRange<long>[]>(DataTypeNames.Int8Multirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(CreateRangeConverter(new Int8Converter<long>(), options), options)),
                isDefault: true);
            mappings.AddType<List<EDBRange<long>>>(DataTypeNames.Int8Multirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(CreateRangeConverter(new Int8Converter<long>(), options), options)));

            // nummultirange
            mappings.AddType<EDBRange<decimal>[]>(DataTypeNames.NumMultirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(CreateRangeConverter(new DecimalNumericConverter<decimal>(), options), options)),
                isDefault: true);
            mappings.AddType<List<EDBRange<decimal>>>(DataTypeNames.NumMultirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(CreateRangeConverter(new DecimalNumericConverter<decimal>(), options), options)));

            // tsmultirange
            if (Statics.LegacyTimestampBehavior)
            {
                mappings.AddType<EDBRange<DateTime>[]>(DataTypeNames.TsMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateArrayMultirangeConverter(
                            CreateRangeConverter(new LegacyDateTimeConverter(options.EnableDateTimeInfinityConversions, timestamp: true),
                                options), options)),
                    isDefault: true);
                mappings.AddType<List<EDBRange<DateTime>>>(DataTypeNames.TsMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateListMultirangeConverter(
                            CreateRangeConverter(new LegacyDateTimeConverter(options.EnableDateTimeInfinityConversions, timestamp: true),
                                options), options)));
            }
            else
            {
                mappings.AddResolverType<EDBRange<DateTime>[]>(DataTypeNames.TsMultirange,
                    static (options, mapping, requiresDataTypeName) => mapping.CreateInfo(options,
                        DateTimeConverterResolver.CreateMultirangeResolver<EDBRange<DateTime>[], EDBRange<DateTime>>(options,
                            options.GetCanonicalTypeId(DataTypeNames.TsTzMultirange),
                            options.GetCanonicalTypeId(DataTypeNames.TsMultirange),
                            options.EnableDateTimeInfinityConversions), requiresDataTypeName),
                    isDefault: true);
                mappings.AddResolverType<List<EDBRange<DateTime>>>(DataTypeNames.TsMultirange,
                    static (options, mapping, requiresDataTypeName) => mapping.CreateInfo(options,
                        DateTimeConverterResolver.CreateMultirangeResolver<List<EDBRange<DateTime>>, EDBRange<DateTime>>(options,
                            options.GetCanonicalTypeId(DataTypeNames.TsTzMultirange),
                            options.GetCanonicalTypeId(DataTypeNames.TsMultirange),
                            options.EnableDateTimeInfinityConversions), requiresDataTypeName));
            }

            mappings.AddType<EDBRange<long>[]>(DataTypeNames.TsMultirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(CreateRangeConverter(new Int8Converter<long>(), options), options)));
            mappings.AddType<List<EDBRange<long>>>(DataTypeNames.TsMultirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(CreateRangeConverter(new Int8Converter<long>(), options), options)));

            // tstzmultirange
            if (Statics.LegacyTimestampBehavior)
            {
                mappings.AddType<EDBRange<DateTime>[]>(DataTypeNames.TsTzMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateArrayMultirangeConverter(
                            CreateRangeConverter(new LegacyDateTimeConverter(options.EnableDateTimeInfinityConversions, timestamp: false),
                                options), options)),
                    isDefault: true);
                mappings.AddType<List<EDBRange<DateTime>>>(DataTypeNames.TsTzMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateListMultirangeConverter(
                            CreateRangeConverter(new LegacyDateTimeConverter(options.EnableDateTimeInfinityConversions, timestamp: false),
                                options), options)));
                mappings.AddType<EDBRange<DateTimeOffset>[]>(DataTypeNames.TsTzMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateArrayMultirangeConverter(
                            CreateRangeConverter(new LegacyDateTimeOffsetConverter(options.EnableDateTimeInfinityConversions), options),
                            options)),
                    isDefault: true);
                mappings.AddType<List<EDBRange<DateTimeOffset>>>(DataTypeNames.TsTzMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateListMultirangeConverter(
                            CreateRangeConverter(new LegacyDateTimeOffsetConverter(options.EnableDateTimeInfinityConversions), options),
                            options)));
            }
            else
            {
                mappings.AddResolverType<EDBRange<DateTime>[]>(DataTypeNames.TsTzMultirange,
                    static (options, mapping, requiresDataTypeName) => mapping.CreateInfo(options,
                        DateTimeConverterResolver.CreateMultirangeResolver<EDBRange<DateTime>[], EDBRange<DateTime>>(options,
                            options.GetCanonicalTypeId(DataTypeNames.TsTzMultirange),
                            options.GetCanonicalTypeId(DataTypeNames.TsMultirange),
                            options.EnableDateTimeInfinityConversions), requiresDataTypeName),
                    isDefault: true);
                mappings.AddResolverType<List<EDBRange<DateTime>>>(DataTypeNames.TsTzMultirange,
                    static (options, mapping, requiresDataTypeName) => mapping.CreateInfo(options,
                        DateTimeConverterResolver.CreateMultirangeResolver<List<EDBRange<DateTime>>, EDBRange<DateTime>>(options,
                            options.GetCanonicalTypeId(DataTypeNames.TsTzMultirange),
                            options.GetCanonicalTypeId(DataTypeNames.TsMultirange),
                            options.EnableDateTimeInfinityConversions), requiresDataTypeName));
                mappings.AddType<EDBRange<DateTimeOffset>[]>(DataTypeNames.TsTzMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateArrayMultirangeConverter(
                            CreateRangeConverter(new DateTimeOffsetConverter(options.EnableDateTimeInfinityConversions), options), options)),
                    isDefault: true);
                mappings.AddType<List<EDBRange<DateTimeOffset>>>(DataTypeNames.TsTzMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateListMultirangeConverter(
                            CreateRangeConverter(new DateTimeOffsetConverter(options.EnableDateTimeInfinityConversions), options), options)));
            }

            mappings.AddType<EDBRange<long>[]>(DataTypeNames.TsTzMultirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(CreateRangeConverter(new Int8Converter<long>(), options), options)));
            mappings.AddType<List<EDBRange<long>>>(DataTypeNames.TsTzMultirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(CreateRangeConverter(new Int8Converter<long>(), options), options)));

            // datemultirange
            mappings.AddType<EDBRange<DateTime>[]>(DataTypeNames.DateMultirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, CreateArrayMultirangeConverter(
                        CreateRangeConverter(new DateTimeDateConverter(options.EnableDateTimeInfinityConversions), options), options)),
                isDefault: true);
            mappings.AddType<List<EDBRange<DateTime>>>(DataTypeNames.DateMultirange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, CreateListMultirangeConverter(
                        CreateRangeConverter(new DateTimeDateConverter(options.EnableDateTimeInfinityConversions), options), options)));
    #if NET6_0_OR_GREATER // EnterpriseDB (NETFRAMEWORK)
                mappings.AddType<EDBRange<DateOnly>[]>(DataTypeNames.DateMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateArrayMultirangeConverter(
                            CreateRangeConverter(new DateOnlyDateConverter(options.EnableDateTimeInfinityConversions), options), options)),
                    isDefault: true);
                mappings.AddType<List<EDBRange<DateOnly>>>(DataTypeNames.DateMultirange,
                    static (options, mapping, _) =>
                        mapping.CreateInfo(options, CreateListMultirangeConverter(
                            CreateRangeConverter(new DateOnlyDateConverter(options.EnableDateTimeInfinityConversions), options), options)));
    #endif

            return mappings;
        }
    }

    sealed class MultirangeArrayResolver : MultirangeResolver, IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        new TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new(base.Mappings));

        public new PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => options.DatabaseInfo.SupportsMultirangeTypes ? Mappings.Find(type, dataTypeName, options) : null;

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            // int4multirange
            mappings.AddArrayType<EDBRange<int>[]>(DataTypeNames.Int4Multirange);
            mappings.AddArrayType<List<EDBRange<int>>>(DataTypeNames.Int4Multirange);

            // int8multirange
            mappings.AddArrayType<EDBRange<long>[]>(DataTypeNames.Int8Multirange);
            mappings.AddArrayType<List<EDBRange<long>>>(DataTypeNames.Int8Multirange);

            // nummultirange
            mappings.AddArrayType<EDBRange<decimal>[]>(DataTypeNames.NumMultirange);
            mappings.AddArrayType<List<EDBRange<decimal>>>(DataTypeNames.NumMultirange);

            // tsmultirange
            if (Statics.LegacyTimestampBehavior)
            {
                mappings.AddArrayType<EDBRange<DateTime>[]>(DataTypeNames.TsMultirange);
                mappings.AddArrayType<List<EDBRange<DateTime>>>(DataTypeNames.TsMultirange);
            }
            else
            {
                mappings.AddResolverArrayType<EDBRange<DateTime>[]>(DataTypeNames.TsMultirange);
                mappings.AddResolverArrayType<List<EDBRange<DateTime>>>(DataTypeNames.TsMultirange);
            }

            mappings.AddArrayType<EDBRange<long>[]>(DataTypeNames.TsMultirange);
            mappings.AddArrayType<List<EDBRange<long>>>(DataTypeNames.TsMultirange);

            // tstzmultirange
            if (Statics.LegacyTimestampBehavior)
            {
                mappings.AddArrayType<EDBRange<DateTime>[]>(DataTypeNames.TsTzMultirange);
                mappings.AddArrayType<List<EDBRange<DateTime>>>(DataTypeNames.TsTzMultirange);
                mappings.AddArrayType<EDBRange<DateTimeOffset>[]>(DataTypeNames.TsTzMultirange);
                mappings.AddArrayType<List<EDBRange<DateTimeOffset>>>(DataTypeNames.TsTzMultirange);
            }
            else
            {
                mappings.AddResolverArrayType<EDBRange<DateTime>[]>(DataTypeNames.TsTzMultirange);
                mappings.AddResolverArrayType<List<EDBRange<DateTime>>>(DataTypeNames.TsTzMultirange);
                mappings.AddArrayType<EDBRange<DateTimeOffset>[]>(DataTypeNames.TsTzMultirange);
                mappings.AddArrayType<List<EDBRange<DateTimeOffset>>>(DataTypeNames.TsTzMultirange);
            }

            mappings.AddArrayType<EDBRange<long>[]>(DataTypeNames.TsTzMultirange);
            mappings.AddArrayType<List<EDBRange<long>>>(DataTypeNames.TsTzMultirange);

            // datemultirange
            mappings.AddArrayType<EDBRange<DateTime>[]>(DataTypeNames.DateMultirange);
            mappings.AddArrayType<List<EDBRange<DateTime>>>(DataTypeNames.DateMultirange);
    #if NET6_0_OR_GREATER // EnterpriseDB (NETFRAMEWORK)
                mappings.AddArrayType<EDBRange<DateOnly>[]>(DataTypeNames.DateMultirange);
                mappings.AddArrayType<List<EDBRange<DateOnly>>>(DataTypeNames.DateMultirange);
    #endif

            return mappings;
        }
    }
}
