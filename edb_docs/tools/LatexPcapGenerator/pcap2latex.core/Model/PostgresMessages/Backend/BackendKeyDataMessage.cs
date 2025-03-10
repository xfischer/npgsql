using System.Diagnostics;

namespace pcap2latex;

public class BackendKeyDataMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public int ProcessId { get; private set; }
    public uint SecretKey { get; private set; }

    internal static BackendKeyDataMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        Debug.Assert(len == 12);
        var message = new BackendKeyDataMessage(pgMessage, len)
        {
            ProcessId = reader.ReadInt32(),
            SecretKey = reader.ReadUInt32()
        };

        return message;
    }
    public override string GetStringRepresentation() => $"PID: {ProcessId}, SecretKey: {SecretKey}";
}