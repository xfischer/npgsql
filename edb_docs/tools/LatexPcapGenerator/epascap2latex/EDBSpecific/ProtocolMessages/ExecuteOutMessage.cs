using pcap2latex;

namespace epascap2latex;

public class ExecuteOutMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public string PortalName { get; internal set; } = "";

    public static ExecuteOutMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ExecuteOutMessage(messageCode, len)
        {
            PortalName = reader.ReadNullTerminatedString(len)
        };

        return message;
    }
}