using System;
using NodaTime;
using NodaTime.TimeZones;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.NodaTime
{
    public class TimestampTzHandlerFactory : EDBTypeHandlerFactory<Instant>
    {
        // Check for the legacy floating point timestamps feature
        public override EDBTypeHandler<Instant> Create(PostgresType postgresType, EDBConnection conn)
            => conn.HasIntegerDateTimes
                ? new TimestampTzHandler(postgresType)
                : throw new NotSupportedException($"The deprecated floating-point date/time format is not supported by {nameof(EnterpriseDB.EDBClient)}.");
    }

    class TimestampTzHandler : EDBSimpleTypeHandler<Instant>, IEDBSimpleTypeHandler<ZonedDateTime>,
                               IEDBSimpleTypeHandler<OffsetDateTime>
    {
        readonly IDateTimeZoneProvider _dateTimeZoneProvider;

        public TimestampTzHandler(PostgresType postgresType)
            : base(postgresType) => _dateTimeZoneProvider = DateTimeZoneProviders.Tzdb;

        #region Read

        public override Instant Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var value = buf.ReadInt64();
            if (value == long.MaxValue || value == long.MinValue)
                throw new EDBSafeReadException(new NotSupportedException("Infinity values not supported for timestamp with time zone"));
            return TimestampHandler.Decode(value);
        }

        ZonedDateTime IEDBSimpleTypeHandler<ZonedDateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        {
            try
            {
                var value = buf.ReadInt64();
                if (value == long.MaxValue || value == long.MinValue)
                    throw new EDBSafeReadException(new NotSupportedException("Infinity values not supported for timestamp with time zone"));
                return TimestampHandler.Decode(value).InZone(_dateTimeZoneProvider[buf.Connection.Timezone]);
            }
            catch (Exception e) when (
                string.Equals(buf.Connection.Timezone, "localtime", StringComparison.OrdinalIgnoreCase) &&
                (e is TimeZoneNotFoundException || e is DateTimeZoneNotFoundException))
            {
                throw new EDBSafeReadException(
                    new TimeZoneNotFoundException(
                        "The special PostgreSQL timezone 'localtime' is not supported when reading values of type 'timestamp with time zone'. " +
                        "Please specify a real timezone in 'postgresql.conf' on the server, or set the 'PGTZ' environment variable on the client.",
                        e));
            }
            catch (TimeZoneNotFoundException e)
            {
                throw new EDBSafeReadException(e);
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
            => TimestampHandler.WriteInteger(value, buf);

        void IEDBSimpleTypeHandler<ZonedDateTime>.Write(ZonedDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => Write(value.ToInstant(), buf, parameter);

        public void Write(OffsetDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => Write(value.ToInstant(), buf, parameter);

        #endregion Write
    }
}
