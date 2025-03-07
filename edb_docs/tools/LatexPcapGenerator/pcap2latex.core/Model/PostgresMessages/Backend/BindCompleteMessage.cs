namespace pcap2latex;

internal class BindCompleteMessage(char code, int length) : PostgresMessageBase(code, length)
{
    internal static BindCompleteMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new BindCompleteMessage(messageCode, len);

        return message;
    }
}