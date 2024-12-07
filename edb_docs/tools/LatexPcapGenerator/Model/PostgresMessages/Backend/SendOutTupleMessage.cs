namespace pcap2latex;

public class SendOutTupleMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public short ParameterCount { get; internal set; }

    public List<(int Length, byte[] Data)> ParameterValues { get; internal set; } = new();

    internal static SendOutTupleMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new SendOutTupleMessage(messageCode, len);
        message.ParameterCount = reader.ReadInt16();
        
        for (int i = 0; i < message.ParameterCount; i++)
        {
            int colLength = reader.ReadInt32();
            message.ParameterValues.Add(new(colLength, colLength > 0 ? reader.ReadBytes(colLength) : Array.Empty<byte>()));
        }

        return message;
    }

    internal static SendOutTupleMessage Read(char code, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new SendOutTupleMessage(code, len);
        message.ParameterCount = Convert.ToInt16(proto.Fields[3].Value, 16);
        var fields = proto.Fields[3].Fields;
        for (int i = 0; i < fields.Count; i++)
        {
            if (fields[i].Name.EndsWith("length"))
            {
                var length = int.Parse(fields[i].Show);
                if (length >0 )
                {
                    i++;
                    var data = Convert.FromHexString(fields[i].Value);
                    message.ParameterValues.Add((length, data));
                }
                else
                {
                    message.ParameterValues.Add((length, Array.Empty<byte>()));
                }
            }
        }

        return message;
    }
}
