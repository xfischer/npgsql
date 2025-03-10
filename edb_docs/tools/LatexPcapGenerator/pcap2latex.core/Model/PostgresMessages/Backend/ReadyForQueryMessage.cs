namespace pcap2latex;

public class ReadyForQueryMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    public char StatusCode { get; private set; }
    
    public TransactionStatus Status { get; private set; }

    internal static ReadyForQueryMessage Read(PostgresMessage pgMessage, PcapBinaryReader reader)
    {
        var len = reader.ReadInt32();
        var message = new ReadyForQueryMessage(pgMessage, len)
        {
            StatusCode = reader.ReadChar()
        };
        message.Status = message.StatusCode switch
        {
            'I' => TransactionStatus.Idle,
            'T' => TransactionStatus.InTransaction,
            'E' => TransactionStatus.TransationInError,
            _ => TransactionStatus.Unknown,
        };

        return message;
    }

    public override string GetStringRepresentation() => Status.ToString();
}

public enum TransactionStatus
{
    Unknown,
    Idle,
    InTransaction,
    TransationInError
}