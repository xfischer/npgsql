using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using BclTimeHandler = EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers.TimeHandler;

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

    sealed class TimeHandler : EDBSimpleTypeHandler<LocalTime>, IEDBSimpleTypeHandler<TimeSpan>
    {
        readonly BclTimeHandler _bclHandler;

        internal TimeHandler(PostgresType postgresType) : base(postgresType)
            => _bclHandler = new BclTimeHandler(postgresType);

        // PostgreSQL time resolution == 1 microsecond == 10 ticks
        public override LocalTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => LocalTime.FromTicksSinceMidnight(buf.ReadInt64() * 10);

        public override int ValidateAndGetLength(LocalTime value, EDBParameter? parameter)
            => 8;

        public override void Write(LocalTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteInt64(value.TickOfDay / 10);

        TimeSpan IEDBSimpleTypeHandler<TimeSpan>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
           => _bclHandler.Read<TimeSpan>(buf, len, fieldDescription);

        int IEDBSimpleTypeHandler<TimeSpan>.ValidateAndGetLength(TimeSpan value, EDBParameter? parameter)
            => _bclHandler.ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<TimeSpan>.Write(TimeSpan value, EDBWriteBuffer buf, EDBParameter? parameter)
            => _bclHandler.Write(value, buf, parameter);
    }
}
