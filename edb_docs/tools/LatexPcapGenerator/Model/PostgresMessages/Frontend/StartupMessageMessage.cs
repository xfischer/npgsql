using System.Diagnostics;

namespace pcap2latex;

public class StartupMessageMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public short ProtocolMajorVersion { get; private set; }
    public short ProtocolMinorVersion { get; private set; }

    public Dictionary<string, string> Parameters { get; set; } = new();

    internal static StartupMessageMessage Read(char messageCode, int length, PcapBinaryReader reader)
    {
        var message = new StartupMessageMessage(messageCode, length);
        message.ProtocolMajorVersion = reader.ReadInt16();
        message.ProtocolMinorVersion = reader.ReadInt16();

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

    internal static StartupMessageMessage Read(char code, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new StartupMessageMessage(code, len);
        message.ProtocolMajorVersion = LatexHelper.SafeGet(proto, 3, f => Convert.ToInt16(f.Value, 16));
        message.ProtocolMinorVersion= LatexHelper.SafeGet(proto, 4, f => Convert.ToInt16(f.Value, 16));

        (string ParamName, string ParamValue) currentTuple = ("", "");
        foreach (var field in proto.Fields.Skip(5))
        {
            if (field.Name.EndsWith("parameter_name"))
            {
                currentTuple.ParamName = LatexHelper.Unescape(field.Show);
            }
            else if (field.Name.EndsWith("parameter_value"))
            {
                currentTuple.ParamValue = LatexHelper.Unescape(field.Show);
                message.Parameters.Add(currentTuple.ParamName, currentTuple.ParamValue);
                currentTuple = ("", "");
            }
        }

        return message;
    }
}
