
namespace pcap2latex
{
    public class ExecuteMessage(char code, int length) : PostgresMessageBase(code, length)
    {
        public string PortalName { get; internal set; } = "";
        public int MaxRows { get; internal set; }

        internal static ExecuteMessage Read(char messageCode, PcapBinaryReader reader)
        {
            var len = reader.ReadInt32();
            var message = new ExecuteMessage(messageCode, len);
            message.PortalName = reader.ReadNullTerminatedString(len);
            message.MaxRows = reader.ReadInt32();

            return message;
        }

        internal static ExecuteMessage Read(char code, Serialization.Proto proto)
        {
            var len = Convert.ToInt16(proto.Fields[1].Value, 16);
            var message = new ExecuteMessage(code, len);
            message.PortalName = proto.Fields[3].Showname;
            message.MaxRows = Convert.ToInt16(proto.Fields[4].Value, 16);

            return message;
        }
    }
}