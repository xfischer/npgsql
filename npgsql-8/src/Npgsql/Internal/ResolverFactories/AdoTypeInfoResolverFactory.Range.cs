using System;
using System.Numerics;
using EnterpriseDB.EDBClient.Internal.Converters;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EnterpriseDB.EDBClient.Util;
using EDBTypes;
using static EnterpriseDB.EDBClient.Internal.PgConverterFactory;

namespace EnterpriseDB.EDBClient.Internal.ResolverFactories;

sealed partial class AdoTypeInfoResolverFactory
{
    public override IPgTypeInfoResolver CreateRangeResolver() => new RangeResolver();
    public override IPgTypeInfoResolver CreateRangeArrayResolver() => new RangeArrayResolver();

    class RangeResolver : IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            // numeric ranges
            mappings.AddStructType<EDBRange<int>>(DataTypeNames.Int4Range,
                static (options, mapping, _) => mapping.CreateInfo(options, CreateRangeConverter(new Int4Converter<int>(), options)),
                isDefault: true);
            mappings.AddStructType<EDBRange<long>>(DataTypeNames.Int8Range,
                static (options, mapping, _) => mapping.CreateInfo(options, CreateRangeConverter(new Int8Converter<long>(), options)),
                isDefault: true);
            mappings.AddStructType<EDBRange<decimal>>(DataTypeNames.NumRange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, CreateRangeConverter(new DecimalNumericConverter<decimal>(), options)),
                isDefault: true);
            mappings.AddStructType<EDBRange<BigInteger>>(DataTypeNames.NumRange,
                static (options, mapping, _) => mapping.CreateInfo(options, CreateRangeConverter(new BigIntegerNumericConverter(), options)));

            // tsrange
            if (Statics.LegacyTimestampBehavior)
            {
                mappings.AddStructType<EDBRange<DateTime>>(DataTypeNames.TsRange,
                    static (options, mapping, _) => mapping.CreateInfo(options,
                        CreateRangeConverter(new LegacyDateTimeConverter(options.EnableDateTimeInfinityConversions, timestamp: true), options)),
                    isDefault: true);
            }
            else
            {
                mappings.AddResolverStructType<EDBRange<DateTime>>(DataTypeNames.TsRange,
                    static (options, mapping, dataTypeNameMatch) => mapping.CreateInfo(options,
                        DateTimeConverterResolver.CreateRangeResolver(options,
                            options.GetCanonicalTypeId(DataTypeNames.TsTzRange),
                            options.GetCanonicalTypeId(DataTypeNames.TsRange),
                            options.EnableDateTimeInfinityConversions), dataTypeNameMatch),
                    isDefault: true);
            }
            mappings.AddStructType<EDBRange<long>>(DataTypeNames.TsRange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, CreateRangeConverter(new Int8Converter<long>(), options)));

            // tstzrange
            if (Statics.LegacyTimestampBehavior)
            {
                mappings.AddStructType<EDBRange<DateTime>>(DataTypeNames.TsTzRange,
                    static (options, mapping, _) => mapping.CreateInfo(options,
                        CreateRangeConverter(new LegacyDateTimeConverter(options.EnableDateTimeInfinityConversions, timestamp: false), options)),
                    isDefault: true);
                mappings.AddStructType<EDBRange<DateTimeOffset>>(DataTypeNames.TsTzRange,
                    static (options, mapping, _) => mapping.CreateInfo(options,
                        CreateRangeConverter(new LegacyDateTimeOffsetConverter(options.EnableDateTimeInfinityConversions), options)));
            }
            else
            {
                mappings.AddResolverStructType<EDBRange<DateTime>>(DataTypeNames.TsTzRange,
                    static (options, mapping, dataTypeNameMatch) => mapping.CreateInfo(options,
                        DateTimeConverterResolver.CreateRangeResolver(options,
                            options.GetCanonicalTypeId(DataTypeNames.TsTzRange),
                            options.GetCanonicalTypeId(DataTypeNames.TsRange),
                            options.EnableDateTimeInfinityConversions), dataTypeNameMatch),
                    isDefault: true);
                mappings.AddStructType<EDBRange<DateTimeOffset>>(DataTypeNames.TsTzRange,
                    static (options, mapping, _) => mapping.CreateInfo(options,
                        CreateRangeConverter(new DateTimeOffsetConverter(options.EnableDateTimeInfinityConversions), options)));
            }
            mappings.AddStructType<EDBRange<long>>(DataTypeNames.TsTzRange,
                static (options, mapping, _) => mapping.CreateInfo(options, CreateRangeConverter(new Int8Converter<long>(), options)));

            // daterange
            mappings.AddStructType<EDBRange<DateTime>>(DataTypeNames.DateRange,
                static (options, mapping, _) => mapping.CreateInfo(options,
                    CreateRangeConverter(new DateTimeDateConverter(options.EnableDateTimeInfinityConversions), options)),
                isDefault: true);
            mappings.AddStructType<EDBRange<int>>(DataTypeNames.DateRange,
                static (options, mapping, _) => mapping.CreateInfo(options, CreateRangeConverter(new Int4Converter<int>(), options)));
    #if NET6_0_OR_GREATER
            mappings.AddStructType<EDBRange<DateOnly>>(DataTypeNames.DateRange,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, CreateRangeConverter(new DateOnlyDateConverter(options.EnableDateTimeInfinityConversions), options)));
    #endif

            return mappings;
        }
    }

    sealed class RangeArrayResolver : RangeResolver, IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        new TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new(base.Mappings));

        public new PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            // numeric ranges
            mappings.AddStructArrayType<EDBRange<int>>(DataTypeNames.Int4Range);
            mappings.AddStructArrayType<EDBRange<long>>(DataTypeNames.Int8Range);
            mappings.AddStructArrayType<EDBRange<decimal>>(DataTypeNames.NumRange);
            mappings.AddStructArrayType<EDBRange<BigInteger>>(DataTypeNames.NumRange);

            // tsrange
            if (Statics.LegacyTimestampBehavior)
                mappings.AddStructArrayType<EDBRange<DateTime>>(DataTypeNames.TsRange);
            else
                mappings.AddResolverStructArrayType<EDBRange<DateTime>>(DataTypeNames.TsRange);
            mappings.AddStructArrayType<EDBRange<long>>(DataTypeNames.TsRange);

            // tstzrange
            if (Statics.LegacyTimestampBehavior)
            {
                mappings.AddStructArrayType<EDBRange<DateTime>>(DataTypeNames.TsTzRange);
                mappings.AddStructArrayType<EDBRange<DateTimeOffset>>(DataTypeNames.TsTzRange);
            }
            else
            {
                mappings.AddResolverStructArrayType<EDBRange<DateTime>>(DataTypeNames.TsTzRange);
                mappings.AddStructArrayType<EDBRange<DateTimeOffset>>(DataTypeNames.TsTzRange);
            }
            mappings.AddStructArrayType<EDBRange<long>>(DataTypeNames.TsTzRange);

            // daterange
            mappings.AddStructArrayType<EDBRange<DateTime>>(DataTypeNames.DateRange);
            mappings.AddStructArrayType<EDBRange<int>>(DataTypeNames.DateRange);
#if NET6_0_OR_GREATER
            mappings.AddStructArrayType<EDBRange<DateOnly>>(DataTypeNames.DateRange);
#endif

            return mappings;
        }
    }
}
