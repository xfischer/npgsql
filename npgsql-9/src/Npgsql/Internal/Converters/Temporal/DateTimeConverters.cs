using System;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class DateTimeConverter(bool dateTimeInfinityConversions, DateTimeKind kind) : PgBufferedConverter<DateTime>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(long));
        return format is DataFormat.Binary;
    }

    protected override DateTime ReadCore(PgReader reader)
        => PgTimestamp.Decode(reader.ReadInt64(), kind, dateTimeInfinityConversions);

    protected override void WriteCore(PgWriter writer, DateTime value)
        => writer.WriteInt64(PgTimestamp.Encode(value, dateTimeInfinityConversions));
}

#if NET6_0_OR_GREATER 
// EnterpriseDB
sealed class DateOnlyFromRedwoodDateConverter : PgBufferedConverter<DateOnly>
{
    readonly bool _dateTimeInfinityConversions;
    readonly DateTimeKind _kind;

    public DateOnlyFromRedwoodDateConverter(bool dateTimeInfinityConversions, DateTimeKind kind)
    {
        _dateTimeInfinityConversions = dateTimeInfinityConversions;
        _kind = kind;
    }

    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(long));
        return format is DataFormat.Binary;
    }

    protected override DateOnly ReadCore(PgReader reader)
        => DateOnly.FromDateTime(PgTimestamp.Decode(reader.ReadInt64(), _kind, _dateTimeInfinityConversions));

    protected override void WriteCore(PgWriter writer, DateOnly value)
        => writer.WriteInt64(PgTimestamp.Encode(value.ToDateTime(TimeOnly.MinValue), _dateTimeInfinityConversions));
}
#endif

sealed class DateTimeOffsetConverter(bool dateTimeInfinityConversions) : PgBufferedConverter<DateTimeOffset>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(long));
        return format is DataFormat.Binary;
    }

    protected override DateTimeOffset ReadCore(PgReader reader)
        => new(PgTimestamp.Decode(reader.ReadInt64(), DateTimeKind.Utc, dateTimeInfinityConversions), TimeSpan.Zero);

    protected override void WriteCore(PgWriter writer, DateTimeOffset value)
    {
        if (value.Offset != TimeSpan.Zero)
            throw new ArgumentException($"Cannot write DateTimeOffset with Offset={value.Offset} to PostgreSQL type 'timestamp with time zone', only offset 0 (UTC) is supported. ", nameof(value));

        writer.WriteInt64(PgTimestamp.Encode(value.DateTime, dateTimeInfinityConversions));

    }
}
