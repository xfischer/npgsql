namespace pcap2latex;

public class QueryMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public string Query { get; private set; } = "";

    internal static QueryMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new QueryMessage(pgMessage, len)
        {
            Query = reader.ReadNullTerminatedString(len)
        };

        return packet;
    }
    public override string GetStringRepresentation() => Query;
}
