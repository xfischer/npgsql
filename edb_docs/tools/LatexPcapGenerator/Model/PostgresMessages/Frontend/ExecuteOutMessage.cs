namespace pcap2latex
{
    public class ExecuteOutMessage(char code, int length) : PostgresMessageBase(code, length)
    {
        public string PortalName { get; internal set; } = "";

        internal static ExecuteOutMessage Read(char messageCode, PcapBinaryReader reader)
        {
            var len = reader.ReadInt32();
            var message = new ExecuteOutMessage(messageCode, len);
            message.PortalName = reader.ReadNullTerminatedString(len);

            return message;
        }
    }
}