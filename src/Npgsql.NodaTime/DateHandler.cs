using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EDBTypes;
using BclDateHandler = EnterpriseDB.EDBClient.TypeHandlers.DateTimeHandlers.DateHandler;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.NodaTime
{
    public class DateHandlerFactory : EDBTypeHandlerFactory<LocalDate>
    {
        public override EDBTypeHandler<LocalDate> Create(PostgresType postgresType, EDBConnection conn)
        {
            var csb = new EDBConnectionStringBuilder(conn.ConnectionString);
            return new DateHandler(postgresType, csb.ConvertInfinityDateTime);
        }
    }

    sealed class DateHandler : EDBSimpleTypeHandler<LocalDate>, IEDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<EDBDate>
    {
        /// <summary>
        /// Whether to convert positive and negative infinity values to Instant.{Max,Min}Value when
        /// an Instant is requested
        /// </summary>
        readonly bool _convertInfinityDateTime;
        readonly BclDateHandler _bclHandler;

        internal DateHandler(PostgresType postgresType, bool convertInfinityDateTime)
            : base(postgresType)
        {
            _convertInfinityDateTime = convertInfinityDateTime;
            _bclHandler = new BclDateHandler(postgresType, convertInfinityDateTime);
        }

        public override LocalDate Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            var value = buf.ReadInt32();
            if (_convertInfinityDateTime)
            {
                if (value == int.MaxValue)
                    return LocalDate.MaxIsoValue;
                if (value == int.MinValue)
                    return LocalDate.MinIsoValue;
            }
            return new LocalDate().PlusDays(value + 730119);
        }

        public override int ValidateAndGetLength(LocalDate value, EDBParameter? parameter)
            => 4;

        public override void Write(LocalDate value, EDBWriteBuffer buf, EDBParameter? parameter)
        {
            if (_convertInfinityDateTime)
            {
                if (value == LocalDate.MaxIsoValue)
                {
                    buf.WriteInt32(int.MaxValue);
                    return;
                }
                if (value == LocalDate.MinIsoValue)
                {
                    buf.WriteInt32(int.MinValue);
                    return;
                }
            }

            var totalDaysSinceEra = Period.Between(default(LocalDate), value, PeriodUnits.Days).Days;
            buf.WriteInt32(totalDaysSinceEra - 730119);
        }

        EDBDate IEDBSimpleTypeHandler<EDBDate>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => _bclHandler.Read<EDBDate>(buf, len, fieldDescription);

        int IEDBSimpleTypeHandler<EDBDate>.ValidateAndGetLength(EDBDate value, EDBParameter? parameter)
            => _bclHandler.ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<EDBDate>.Write(EDBDate value, EDBWriteBuffer buf, EDBParameter? parameter)
            => _bclHandler.Write(value, buf, parameter);

        DateTime IEDBSimpleTypeHandler<DateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
            => _bclHandler.Read<DateTime>(buf, len, fieldDescription);

        int IEDBSimpleTypeHandler<DateTime>.ValidateAndGetLength(DateTime value, EDBParameter? parameter)
            => _bclHandler.ValidateAndGetLength(value, parameter);

        void IEDBSimpleTypeHandler<DateTime>.Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
            => _bclHandler.Write(value, buf, parameter);
    }
}
