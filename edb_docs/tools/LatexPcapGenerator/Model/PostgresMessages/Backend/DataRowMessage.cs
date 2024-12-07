namespace pcap2latex;

public class DataRowMessage(char code, int length) : PostgresMessageBase(code, length)
{

    public short FieldCount { get; internal set; }

    public List<(int Length, byte[]? Data)> ColumnValues { get; internal set; } = new();

    internal static DataRowMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new DataRowMessage(messageCode, len);
        message.FieldCount = reader.ReadInt16();

        for (int i = 0; i < message.FieldCount; i++)
        {
            int colLength = reader.ReadInt32();
            message.ColumnValues.Add(new(colLength, colLength > 0 ? reader.ReadBytes(colLength) : null));
        }

        return message;
    }

    internal static DataRowMessage Read(char code, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new DataRowMessage(code, len);
        message.FieldCount = Convert.ToInt16(proto.Fields[3].Value, 16);
        var fields = proto.Fields[3].Fields;
        for (int i = 0; i < fields.Count; i++)
        {
            if (fields[i].Name.EndsWith("length"))
            {
                var length = int.Parse(fields[i].Show);
                if (length > 0)
                {
                    i++;
                    var data = Convert.FromHexString(fields[i].Value);
                    message.ColumnValues.Add((length, data));
                }
                else
                {
                    message.ColumnValues.Add((length, Array.Empty<byte>()));
                }

            }
        }

        return message;
    }
}
