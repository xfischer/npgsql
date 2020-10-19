using System;
using System.Data;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    /// <summary>
    /// A factory for type handlers for the PostgreSQL timestamptz data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("timestamp with time zone", EDBDbType.TimestampTz, DbType.DateTimeOffset, typeof(DateTimeOffset))]
    public class TimestampTzHandlerFactory : EDBTypeHandlerFactory<DateTime>
    {
        /// <inheritdoc />
        public override EDBTypeHandler<DateTime> Create(PostgresType postgresType, EDBConnection conn)
            => conn.HasIntegerDateTimes  // Check for the legacy floating point timestamps feature
                ? new TimestampTzHandler(postgresType, conn.Connector!.ConvertInfinityDateTime)
                : throw new NotSupportedException($"The deprecated floating-point date/time format is not supported by {nameof(EnterpriseDB.EDBClient)}.");
    }

    /// <summary>
    /// A type handler for the PostgreSQL timestamptz data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public class TimestampTzHandler : TimestampHandler, IEDBSimpleTypeHandler<DateTimeOffset>
    {
        /// <summary>
        /// Constructs an <see cref="TimestampTzHandler"/>.
        /// </summary>
        public TimestampTzHandler(PostgresType postgresType, bool convertInfinityDateTime)
            : base(postgresType, convertInfinityDateTime) {}

        #region Read

        /// <inheritdoc />
        public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            // TODO: Convert directly to DateTime without passing through EDBTimeStamp?
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            try
            {
                if (ts.IsFinite)
                    return ts.ToDateTime().ToLocalTime();
                if (!ConvertInfinityDateTime)
                    throw new InvalidCastException("Can't convert infinite timestamptz values to DateTime");
                if (ts.IsInfinity)
                    return DateTime.MaxValue;
                return DateTime.MinValue;
            }
            catch (Exception e)
            {
                throw new EDBSafeReadException(e);
            }
        }

        /// <inheritdoc />
        protected override EDBDateTime ReadPsv(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            return new EDBDateTime(ts.Date, ts.Time, DateTimeKind.Utc).ToLocalTime();
        }

        DateTimeOffset IEDBSimpleTypeHandler<DateTimeOffset>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        {
            // TODO: Convert directly to DateTime without passing through EDBTimeStamp?
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            try
            {
                if (ts.IsFinite)
                    return ts.ToDateTime().ToLocalTime();
                if (!ConvertInfinityDateTime)
                    throw new InvalidCastException("Can't convert infinite timestamptz values to DateTime");
                if (ts.IsInfinity)
                    return DateTimeOffset.MaxValue;
                return DateTimeOffset.MinValue;
            }
            catch (Exception e)
            {
                throw new EDBSafeReadException(e);
            }
        }

        #endregion Read

        #region Write

        /// <inheritdoc />
        public int ValidateAndGetLength(DateTimeOffset value, EDBParameter? parameter) => 8;

        /// <inheritdoc />
        public override void Write(EDBDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            switch (value.Kind)
            {
            case DateTimeKind.Unspecified:
            case DateTimeKind.Utc:
                break;
            case DateTimeKind.Local:
                value = value.ToUniversalTime();
                break;
            default:
                throw new InvalidOperationException($"Internal EnterpriseDB.EDBClient bug: unexpected value {value.Kind} of enum {nameof(DateTimeKind)}. Please file a bug.");
            }

            base.Write(value, buf, parameter);
        }

        /// <inheritdoc />
        public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            switch (value.Kind)
            {
            case DateTimeKind.Unspecified:
            case DateTimeKind.Utc:
                break;
            case DateTimeKind.Local:
                value = value.ToUniversalTime();
                break;
            default:
                throw new InvalidOperationException($"Internal EnterpriseDB.EDBClient bug: unexpected value {value.Kind} of enum {nameof(DateTimeKind)}. Please file a bug.");
            }

            base.Write(value, buf, parameter);
        }

        /// <inheritdoc />
        public void Write(DateTimeOffset value, EDBWriteBuffer buf, EDBParameter? parameter)
            => base.Write(value.ToUniversalTime().DateTime, buf, parameter);

        #endregion Write
    }
}
