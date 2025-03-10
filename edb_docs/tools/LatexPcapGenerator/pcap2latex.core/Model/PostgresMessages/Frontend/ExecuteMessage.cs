namespace pcap2latex;

public class ExecuteMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public string PortalName { get; internal set; } = "";
    public int MaxRows { get; internal set; }

    internal static ExecuteMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ExecuteMessage(pgMessage, len)
        {
            PortalName = reader.ReadNullTerminatedString(len),
            MaxRows = reader.ReadInt32()
        };

        return message;
    }

    public override string GetStringRepresentation() => $"{PortalName}, maxrows={MaxRows}";
}