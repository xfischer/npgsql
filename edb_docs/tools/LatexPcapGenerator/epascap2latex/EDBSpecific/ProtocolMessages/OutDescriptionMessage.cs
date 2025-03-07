using pcap2latex;

namespace epascap2latex;

public class OutDescriptionMessage(char code, int length) : PostgresMessageBase(code, length)
{

    public short FieldCount { get; internal set; }

    public List<OutParamDescription> ParameterDescriptions { get; internal set; } = [];

    public static OutDescriptionMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new OutDescriptionMessage(messageCode, len)
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
