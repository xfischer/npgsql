namespace pcap2latex;

internal class BindCompleteMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    internal static BindCompleteMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new BindCompleteMessage(pgMessage, len);

        return message;
    }

    public override string GetStringRepresentation() => string.Empty;
}