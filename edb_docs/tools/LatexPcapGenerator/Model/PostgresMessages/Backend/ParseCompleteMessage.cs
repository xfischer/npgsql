namespace pcap2latex;

public class ParseCompleteMessage(char code, int length) : PostgresMessageBase(code, length)
{
    internal static ParseCompleteMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ParseCompleteMessage(messageCode, len);

        return message;
    }

    internal static ParseCompleteMessage Read(char messageCode, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new ParseCompleteMessage(messageCode, len);

        return message;
    }
}