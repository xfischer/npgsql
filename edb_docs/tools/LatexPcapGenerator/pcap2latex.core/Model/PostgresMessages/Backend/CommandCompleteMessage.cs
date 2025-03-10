namespace pcap2latex;

public class CommandCompleteMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public string Message { get; private set; } = "";

    internal static CommandCompleteMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new CommandCompleteMessage(pgMessage, len)
        {
            Message = reader.ReadNullTerminatedString(len)
        };

        return message;
    }

    public override string GetStringRepresentation() => Message;
}