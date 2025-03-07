using System.Diagnostics;

namespace pcap2latex;

public class BackendKeyDataMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public int ProcessId { get; private set; }
    public uint SecretKey { get; private set; }

    internal static BackendKeyDataMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        Debug.Assert(len == 12);
        var message = new BackendKeyDataMessage(messageCode, len)
        {
            ProcessId = reader.ReadInt32(),
            SecretKey = reader.ReadUInt32()
        };

        return message;
    }
}