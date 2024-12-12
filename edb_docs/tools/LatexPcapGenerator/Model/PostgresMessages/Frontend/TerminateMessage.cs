
namespace pcap2latex;

internal class TerminateMessage(char code, int length) : PostgresMessageBase(code, length)
{
    internal static TerminateMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var packet = new TerminateMessage(messageCode, len);

        return packet;
    }
}