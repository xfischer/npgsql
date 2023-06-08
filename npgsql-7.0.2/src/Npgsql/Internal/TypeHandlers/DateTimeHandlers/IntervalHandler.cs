using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.Properties;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers;

/// <summary>
/// A type handler for the PostgreSQL date interval type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
public partial class IntervalHandler : EDBSimpleTypeHandler<TimeSpan>, IEDBSimpleTypeHandler<EDBInterval>
{
    /// <summary>
    /// Constructs an <see cref="IntervalHandler"/>
    /// </summary>
    public IntervalHandler(PostgresType postgresType) : base(postgresType) {}

    /// <inheritdoc />
    public override TimeSpan Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
    {
        var microseconds = buf.ReadInt64();
        var days = buf.ReadInt32();
        var months = buf.ReadInt32();

        if (months > 0)
            throw new InvalidCastException(EDBStrings.CannotReadIntervalWithMonthsAsTimeSpan);

        return new(microseconds * 10 + days * TimeSpan.TicksPerDay);
    }

    EDBInterval IEDBSimpleTypeHandler<EDBInterval>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
    {
        var ticks = buf.ReadInt64();
        var day = buf.ReadInt32();
        var month = buf.ReadInt32();
        return new EDBInterval(month, day, ticks);
    }

    /// <inheritdoc />
    public override int ValidateAndGetLength(TimeSpan value, EDBParameter? parameter) => 16;

    /// <inheritdoc />
    public int ValidateAndGetLength(EDBInterval value, EDBParameter? parameter) => 16;

    /// <inheritdoc />
    public override void Write(TimeSpan value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        var ticksInDay = value.Ticks - TimeSpan.TicksPerDay * value.Days;

        buf.WriteInt64(ticksInDay / 10);
        buf.WriteInt32(value.Days);
        buf.WriteInt32(0);
    }

    public void Write(EDBInterval value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        buf.WriteInt64(value.Time);
        buf.WriteInt32(value.Days);
        buf.WriteInt32(value.Months);
    }
}
