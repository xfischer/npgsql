namespace pcap2latex;

public class NoticeResponseMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public List<(char FieldType, string Message)> Fields { get; private set; } = [];

    internal static NoticeResponseMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new NoticeResponseMessage(messageCode, len);
                    
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
}