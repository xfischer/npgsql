namespace pcap2latex
{
    public interface IPostgresMessage
    {
        char Code { get; }
        int Length { get; }
    }

    public abstract class PostgresMessageBase : IPostgresMessage
    {
        char _code;
        public char Code => _code;

        int _length;
        public int Length => _length;

        protected PostgresMessageBase(char code, int length)
        {
            _length = length;
            _code = code;
        }
    }
}