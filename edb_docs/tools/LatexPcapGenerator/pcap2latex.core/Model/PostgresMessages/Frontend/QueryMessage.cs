namespace pcap2latex;

public class QueryMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public string Query { get; private set; } = "";

    internal static QueryMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new QueryMessage(messageCode, len)
        {
            Query = reader.ReadNullTerminatedString(len)
        };

        return packet;
    }
}
