namespace pcap2latex;

public class DescribeMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public char PortalOrStatement { get; internal set; }
    public string PortalOrStatementName { get; internal set; } = "";

    internal static DescribeMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new DescribeMessage(pgMessage, len)
        {
            PortalOrStatement = reader.ReadChar(),
            PortalOrStatementName = reader.ReadNullTerminatedString(len)
        };

        return message;
    }

    public override string GetStringRepresentation() => $"{PortalOrStatement}: {PortalOrStatementName}";
}