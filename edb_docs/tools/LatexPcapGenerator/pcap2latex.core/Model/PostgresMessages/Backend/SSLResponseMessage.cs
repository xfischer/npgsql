namespace pcap2latex;

public class SSLResponseMessage(PostgresMessage pgMessage, int length) : PostgresMessageBase(pgMessage, length)
{
    internal static SSLResponseMessage Read(PostgresMessage pgMessage) 
    {
        var message = new SSLResponseMessage(pgMessage, 0);
        return message;
    }
}