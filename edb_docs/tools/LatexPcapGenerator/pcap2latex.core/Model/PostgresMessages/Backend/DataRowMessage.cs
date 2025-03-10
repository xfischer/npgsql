using System.Text;

namespace pcap2latex;

public class DataRowMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public class Row(int Length, bool IsText, byte[]? Data, string? TextRepresentation)
    {
        public string? Name { get; set; }

        public int Length { get; } = Length;
        public bool IsText { get; } = IsText;
        public byte[]? Data { get; } = Data;
        public string? TextRepresentation { get; } = TextRepresentation;
    };

    public short FieldCount { get; internal set; }

    public List<Row> ColumnValues { get; internal set; } = [];

    internal static DataRowMessage? Read(PostgresMessage pgMessage, PcapBinaryReader reader, RowDescriptionMessage? lastRowDescription)
    {
        if (!reader.HasSufficientData(4))
            return null;

        var len = reader.ReadInt32();
        if (!reader.HasSufficientData(len))
            return null;

        var message = new DataRowMessage(pgMessage, len)
        {
            FieldCount = reader.ReadInt16()
        };

        for (int i = 0; i < message.FieldCount; i++)
        {
            int colLength = reader.ReadInt32();
            bool isText = lastRowDescription?.FieldDescriptions[i].Format == 0;
            byte[]? data = (colLength > 0) ? reader.ReadBytes(colLength) : null;
            string? text = (data != null) ? Encoding.UTF8.GetString(data) : null;

            var row = new Row(colLength, isText, data, text) { Name = lastRowDescription?.FieldDescriptions[i].ColumnName };
            message.ColumnValues.Add(row);
        }

        return message;
    }

    public override string GetStringRepresentation() 
        => $"[{string.Join(", ", ColumnValues.Select(c => $"{c.Name}:{c.TextRepresentation}"))}]";
}
