using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.NodaTime.Properties;
using EnterpriseDB.EDBClient.PostgresTypes;
using static EnterpriseDB.EDBClient.NodaTime.Internal.NodaTimeUtils;
using BclDateHandler = EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers.DateHandler;

namespace EnterpriseDB.EDBClient.NodaTime.Internal;

sealed partial class DateHandler : EDBSimpleTypeHandler<LocalDate>,
    IEDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<int>
#if NET6_0_OR_GREATER
    , IEDBSimpleTypeHandler<DateOnly>
#endif
{
    readonly BclDateHandler _bclHandler;

    internal DateHandler(PostgresType postgresType)
        : base(postgresType)
        => _bclHandler = new BclDateHandler(postgresType);

    public override LocalDate Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => buf.ReadInt32() switch
        {
            int.MaxValue => DisableDateTimeInfinityConversions
                ? throw new InvalidCastException(NpgsqlNodaTimeStrings.CannotReadInfinityValue)
                : LocalDate.MaxIsoValue,
            int.MinValue => DisableDateTimeInfinityConversions
                ? throw new InvalidCastException(NpgsqlNodaTimeStrings.CannotReadInfinityValue)
                : LocalDate.MinIsoValue,
            var value => new LocalDate().PlusDays(value + 730119)
        };

    public override int ValidateAndGetLength(LocalDate value, EDBParameter? parameter)
        => 4;

    public override void Write(LocalDate value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        if (!DisableDateTimeInfinityConversions)
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

        var totalDaysSinceEra = Period.Between(default, value, PeriodUnits.Days).Days;
        buf.WriteInt32(totalDaysSinceEra - 730119);
    }

    DateTime IEDBSimpleTypeHandler<DateTime>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => _bclHandler.Read<DateTime>(buf, len, fieldDescription);

    int IEDBSimpleTypeHandler<DateTime>.ValidateAndGetLength(DateTime value, EDBParameter? parameter)
        => _bclHandler.ValidateAndGetLength(value, parameter);

    void IEDBSimpleTypeHandler<DateTime>.Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
        => _bclHandler.Write(value, buf, parameter);

    int IEDBSimpleTypeHandler<int>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => _bclHandler.Read<int>(buf, len, fieldDescription);

    int IEDBSimpleTypeHandler<int>.ValidateAndGetLength(int value, EDBParameter? parameter)
        => _bclHandler.ValidateAndGetLength(value, parameter);

    void IEDBSimpleTypeHandler<int>.Write(int value, EDBWriteBuffer buf, EDBParameter? parameter)
        => _bclHandler.Write(value, buf, parameter);

#if NET6_0_OR_GREATER
    DateOnly IEDBSimpleTypeHandler<DateOnly>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => _bclHandler.Read<DateOnly>(buf, len, fieldDescription);

    public int ValidateAndGetLength(DateOnly value, EDBParameter? parameter)
        => _bclHandler.ValidateAndGetLength(value, parameter);

    public void Write(DateOnly value, EDBWriteBuffer buf, EDBParameter? parameter)
        => _bclHandler.Write(value, buf, parameter);
#endif

    public override EDBTypeHandler CreateRangeHandler(PostgresType pgRangeType)
        => new DateRangeHandler(pgRangeType, this);
}
