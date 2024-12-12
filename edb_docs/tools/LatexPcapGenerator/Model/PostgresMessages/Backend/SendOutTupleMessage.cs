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

    internal static SendOutTupleMessage Read(char messageCode, PcapBinaryReader reader, OutDescriptionMessage? lastOutDescription)
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
}
