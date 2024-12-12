
namespace pcap2latex
{
    public class DescribeOutMessage(char code, int length) : PostgresMessageBase(code, length)
    {
        public char PortalOrStatement { get; internal set; }
        public string PortalOrStatementName { get; internal set; } = "";

        internal static DescribeOutMessage Read(char messageCode, PcapBinaryReader reader)
        {
            var len = reader.ReadInt32();
            var message = new DescribeOutMessage(messageCode, len);
            message.PortalOrStatement = reader.ReadChar();
            message.PortalOrStatementName = reader.ReadNullTerminatedString(len);

            return message;
        }
    }
}