using System;
using System.Diagnostics;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.Properties;
using EDBTypes;
using static EnterpriseDB.EDBClient.Util.Statics;
using static EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers.DateTimeUtils;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers;

/// <summary>
/// A type handler for the PostgreSQL timestamptz data type.
/// </summary>
/// <remarks>
/// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
///
/// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
/// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
/// Use it at your own risk.
/// </remarks>
public partial class TimestampTzHandler : EDBSimpleTypeHandler<DateTime>,
    IEDBSimpleTypeHandler<DateTimeOffset>, IEDBSimpleTypeHandler<long>
{
    /// <summary>
    /// Constructs an <see cref="TimestampTzHandler"/>.
    /// </summary>
    public TimestampTzHandler(PostgresType postgresType) : base(postgresType) {}

    /// <inheritdoc />
    public override EDBTypeHandler CreateRangeHandler(PostgresType pgRangeType)
        => new RangeHandler<DateTime, DateTimeOffset>(pgRangeType, this);

    #region Read

    /// <inheritdoc />
    public override DateTime Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
    {
        var dateTime = ReadDateTime(buf, DateTimeKind.Utc);
        return LegacyTimestampBehavior && (DisableDateTimeInfinityConversions || dateTime != DateTime.MaxValue && dateTime != DateTime.MinValue)
            ? dateTime.ToLocalTime()
            : dateTime;
    }

    DateTimeOffset IEDBSimpleTypeHandler<DateTimeOffset>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
    {
        try
        {
            var value = buf.ReadInt64();
            switch (value)
            {
            case long.MaxValue:
                return DisableDateTimeInfinityConversions
                    ? throw new InvalidCastException(EDBStrings.CannotReadInfinityValue)
                    : DateTimeOffset.MaxValue;
            case long.MinValue:
                return DisableDateTimeInfinityConversions
                    ? throw new InvalidCastException(EDBStrings.CannotReadInfinityValue)
                    : DateTimeOffset.MinValue;
            default:
                var dateTime = DecodeTimestamp(value, DateTimeKind.Utc);
                return LegacyTimestampBehavior ? dateTime.ToLocalTime() : dateTime;
            }
        }
        catch (ArgumentOutOfRangeException e)
        {
            throw new InvalidCastException("Out of the range of DateTime (year must be between 1 and 9999)", e);
        }
    }

    long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
        => buf.ReadInt64();

    #endregion Read

    #region Write

    /// <inheritdoc />
    public override int ValidateAndGetLength(DateTime value, EDBParameter? parameter)
        => value.Kind == DateTimeKind.Utc ||
           value == DateTime.MinValue || // Allowed since this is default(DateTime) - sent without any timezone conversion.
           value == DateTime.MaxValue && !DisableDateTimeInfinityConversions ||
           LegacyTimestampBehavior
            ? 8
            : throw new InvalidCastException(
                $"Cannot write DateTime with Kind={value.Kind} to PostgreSQL type 'timestamp with time zone', only UTC is supported. " +
                "Note that it's not possible to mix DateTimes with different Kinds in an array/range. " +
                "See the EnterpriseDB.EDBClient.EnableLegacyTimestampBehavior AppContext switch to enable legacy behavior.");

    /// <inheritdoc />
    public int ValidateAndGetLength(DateTimeOffset value, EDBParameter? parameter)
        => value.Offset == TimeSpan.Zero || LegacyTimestampBehavior
            ? 8
            : throw new InvalidCastException(
                $"Cannot write DateTimeOffset with Offset={value.Offset} to PostgreSQL type 'timestamp with time zone', " +
                "only offset 0 (UTC) is supported. " +
                "Note that it's not possible to mix DateTimes with different Kinds in an array/range. " +
                "See the EnterpriseDB.EDBClient.EnableLegacyTimestampBehavior AppContext switch to enable legacy behavior.");

    /// <inheritdoc />
    public int ValidateAndGetLength(long value, EDBParameter? parameter) => 8;

    /// <inheritdoc />
    public override void Write(DateTime value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        if (LegacyTimestampBehavior)
        {
            switch (value.Kind)
            {
            case DateTimeKind.Unspecified:
            case DateTimeKind.Utc:
                break;
            case DateTimeKind.Local:
                value = value.ToUniversalTime();
                break;
            default:
                throw new InvalidOperationException($"Internal EDB bug: unexpected value {value.Kind} of enum {nameof(DateTimeKind)}. Please file a bug.");
            }
        }
        else
            Debug.Assert(value.Kind == DateTimeKind.Utc || value == DateTime.MinValue || value == DateTime.MaxValue);

        WriteTimestamp(value, buf);
    }

    /// <inheritdoc />
    public void Write(DateTimeOffset value, EDBWriteBuffer buf, EDBParameter? parameter)
    {
        if (LegacyTimestampBehavior)
            value = value.ToUniversalTime();

        Debug.Assert(value.Offset == TimeSpan.Zero);

        WriteTimestamp(value.DateTime, buf);
    }

    /// <inheritdoc />
    public void Write(long value, EDBWriteBuffer buf, EDBParameter? parameter)
        => buf.WriteInt64(value);

    #endregion Write
}