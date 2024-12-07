
namespace pcap2latex;

public class ParameterStatusMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public string Name { get; private set; } = "";
    public string Value { get; private set; } = "";

    internal static ParameterStatusMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ParameterStatusMessage(messageCode, len);
        message.Name = reader.ReadNullTerminatedString(len);
        message.Value = reader.ReadNullTerminatedString(len);

        return message;
    }

    internal static ParameterStatusMessage Read(char code, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new ParameterStatusMessage(code, len);
        message.Name = LatexHelper.SafeGet(proto, 3, f => f.Show);
        message.Value = LatexHelper.SafeGet(proto, 4, f => f.Show);

        return message;
    }
}