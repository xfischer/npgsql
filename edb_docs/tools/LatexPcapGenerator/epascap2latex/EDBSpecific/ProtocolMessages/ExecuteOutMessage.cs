using pcap2latex;

namespace epascap2latex;

public class ExecuteOutMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public string PortalName { get; internal set; } = "";

    public static ExecuteOutMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ExecuteOutMessage(pgMessage, len)
        {
            PortalName = reader.ReadNullTerminatedString(len)
        };

        return message;
    }
}