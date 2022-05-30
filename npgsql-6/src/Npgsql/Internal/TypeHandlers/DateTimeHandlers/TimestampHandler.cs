using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;
using static EnterpriseDB.EDBClient.Util.Statics;
using static EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers.DateTimeUtils;

#pragma warning disable 618 // EDBDateTime is obsolete, remove in 7.0

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL timestamp data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public partial class TimestampHandler : EDBSimpleTypeHandlerWithPsv<DateTime, EDBDateTime>, IEDBSimpleTypeHandler<long>
    {
        /// <summary>
        /// Constructs a <see cref="TimestampHandler"/>.
        /// </summary>
        public TimestampHandler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        /// <inheritdoc />
        public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => ReadDateTime(buf, DateTimeKind.Unspecified);

        /// <inheritdoc />
        protected override EDBDateTime ReadPsv(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => ReadEDBDateTime(buf, len, fieldDescription);

        long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => buf.ReadInt64();

        #endregion Read

        #region Write

        /// <inheritdoc />
        public override int ValidateAndGetLength(DateTime value, EDBParameter? parameter)
            => value.Kind != DateTimeKind.Utc || LegacyTimestampBehavior
                ? 8
                : throw new InvalidCastException(
                    "Cannot write DateTime with Kind=UTC to PostgreSQL type 'timestamp without time zone', " +
                    "consider using 'timestamp with time zone'. " +
                    "Note that it's not possible to mix DateTimes with different Kinds in an array/range. " +
                    "See the EnterpriseDB.EDBClient.EnableLegacyTimestampBehavior AppContext switch to enable legacy behavior.");

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBDateTime value, EDBParameter? parameter)
            => value.Kind != DateTimeKind.Utc || LegacyTimestampBehavior
                ? 8
                : throw new InvalidCastException(
                    "Cannot write DateTime with Kind=UTC to PostgreSQL type 'timestamp without time zone', " +
                    "consider using 'timestamp with time zone'. " +
                    "Note that it's not possible to mix DateTimes with different Kinds in an array/range. " +
                    "See the EnterpriseDB.EDBClient.EnableLegacyTimestampBehavior AppContext switch to enable legacy behavior.");

        /// <inheritdoc />
        public int ValidateAndGetLength(long value, EDBParameter? parameter) => 8;

        /// <inheritdoc />
        public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => WriteTimestamp(value, buf);

        /// <inheritdoc />
        public override void Write(EDBDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => WriteTimestamp(value, buf);

        /// <inheritdoc />
        public void Write(long value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteInt64(value);

        #endregion Write
    }
}
