using pcap2latex;

namespace epascap2latex;

public class OutDescriptionMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{

    public short FieldCount { get; internal set; }

    public List<OutParamDescription> ParameterDescriptions { get; internal set; } = [];

    public static OutDescriptionMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new OutDescriptionMessage(pgMessage, len)
        {
            FieldCount = reader.ReadInt16()
        };

        for (int i = 0; i < message.FieldCount; i++)
        {
            message.ParameterDescriptions.Add(OutParamDescription.Read(reader, len));
        }

        return message;
    }
}
