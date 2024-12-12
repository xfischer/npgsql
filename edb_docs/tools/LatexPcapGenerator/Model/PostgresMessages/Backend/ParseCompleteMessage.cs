namespace pcap2latex;

public class ParseCompleteMessage(char code, int length) : PostgresMessageBase(code, length)
{
    internal static ParseCompleteMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ParseCompleteMessage(messageCode, len);

        return message;
    }
}