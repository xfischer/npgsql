using System;
using NodaTime;
using NodaTime.TimeZones;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using BclTimestampTzHandler = EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers.TimestampTzHandler;
using static EnterpriseDB.EDBClient.NodaTime.Internal.NodaTimeUtils;

namespace EnterpriseDB.EDBClient.NodaTime.Internal;

sealed partial class LegacyTimestampTzHandler : EDBSimpleTypeHandler<Instant>, IEDBSimpleTypeHandler<ZonedDateTime>,
    IEDBSimpleTypeHandler<OffsetDateTime>, IEDBSimpleTypeHandler<DateTimeOffset>, 
    IEDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<long>
{
    readonly IDateTimeZoneProvider _dateTimeZoneProvider;
    readonly TimestampTzHandler _wrappedHandler;

    public LegacyTimestampTzHandler(PostgresType postgresType)
        : base(postgresType)
    {
        _dateTimeZoneProvider = DateTimeZoneProviders.Tzdb;
        _wrappedHandler = new TimestampTzHandler(postgresType);
    }

    #region Read

    public override Instant Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => _wrappedHandler.Read(buf, len, fieldDescription);

    ZonedDateTime IEDBSimpleTypeHandler<ZonedDateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
    {
        try
        {
            var instant = Read(buf, len, fieldDescription);

            if (!DisableDateTimeInfinityConversions && (instant == Instant.MaxValue || instant == Instant.MinValue))
                throw new InvalidCastException("Infinity values not supported for timestamp with time zone");

            return instant.InZone(_dateTimeZoneProvider[buf.Connection.Timezone]);
        }
        catch (Exception e) when (
            string.Equals(buf.Connection.Timezone, "localtime", StringComparison.OrdinalIgnoreCase) &&
            (e is TimeZoneNotFoundException || e is DateTimeZoneNotFoundException))
        {
            throw new TimeZoneNotFoundException(
                "The special PostgreSQL timezone 'localtime' is not supported when reading values of type 'timestamp with time zone'. " +
                "Please specify a real timezone in 'postgresql.conf' on the server, or set the 'PGTZ' environment variable on the client.",
                e);
        }
    }

    OffsetDateTime IEDBSimpleTypeHandler<OffsetDateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => ((IEDBSimpleTypeHandler<ZonedDateTime>)this).Read(buf, len, fieldDescription).ToOffsetDateTime();

    DateTimeOffset IEDBSimpleTypeHandler<DateTimeOffset>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => _wrappedHandler.Read<DateTimeOffset>(buf, len, fieldDescription);

    DateTime IEDBSimpleTypeHandler<DateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => _wrappedHandler.Read<DateTime>(buf, len, fieldDescription);

    long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => _wrappedHandler.Read<long>(buf, len, fieldDescription);

    #endregion Read

    #region Write

    public override int ValidateAndGetLength(Instant value, EDBParameter? parameter)
        => 8;

    int IEDBSimpleTypeHandler<ZonedDateTime>.ValidateAndGetLength(ZonedDateTime value, EDBParameter? parameter)
        => 8;

    public int ValidateAndGetLength(OffsetDateTime value, EDBParameter? parameter)
        => 8;

    public override void Write(Instant value, EDBWriteBuffer buf, EDBParameter? parameter)
        => _wrappedHandler.Write(value, buf, parameter);

    void IEDBSimpleTypeHandler<ZonedDateTime>.Write(ZonedDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        var instant = value.ToInstant();

        if (!DisableDateTimeInfinityConversions && (instant == Instant.MaxValue || instant == Instant.MinValue))
            throw new InvalidCastException("Infinity values not supported for timestamp with time zone");

        _wrappedHandler.Write(instant, buf, parameter);
    }

    public void Write(OffsetDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        var instant = value.ToInstant();

        if (!DisableDateTimeInfinityConversions && (instant == Instant.MaxValue || instant == Instant.MinValue))
            throw new InvalidCastException("Infinity values not supported for timestamp with time zone");

        _wrappedHandler.Write(instant, buf, parameter);
    }

    int IEDBSimpleTypeHandler<DateTimeOffset>.ValidateAndGetLength(DateTimeOffset value, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<DateTimeOffset>)_wrappedHandler).ValidateAndGetLength(value, parameter);

    void IEDBSimpleTypeHandler<DateTimeOffset>.Write(DateTimeOffset value, EDBWriteBuffer buf, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<DateTimeOffset>)_wrappedHandler).Write(value, buf, parameter);

    int IEDBSimpleTypeHandler<DateTime>.ValidateAndGetLength(DateTime value, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<DateTime>)_wrappedHandler).ValidateAndGetLength(value, parameter);

    void IEDBSimpleTypeHandler<DateTime>.Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<DateTime>)_wrappedHandler).Write(value, buf, parameter);

    int IEDBSimpleTypeHandler<long>.ValidateAndGetLength(long value, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<long>)_wrappedHandler).ValidateAndGetLength(value, parameter);

    void IEDBSimpleTypeHandler<long>.Write(long value, EDBWriteBuffer buf, EDBParameter? parameter)
        => ((IEDBSimpleTypeHandler<long>)_wrappedHandler).Write(value, buf, parameter);

    #endregion Write
}
