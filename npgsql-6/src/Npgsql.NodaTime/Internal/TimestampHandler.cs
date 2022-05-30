using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using BclTimestampHandler = EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers.TimestampHandler;
using static EnterpriseDB.EDBClient.NodaTime.Internal.NodaTimeUtils;

namespace EnterpriseDB.EDBClient.NodaTime.Internal
{
    sealed partial class TimestampHandler : EDBSimpleTypeHandler<LocalDateTime>,
        IEDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<long>
    {
        readonly BclTimestampHandler _bclHandler;

        const string InfinityExceptionMessage = "Can't read infinity value since EnterpriseDB.EDBClient.DisableDateTimeInfinityConversions is enabled";

        internal TimestampHandler(PostgresType postgresType)
            : base(postgresType)
            => _bclHandler = new BclTimestampHandler(postgresType);

        #region Read

        public override LocalDateTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => ReadLocalDateTime(buf);

        // TODO: Switch to use LocalDateTime.MinMaxValue when available (#4061)
        internal static LocalDateTime ReadLocalDateTime(EDBReadBuffer buf)
            => buf.ReadInt64() switch
            {
                long.MaxValue => DisableDateTimeInfinityConversions
                    ? throw new InvalidCastException(InfinityExceptionMessage)
                    : LocalDate.MaxIsoValue + LocalTime.MaxValue,
                long.MinValue => DisableDateTimeInfinityConversions
                    ? throw new InvalidCastException(InfinityExceptionMessage)
                    : LocalDate.MinIsoValue + LocalTime.MinValue,
                var value => DecodeInstant(value).InUtc().LocalDateTime
            };

        DateTime IEDBSimpleTypeHandler<DateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => _bclHandler.Read(buf, len, fieldDescription);

        long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => ((IEDBSimpleTypeHandler<long>)_bclHandler).Read(buf, len, fieldDescription);

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(LocalDateTime value, EDBParameter? parameter)
            => 8;

        public override void Write(LocalDateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => WriteLocalDateTime(value, buf);

        internal static void WriteLocalDateTime(LocalDateTime value, EDBWriteBuffer buf)
        {
            // TODO: Switch to use LocalDateTime.MinMaxValue when available (#4061)
            if (!DisableDateTimeInfinityConversions)
            {
                if (value == LocalDate.MaxIsoValue + LocalTime.MaxValue)
                {
                    buf.WriteInt64(long.MaxValue);
                    return;
                }

                if (value == LocalDate.MinIsoValue + LocalTime.MinValue)
                {
                    buf.WriteInt64(long.MinValue);
                    return;
                }
            }

            buf.WriteInt64(EncodeInstant(value.InUtc().ToInstant()));
        }

        public int ValidateAndGetLength(DateTime value, EDBParameter? parameter)
            => ((IEDBSimpleTypeHandler<DateTime>)_bclHandler).ValidateAndGetLength(value, parameter);

        public int ValidateAndGetLength(long value, EDBParameter? parameter)
            => ((IEDBSimpleTypeHandler<long>)_bclHandler).ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<DateTime>.Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => ((IEDBSimpleTypeHandler<DateTime>)_bclHandler).Write(value, buf, parameter);

        void IEDBSimpleTypeHandler<long>.Write(long value, EDBWriteBuffer buf, EDBParameter? parameter)
            => ((IEDBSimpleTypeHandler<long>)_bclHandler).Write(value, buf, parameter);

        #endregion Write
    }
}
