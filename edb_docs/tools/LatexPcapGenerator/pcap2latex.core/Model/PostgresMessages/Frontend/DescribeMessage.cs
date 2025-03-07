namespace pcap2latex;

public class DescribeMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public char PortalOrStatement { get; internal set; }
    public string PortalOrStatementName { get; internal set; } = "";

    internal static DescribeMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new DescribeMessage(messageCode, len)
        {
            PortalOrStatement = reader.ReadChar(),
            PortalOrStatementName = reader.ReadNullTerminatedString(len)
        };

        return message;
    }
}