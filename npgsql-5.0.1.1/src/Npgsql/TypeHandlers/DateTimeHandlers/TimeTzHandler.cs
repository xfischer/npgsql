using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    /// <summary>
    /// A factory for type handlers for the PostgreSQL timetz data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("time with time zone", EDBDbType.TimeTz)]
    public class TimeTzHandlerFactory : EDBTypeHandlerFactory<DateTimeOffset>
    {
        /// <inheritdoc />
        public override EDBTypeHandler<DateTimeOffset> Create(PostgresType postgresType, EDBConnection conn)
            => conn.HasIntegerDateTimes  // Check for the legacy floating point timestamps feature
                ? new TimeTzHandler(postgresType)
                : throw new NotSupportedException($"The deprecated floating-point date/time format is not supported by {nameof(EnterpriseDB.EDBClient)}.");
    }

    /// <summary>
    /// A type handler for the PostgreSQL timetz data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public class TimeTzHandler : EDBSimpleTypeHandler<DateTimeOffset>, IEDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<TimeSpan>
    {
        // Binary Format: int64 expressing microseconds, int32 expressing timezone in seconds, negative

        /// <summary>
        /// Constructs an <see cref="TimeTzHandler"/>.
        /// </summary>
        public TimeTzHandler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        /// <inheritdoc />
        public override DateTimeOffset Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            // Adjust from 1 microsecond to 100ns. Time zone (in seconds) is inverted.
            var ticks = buf.ReadInt64() * 10;
            var offset = new TimeSpan(0, 0, -buf.ReadInt32());
            return new DateTimeOffset(ticks + TimeSpan.TicksPerDay, offset);
        }

        DateTime IEDBSimpleTypeHandler<DateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription).LocalDateTime;

        TimeSpan IEDBSimpleTypeHandler<TimeSpan>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription).LocalDateTime.TimeOfDay;

        #endregion Read

        #region Write

        /// <inheritdoc />
        public override int ValidateAndGetLength(DateTimeOffset value, EDBParameter? parameter) => 12;
        /// <inheritdoc />
        public int ValidateAndGetLength(TimeSpan value, EDBParameter? parameter)                => 12;
        /// <inheritdoc />
        public int ValidateAndGetLength(DateTime value, EDBParameter? parameter)                => 12;

        /// <inheritdoc />
        public override void Write(DateTimeOffset value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteInt64(value.TimeOfDay.Ticks / 10);
            buf.WriteInt32(-(int)(value.Offset.Ticks / TimeSpan.TicksPerSecond));
        }

        /// <inheritdoc />
        public void Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteInt64(value.TimeOfDay.Ticks / 10);

            switch (value.Kind)
            {
            case DateTimeKind.Utc:
                buf.WriteInt32(0);
                break;
            case DateTimeKind.Unspecified:
            // Treat as local...
            case DateTimeKind.Local:
                buf.WriteInt32(-(int)(TimeZoneInfo.Local.BaseUtcOffset.Ticks / TimeSpan.TicksPerSecond));
                break;
            default:
                throw new InvalidOperationException($"Internal EnterpriseDB.EDBClient bug: unexpected value {value.Kind} of enum {nameof(DateTimeKind)}. Please file a bug.");
            }
        }

        /// <inheritdoc />
        public void Write(TimeSpan value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteInt64(value.Ticks / 10);
            buf.WriteInt32(-(int)(TimeZoneInfo.Local.BaseUtcOffset.Ticks / TimeSpan.TicksPerSecond));
        }

        #endregion Write
    }
}
