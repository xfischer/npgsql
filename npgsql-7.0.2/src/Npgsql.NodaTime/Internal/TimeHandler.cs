using System;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using BclTimeHandler = EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers.TimeHandler;

namespace EnterpriseDB.EDBClient.NodaTime.Internal;

sealed partial class TimeHandler : EDBSimpleTypeHandler<LocalTime>, IEDBSimpleTypeHandler<TimeSpan>
#if NET6_0_OR_GREATER
    , IEDBSimpleTypeHandler<TimeOnly>
#endif
{
    readonly BclTimeHandler _bclHandler;

    internal TimeHandler(PostgresType postgresType)
        : base(postgresType)
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

#if NET6_0_OR_GREATER
    TimeOnly IEDBSimpleTypeHandler<TimeOnly>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => _bclHandler.Read<TimeOnly>(buf, len, fieldDescription);

    public int ValidateAndGetLength(TimeOnly value, EDBParameter? parameter)
        => _bclHandler.ValidateAndGetLength(value, parameter);

    public void Write(TimeOnly value, EDBWriteBuffer buf, EDBParameter? parameter)
        => _bclHandler.Write(value, buf, parameter);
#endif
}