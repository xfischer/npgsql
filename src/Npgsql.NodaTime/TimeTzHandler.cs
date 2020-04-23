using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;

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

    class TimeTzHandler : EDBSimpleTypeHandler<OffsetTime>
    {
        public TimeTzHandler(PostgresType postgresType) : base(postgresType) {}

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
    }
}
