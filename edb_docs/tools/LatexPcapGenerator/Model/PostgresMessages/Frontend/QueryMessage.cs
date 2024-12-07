namespace pcap2latex;

public class QueryMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public string Query { get; private set; }

    internal static QueryMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new QueryMessage(messageCode, len);
        packet.Query = reader.ReadNullTerminatedString(len);
        
        return packet;
    }

    internal static QueryMessage Read(char messageCode, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var packet = new QueryMessage(messageCode, len);
        packet.Query = proto.Fields[3].Showname;

        return packet;
    }
}
