namespace pcap2latex;

public class ExecuteMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public string PortalName { get; internal set; } = "";
    public int MaxRows { get; internal set; }

    internal static ExecuteMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ExecuteMessage(messageCode, len)
        {
            PortalName = reader.ReadNullTerminatedString(len),
            MaxRows = reader.ReadInt32()
        };

        return message;
    }
}