using pcap2latex;

namespace epascap2latex;

public class DescribeOutMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public char PortalOrStatement { get; internal set; }
    public string PortalOrStatementName { get; internal set; } = "";

    public static DescribeOutMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new DescribeOutMessage(messageCode, len)
        {
            PortalOrStatement = reader.ReadChar(),
            PortalOrStatementName = reader.ReadNullTerminatedString(len)
        };

        return message;
    }
}