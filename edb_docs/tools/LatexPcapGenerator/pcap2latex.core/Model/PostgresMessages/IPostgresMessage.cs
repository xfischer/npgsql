namespace pcap2latex;

public interface IPostgresMessage
{
    char Code { get; }
    int Length { get; }
}

public abstract class PostgresMessageBase(char code, int length) : IPostgresMessage
{
    public char Code => code;

    public int Length => length;
}