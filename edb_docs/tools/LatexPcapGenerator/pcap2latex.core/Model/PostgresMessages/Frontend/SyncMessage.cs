namespace pcap2latex;

public class SyncMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    internal static SyncMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new SyncMessage(pgMessage, len);

        return message;
    }
}