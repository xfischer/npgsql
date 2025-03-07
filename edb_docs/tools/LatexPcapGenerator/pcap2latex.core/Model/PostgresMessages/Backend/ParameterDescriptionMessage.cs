namespace pcap2latex;

public class ParameterDescriptionMessage(char code, int length) : PostgresMessageBase(code, length)
{

    public short ParameterCount { get; internal set; }

    public List<int> ParameterOids { get; internal set; } = [];

    internal static ParameterDescriptionMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ParameterDescriptionMessage(messageCode, len)
        {
            ParameterCount = reader.ReadInt16()
        };

        for (int i = 0; i < message.ParameterCount; i++)
        {
            message.ParameterOids.Add(reader.ReadInt32());
        }

        return message;
    }
}
