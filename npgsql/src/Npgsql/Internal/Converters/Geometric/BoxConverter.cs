using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class BoxConverter : PgBufferedConverter<EDBBox>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(double) * 4);
        return format is DataFormat.Binary;
    }

    protected override EDBBox ReadCore(PgReader reader)
        => new(
            new EDBPoint(reader.ReadDouble(), reader.ReadDouble()),
            new EDBPoint(reader.ReadDouble(), reader.ReadDouble()));

    protected override void WriteCore(PgWriter writer, EDBBox value)
    {
        writer.WriteDouble(value.Right);
        writer.WriteDouble(value.Top);
        writer.WriteDouble(value.Left);
        writer.WriteDouble(value.Bottom);
    }
}
