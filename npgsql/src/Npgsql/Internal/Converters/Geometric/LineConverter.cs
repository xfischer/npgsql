using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class LineConverter : PgBufferedConverter<EDBLine>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(double) * 3);
        return format is DataFormat.Binary;
    }

    protected override EDBLine ReadCore(PgReader reader)
        => new(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());

    protected override void WriteCore(PgWriter writer, EDBLine value)
    {
        writer.WriteDouble(value.A);
        writer.WriteDouble(value.B);
        writer.WriteDouble(value.C);
    }
}
