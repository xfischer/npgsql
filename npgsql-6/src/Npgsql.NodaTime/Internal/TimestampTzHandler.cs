using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using BclTimestampTzHandler = EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers.TimestampTzHandler;
using static EnterpriseDB.EDBClient.NodaTime.Internal.NodaTimeUtils;

namespace EnterpriseDB.EDBClient.NodaTime.Internal
{
    sealed partial class TimestampTzHandler : EDBSimpleTypeHandler<Instant>, IEDBSimpleTypeHandler<ZonedDateTime>,
        IEDBSimpleTypeHandler<OffsetDateTime>, IEDBSimpleTypeHandler<DateTimeOffset>,
        IEDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<long>
    {
        readonly BclTimestampTzHandler _bclHandler;

        const string InfinityExceptionMessage = "Can't read infinity value since EnterpriseDB.EDBClient.DisableDateTimeInfinityConversions is enabled";

        public TimestampTzHandler(PostgresType postgresType)
            : base(postgresType)
            => _bclHandler = new BclTimestampTzHandler(postgresType);

        #region Read

        public override Instant Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => ReadInstant(buf);

        internal static Instant ReadInstant(EDBReadBuffer buf)
            => buf.ReadInt64() switch
            {
                long.MaxValue => DisableDateTimeInfinityConversions ? throw new InvalidCastException(InfinityExceptionMessage) : Instant.MaxValue,
                long.MinValue => DisableDateTimeInfinityConversions ? throw new InvalidCastException(InfinityExceptionMessage) : Instant.MinValue,
                var value => DecodeInstant(value)
            };

        ZonedDateTime IEDBSimpleTypeHandler<ZonedDateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription).InUtc();

        OffsetDateTime IEDBSimpleTypeHandler<OffsetDateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => Read(buf, len, fieldDescription).WithOffset(Offset.Zero);

        DateTimeOffset IEDBSimpleTypeHandler<DateTimeOffset>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => _bclHandler.Read<DateTimeOffset>(buf, len, fieldDescription);

        DateTime IEDBSimpleTypeHandler<DateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => _bclHandler.Read<DateTime>(buf, len, fieldDescription);

        long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => ((IEDBSimpleTypeHandler<long>)_bclHandler).Read(buf, len, fieldDescription);

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(Instant value, EDBParameter? parameter)
            => 8;

        int IEDBSimpleTypeHandler<ZonedDateTime>.ValidateAndGetLength(ZonedDateTime value, EDBParameter? parameter)
            => value.Zone == DateTimeZone.Utc || LegacyTimestampBehavior
                ? 8
                : throw new InvalidCastException(
                    $"Cannot write ZonedDateTime with Zone={value.Zone} to PostgreSQL type 'timestamp with time zone', " +
                    "only UTC is supported. " +
                    "See the EnterpriseDB.EDBClient.EnableLegacyTimestampBehavior AppContext switch to enable legacy behavior.");

        public int ValidateAndGetLength(OffsetDateTime value, EDBParameter? parameter)
            => value.Offset == Offset.Zero || LegacyTimestampBehavior
                ? 8
                : throw new InvalidCastException(
                    $"Cannot write OffsetDateTime with Offset={value.Offset} to PostgreSQL type 'timestamp with time zone', " +
                    "only offset 0 (UTC) is supported. " +
                    "See the EnterpriseDB.EDBClient.EnableLegacyTimestampBehavior AppContext switch to enable legacy behavior.");

        public override void Write(Instant value, EDBWriteBuffer buf, EDBParameter? parameter)
            => WriteInstant(value, buf);

        internal static void WriteInstant(Instant value, EDBWriteBuffer buf)
        {
            if (!DisableDateTimeInfinityConversions)
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

            buf.WriteInt64(EncodeInstant(value));
        }

        void IEDBSimpleTypeHandler<ZonedDateTime>.Write(ZonedDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => Write(value.ToInstant(), buf, parameter);

        public void Write(OffsetDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => Write(value.ToInstant(), buf, parameter);

        int IEDBSimpleTypeHandler<DateTimeOffset>.ValidateAndGetLength(DateTimeOffset value, EDBParameter? parameter)
            => _bclHandler.ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<DateTimeOffset>.Write(DateTimeOffset value, EDBWriteBuffer buf, EDBParameter? parameter)
            => _bclHandler.Write(value, buf, parameter);

        int IEDBSimpleTypeHandler<DateTime>.ValidateAndGetLength(DateTime value, EDBParameter? parameter)
            => ((IEDBSimpleTypeHandler<DateTime>)_bclHandler).ValidateAndGetLength(value, parameter);

        public int ValidateAndGetLength(long value, EDBParameter? parameter)
            => ((IEDBSimpleTypeHandler<long>)_bclHandler).ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<DateTime>.Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => _bclHandler.Write(value, buf, parameter);

        void IEDBSimpleTypeHandler<long>.Write(long value, EDBWriteBuffer buf, EDBParameter? parameter)
            => ((IEDBSimpleTypeHandler<long>)_bclHandler).Write(value, buf, parameter);

        #endregion Write
    }
}
