namespace pcap2latex;

internal class TerminateMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    internal static TerminateMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new TerminateMessage(pgMessage, len);

        return packet;
    }
}