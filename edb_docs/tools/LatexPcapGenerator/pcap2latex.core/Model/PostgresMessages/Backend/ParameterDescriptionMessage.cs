namespace pcap2latex;

public class ParameterDescriptionMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public short ParameterCount { get; internal set; }

    public List<int> ParameterOids { get; internal set; } = [];

    internal static ParameterDescriptionMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ParameterDescriptionMessage(pgMessage, len)
        {
            ParameterCount = reader.ReadInt16()
        };

        for (int i = 0; i < message.ParameterCount; i++)
        {
            message.ParameterOids.Add(reader.ReadInt32());
        }

        return message;
    }

    public override string GetStringRepresentation() => $"oids: [{string.Join(", ", ParameterOids)}]";
}
