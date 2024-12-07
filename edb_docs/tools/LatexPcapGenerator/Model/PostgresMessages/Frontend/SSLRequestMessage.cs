using System.Diagnostics;

namespace pcap2latex;

public class SSLRequestMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public int Payload { get; private set; }

    internal static SSLRequestMessage Read(char messageCode, int messageLength, PcapBinaryReader reader)
    {
        var message = new SSLRequestMessage(messageCode, messageLength);
        message.Payload = reader.ReadInt32();
        Debug.Assert(message.Payload == 80877103);

        return message;
    }

    internal static SSLRequestMessage Read(char code, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new SSLRequestMessage(code, len);
        message.Payload = LatexHelper.SafeGet(proto, 3, f => int.Parse(f.Show));

        Debug.Assert(message.Payload == 80877103);

        return message;
    }
}