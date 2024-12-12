namespace pcap2latex;

public class BindMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public string PortalName { get; internal set; } = "";
    public string StatementName { get; internal set; } = "";
    public short ParameterCount { get; internal set; }


    public short ParameterFormatsCount { get; internal set; }
    public List<short> ParameterFormats { get; internal set; } = new();


    public short ParameterValuesCount { get; internal set; }
    public List<(int Length, byte[] Data)> ParameterValues { get; internal set; } = new();

    public short ResultsFormatCount { get; internal set; }
    public List<short> ResultsFormat { get; internal set; } = new();

    internal static BindMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new BindMessage(messageCode, len);
        message.PortalName = reader.ReadNullTerminatedString(len);
        message.StatementName = reader.ReadNullTerminatedString(len);

        message.ParameterFormatsCount = reader.ReadInt16();
        for (int i = 0; i < message.ParameterFormatsCount; i++)
        {
            message.ParameterFormats.Add(reader.ReadInt16());
        }

        message.ParameterValuesCount = reader.ReadInt16();
        for (int i = 0; i < message.ParameterValuesCount; i++)
        {
            int paramLength = reader.ReadInt32();
            if (paramLength >0)
            {
                message.ParameterValues.Add(new(paramLength, reader.ReadBytes(paramLength)));
            }
            else
            {
                message.ParameterValues.Add(new(paramLength, Array.Empty<byte>()));
            }
        }

        message.ResultsFormatCount = reader.ReadInt16();
        for (int i = 0; i < message.ResultsFormatCount; i++)
        {
            message.ResultsFormat.Add(reader.ReadInt16());
        }
        return message;
    }
}
