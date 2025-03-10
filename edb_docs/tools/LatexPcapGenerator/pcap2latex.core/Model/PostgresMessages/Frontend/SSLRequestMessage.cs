using System.Diagnostics;

namespace pcap2latex;

public class SSLRequestMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public int Payload { get; private set; }

    internal static SSLRequestMessage Read(PostgresMessage pgMessage, int messageLength, PcapBinaryReader reader)
    {
        var message = new SSLRequestMessage(pgMessage, messageLength)
        {
            Payload = reader.ReadInt32()
        };
        Debug.Assert(message.Payload == 80877103);

        return message;
    }
}