using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.Properties;
using EDBTypes;
using static EnterpriseDB.EDBClient.Util.Statics;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers;

/// <summary>
/// A type handler for the PostgreSQL date data type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
public partial class DateHandler : EDBSimpleTypeHandler<DateTime>, IEDBSimpleTypeHandler<int>
#if NET6_0_OR_GREATER
    , IEDBSimpleTypeHandler<DateOnly>
#endif
{
    static readonly DateTime BaseValueDateTime = new(2000, 1, 1, 0, 0, 0);

    /// <summary>
    /// Constructs a <see cref="DateHandler"/>
    /// </summary>
    public DateHandler(PostgresType postgresType) : base(postgresType) {}

    #region Read

    /// <inheritdoc />
    public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        => buf.ReadInt32() switch
        {
            int.MaxValue => DisableDateTimeInfinityConversions
                ? throw new InvalidCastException(EDBStrings.CannotReadInfinityValue)
                : DateTime.MaxValue,
            int.MinValue => DisableDateTimeInfinityConversions
                ? throw new InvalidCastException(EDBStrings.CannotReadInfinityValue)
                : DateTime.MinValue,
            var value => BaseValueDateTime + TimeSpan.FromDays(value)
        };

    int IEDBSimpleTypeHandler<int>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => buf.ReadInt32();

    #endregion Read

    #region Write

    /// <inheritdoc />
    public override int ValidateAndGetLength(DateTime value, EDBParameter? parameter) => 4;

    /// <inheritdoc />
    public int ValidateAndGetLength(int value, EDBParameter? parameter) => 4;

    /// <inheritdoc />
    public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        if (!DisableDateTimeInfinityConversions)
        {
            if (value == DateTime.MaxValue)
            {
                buf.WriteInt32(int.MaxValue);
                return;
            }

            if (value == DateTime.MinValue)
            {
                buf.WriteInt32(int.MinValue);
                return;
            }
        }

        buf.WriteInt32((value.Date - BaseValueDateTime).Days);
    }

    /// <inheritdoc />
    public void Write(int value, EDBWriteBuffer buf, EDBParameter? parameter)
        => buf.WriteInt32(value);

    #endregion Write

#if NET6_0_OR_GREATER
    static readonly DateOnly BaseValueDateOnly = new(2000, 1, 1);

    DateOnly IEDBSimpleTypeHandler<DateOnly>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => buf.ReadInt32() switch
        {
            int.MaxValue => DisableDateTimeInfinityConversions
                ? throw new InvalidCastException(EDBStrings.CannotReadInfinityValue)
                : DateOnly.MaxValue,
            int.MinValue => DisableDateTimeInfinityConversions
                ? throw new InvalidCastException(EDBStrings.CannotReadInfinityValue)
                : DateOnly.MinValue,
            var value => BaseValueDateOnly.AddDays(value)
        };

    public int ValidateAndGetLength(DateOnly value, EDBParameter? parameter) => 4;

    public void Write(DateOnly value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        if (!DisableDateTimeInfinityConversions)
        {
            if (value == DateOnly.MaxValue)
            {
                buf.WriteInt32(int.MaxValue);
                return;
            }

            if (value == DateOnly.MinValue)
            {
                buf.WriteInt32(int.MinValue);
                return;
            }
        }

        buf.WriteInt32(value.DayNumber - BaseValueDateOnly.DayNumber);
    }

    public override EDBTypeHandler CreateRangeHandler(PostgresType pgRangeType)
        => new RangeHandler<DateTime, DateOnly>(pgRangeType, this);

    public override EDBTypeHandler CreateMultirangeHandler(PostgresMultirangeType pgRangeType)
        => new MultirangeHandler<DateTime, DateOnly>(pgRangeType, new RangeHandler<DateTime, DateOnly>(pgRangeType, this));
#endif
}