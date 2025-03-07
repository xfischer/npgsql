namespace pcap2latex;

internal class SyncMessage(char code, int length) : PostgresMessageBase(code, length)
{
    internal static SyncMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new SyncMessage(messageCode, len);

        return message;
    }
}