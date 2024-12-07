
namespace pcap2latex;

internal class TerminateMessage(char code, int length) : PostgresMessageBase(code, length)
{
    internal static TerminateMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new TerminateMessage(messageCode, len);

        return packet;
    }

    internal static TerminateMessage Read(char messageCode, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var packet = new TerminateMessage(messageCode, len);

        return packet;
    }
}