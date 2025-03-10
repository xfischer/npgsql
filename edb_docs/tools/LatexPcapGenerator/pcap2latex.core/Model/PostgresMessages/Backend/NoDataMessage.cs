namespace pcap2latex;

internal class NoDataMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    internal static NoDataMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new NoDataMessage(pgMessage, len);

        return packet;
    }
}