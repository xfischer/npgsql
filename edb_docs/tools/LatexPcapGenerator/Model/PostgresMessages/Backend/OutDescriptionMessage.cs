namespace pcap2latex;

public class OutDescriptionMessage(char code, int length) : PostgresMessageBase(code, length)
{

    public short FieldCount { get; internal set; }

    public List<OutParamDescription> ParameterDescriptions { get; internal set; } = new();

    internal static OutDescriptionMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new OutDescriptionMessage(messageCode, len);
        message.FieldCount = reader.ReadInt16();

        for (int i = 0; i < message.FieldCount; i++)
        {
            message.ParameterDescriptions.Add(OutParamDescription.Read(reader, len));
        }

        return message;
    }

    internal static OutDescriptionMessage Read(char code, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new OutDescriptionMessage(code, len);
        message.FieldCount = Convert.ToInt16(proto.Fields[3].Value, 16);

        foreach (var column in proto.Fields[3].Fields)
        {
            message.ParameterDescriptions.Add(new OutParamDescription()
            {
                ColumnIndex = short.Parse(column.Fields[0].Show),
                ColumnLength = short.Parse(column.Fields[2].Show),
                ColumnName = column.Show,
                Format = short.Parse(column.Fields[4].Show),
                TypeModifier = int.Parse(column.Fields[3].Show),
                TypeOid = int.Parse(column.Fields[1].Show)
            });
        }

        return message;
    }
}
