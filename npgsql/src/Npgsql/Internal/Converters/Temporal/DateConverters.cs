using System;
using EnterpriseDB.EDBClient.Properties;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

#if NET8_0_OR_GREATER // EnterpriseDB (NETFRAMEWORK)
sealed class DateOnlyDateConverter(bool dateTimeInfinityConversions) : PgBufferedConverter<DateOnly>
{
    static readonly DateOnly BaseValue = new(2000, 1, 1);

    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(int));
        return format is DataFormat.Binary;
    }

    protected override DateOnly ReadCore(PgReader reader)
        => reader.ReadInt32() switch
        {
            int.MaxValue => dateTimeInfinityConversions
                ? DateOnly.MaxValue
                : throw new InvalidCastException(EDBStrings.CannotReadInfinityValue),
            int.MinValue => dateTimeInfinityConversions
                ? DateOnly.MinValue
                : throw new InvalidCastException(EDBStrings.CannotReadInfinityValue),
            var value => BaseValue.AddDays(value)
        };

    protected override void WriteCore(PgWriter writer, DateOnly value)
    {
        if (dateTimeInfinityConversions)
        {
            if (value == DateOnly.MaxValue)
            {
                writer.WriteInt32(int.MaxValue);
                return;
            }

            if (value == DateOnly.MinValue)
            {
                writer.WriteInt32(int.MinValue);
                return;
            }
        }

        writer.WriteInt32(value.DayNumber - BaseValue.DayNumber);
    }
}

// EnterpriseDB (EC-3056 dateonly / timeonly fix with EPAS)
sealed class DateOnlyTimeStampConverter : PgBufferedConverter<DateOnly>
{
    readonly bool _dateTimeInfinityConversions;

    static readonly DateOnly BaseValue = new(2000, 1, 1);

    public DateOnlyTimeStampConverter(bool dateTimeInfinityConversions)
        => _dateTimeInfinityConversions = dateTimeInfinityConversions;

    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(int));
        return format is DataFormat.Binary;
    }

    protected override DateOnly ReadCore(PgReader reader)
        => reader.ReadInt32() switch
        {
            int.MaxValue => _dateTimeInfinityConversions
                ? DateOnly.MaxValue
                : throw new InvalidCastException(EDBStrings.CannotReadInfinityValue),
            int.MinValue => _dateTimeInfinityConversions
                ? DateOnly.MinValue
                : throw new InvalidCastException(EDBStrings.CannotReadInfinityValue),
            var value => BaseValue.AddDays(value)
        };

    protected override void WriteCore(PgWriter writer, DateOnly value)
    {
        if (_dateTimeInfinityConversions)
        {
            if (value == DateOnly.MaxValue)
            {
                writer.WriteInt32(int.MaxValue);
                return;
            }

            if (value == DateOnly.MinValue)
            {
                writer.WriteInt32(int.MinValue);
                return;
            }
        }

        writer.WriteInt32(value.DayNumber - BaseValue.DayNumber);
    }
}
#endif

sealed class DateTimeDateConverter(bool dateTimeInfinityConversions) : PgBufferedConverter<DateTime>
{
    static readonly DateTime BaseValue = new(2000, 1, 1, 0, 0, 0);

    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(int));
        return format is DataFormat.Binary;
    }

    protected override DateTime ReadCore(PgReader reader)
        => reader.ReadInt32() switch
        {
            int.MaxValue => dateTimeInfinityConversions
                ? DateTime.MaxValue
                : throw new InvalidCastException(EDBStrings.CannotReadInfinityValue),
            int.MinValue => dateTimeInfinityConversions
                ? DateTime.MinValue
                : throw new InvalidCastException(EDBStrings.CannotReadInfinityValue),
            var value => BaseValue + TimeSpan.FromDays(value)
        };

    protected override void WriteCore(PgWriter writer, DateTime value)
    {
        if (dateTimeInfinityConversions)
        {
            if (value == DateTime.MaxValue)
            {
                writer.WriteInt32(int.MaxValue);
                return;
            }

            if (value == DateTime.MinValue)
            {
                writer.WriteInt32(int.MinValue);
                return;
            }
        }

        writer.WriteInt32((value.Date - BaseValue).Days);
    }
}
