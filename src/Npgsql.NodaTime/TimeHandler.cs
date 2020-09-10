using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.NodaTime
{
    public class TimeHandlerFactory : EDBTypeHandlerFactory<LocalTime>
    {
        // Check for the legacy floating point timestamps feature
        public override EDBTypeHandler<LocalTime> Create(PostgresType postgresType, EDBConnection conn)
            => conn.HasIntegerDateTimes
                ? new TimeHandler(postgresType)
                : throw new NotSupportedException($"The deprecated floating-point date/time format is not supported by {nameof(EnterpriseDB.EDBClient)}.");
    }

    class TimeHandler : EDBSimpleTypeHandler<LocalTime>
    {
        public TimeHandler(PostgresType postgresType) : base(postgresType) {}

        // PostgreSQL time resolution == 1 microsecond == 10 ticks
        public override LocalTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => LocalTime.FromTicksSinceMidnight(buf.ReadInt64() * 10);

        public override int ValidateAndGetLength(LocalTime value, EDBParameter? parameter)
            => 8;

        public override void Write(LocalTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteInt64(value.TickOfDay / 10);
    }
}
