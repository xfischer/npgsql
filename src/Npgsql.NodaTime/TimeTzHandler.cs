using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using BclTimeTzHandler = EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers.TimeTzHandler;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.NodaTime
{
    public class TimeTzHandlerFactory : EDBTypeHandlerFactory<OffsetTime>
    {
        // Check for the legacy floating point timestamps feature
        public override EDBTypeHandler<OffsetTime> Create(PostgresType postgresType, EDBConnection conn)
            => conn.HasIntegerDateTimes
                ? new TimeTzHandler(postgresType)
                : throw new NotSupportedException($"The deprecated floating-point date/time format is not supported by {nameof(EnterpriseDB.EDBClient)}.");
    }

    sealed class TimeTzHandler : EDBSimpleTypeHandler<OffsetTime>, IEDBSimpleTypeHandler<DateTimeOffset>,
                                  IEDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<TimeSpan>
    {
        readonly BclTimeTzHandler _bclHandler;

        internal TimeTzHandler(PostgresType postgresType) : base(postgresType)
            => _bclHandler = new BclTimeTzHandler(postgresType);

        // Adjust from 1 microsecond to 100ns. Time zone (in seconds) is inverted.
        public override OffsetTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => new OffsetTime(
                LocalTime.FromTicksSinceMidnight(buf.ReadInt64() * 10),
                Offset.FromSeconds(-buf.ReadInt32()));

        public override int ValidateAndGetLength(OffsetTime value, EDBParameter? parameter) => 12;

        public override void Write(OffsetTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            buf.WriteInt64(value.TickOfDay / 10);
            buf.WriteInt32(-(int)(value.Offset.Ticks / NodaConstants.TicksPerSecond));
        }

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

        TimeSpan IEDBSimpleTypeHandler<TimeSpan>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => _bclHandler.Read<TimeSpan>(buf, len, fieldDescription);

        int IEDBSimpleTypeHandler<TimeSpan>.ValidateAndGetLength(TimeSpan value, EDBParameter? parameter)
            => _bclHandler.ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<TimeSpan>.Write(TimeSpan value, EDBWriteBuffer buf, EDBParameter? parameter)
            => _bclHandler.Write(value, buf, parameter);
    }
}
