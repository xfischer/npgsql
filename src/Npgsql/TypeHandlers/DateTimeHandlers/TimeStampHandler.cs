using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    /// <summary>
    /// A factory for type handlers for the PostgreSQL timestamp data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("timestamp", EDBDbType.Timestamp, new[] { DbType.DateTime, DbType.DateTime2 }, new[] { typeof(EDBDateTime), typeof(DateTime) }, DbType.DateTime)]
    public class TimestampHandlerFactory : EDBTypeHandlerFactory<DateTime>
    {
        /// <inheritdoc />
        public override EDBTypeHandler<DateTime> Create(PostgresType postgresType, EDBConnection conn)
            => conn.HasIntegerDateTimes  // Check for the legacy floating point timestamps feature
                ? new TimestampHandler(postgresType, conn.Connector!.ConvertInfinityDateTime)
                : throw new NotSupportedException($"The deprecated floating-point date/time format is not supported by {nameof(EnterpriseDB.EDBClient)}.");
    }

    /// <summary>
    /// A type handler for the PostgreSQL timestamp data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EnterpriseDB.EDBClient's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public class TimestampHandler : EDBSimpleTypeHandlerWithPsv<DateTime, EDBDateTime>
    {
        /// <summary>
        /// Whether to convert positive and negative infinity values to DateTime.{Max,Min}Value when
        /// a DateTime is requested
        /// </summary>
        protected readonly bool ConvertInfinityDateTime;

        /// <summary>
        /// Constructs a <see cref="TimestampHandler"/>.
        /// </summary>
        public TimestampHandler(PostgresType postgresType, bool convertInfinityDateTime)
            : base(postgresType) => ConvertInfinityDateTime = convertInfinityDateTime;

        #region Read

        /// <inheritdoc />
        public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            // TODO: Convert directly to DateTime without passing through EDBTimeStamp?
            var ts = ReadTimeStamp(buf, len, fieldDescription);
            try
            {
                if (ts.IsFinite)
                    return ts.ToDateTime();
                if (!ConvertInfinityDateTime)
                    throw new InvalidCastException("Can't convert infinite timestamp values to DateTime");
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
            => ReadTimeStamp(buf, len, fieldDescription);

        /// <summary>
        /// Reads a timestamp from the buffer as an <see cref="EDBDateTime"/>.
        /// </summary>
        protected EDBDateTime ReadTimeStamp(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var value = buf.ReadInt64();
            if (value == long.MaxValue)
                return EDBDateTime.Infinity;
            if (value == long.MinValue)
                return EDBDateTime.NegativeInfinity;
            if (value >= 0)
            {
                var date = (int)(value / 86400000000L);
                var time = value % 86400000000L;

                date += 730119; // 730119 = days since era (0001-01-01) for 2000-01-01
                time *= 10; // To 100ns

                return new EDBDateTime(new EDBDate(date), new TimeSpan(time));
            }
            else
            {
                value = -value;
                var date = (int)(value / 86400000000L);
                var time = value % 86400000000L;
                if (time != 0)
                {
                    ++date;
                    time = 86400000000L - time;
                }

                date = 730119 - date; // 730119 = days since era (0001-01-01) for 2000-01-01
                time *= 10; // To 100ns

                return new EDBDateTime(new EDBDate(date), new TimeSpan(time));
            }
        }

        #endregion Read

        #region Write

        /// <inheritdoc />
        public override int ValidateAndGetLength(DateTime value, EDBParameter? parameter) => 8;

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBDateTime value, EDBParameter? parameter) => 8;

        /// <inheritdoc />
        public override void Write(EDBDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            if (value.IsInfinity)
            {
                buf.WriteInt64(long.MaxValue);
                return;
            }

            if (value.IsNegativeInfinity)
            {
                buf.WriteInt64(long.MinValue);
                return;
            }

            var uSecsTime = value.Time.Ticks / 10;

            if (value >= new EDBDateTime(2000, 1, 1, 0, 0, 0))
            {
                var uSecsDate = (value.Date.DaysSinceEra - 730119) * 86400000000L;
                buf.WriteInt64(uSecsDate + uSecsTime);
            }
            else
            {
                var uSecsDate = (730119 - value.Date.DaysSinceEra) * 86400000000L;
                buf.WriteInt64(-(uSecsDate - uSecsTime));
            }
        }

        /// <inheritdoc />
        public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            if (ConvertInfinityDateTime)
            {
                if (value == DateTime.MaxValue)
                {
                    buf.WriteInt64(long.MaxValue);
                    return;
                }

                if (value == DateTime.MinValue)
                {
                    buf.WriteInt64(long.MinValue);
                    return;
                }
            }

            Write(new EDBDateTime(value), buf, parameter);
        }

        #endregion Write
    }
}
