using System;
using System.Collections.Generic;
using NodaTime;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;
using static EnterpriseDB.EDBClient.NodaTime.Internal.NodaTimeUtils;

namespace EnterpriseDB.EDBClient.NodaTime.Internal;

public class NodaTimeTypeHandlerResolver : TypeHandlerResolver
{
    readonly EDBDatabaseInfo _databaseInfo;

    readonly EDBTypeHandler _timestampHandler;
    readonly EDBTypeHandler _timestampTzHandler;
    readonly DateHandler _dateHandler;
    readonly TimeHandler _timeHandler;
    readonly TimeTzHandler _timeTzHandler;
    readonly IntervalHandler _intervalHandler;

    TimestampTzRangeHandler? _timestampTzRangeHandler;
    DateRangeHandler? _dateRangeHandler;
    DateMultirangeHandler? _dateMultirangeHandler;
    TimestampTzMultirangeHandler? _timestampTzMultirangeHandler;

    EDBTypeHandler? _timestampTzRangeArray;
    EDBTypeHandler? _dateRangeArray;

    readonly ArrayNullabilityMode _arrayNullabilityMode;

    internal NodaTimeTypeHandlerResolver(EDBConnector connector)
    {
        _databaseInfo = connector.DatabaseInfo;

        _timestampHandler = LegacyTimestampBehavior
            ? new LegacyTimestampHandler(PgType("timestamp without time zone"))
            : new TimestampHandler(PgType("timestamp without time zone"));
        _timestampTzHandler = LegacyTimestampBehavior
            ? new LegacyTimestampTzHandler(PgType("timestamp with time zone"))
            : new TimestampTzHandler(PgType("timestamp with time zone"));
        _dateHandler = new DateHandler(PgType("date"));
        _timeHandler = new TimeHandler(PgType("time without time zone"));
        _timeTzHandler = new TimeTzHandler(PgType("time with time zone"));
        _intervalHandler = new IntervalHandler(PgType("interval"));

        // Note that the range handlers are absent on some pseudo-PostgreSQL databases (e.g. CockroachDB), and multirange types
        // were only introduced in PG14. So we resolve these lazily.

        _arrayNullabilityMode = connector.Settings.ArrayNullabilityMode;
    }

    public override EDBTypeHandler? ResolveByDataTypeName(string typeName)
        => typeName switch
        {
            "timestamp" or "timestamp without time zone" => _timestampHandler,
            "timestamptz" or "timestamp with time zone" => _timestampTzHandler,
            "date" => _dateHandler,
            "time without time zone" => _timeHandler,
            "time with time zone" => _timeTzHandler,
            "interval" => _intervalHandler,

            "tstzrange" => TsTzRange(),
            "daterange" => DateRange(),
            "tstzmultirange" => TsTzMultirange(),
            "datemultirange" => DateMultirange(),

            "tstzrange[]" => TsTzRangeArray(),
            "daterange[]" => DateRangeArray(),

            _ => null
        };

    public override EDBTypeHandler? ResolveByClrType(Type type)
        => ClrTypeToDataTypeName(type) is { } dataTypeName && ResolveByDataTypeName(dataTypeName) is { } handler
            ? handler
            : null;

    public override EDBTypeHandler? ResolveValueTypeGenerically<T>(T value)
    {
        // This method only ever gets called for value types, and relies on the JIT specializing the method for T by eliding all the
        // type checks below.

        if (typeof(T) == typeof(Instant))
            return LegacyTimestampBehavior ? _timestampHandler : _timestampTzHandler;

        if (typeof(T) == typeof(LocalDateTime))
            return _timestampHandler;
        if (typeof(T) == typeof(ZonedDateTime))
            return _timestampTzHandler;
        if (typeof(T) == typeof(OffsetDateTime))
            return _timestampTzHandler;
        if (typeof(T) == typeof(LocalDate))
            return _dateHandler;
        if (typeof(T) == typeof(LocalTime))
            return _timeHandler;
        if (typeof(T) == typeof(OffsetTime))
            return _timeTzHandler;
        if (typeof(T) == typeof(Period))
            return _intervalHandler;
        if (typeof(T) == typeof(Duration))
            return _intervalHandler;

        if (typeof(T) == typeof(Interval))
            return _timestampTzRangeHandler;
        if (typeof(T) == typeof(EDBRange<Instant>))
            return _timestampTzRangeHandler;
        if (typeof(T) == typeof(EDBRange<ZonedDateTime>))
            return _timestampTzRangeHandler;
        if (typeof(T) == typeof(EDBRange<OffsetDateTime>))
            return _timestampTzRangeHandler;

        // Note that DateInterval is a reference type, so not included in this method
        if (typeof(T) == typeof(EDBRange<LocalDate>))
            return _dateRangeHandler;

        return null;
    }

    internal static string? ClrTypeToDataTypeName(Type type)
    {
        if (type == typeof(Instant))
            return LegacyTimestampBehavior ? "timestamp without time zone" : "timestamp with time zone";

        if (type == typeof(LocalDateTime))
            return "timestamp without time zone";
        if (type == typeof(ZonedDateTime) || type == typeof(OffsetDateTime))
            return "timestamp with time zone";
        if (type == typeof(LocalDate))
            return "date";
        if (type == typeof(LocalTime))
            return "time without time zone";
        if (type == typeof(OffsetTime))
            return "time with time zone";
        if (type == typeof(Period) || type == typeof(Duration))
            return "interval";

        // Ranges
        if (type == typeof(EDBRange<LocalDateTime>))
            return "tsrange";

        if (type == typeof(Interval) ||
            type == typeof(EDBRange<Instant>) ||
            type == typeof(EDBRange<ZonedDateTime>) ||
            type == typeof(EDBRange<OffsetDateTime>))
        {
            return "tstzrange";
        }

        if (type == typeof(DateInterval) || type == typeof(EDBRange<LocalDate>))
            return "daterange";

        // Multiranges
        if (type == typeof(EDBRange<LocalDateTime>[]) || type == typeof(List<EDBRange<LocalDateTime>>))
            return "tsmultirange";

        if (type == typeof(Interval[]) ||
            type == typeof(List<Interval>) ||
            type == typeof(EDBRange<Instant>[]) ||
            type == typeof(List<EDBRange<Instant>>) ||
            type == typeof(EDBRange<ZonedDateTime>[]) ||
            type == typeof(List<EDBRange<ZonedDateTime>>) ||
            type == typeof(EDBRange<OffsetDateTime>[]) ||
            type == typeof(List<EDBRange<OffsetDateTime>>))
        {
            return "tstzmultirange";
        }
        if (type == typeof(DateInterval[]) ||
            type == typeof(List<DateInterval>) ||
            type == typeof(EDBRange<LocalDate>[]) ||
            type == typeof(List<EDBRange<LocalDate>>))
        {
            return "datemultirange";
        }

        return null;
    }

    public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName)
        => DoGetMappingByDataTypeName(dataTypeName);

    internal static TypeMappingInfo? DoGetMappingByDataTypeName(string dataTypeName)
        => dataTypeName switch
        {
            "timestamp" or "timestamp without time zone" => new(EDBDbType.Timestamp,             "timestamp without time zone"),
            "timestamptz" or "timestamp with time zone"  => new(EDBDbType.TimestampTz,           "timestamp with time zone"),
            "date"                                       => new(EDBDbType.Date,                  "date"),
            "time without time zone"                     => new(EDBDbType.Time,                  "time without time zone"),
            "time with time zone"                        => new(EDBDbType.TimeTz,                "time with time zone"),
            "interval"                                   => new(EDBDbType.Interval,              "interval"),

            "tsrange"                                    => new(EDBDbType.TimestampRange,        "tsrange"),
            "tstzrange"                                  => new(EDBDbType.TimestampTzRange,      "tstzrange"),
            "daterange"                                  => new(EDBDbType.DateRange,             "daterange"),

            "tsmultirange"                               => new(EDBDbType.TimestampMultirange,   "tsmultirange"),
            "tstzmultirange"                             => new(EDBDbType.TimestampTzMultirange, "tstzmultirange"),
            "datemultirange"                             => new(EDBDbType.DateMultirange,        "datemultirange"),

            _ => null
        };


    PostgresType PgType(string pgTypeName) => _databaseInfo.GetPostgresTypeByName(pgTypeName);

    TimestampTzRangeHandler TsTzRange()
        => _timestampTzRangeHandler ??= new TimestampTzRangeHandler(PgType("tstzrange"), _timestampTzHandler);

    DateRangeHandler DateRange()
        => _dateRangeHandler ??= new DateRangeHandler(PgType("daterange"), _dateHandler);

    EDBTypeHandler TsTzMultirange()
        => _timestampTzMultirangeHandler ??=
            new TimestampTzMultirangeHandler((PostgresMultirangeType)PgType("tstzmultirange"), TsTzRange());

    EDBTypeHandler DateMultirange()
        => _dateMultirangeHandler ??= new DateMultirangeHandler((PostgresMultirangeType)PgType("datemultirange"), DateRange());

    EDBTypeHandler TsTzRangeArray()
        => _timestampTzRangeArray ??=
            new ArrayHandler<Interval>((PostgresArrayType)PgType("tstzrange[]"), TsTzRange(), _arrayNullabilityMode);

    EDBTypeHandler DateRangeArray()
        => _dateRangeArray ??=
            new ArrayHandler<DateInterval>((PostgresArrayType)PgType("daterange[]"), DateRange(), _arrayNullabilityMode);
}