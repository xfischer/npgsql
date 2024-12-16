using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class PgLsnConverter : PgBufferedConverter<EDBLogSequenceNumber>
{
    public override bool CanConvert(DataFormat format, out BufferRequirements bufferRequirements)
    {
        bufferRequirements = BufferRequirements.CreateFixedSize(sizeof(ulong));
        return format is DataFormat.Binary;
    }
    protected override EDBLogSequenceNumber ReadCore(PgReader reader) => new(reader.ReadUInt64());
    protected override void WriteCore(PgWriter writer, EDBLogSequenceNumber value) => writer.WriteUInt64((ulong)value);
}
