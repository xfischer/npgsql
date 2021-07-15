using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EDBTypes;
using BclIntervalHandler = EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers.IntervalHandler;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.NodaTime
{
    public class IntervalHandlerFactory : EDBTypeHandlerFactory<Period>
    {
        // Check for the legacy floating point timestamps feature
        public override EDBTypeHandler<Period> Create(PostgresType postgresType, EDBConnection conn)
            => conn.HasIntegerDateTimes
                ? new IntervalHandler(postgresType)
                : throw new NotSupportedException($"The deprecated floating-point date/time format is not supported by {nameof(EnterpriseDB.EDBClient)}.");
    }

    sealed class IntervalHandler :
        EDBSimpleTypeHandler<Period>,
        IEDBSimpleTypeHandler<Duration>,
        IEDBSimpleTypeHandler<EDBTimeSpan>,
        IEDBSimpleTypeHandler<TimeSpan>
    {
        readonly BclIntervalHandler _bclHandler;

        internal IntervalHandler(PostgresType postgresType) : base(postgresType)
            => _bclHandler = new BclIntervalHandler(postgresType);

        public override Period Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var microsecondsInDay = buf.ReadInt64();
            var days = buf.ReadInt32();
            var totalMonths = buf.ReadInt32();

            // NodaTime will normalize most things (i.e. nanoseconds to milliseconds, seconds...)
            // but it will not normalize months to years.
            var months = totalMonths % 12;
            var years = totalMonths / 12;

            return new PeriodBuilder
            {
                Nanoseconds = microsecondsInDay * 1000,
                Days = days,
                Months = months,
                Years = years
            }.Build().Normalize();
        }

        public override int ValidateAndGetLength(Period value, EDBParameter? parameter)
            => 16;

        public override void Write(Period value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            // Note that the end result must be long
            // see #3438
            var microsecondsInDay =
                (((value.Hours * NodaConstants.MinutesPerHour + value.Minutes) * NodaConstants.SecondsPerMinute + value.Seconds) * NodaConstants.MillisecondsPerSecond + value.Milliseconds) * 1000 +
                value.Nanoseconds / 1000; // Take the microseconds, discard the nanosecond remainder

            buf.WriteInt64(microsecondsInDay);
            buf.WriteInt32(value.Weeks * 7 + value.Days); // days
            buf.WriteInt32(value.Years * 12 + value.Months); // months
        }

        Duration IEDBSimpleTypeHandler<Duration>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        {
            var microsecondsInDay = buf.ReadInt64();
            var days = buf.ReadInt32();
            var totalMonths = buf.ReadInt32();

            if (totalMonths != 0)
                throw new EDBException("Cannot read PostgreSQL interval with non-zero months to NodaTime Duration. Try reading as a NodaTime Period instead.");

            return Duration.FromDays(days) + Duration.FromNanoseconds(microsecondsInDay * 1000);
        }

        public int ValidateAndGetLength(Duration value, EDBParameter? parameter) => 16;

        public void Write(Duration value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            const long microsecondsPerSecond = 1_000_000;

            // Note that the end result must be long
            // see #3438
            var microsecondsInDay =
                (((value.Hours * NodaConstants.MinutesPerHour + value.Minutes) * NodaConstants.SecondsPerMinute + value.Seconds) *
                    microsecondsPerSecond + value.SubsecondNanoseconds / 1000); // Take the microseconds, discard the nanosecond remainder

            buf.WriteInt64(microsecondsInDay);
            buf.WriteInt32(value.Days); // days
            buf.WriteInt32(0); // months
        }

        EDBTimeSpan IEDBSimpleTypeHandler<EDBTimeSpan>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => _bclHandler.Read<EDBTimeSpan>(buf, len, fieldDescription);

        int IEDBSimpleTypeHandler<EDBTimeSpan>.ValidateAndGetLength(EDBTimeSpan value, EDBParameter? parameter)
            => _bclHandler.ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<EDBTimeSpan>.Write(EDBTimeSpan value, EDBWriteBuffer buf, EDBParameter? parameter)
            => _bclHandler.Write(value, buf, parameter);

        TimeSpan IEDBSimpleTypeHandler<TimeSpan>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => _bclHandler.Read<TimeSpan>(buf, len, fieldDescription);

        int IEDBSimpleTypeHandler<TimeSpan>.ValidateAndGetLength(TimeSpan value, EDBParameter? parameter)
            => ((IEDBSimpleTypeHandler<TimeSpan>)_bclHandler).ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<TimeSpan>.Write(TimeSpan value, EDBWriteBuffer buf, EDBParameter? parameter)
            => ((IEDBSimpleTypeHandler<TimeSpan>)_bclHandler).Write(value, buf, parameter);
    }
}
