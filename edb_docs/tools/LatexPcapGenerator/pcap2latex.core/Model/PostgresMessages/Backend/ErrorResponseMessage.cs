namespace pcap2latex;

public class ErrorResponseMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public List<(char FieldType, string Message)> Fields { get; private set; } = [];

    internal static ErrorResponseMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ErrorResponseMessage(pgMessage, len);

        char fieldType = reader.ReadChar();
        do
        {
            if (fieldType == 0)
            {
                fieldType = reader.ReadChar();
            }
            else
            {
                message.Fields.Add((fieldType, reader.ReadNullTerminatedString(len)));
                fieldType = reader.ReadChar();
            }
        }
        while (fieldType != 0);

        return message;
    }

    public override string GetStringRepresentation() => $"[{string.Join(", ", Fields.Select(f => $"{f.FieldType}: {f.Message}"))}]";
}