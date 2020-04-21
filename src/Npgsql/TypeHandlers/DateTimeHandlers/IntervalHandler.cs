using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers
{
    /// <summary>
    /// A factory for type handlers for the PostgreSQL interval data type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("interval", EDBDbType.Interval, new[] { typeof(TimeSpan), typeof(EDBTimeSpan) })]
    public class IntervalHandlerFactory : EDBTypeHandlerFactory<TimeSpan>
    {
        /// <inheritdoc />
        public override EDBTypeHandler<TimeSpan> Create(PostgresType postgresType, EDBConnection conn)
            => conn.HasIntegerDateTimes  // Check for the legacy floating point timestamps feature
                ? new IntervalHandler(postgresType)
                : throw new NotSupportedException($"The deprecated floating-point date/time format is not supported by {nameof(EnterpriseDB.EDBClient)}.");
    }

    /// <summary>
    /// A type handler for the PostgreSQL date interval type.
    /// </summary>
    /// <remarks>
    /// See http://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public class IntervalHandler : EDBSimpleTypeHandlerWithPsv<TimeSpan, EDBTimeSpan>
    {
        internal IntervalHandler(PostgresType postgresType) : base(postgresType) {}

        /// <inheritdoc />
        public override TimeSpan Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => (TimeSpan)((IEDBSimpleTypeHandler<EDBTimeSpan>)this).Read(buf, len, fieldDescription);

        /// <inheritdoc />
        protected override EDBTimeSpan ReadPsv(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var ticks = buf.ReadInt64();
            var day = buf.ReadInt32();
            var month = buf.ReadInt32();
            return new EDBTimeSpan(month, day, ticks * 10);
        }

        /// <inheritdoc />
        public override int ValidateAndGetLength(TimeSpan value, EDBParameter? parameter) => 16;

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBTimeSpan value, EDBParameter? parameter) => 16;

        /// <inheritdoc />
        public override void Write(EDBTimeSpan value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteInt64(value.Ticks / 10); // TODO: round?
            buf.WriteInt32(value.Days);
            buf.WriteInt32(value.Months);
        }

        // TODO: Can write directly from TimeSpan
        /// <inheritdoc />
        public override void Write(TimeSpan value, EDBWriteBuffer buf, EDBParameter? parameter)
            => Write(value, buf, parameter);
    }
}
