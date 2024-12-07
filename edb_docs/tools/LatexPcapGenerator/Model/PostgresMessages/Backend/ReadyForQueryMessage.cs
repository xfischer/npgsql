namespace pcap2latex;

public class ReadyForQueryMessage(char code, int length) : PostgresMessageBase(code, length)
{
    public char StatusCode { get; private set; }
    
    public TransactionStatus Status { get; private set; }

    internal static ReadyForQueryMessage Read(char messageCode, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ReadyForQueryMessage(messageCode, len);
        message.StatusCode = reader.ReadChar();
        message.Status = message.StatusCode switch
        {
            'I' => TransactionStatus.Idle,
            'T' => TransactionStatus.InTransaction,
            'E' => TransactionStatus.TransationInError,
            _ => TransactionStatus.Unknown,
        };

        return message;
    }
    internal static ReadyForQueryMessage Read(char messageCode, Serialization.Proto proto)
    {
        var len = Convert.ToInt16(proto.Fields[1].Value, 16);
        var message = new ReadyForQueryMessage(messageCode, len);
        message.StatusCode = (char)Convert.ToInt16(proto.Fields[3].Value, 16);
        message.Status = message.StatusCode switch
        {
            'I' => TransactionStatus.Idle,
            'T' => TransactionStatus.InTransaction,
            'E' => TransactionStatus.TransationInError,
            _ => TransactionStatus.Unknown,
        };

        return message;
    }
}

public enum TransactionStatus
{
    Unknown,
    Idle,
    InTransaction,
    TransationInError
}