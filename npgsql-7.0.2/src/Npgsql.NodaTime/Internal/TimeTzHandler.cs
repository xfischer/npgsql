using System;
using NodaTime;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using BclTimeTzHandler = EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers.TimeTzHandler;

namespace EnterpriseDB.EDBClient.NodaTime.Internal;

sealed partial class TimeTzHandler : EDBSimpleTypeHandler<OffsetTime>, IEDBSimpleTypeHandler<DateTimeOffset>
{
    readonly BclTimeTzHandler _bclHandler;

    internal TimeTzHandler(PostgresType postgresType)
        : base(postgresType)
        => _bclHandler = new BclTimeTzHandler(postgresType);

    // Adjust from 1 microsecond to 100ns. Time zone (in seconds) is inverted.
    public override OffsetTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => new(
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
}