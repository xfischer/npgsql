using System;
using NodaTime;
using NodaTime.TimeZones;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using BclTimestampTzHandler = EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers.TimestampTzHandler;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.NodaTime
{
    public class TimestampTzHandlerFactory : EDBTypeHandlerFactory<Instant>
    {
        // Check for the legacy floating point timestamps feature
        public override EDBTypeHandler<Instant> Create(PostgresType postgresType, EDBConnection conn)
        {
            var csb = new EDBConnectionStringBuilder(conn.ConnectionString);
            return conn.HasIntegerDateTimes
                ? new TimestampTzHandler(postgresType, csb.ConvertInfinityDateTime)
                : throw new NotSupportedException(
                    $"The deprecated floating-point date/time format is not supported by {nameof(EnterpriseDB.EDBClient)}.");
        }
    }

    sealed class TimestampTzHandler : EDBSimpleTypeHandler<Instant>, IEDBSimpleTypeHandler<ZonedDateTime>,
                              IEDBSimpleTypeHandler<OffsetDateTime>, IEDBSimpleTypeHandler<DateTimeOffset>, 
                              IEDBSimpleTypeHandler<DateTime>
    {
        readonly IDateTimeZoneProvider _dateTimeZoneProvider;
        readonly BclTimestampTzHandler _bclHandler;

        /// <summary>
        /// Whether to convert positive and negative infinity values to Instant.{Max,Min}Value when
        /// an Instant is requested
        /// </summary>
        readonly bool _convertInfinityDateTime;

        public TimestampTzHandler(PostgresType postgresType, bool convertInfinityDateTime)
            : base(postgresType)
        {
            _dateTimeZoneProvider = DateTimeZoneProviders.Tzdb;
            _convertInfinityDateTime = convertInfinityDateTime;
            _bclHandler = new BclTimestampTzHandler(postgresType, convertInfinityDateTime);
        }

        #region Read

        public override Instant Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var value = buf.ReadInt64();
            if (_convertInfinityDateTime)
            {
                if (value == long.MaxValue)
                    return Instant.MaxValue;
                if (value == long.MinValue)
                    return Instant.MinValue;
            }
            return TimestampHandler.Decode(value);
        }

        ZonedDateTime IEDBSimpleTypeHandler<ZonedDateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        {
            try
            {
                var value = buf.ReadInt64();
                if (value == long.MaxValue || value == long.MinValue)
                    throw new NotSupportedException("Infinity values not supported for timestamp with time zone");
                return TimestampHandler.Decode(value).InZone(_dateTimeZoneProvider[buf.Connection.Timezone]);
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

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(Instant value, EDBParameter? parameter)
            => 8;

        int IEDBSimpleTypeHandler<ZonedDateTime>.ValidateAndGetLength(ZonedDateTime value, EDBParameter? parameter)
            => 8;

        public int ValidateAndGetLength(OffsetDateTime value, EDBParameter? parameter)
            => 8;

        public override void Write(Instant value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            if (_convertInfinityDateTime)
            {
                if (value == Instant.MaxValue)
                {
                    buf.WriteInt64(long.MaxValue);
                    return;
                }

                if (value == Instant.MinValue)
                {
                    buf.WriteInt64(long.MinValue);
                    return;
                }
            }
            TimestampHandler.WriteInteger(value, buf);
        }

        void IEDBSimpleTypeHandler<ZonedDateTime>.Write(ZonedDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => Write(value.ToInstant(), buf, parameter);

        public void Write(OffsetDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => Write(value.ToInstant(), buf, parameter);

        #endregion Write

        DateTimeOffset IEDBSimpleTypeHandler<DateTimeOffset>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => _bclHandler.Read<DateTimeOffset>(buf, len, fieldDescription);

        int IEDBSimpleTypeHandler<DateTimeOffset>.ValidateAndGetLength(DateTimeOffset value, EDBParameter? parameter)
            => _bclHandler.ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<DateTimeOffset>.Write(DateTimeOffset value, EDBWriteBuffer buf, EDBParameter? parameter)
            => _bclHandler.Write(value, buf, parameter);

        DateTime IEDBSimpleTypeHandler<DateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription) 
            => _bclHandler.Read<DateTime>(buf, len, fieldDescription);

        int IEDBSimpleTypeHandler<DateTime>.ValidateAndGetLength(DateTime value, EDBParameter? parameter) 
            => _bclHandler.ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<DateTime>.Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter) 
            => _bclHandler.Write(value, buf, parameter);
    }
}
