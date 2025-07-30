using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class CircleConverter : PgBufferedConverter<EDBCircle>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(double) * 3);
        return format is DataFormat.Binary;
    }

    protected override EDBCircle ReadCore(PgReader reader)
        => new(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());

    protected override void WriteCore(PgWriter writer, EDBCircle value)
    {
        writer.WriteDouble(value.X);
        writer.WriteDouble(value.Y);
        writer.WriteDouble(value.Radius);
    }
}
