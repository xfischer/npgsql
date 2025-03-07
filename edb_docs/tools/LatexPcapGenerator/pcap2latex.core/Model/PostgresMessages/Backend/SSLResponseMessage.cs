namespace pcap2latex;

public class SSLResponseMessage(char code, int length) : PostgresMessageBase(code, length)
{
    internal static SSLResponseMessage Read(char messageCode) 
    {
        var message = new SSLResponseMessage(messageCode, 0);
        return message;
    }
}