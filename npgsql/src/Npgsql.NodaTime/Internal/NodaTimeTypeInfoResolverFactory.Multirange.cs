using System;
using System.Collections.Generic;
using NodaTime;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.Postgres;
using EDBTypes;
using static EnterpriseDB.EDBClient.Internal.PgConverterFactory;

namespace EnterpriseDB.EDBClient.NodaTime.Internal;

sealed partial class NodaTimeTypeInfoResolverFactory
{
    public override IPgTypeInfoResolver? CreateMultirangeResolver() => new MultirangeResolver();
    public override IPgTypeInfoResolver? CreateMultirangeArrayResolver() => new MultirangeArrayResolver();

    class MultirangeResolver : IPgTypeInfoResolver
    {
        protected static DataTypeName DateMultirangeDataTypeName => new("pg_catalog.datemultirange");
        protected static DataTypeName TimestampTzMultirangeDataTypeName => new("pg_catalog.tstzmultirange");
        protected static DataTypeName TimestampMultirangeDataTypeName => new("pg_catalog.tsmultirange");

        TypeInfoMappingCollection? _mappings;
        protected TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new());

        public PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            // tstzmultirange
            mappings.AddType<Interval[]>(TimestampTzMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, CreateArrayMultirangeConverter(new IntervalConverter(
                        CreateRangeConverter(new InstantConverter(options.EnableDateTimeInfinityConversions), options), options.EnableDateTimeInfinityConversions), options)),
                isDefault: true);
            mappings.AddType<List<Interval>>(TimestampTzMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, CreateListMultirangeConverter(new IntervalConverter(
                        CreateRangeConverter(new InstantConverter(options.EnableDateTimeInfinityConversions), options), options.EnableDateTimeInfinityConversions), options)));
            mappings.AddType<EDBRange<Instant>[]>(TimestampTzMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(
                            CreateRangeConverter(new InstantConverter(options.EnableDateTimeInfinityConversions), options), options)));
            mappings.AddType<List<EDBRange<Instant>>>(TimestampTzMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(
                            CreateRangeConverter(new InstantConverter(options.EnableDateTimeInfinityConversions), options), options)));
            mappings.AddType<EDBRange<ZonedDateTime>[]>(TimestampTzMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(
                            CreateRangeConverter(new ZonedDateTimeConverter(options.EnableDateTimeInfinityConversions), options),
                            options)));
            mappings.AddType<List<EDBRange<ZonedDateTime>>>(TimestampTzMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(
                            CreateRangeConverter(new ZonedDateTimeConverter(options.EnableDateTimeInfinityConversions), options),
                            options)));
            mappings.AddType<EDBRange<OffsetDateTime>[]>(TimestampTzMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(
                            CreateRangeConverter(new OffsetDateTimeConverter(options.EnableDateTimeInfinityConversions), options),
                            options)));
            mappings.AddType<List<EDBRange<OffsetDateTime>>>(TimestampTzMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(
                            CreateRangeConverter(new OffsetDateTimeConverter(options.EnableDateTimeInfinityConversions), options),
                            options)));

            // tsmultirange
            mappings.AddType<EDBRange<LocalDateTime>[]>(TimestampMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(
                            CreateRangeConverter(new LocalDateTimeConverter(options.EnableDateTimeInfinityConversions), options), options)),
                isDefault: true);
            mappings.AddType<List<EDBRange<LocalDateTime>>>(TimestampMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(
                            CreateRangeConverter(new LocalDateTimeConverter(options.EnableDateTimeInfinityConversions), options),
                            options)));

            // datemultirange
            mappings.AddType<DateInterval[]>(DateMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, CreateArrayMultirangeConverter(new DateIntervalConverter(
                        CreateRangeConverter(new LocalDateConverter(options.EnableDateTimeInfinityConversions), options),
                        options.EnableDateTimeInfinityConversions), options)),
                isDefault: true);
            mappings.AddType<List<DateInterval>>(DateMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options, CreateListMultirangeConverter(new DateIntervalConverter(
                        CreateRangeConverter(new LocalDateConverter(options.EnableDateTimeInfinityConversions), options),
                        options.EnableDateTimeInfinityConversions), options)));
            mappings.AddType<EDBRange<LocalDate>[]>(DateMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateArrayMultirangeConverter(
                            CreateRangeConverter(new LocalDateConverter(options.EnableDateTimeInfinityConversions), options), options)));
            mappings.AddType<List<EDBRange<LocalDate>>>(DateMultirangeDataTypeName,
                static (options, mapping, _) =>
                    mapping.CreateInfo(options,
                        CreateListMultirangeConverter(
                            CreateRangeConverter(new LocalDateConverter(options.EnableDateTimeInfinityConversions), options), options)));

            return mappings;
        }
    }

    sealed class MultirangeArrayResolver : MultirangeResolver, IPgTypeInfoResolver
    {
        TypeInfoMappingCollection? _mappings;
        new TypeInfoMappingCollection Mappings => _mappings ??= AddMappings(new(base.Mappings));

        public new PgTypeInfo? GetTypeInfo(Type? type, DataTypeName? dataTypeName, PgSerializerOptions options)
            => Mappings.Find(type, dataTypeName, options);

        static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
        {
            // tstzmultirange
            mappings.AddArrayType<Interval[]>(TimestampTzMultirangeDataTypeName);
            mappings.AddArrayType<List<Interval>>(TimestampTzMultirangeDataTypeName);
            mappings.AddArrayType<EDBRange<Instant>[]>(TimestampTzMultirangeDataTypeName);
            mappings.AddArrayType<List<EDBRange<Instant>>>(TimestampTzMultirangeDataTypeName);
            mappings.AddArrayType<EDBRange<ZonedDateTime>[]>(TimestampTzMultirangeDataTypeName);
            mappings.AddArrayType<List<EDBRange<ZonedDateTime>>>(TimestampTzMultirangeDataTypeName);
            mappings.AddArrayType<EDBRange<OffsetDateTime>[]>(TimestampTzMultirangeDataTypeName);
            mappings.AddArrayType<List<EDBRange<OffsetDateTime>>>(TimestampTzMultirangeDataTypeName);

            // tsmultirange
            mappings.AddArrayType<EDBRange<LocalDateTime>[]>(TimestampMultirangeDataTypeName);
            mappings.AddArrayType<List<EDBRange<LocalDateTime>>>(TimestampMultirangeDataTypeName);

            // datemultirange
            mappings.AddArrayType<DateInterval[]>(DateMultirangeDataTypeName);
            mappings.AddArrayType<List<DateInterval>>(DateMultirangeDataTypeName);
            mappings.AddArrayType<EDBRange<LocalDate>[]>(DateMultirangeDataTypeName);
            mappings.AddArrayType<List<EDBRange<LocalDate>>>(DateMultirangeDataTypeName);

            return mappings;
        }
    }
}
