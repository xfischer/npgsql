using System;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class TimeSpanIntervalConverter : PgBufferedConverter<TimeSpan>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(long) + sizeof(int) + sizeof(int));
        return format is DataFormat.Binary;
    }

    protected override TimeSpan ReadCore(PgReader reader)
    {
        var microseconds = reader.ReadInt64();
        var days = reader.ReadInt32();
        var months = reader.ReadInt32();

        return months > 0
            ? throw new InvalidCastException(
                "Cannot read interval values with non-zero months as TimeSpan, since that type doesn't support months. Consider using NodaTime Period which better corresponds to PostgreSQL interval, or read the value as EDBInterval, or transform the interval to not contain months or years in PostgreSQL before reading it.")
            : new(microseconds * 10 + days * TimeSpan.TicksPerDay);
    }

    protected override void WriteCore(PgWriter writer, TimeSpan value)
    {
        var ticksInDay = value.Ticks - TimeSpan.TicksPerDay * value.Days;
        writer.WriteInt64(ticksInDay / 10);
        writer.WriteInt32(value.Days);
        writer.WriteInt32(0);
    }
}

sealed class EDBIntervalConverter : PgBufferedConverter<EDBInterval>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(long) + sizeof(int) + sizeof(int));
        return format is DataFormat.Binary;
    }

    protected override EDBInterval ReadCore(PgReader reader)
    {
        var ticks = reader.ReadInt64();
        var day = reader.ReadInt32();
        var month = reader.ReadInt32();
        return new EDBInterval(month, day, ticks);
    }

    protected override void WriteCore(PgWriter writer, EDBInterval value)
    {
        writer.WriteInt64(value.Time);
        writer.WriteInt32(value.Days);
        writer.WriteInt32(value.Months);
    }
}
