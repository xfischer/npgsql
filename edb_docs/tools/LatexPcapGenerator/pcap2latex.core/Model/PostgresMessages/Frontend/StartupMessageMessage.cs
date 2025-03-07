using System.Diagnostics;

namespace pcap2latex;

public class StartupMessageMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public short ProtocolMajorVersion { get; private set; }
    public short ProtocolMinorVersion { get; private set; }

    public Dictionary<string, string> Parameters { get; set; } = [];

    internal static StartupMessageMessage Read(char messageCode, int length, PcapBinaryReader reader)
    {
        var message = new StartupMessageMessage(messageCode, length)
        {
            ProtocolMajorVersion = reader.ReadInt16(),
            ProtocolMinorVersion = reader.ReadInt16()
        };

        int bytesRead = 4 + 2 + 2;
        
        string? paramName = null;
        while (bytesRead < length - 1)
        {
            var param = reader.ReadNullTerminatedString(length);
            bytesRead += param.Length;
            bytesRead += 1; // null terminator

            if (paramName is null)
            {
                paramName = param;
            }
            else
            {
                message.Parameters.Add(paramName, param);
                paramName = null;
            }
        }
        var lastByte = reader.ReadByte();
        Debug.Assert(lastByte == 0);

        return message;
    }
}
