using System.Collections.Concurrent;
using EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>

[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "Used in debug builds for tests only")]
public class NpgsqlNodaTimeTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
#if DEBUG
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static bool LegacyTimestampBehavior;
    public static bool DisableDateTimeInfinityConversions;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#else
    internal static readonly bool LegacyTimestampBehavior;
    internal static readonly bool DisableDateTimeInfinityConversions;
#endif

    static NpgsqlNodaTimeTypeMappingSourcePlugin()
    {
        LegacyTimestampBehavior = AppContext.TryGetSwitch("Npgsql.EnableLegacyTimestampBehavior", out var enabled) && enabled;
        DisableDateTimeInfinityConversions = AppContext.TryGetSwitch("Npgsql.DisableDateTimeInfinityConversions", out enabled) && enabled;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConcurrentDictionary<string, RelationalTypeMapping[]> StoreTypeMappings { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConcurrentDictionary<Type, RelationalTypeMapping> ClrTypeMappings { get; }

    #region TypeMapping

    private readonly TimestampLocalDateTimeMapping _timestampLocalDateTime = TimestampLocalDateTimeMapping.Default;
    private readonly LegacyTimestampInstantMapping _legacyTimestampInstant = LegacyTimestampInstantMapping.Default;

    private readonly TimestampTzInstantMapping _timestamptzInstant = TimestampTzInstantMapping.Default;
    private readonly TimestampTzZonedDateTimeMapping _timestamptzZonedDateTime = TimestampTzZonedDateTimeMapping.Default;
    private readonly TimestampTzOffsetDateTimeMapping _timestamptzOffsetDateTime = TimestampTzOffsetDateTimeMapping.Default;

    private readonly DateMapping _date = DateMapping.Default;
    private readonly TimeMapping _time = TimeMapping.Default;
    private readonly TimeTzMapping _timetz = TimeTzMapping.Default;
    private readonly PeriodIntervalMapping _periodInterval = PeriodIntervalMapping.Default;
    private readonly DurationIntervalMapping _durationInterval = DurationIntervalMapping.Default;

    // PostgreSQL has no native type for representing time zones - it just uses the IANA ID as text.
    private readonly DateTimeZoneMapping _timeZone = new("text");

    // Built-in ranges
    private readonly NpgsqlRangeTypeMapping _timestampLocalDateTimeRange;
    private readonly NpgsqlRangeTypeMapping _legacyTimestampInstantRange;
    private readonly NpgsqlRangeTypeMapping _timestamptzInstantRange;
    private readonly NpgsqlRangeTypeMapping _timestamptzZonedDateTimeRange;
    private readonly NpgsqlRangeTypeMapping _timestamptzOffsetDateTimeRange;
    private readonly NpgsqlRangeTypeMapping _dateRange;
    private readonly DateIntervalRangeMapping _dateIntervalRange = new();
    private readonly IntervalRangeMapping _intervalRange = new();

    #endregion

    /// <summary>
    /// Constructs an instance of the <see cref="NpgsqlNodaTimeTypeMappingSourcePlugin"/> class.
    /// </summary>
    public NpgsqlNodaTimeTypeMappingSourcePlugin(ISqlGenerationHelper sqlGenerationHelper)
    {
        _timestampLocalDateTimeRange = NpgsqlRangeTypeMapping.CreatBuiltInRangeMapping(
            "tsrange", typeof(EDBRange<LocalDateTime>), EDBDbType.TimestampRange, _timestampLocalDateTime);
        _legacyTimestampInstantRange = NpgsqlRangeTypeMapping.CreatBuiltInRangeMapping(
            "tsrange", typeof(EDBRange<Instant>), EDBDbType.TimestampRange, _legacyTimestampInstant);
        _timestamptzInstantRange = NpgsqlRangeTypeMapping.CreatBuiltInRangeMapping(
            "tstzrange", typeof(EDBRange<Instant>), EDBDbType.TimestampTzRange, _timestamptzInstant);
        _timestamptzZonedDateTimeRange = NpgsqlRangeTypeMapping.CreatBuiltInRangeMapping(
            "tstzrange", typeof(EDBRange<ZonedDateTime>), EDBDbType.TimestampTzRange, _timestamptzZonedDateTime);
        _timestamptzOffsetDateTimeRange = NpgsqlRangeTypeMapping.CreatBuiltInRangeMapping(
            "tstzrange", typeof(EDBRange<OffsetDateTime>), EDBDbType.TimestampTzRange, _timestamptzOffsetDateTime);
        _dateRange = NpgsqlRangeTypeMapping.CreatBuiltInRangeMapping(
            "daterange", typeof(EDBRange<LocalDate>), EDBDbType.DateRange, _date);

        var storeTypeMappings = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
        {
            {
                // We currently allow _legacyTimestampInstant even in non-legacy mode, since when upgrading to 6.0 with existing
                // migrations, model snapshots still contain old mappings (Instant mapped to timestamp), and EF Core's model differ
                // expects type mappings to be found for these. See https://github.com/dotnet/efcore/issues/26168.
                "timestamp without time zone", LegacyTimestampBehavior
                    ? [_legacyTimestampInstant, _timestampLocalDateTime]
                    : [_timestampLocalDateTime, _legacyTimestampInstant]
            },
            {
                "timestamp with time zone", [_timestamptzInstant, _timestamptzZonedDateTime, _timestamptzOffsetDateTime]
            },
            { "date", [_date] },
            { "time without time zone", [_time] },
            { "time with time zone", [_timetz] },
            { "interval", [_periodInterval, _durationInterval] },
            {
                "tsrange", LegacyTimestampBehavior
                    ? [_legacyTimestampInstantRange, _timestampLocalDateTimeRange]
                    : [_timestampLocalDateTimeRange, _legacyTimestampInstantRange]
            },
            {
                "tstzrange", [
                    _intervalRange, _timestamptzInstantRange, _timestamptzZonedDateTimeRange, _timestamptzOffsetDateTimeRange
                ]
            },
            { "daterange", [_dateIntervalRange, _dateRange] }
        };

        // Set up aliases
        storeTypeMappings["timestamp"] = storeTypeMappings["timestamp without time zone"];
        storeTypeMappings["timestamptz"] = storeTypeMappings["timestamp with time zone"];
        storeTypeMappings["time"] = storeTypeMappings["time without time zone"];
        storeTypeMappings["timetz"] = storeTypeMappings["time with time zone"];

        var clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
        {
            { typeof(Instant), LegacyTimestampBehavior ? _legacyTimestampInstant : _timestamptzInstant },
            { typeof(LocalDateTime), _timestampLocalDateTime },
            { typeof(ZonedDateTime), _timestamptzZonedDateTime },
            { typeof(OffsetDateTime), _timestamptzOffsetDateTime },
            { typeof(LocalDate), _date },
            { typeof(LocalTime), _time },
            { typeof(OffsetTime), _timetz },
            { typeof(Period), _periodInterval },
            { typeof(Duration), _durationInterval },
            // See DateTimeZone below

            { typeof(EDBRange<Instant>), LegacyTimestampBehavior ? _legacyTimestampInstantRange : _timestamptzInstantRange },
            { typeof(EDBRange<LocalDateTime>), _timestampLocalDateTimeRange },
            { typeof(EDBRange<ZonedDateTime>), _timestamptzZonedDateTimeRange },
            { typeof(EDBRange<OffsetDateTime>), _timestamptzOffsetDateTimeRange },
            { typeof(EDBRange<LocalDate>), _dateRange },
            { typeof(DateInterval), _dateIntervalRange },
            { typeof(Interval), _intervalRange },
        };

        StoreTypeMappings = new ConcurrentDictionary<string, RelationalTypeMapping[]>(storeTypeMappings, StringComparer.OrdinalIgnoreCase);
        ClrTypeMappings = new ConcurrentDictionary<Type, RelationalTypeMapping>(clrTypeMappings);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => FindBaseMapping(mappingInfo)?.Clone(mappingInfo);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping? FindBaseMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;
        var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

        if (storeTypeName is not null)
        {
            if (StoreTypeMappings.TryGetValue(storeTypeName, out var mappings))
            {
                if (clrType is null)
                {
                    return mappings[0];
                }

                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                return null;
            }

            if (StoreTypeMappings.TryGetValue(storeTypeNameBase!, out mappings))
            {
                if (clrType is null)
                {
                    return mappings[0];
                }

                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                return null;
            }
        }

        if (clrType is not null)
        {
            if (ClrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                return mapping;
            }

            if (clrType.IsAssignableTo(typeof(DateTimeZone)))
            {
                return _timeZone;
            }
        }

        return null;
    }
}
