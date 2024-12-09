using static pcap2latex.DataRowMessage;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace pcap2latex;

public class SendOutTupleMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public class Param(int Length, bool IsText, byte[]? Data, string? TextRepresentation)
    {
        public string? Name { get; set; }
        public int Length { get; } = Length;
        public bool IsText { get; } = IsText;
        public byte[]? Data { get; } = Data;
        public string? TextRepresentation { get; } = TextRepresentation;
    };

    public short ParameterCount { get; internal set; }

    public List<Param> ParameterValues { get; internal set; } = new();

    internal static SendOutTupleMessage Read(char messageCode, PcapBinaryReader reader, OutDescriptionMessage lastOutDescription)
    {
        var len = reader.ReadInt32();
        var message = new SendOutTupleMessage(messageCode, len);
        message.ParameterCount = reader.ReadInt16();
        
        for (int i = 0; i < message.ParameterCount; i++)
        {
            int colLength = reader.ReadInt32();
            bool isText = lastOutDescription?.ParameterDescriptions[i].Format == 0;
            byte[]? data = (colLength > 0) ? reader.ReadBytes(colLength) : null;
            string? text = (data != null) ? Encoding.UTF8.GetString(data) : null;

            var param = new Param(colLength, isText, data, text) { Name = lastOutDescription?.ParameterDescriptions[i].ColumnName };
            message.ParameterValues.Add(param);
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
                    message.ParameterValues.Add(new Param(length, false, data, null));
                }
                else
                {
                    message.ParameterValues.Add(new Param(length, false, Array.Empty<byte>(), null));
                }
            }
        }

        return message;
    }
}
