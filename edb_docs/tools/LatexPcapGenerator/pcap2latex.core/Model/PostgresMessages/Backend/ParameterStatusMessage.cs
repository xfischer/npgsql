namespace pcap2latex;

public class ParameterStatusMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public new string Name { get; private set; } = "";
    public string Value { get; private set; } = "";

    internal static ParameterStatusMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ParameterStatusMessage(pgMessage, len)
        {
            Name = reader.ReadNullTerminatedString(len),
            Value = reader.ReadNullTerminatedString(len)
        };

        return message;
    }

    public override string GetStringRepresentation() => $"{Name}: {Value}";
}