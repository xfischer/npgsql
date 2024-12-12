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
}