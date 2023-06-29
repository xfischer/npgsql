using System;
using System.Runtime.CompilerServices;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Properties;
using static EnterpriseDB.EDBClient.Util.Statics;

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.DateTimeHandlers;

static class DateTimeUtils
{
    const long PostgresTimestampOffsetTicks = 630822816000000000L;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static DateTime DecodeTimestamp(long value, DateTimeKind kind)
        => new(value * 10 + PostgresTimestampOffsetTicks, kind);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long EncodeTimestamp(DateTime value)
        // Rounding here would cause problems because we would round up DateTime.MaxValue
        // which would make it impossible to retrieve it back from the database, so we just drop the additional precision
        => (value.Ticks - PostgresTimestampOffsetTicks) / 10;

    internal static DateTime ReadDateTime(EDBReadBuffer buf, DateTimeKind kind)
    {
        try
        {
            return buf.ReadInt64() switch
            {
                long.MaxValue => DisableDateTimeInfinityConversions
                    ? throw new InvalidCastException(EDBStrings.CannotReadInfinityValue)
                    : DateTime.MaxValue,
                long.MinValue => DisableDateTimeInfinityConversions
                    ? throw new InvalidCastException(EDBStrings.CannotReadInfinityValue)
                    : DateTime.MinValue,
                var value => DecodeTimestamp(value, kind)
            };
        }
        catch (ArgumentOutOfRangeException e)
        {
            throw new InvalidCastException("Out of the range of DateTime (year must be between 1 and 9999)", e);
        }
    }

    internal static void WriteTimestamp(DateTime value, EDBWriteBuffer buf)
    {
        if (!DisableDateTimeInfinityConversions)
        {
            if (value == DateTime.MaxValue)
            {
                buf.WriteInt64(long.MaxValue);
                return;
            }

            if (value == DateTime.MinValue)
            {
                buf.WriteInt64(long.MinValue);
                return;
            }
        }

        var postgresTimestamp = EncodeTimestamp(value);
        buf.WriteInt64(postgresTimestamp);
    }

#if NET6_0_OR_GREATER

    static readonly DateOnly BaseValueDateOnly = new(2000, 1, 1);

    internal static DateOnly ReadDateOnly(EDBReadBuffer buf, int len, FieldDescription? fieldDescription)
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
    internal static void WriteDateOnly(DateOnly value, EDBWriteBuffer buf, EDBParameter? parameter)
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
#endif

}
