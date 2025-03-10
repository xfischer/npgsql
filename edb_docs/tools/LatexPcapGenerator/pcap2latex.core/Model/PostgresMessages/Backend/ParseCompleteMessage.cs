namespace pcap2latex;

public class ParseCompleteMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    internal static ParseCompleteMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ParseCompleteMessage(pgMessage, len);

        return message;
    }
}