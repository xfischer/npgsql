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
}

public enum TransactionStatus
{
    Unknown,
    Idle,
    InTransaction,
    TransationInError
}