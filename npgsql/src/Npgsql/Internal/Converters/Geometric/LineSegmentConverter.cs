using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class LineSegmentConverter : PgBufferedConverter<EDBLSeg>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(double) * 4);
        return format is DataFormat.Binary;
    }

    protected override EDBLSeg ReadCore(PgReader reader)
        => new(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());

    protected override void WriteCore(PgWriter writer, EDBLSeg value)
    {
        writer.WriteDouble(value.Start.X);
        writer.WriteDouble(value.Start.Y);
        writer.WriteDouble(value.End.X);
        writer.WriteDouble(value.End.Y);
    }
}
