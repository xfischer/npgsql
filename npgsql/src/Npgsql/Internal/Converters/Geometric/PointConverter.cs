using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class PointConverter : PgBufferedConverter<EDBPoint>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(double) * 2);
        return format is DataFormat.Binary;
    }

    protected override EDBPoint ReadCore(PgReader reader)
        => new(reader.ReadDouble(), reader.ReadDouble());

    protected override void WriteCore(PgWriter writer, EDBPoint value)
    {
        writer.WriteDouble(value.X);
        writer.WriteDouble(value.Y);
    }
}
