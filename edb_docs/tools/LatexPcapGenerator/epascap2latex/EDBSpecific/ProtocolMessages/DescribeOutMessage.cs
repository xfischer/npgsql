using pcap2latex;

namespace epascap2latex;

public class DescribeOutMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public char PortalOrStatement { get; internal set; }
    public string PortalOrStatementName { get; internal set; } = "";

    public static DescribeOutMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new DescribeOutMessage(pgMessage, len)
        {
            PortalOrStatement = reader.ReadChar(),
            PortalOrStatementName = reader.ReadNullTerminatedString(len)
        };

        return message;
    }
}