
namespace pcap2latex
{
    public class DescribeMessage(char code, int length) : PostgresMessageBase(code, length)
    {
        public char PortalOrStatement { get; internal set; }
        public string PortalOrStatementName { get; internal set; } = "";

        internal static DescribeMessage Read(char messageCode, PcapBinaryReader reader)
        {
            var len = reader.ReadInt32();
            var message = new DescribeMessage(messageCode, len);
            message.PortalOrStatement = reader.ReadChar();
            message.PortalOrStatementName = reader.ReadNullTerminatedString(len);

            return message;
        }

        internal static DescribeMessage Read(char messageCode, Serialization.Proto proto)
        {
            var len = Convert.ToInt16(proto.Fields[1].Value, 16);
            var message = new DescribeMessage(messageCode, len);
            message.PortalOrStatement = proto.Fields[3].Name.EndsWith("statement") ? 'S' : 'P';
            message.PortalOrStatementName = proto.Fields[3].Show;

            return message;
        }
    }
}