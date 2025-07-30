using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class TidConverter : PgBufferedConverter<EDBTid>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(uint) + sizeof(ushort));
        return format is DataFormat.Binary;
    }
    protected override EDBTid ReadCore(PgReader reader) => new(reader.ReadUInt32(), reader.ReadUInt16());
    protected override void WriteCore(PgWriter writer, EDBTid value)
    {
        writer.WriteUInt32(value.BlockNumber);
        writer.WriteUInt16(value.OffsetNumber);
    }
}
